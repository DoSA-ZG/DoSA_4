using Azure;
using iTextSharp.text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using PdfRpt.Core.Contracts;
using PdfRpt.FluentInterface;
using RPPP_WebApp.Model;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using OfficeOpenXml;
using iTextSharp.text.pdf;
using DocumentFormat.OpenXml.InkML;

namespace RPPP_WebApp.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class RecipePlantReportsController : Controller
    {
        private readonly Rppp15Context _context;

        public RecipePlantReportsController(Rppp15Context context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            return View();
        }


        public async Task<IActionResult> SimpleRecipeReport()
        {
            var recipes = await _context.Recipes
                                        .AsNoTracking()
                                        .OrderBy(d => d.IdRecipe)
                                        .ToListAsync();

            var report = CreateReport("RecipeReport");
            report.PagesFooter(footer => {
                footer.DefaultFooter(DateTime.Now
                .ToString("dd.MM.yyyy."));
            })
            .PagesHeader(header => {
                header.CacheHeader(cache: true);
                header.DefaultHeader(defaultHeader => {
                    defaultHeader.RunDirection(
                    PdfRunDirection.LeftToRight);
                    defaultHeader.Message("Recipes");
                });
            });

            report.MainTableDataSource(dataSource =>
                dataSource.StronglyTypedList(recipes));

            report.MainTableColumns(columns =>
            {
                columns.AddColumn(column => {
                    column.IsRowNumber(true);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Left);
                    column.IsVisible(true); column.Order(0); column.Width(1);
                    column.HeaderCell("#",
                    horizontalAlignment: HorizontalAlignment.Left);
                });


                columns.AddColumn(column =>
                {
                    column.PropertyName<Recipe>(x => x.Description);
                    column.IsVisible(true);
                    column.Order(1);
                    column.Width(2);
                    column.HeaderCell("Description");
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<Recipe>(x => x.CaloriesPerServing);
                    column.IsVisible(true);
                    column.Order(2);
                    column.Width(2);
                    column.HeaderCell("Calories Per Serving");

                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<Recipe>(x => x.ApproximateDuration);
                    column.IsVisible(true);
                    column.Order(3);
                    column.Width(2);
                    column.HeaderCell("Approximate Duration");

                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<Recipe>(x => x.Caption);
                    column.IsVisible(true);
                    column.Order(4);
                    column.Width(2);
                    column.HeaderCell("Caption");

                });
            });

            byte[] pdf = report.GenerateAsByteArray();

            if (pdf != null)
            {
                Response.Headers.Add("content-disposition", "inline; filename=RecipeReport.pdf");
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
                    Author = "RPPP15",
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
        public async Task<IActionResult> SimpleExcelRecipeReport()
        {
            var recipes = await _context.Recipes
                .AsNoTracking()
                .OrderBy(d => d.IdRecipe)
                .ToListAsync();

            // Create a new Excel package
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Recipes");

            // Add column headers
            worksheet.Cells["A1"].Value = "ID";
            worksheet.Cells["B1"].Value = "Description";
            worksheet.Cells["C1"].Value = "Calories Per Serving";
            worksheet.Cells["D1"].Value = "Approximate Duration";
            worksheet.Cells["E1"].Value = "Cuisine";
            worksheet.Cells["F1"].Value = "Caption";

            // Populate the worksheet with data
            for (int i = 0; i < recipes.Count; i++)
            {
                var recipe = recipes[i];
                worksheet.Cells[i + 2, 1].Value = recipe.IdRecipe;
                worksheet.Cells[i + 2, 2].Value = recipe.Description;
                worksheet.Cells[i + 2, 3].Value = recipe.CaloriesPerServing;
                worksheet.Cells[i + 2, 4].Value = recipe.ApproximateDuration ?? 0; // Handle nulls
                worksheet.Cells[i + 2, 5].Value = recipe.CuisineId; // Assuming Cuisine has a Name property
                worksheet.Cells[i + 2, 6].Value = recipe.Caption;
            }
            using (var resultStream = new MemoryStream())
            {
                package.SaveAs(resultStream);
                resultStream.Seek(0, SeekOrigin.Begin);

                return File(resultStream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Recipes.xlsx");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UploadModifiedRecipe(IFormFile file)
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
                    var modifiedData = new
                    {
                        Description = worksheet.Cells[row, 2].Value,
                        CaloriesPerServing = worksheet.Cells[row, 3].Value,
                        ApproximateDuration = worksheet.Cells[row, 4].Value,
                        CuisineId = worksheet.Cells[row, 5].Value,
                        Caption = worksheet.Cells[row, 6].Value
                    };


                    bool importSuccess = UpdateRecipeData(modifiedData);
                    importStatuses.Add(importSuccess ? "Imported Successfully" : "Import Failed");
                }

                worksheet.Cells["G1"].Value = "Import Status";

                for (int i = 0; i < importStatuses.Count; i++)
                {
                    worksheet.Cells[i + 2, 7].Value = importStatuses[i];
                }

                using (var resultPackage = new ExcelPackage())
                {
                    resultPackage.Workbook.Worksheets.Add("ModifiedRecipeWithStatus", worksheet);

                    using (var resultStream = new MemoryStream())
                    {
                        resultPackage.SaveAs(resultStream);
                        resultStream.Seek(0, SeekOrigin.Begin);

                        return File(resultStream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ModifiedRecipeWithStatus.xlsx");
                    }
                }

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal Server Error");
            }
        }
        private bool UpdateRecipeData(dynamic modifiedData)
        {
            try
            {
                string description = modifiedData.Description.ToString();
                double calories = double.Parse(modifiedData.CaloriesPerServing.ToString());
                double approximateDuration = double.Parse(modifiedData.ApproximateDuration.ToString());
                int cuisineId = int.Parse(modifiedData.CuisineId.ToString());
                string caption = modifiedData.Caption.ToString();

                var recipe = _context.Recipes.FirstOrDefault(r => r.Caption == caption && r.CaloriesPerServing == calories);

                if (recipe != null)
                {

                    recipe.Description = description;
                    recipe.CaloriesPerServing = calories;
                    recipe.ApproximateDuration = approximateDuration;
                    recipe.CuisineId = cuisineId;
                    recipe.Caption = caption;

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


        public IActionResult RecipePlantExcel()
        {
            var recipes = _context.Recipes
                .Include(r => r.Ingredients)
                .ThenInclude(i => i.PlantClass)
                .ToList();

            using (var package = new ExcelPackage())
            {
                foreach (var recipe in recipes)
                {
                    // Create a new worksheet for each recipe
                    string worksheetName = $"{recipe.IdRecipe}_{recipe.Caption}";
                    var worksheet = package.Workbook.Worksheets.Add(worksheetName);

                    // Add headers
                    worksheet.Cells["A1"].Value = "Caption";
                    worksheet.Cells["B1"].Value = "Description";
                    worksheet.Cells["C1"].Value = "Calories Per Serving";
                    worksheet.Cells["D1"].Value = "Approximate duration";


                    // Populate data
                    worksheet.Cells["A2"].Value = recipe.Caption;
                    worksheet.Cells["B2"].Value = recipe.Description;
                    worksheet.Cells["C2"].Value = recipe.CaloriesPerServing;
                    worksheet.Cells["D2"].Value = recipe.ApproximateDuration;


                    // Add headers for plant details
                    worksheet.Cells["A4"].Value = "Name";
                    worksheet.Cells["B4"].Value = "Passport";
                    worksheet.Cells["C4"].Value = "Fiber Per Serving";
                    worksheet.Cells["D4"].Value = "Potassium Per Serving";

                    // Populate plant details
                    var row = 5;
                    foreach (var ingredient in recipe.Ingredients)
                    {
                        worksheet.Cells[row, 1].Value = ingredient.PlantClass.Name;
                        worksheet.Cells[row, 2].Value = ingredient.PlantClass.Passport;
                        worksheet.Cells[row, 3].Value = ingredient.PlantClass.FiberPerServing;
                        worksheet.Cells[row, 4].Value = ingredient.PlantClass.PotassiumPerServing;

                        row++;
                    }
                }

                // Save the Excel package to a memory stream
                using (var memoryStream = new MemoryStream())
                {
                    package.SaveAs(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    // Return the Excel file as a response
                    return File(memoryStream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "RecipePlant.xlsx");
                }
            }
        }
    }



}
