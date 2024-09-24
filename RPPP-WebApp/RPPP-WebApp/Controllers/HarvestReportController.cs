using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PdfRpt.Core.Contracts;
using PdfRpt.FluentInterface;
using RPPP_WebApp.Model;
using OfficeOpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Globalization;

namespace RPPP_WebApp.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class HarvestReportsController : Controller
    {
        private readonly Rppp15Context _context;

        public HarvestReportsController(Rppp15Context context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            return View();
        }
        public async Task<IActionResult> SimpleHarvestReport()
        {
            var harvest = await _context.Harvests
                .AsNoTracking()
                .OrderBy(d => d.IdHarvest)
                .ToListAsync();

            var report = CreateReport("HarvestReport");
            report.PagesFooter(footer =>
            {
                footer.DefaultFooter(DateTime.Now.ToString("dd.MM.yyyy."));
            })
            .PagesHeader(header =>
            {
                header.CacheHeader(cache: true);
                header.DefaultHeader(defaultHeader =>
                {
                    defaultHeader.RunDirection(PdfRunDirection.LeftToRight);
                    defaultHeader.Message("Harvests");
                });
            });

            report.MainTableDataSource(dataSource =>
                dataSource.StronglyTypedList(harvest));

            report.MainTableColumns(columns =>
            {
                columns.AddColumn(column =>
                {
                    column.IsRowNumber(true);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Left);
                    column.IsVisible(true); column.Order(0); column.Width(1);
                    column.HeaderCell("#", horizontalAlignment: HorizontalAlignment.Left);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<Harvest>(x => x.CollectedOn);
                    column.IsVisible(true);
                    column.Order(1);
                    column.Width(2);
                    column.HeaderCell("Collected on");
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<Harvest>(x => x.Weight);
                    column.IsVisible(true);
                    column.Order(2);
                    column.Width(2);
                    column.HeaderCell("Weight");
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<Harvest>(x => x.Tag);
                    column.IsVisible(true);
                    column.Order(3);
                    column.Width(2);
                    column.HeaderCell("Tag");
                });
            });
            byte[] pdf = report.GenerateAsByteArray();
            if (pdf != null)
            {
                Response.Headers.Add("content-disposition", "inline; filename=HarvestReport.pdf");
                return File(pdf, "application/pdf");
            }
            else
            {
                return NotFound();
            }
        }
        private PdfReport CreateReport(string title)
        {
            var pdf = new PdfReport();

            pdf.DocumentPreferences(doc =>
            {
                doc.Orientation(PageOrientation.Portrait);
                doc.PageSize(PdfPageSize.A4);
                doc.DocumentMetadata(new DocumentMetadata
                {
                    Author = "FER",
                    Application = "RPPP15",
                    Title = title
                });
                doc.Compression(new CompressionSettings
                {
                    EnableCompression = true,
                    EnableFullCompression = true
                });
            })
            .MainTableTemplate(template =>
            {
                template.BasicTemplate(BasicTemplate.ProfessionalTemplate);
            })
            .MainTablePreferences(table =>
            {
                table.ColumnsWidthsType(TableColumnWidthType.Relative);
                table.GroupsPreferences(new GroupsPreferences
                {
                    GroupType = GroupType.HideGroupingColumns,
                    RepeatHeaderRowPerGroup = true,
                    ShowOneGroupPerPage = true
                });
                table.SpacingAfter(4f);
            });

            return pdf;
        }
        public async Task<IActionResult> MasterDetailHarvestReportExcel()
        {
            var harvests = await _context.Harvests
                .Include(h => h.Purchases)
                .AsNoTracking()
                .OrderBy(h => h.IdHarvest)
                .ToListAsync();

            using (var package = new ExcelPackage())
            {
                foreach (var harvest in harvests)
                {
                    var worksheetName = $"{harvest.Tag}_{harvest.IdHarvest}";
                    var worksheet = package.Workbook.Worksheets.Add(worksheetName);

                    worksheet.Cells["A1"].Value = "Collected On";
                    worksheet.Cells["B1"].Value = "Tag";
                    worksheet.Cells["C1"].Value = "Weight";

                    worksheet.Cells["A2"].Value = harvest.CollectedOn.ToString("dd-MM-yyyy");
                    worksheet.Cells["B2"].Value = harvest.Tag;
                    worksheet.Cells["C2"].Value = harvest.Weight;

                    worksheet.Cells["A4"].Value = "Collected On";
                    worksheet.Cells["B4"].Value = "Gain";
                    worksheet.Cells["C4"].Value = "Weight";
                    worksheet.Cells["D4"].Value = "Tag";

                    var purchaseDetailsRow = 5;

                    foreach (var purchase in harvest.Purchases)
                    {
                        worksheet.Cells[purchaseDetailsRow, 1].Value = purchase.CollectedOn.ToString("dd-MM-yyyy");
                        worksheet.Cells[purchaseDetailsRow, 2].Value = purchase.Gain;
                        worksheet.Cells[purchaseDetailsRow, 3].Value = purchase.Weight;
                        worksheet.Cells[purchaseDetailsRow, 4].Value = purchase.Tag;

                        purchaseDetailsRow++;
                    }
                    using (var range = worksheet.Cells["A4:D4"])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                    }

                    worksheet.Column(1).Width = 15;
                    worksheet.Column(2).Width = 15;
                    worksheet.Column(3).Width = 15;
                    worksheet.Column(4).Width = 15;
                }
                using (var memoryStream = new MemoryStream())
                {
                    package.SaveAs(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    return File(memoryStream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "MasterDetailHarvestReport.xlsx");
                }
            }
        }
        [HttpPost]
        public async Task<IActionResult> UploadModifiedExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Invalid file");
            }

            var importStatuses = new List<string>();

            try
            {
                using var stream = file.OpenReadStream();
                using var package = new ExcelPackage(stream);
                var worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension.Rows;

                for (int row = 2; row <= rowCount; row++)
                {
                    try
                    {
                        var collectedOnValue = worksheet.Cells[row, 2].Value;
                        var weightValue = worksheet.Cells[row, 3].Value;
                        var tagValue = worksheet.Cells[row, 4].Value;

                        if (collectedOnValue == null || weightValue == null || tagValue == null)
                        {
                            importStatuses.Add("Import Failed - Missing data in one or more cells");
                            continue;
                        }
                        if (!DateTime.TryParseExact(collectedOnValue?.ToString(), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime collectedOn))
                        {
                            if (double.TryParse(collectedOnValue?.ToString(), out double oleAutomationDate))
                            {
                                collectedOn = DateTime.FromOADate(oleAutomationDate);
                            }
                            else
                            {
                                importStatuses.Add($"Import Failed - Invalid CollectedOn value");
                                continue;
                            }
                        }


                        if (!double.TryParse(weightValue.ToString(), out double weight))
                        {
                            importStatuses.Add($"Import Failed - Invalid Weight value");
                            continue;
                        }
                        var tag = tagValue.ToString();
                        var modifiedData = new
                        {
                            CollectedOn = collectedOn,
                            Weight = weight,
                            Tag = tag
                        };

                        bool importSuccess = UpdateData(modifiedData);
                        importStatuses.Add(importSuccess ? $"Imported Successfully" : $"Import Failed");
                    }
                    catch (Exception ex)
                    {
                        importStatuses.Add($"Error processing row {row}: {ex.Message}");
                    }
                }

                worksheet.Cells["G1"].Value = "Import Status";

                for (int i = 0; i < importStatuses.Count; i++)
                {
                    worksheet.Cells[i + 2, 7].Value = importStatuses[i];
                }

                using (var resultPackage = new ExcelPackage())
                {
                    resultPackage.Workbook.Worksheets.Add("ModifiedDataWithStatus", worksheet);

                    using (var resultStream = new MemoryStream())
                    {
                        resultPackage.SaveAs(resultStream);
                        resultStream.Seek(0, SeekOrigin.Begin);

                        return File(resultStream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ModifiedDataWithStatus.xlsx");
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal Server Error");
            }
        }

        public async Task<IActionResult> SimpleHarvestReportExcel()
        {
            var recipes = await _context.Harvests
                .AsNoTracking()
                .OrderBy(d => d.IdHarvest)
                .ToListAsync();

            // Create a new Excel package
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Harvests");

            // Add column headers
            worksheet.Cells["A1"].Value = "ID";
            worksheet.Cells["B1"].Value = "Collected on";
            worksheet.Cells["C1"].Value = "Weight";
            worksheet.Cells["D1"].Value = "Tag";
            worksheet.Cells["E1"].Value = "Vegetation Id";

            // Populate the worksheet with data
            for (int i = 0; i < recipes.Count; i++)
            {
                var recipe = recipes[i];
                worksheet.Cells[i + 2, 1].Value = recipe.IdHarvest;
                worksheet.Cells[i + 2, 2].Value = recipe.CollectedOn;

                // Apply the custom date format to the cell
                worksheet.Cells[i + 2, 2].Style.Numberformat.Format = "yyyy-MM-dd HH:mm:ss";

                worksheet.Cells[i + 2, 3].Value = recipe.Weight;
                worksheet.Cells[i + 2, 4].Value = recipe.Tag;
                worksheet.Cells[i + 2, 5].Value = recipe.VegetationId;
            }
            using (var resultStream = new MemoryStream())
            {
                package.SaveAs(resultStream);
                resultStream.Seek(0, SeekOrigin.Begin);

                return File(resultStream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Harvests.xlsx");
            }
        }
        private bool UpdateData(dynamic modifiedData)
        {
            try
            {
                double weight = double.Parse(modifiedData.Weight.ToString());
                string tag = modifiedData.Tag.ToString();

                DateTime collectedOn;

                if (!DateTime.TryParse(modifiedData.CollectedOn.ToString(), out collectedOn))
                {
                    if (double.TryParse(modifiedData.CollectedOn.ToString(), out double oleAutomationDate))
                    {
                        collectedOn = DateTime.FromOADate(oleAutomationDate);
                    }
                    else
                    {
                        return false;
                    }
                }
                var harvest = _context.Harvests
                    .FirstOrDefault(h => h.Tag == tag && h.CollectedOn == collectedOn && h.Weight == weight);

                if (harvest != null)
                {
                    harvest.Tag = tag;
                    harvest.Weight = weight;
                    harvest.CollectedOn = collectedOn;

                    _context.SaveChanges();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

    }
}