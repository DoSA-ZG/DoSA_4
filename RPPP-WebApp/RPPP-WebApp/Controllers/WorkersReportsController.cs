using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PdfRpt.ColumnsItemsTemplates;
using PdfRpt.Core.Contracts;
using PdfRpt.Core.Helper;
using PdfRpt.FluentInterface;
using Microsoft.Data.SqlClient;
using OfficeOpenXml;
using RPPP_WebApp.Extensions;
using RPPP_WebApp.Model;
using iTextSharp.text;
using iTextSharp.text.pdf;
using RPPP_WebApp.ViewModels;
using System.Data;  
using System.Data.SqlClient;  
using System.Configuration;  
using System.Data.OleDb;  
using System.Data.Common;  

namespace RPPP_WebAppControllers;

public class WorkerReportsController : Controller
{
  private readonly Rppp15Context ctx;
  private readonly IWebHostEnvironment environment;
  private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

  public WorkerReportsController(Rppp15Context ctx, IWebHostEnvironment environment)
  {
    this.ctx = ctx;
    this.environment = environment;
  }

  public IActionResult Index()
  {
    return View();
  }

  public async Task<IActionResult> ExcelMasterDetail()
  {
    // Limit number of exported workers to 10
    var workers = await ctx.Workers.Take(10).Include(w => w.Measures).ToListAsync();
    byte[] content;
    using (ExcelPackage excel = new ExcelPackage())
    {
      excel.Workbook.Properties.Title = "Workers List";
      excel.Workbook.Properties.Author = "Viktoriia Myts";

      foreach (var worker in workers) {
        var worksheet = excel.Workbook.Worksheets.Add($"Worker {worker.IdWorker}");

        worksheet.Cells[1, 1].Value = "IdWorker";
        worksheet.Cells[2, 1].Value = worker.IdWorker;

        worksheet.Cells[1, 2].Value = "DailyWage";
        worksheet.Cells[2, 2].Value = worker.DailyWage;

        worksheet.Cells[1, 3].Value = "Tag";
        worksheet.Cells[2, 3].Value = worker.Tag;

        worksheet.Cells[1, 4].Value = "Notes";
        worksheet.Cells[2, 4].Value = worker.Notes;

        worksheet.Cells[1, 5].Value = "WorkerTypeId";
        worksheet.Cells[2, 5].Value = worker.WorkerTypeId;

        worksheet.Cells[1, 6].Value = "Email";
        worksheet.Cells[2, 6].Value = worker.Email;

        worksheet.Cells[1, 7].Value = "Phone";
        worksheet.Cells[2, 7].Value = worker.Phone;

        if (worker.Measures.Count == 0) continue;

        worksheet.Cells[4, 1].Value = "IdMeasure";
        worksheet.Cells[4, 2].Value = "PerformedOn";
        worksheet.Cells[4, 3].Value = "Description";
        worksheet.Cells[4, 4].Value = "MeasureTypeId";
        worksheet.Cells[4, 5].Value = "VegetationId";
        worksheet.Cells[4, 6].Value = "DurationMinutes";

        int i = 0;
        foreach (var measure in worker.Measures) {
          worksheet.Cells[i + 5, 1].Value = measure.IdMeasure;
          worksheet.Cells[i + 5, 2].Value = measure.PerformedOn.ToString();
          worksheet.Cells[i + 5, 3].Value = measure.Description;
          worksheet.Cells[i + 5, 4].Value = measure.MeasureTypeId;
          worksheet.Cells[i + 5, 5].Value = measure.VegetationId;
          worksheet.Cells[i + 5, 6].Value = measure.DurationMinutes;
          i++;
        }
      }
      content = excel.GetAsByteArray();
    }
    return File(content, ExcelContentType, "workers_masterdetail.xlsx");
  }

  public async Task<IActionResult> Excel()
  {
    var workers = await ctx.Workers.ToListAsync();
    byte[] content;
    using (ExcelPackage excel = workers.CreateExcel("Workers"))
    {
      content = excel.GetAsByteArray();
    }
    return File(content, ExcelContentType, "workers.xlsx");
  }

  [HttpPost]
  public async Task<IActionResult> ImportExcel(IFormFile file)
  {
    if (file == null || file.Length == 0)
    {
      return BadRequest("File is empty");
    }

    using (var stream = new MemoryStream())
    {
      await file.CopyToAsync(stream);

      using (var package = new ExcelPackage(stream))
      {
        var worksheet = package.Workbook.Worksheets.FirstOrDefault();

        if (worksheet == null)
        {
          return BadRequest("No worksheet found in the Excel file");
        }

        worksheet.Cells[1, 8].Value = "Import Status";

        for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
        {
          bool importStatus = true;
          try {
            var id = worksheet.Cells[row, 1].Value?.ToString();
            var typeId = worksheet.Cells[row, 5].Value?.ToString();
            var worker = new Worker
            {
              IdWorker = id != null ? int.Parse(id) : 0,
              DailyWage = decimal.Parse(worksheet.Cells[row, 2].Value.ToString()),
              Tag = worksheet.Cells[row, 3].Value?.ToString(),
              Notes = worksheet.Cells[row, 4].Value?.ToString(),
              WorkerTypeId = typeId != null ? int.Parse(typeId) : null,
              Email = worksheet.Cells[row, 6].Value?.ToString(),
              Phone = worksheet.Cells[row, 7].Value?.ToString()
            };
            ctx.Workers.Update(worker);
            await ctx.SaveChangesAsync();
          } catch (Exception ex) {
            importStatus = false;
          }
          worksheet.Cells[row, 8].Value = importStatus;
        }

        return File(package.GetAsByteArray(), ExcelContentType, "workers_import.xlsx");
      }
    }
  }

  private IQueryable<MeasureReportViewModel> GetMeasures(int workerCount) {
    // For simplicity every measure is considered done on a separate day
    var measures = ctx.Workers
    .Include(w => w.Measures)
    .Select(worker => new {
      Worker = worker,
      Salary = worker.Measures.Count * worker.DailyWage
    })
    .OrderByDescending(result => result.Salary)
    .Take(workerCount)
    .SelectMany(result => result.Worker.Measures.Select(m => new MeasureReportViewModel {
      IdMeasure = m.IdMeasure,
      PerformedOn = m.PerformedOn,
      Description = m.Description,
      MeasureTypeId = m.MeasureTypeId,
      VegetationId = m.VegetationId,
      WorkerId = m.WorkerId,
      DurationMinutes = m.DurationMinutes,
      WorkerSalary = result.Salary,
      WorkerWage = result.Worker.DailyWage,
      MeasureTypeCaption = m.MeasureType.Caption,
      VegetationSummary = $"{m.Vegetation.PlantClass.Name}"
    }));

    return measures;
  }

  public async Task<IActionResult> Pdf()
  {
    int n = 10;
    string title = $"Top {n} most paid workers";
    PdfReport report = CreateReport(title);

    var measures = GetMeasures(n);

    #region Header and footer
    report.PagesFooter(footer =>
    {
      footer.DefaultFooter(DateTime.Now.ToString("dd.MM.yyyy"));
    })
    .PagesHeader(header =>
    {
      header.CacheHeader(cache: true); // It's a default setting to improve the performance.
      header.CustomHeader(new MasterDetailsHeaders(title)
      {
        PdfRptFont = header.PdfFont
      });
    });
    #endregion
    
    #region Set data source and define columns
    report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(measures));

    report.MainTableColumns(columns =>
    {
      #region Columns used for groupings
      columns.AddColumn(column =>
      {
        column.PropertyName<MeasureReportViewModel>(s => s.WorkerId);
        column.Group(
            (val1, val2) =>
            {
              return (int)val1 == (int)val2;
            });
      });
      #endregion
      columns.AddColumn(column =>
      {
        column.IsRowNumber(true);
        column.CellsHorizontalAlignment(HorizontalAlignment.Right);
        column.IsVisible(true);
        column.Order(0);
        column.Width(1);
        column.HeaderCell("#", horizontalAlignment: HorizontalAlignment.Right);
      });

      columns.AddColumn(column =>
      {
        column.PropertyName(nameof(MeasureReportViewModel.PerformedOn));
        column.CellsHorizontalAlignment(HorizontalAlignment.Center);
        column.IsVisible(true);
        column.Order(2);
        column.Width(1);
        column.HeaderCell("Performed on:");
      });

      columns.AddColumn(column =>
      {
        column.PropertyName(nameof(MeasureReportViewModel.Description));
        column.CellsHorizontalAlignment(HorizontalAlignment.Center);
        column.IsVisible(true);
        column.Order(2);
        column.Width(1);
        column.HeaderCell("Description:");
      });

      columns.AddColumn(column =>
      {
        column.PropertyName(nameof(MeasureReportViewModel.MeasureTypeCaption));
        column.CellsHorizontalAlignment(HorizontalAlignment.Center);
        column.IsVisible(true);
        column.Order(2);
        column.Width(1);
        column.HeaderCell("Type:");
      });

      columns.AddColumn(column =>
      {
        column.PropertyName(nameof(MeasureReportViewModel.VegetationSummary));
        column.CellsHorizontalAlignment(HorizontalAlignment.Center);
        column.IsVisible(true);
        column.Order(2);
        column.Width(1);
        column.HeaderCell("Plant Class:");
      });

      columns.AddColumn(column =>
      {
        column.PropertyName(nameof(MeasureReportViewModel.DurationMinutes));
        column.CellsHorizontalAlignment(HorizontalAlignment.Center);
        column.IsVisible(true);
        column.Order(2);
        column.Width(1);
        column.HeaderCell("Duration:");
      });
    });

    #endregion

    byte[] pdf = report.GenerateAsByteArray();

    if (pdf != null)
    {
      Response.Headers.Add("content-disposition", "inline; filename=documents.pdf");
      return File(pdf, "application/pdf");
    }
    else {
      return NotFound();
    }
  }

  #region Master-detail header
  public class MasterDetailsHeaders : IPageHeader
  {
    private readonly string title;
    public MasterDetailsHeaders(string title)
    {
      this.title = title;
    }
    public IPdfFont PdfRptFont { set; get; }

    public PdfGrid RenderingGroupHeader(iTextSharp.text.Document pdfDoc, PdfWriter pdfWriter, IList<CellData> newGroupInfo, IList<SummaryCellData> summaryData)
    {
      var workerId = newGroupInfo.GetSafeStringValueOf(nameof(MeasureReportViewModel.WorkerId));
      var salary = (decimal)newGroupInfo.GetValueOf(nameof(MeasureReportViewModel.WorkerSalary));
      var wage = (decimal)newGroupInfo.GetValueOf(nameof(MeasureReportViewModel.WorkerWage));

      var table = new PdfGrid(relativeWidths: new[] { 3f, 2f, 5f, 5f, 5f, 5f }) { WidthPercentage = 100 };

      table.AddSimpleRow(
          (cellData, cellProperties) =>
          {
            cellData.Value = "Worker Id:";
            cellProperties.PdfFont = PdfRptFont;
            cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
            cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
          },
          (cellData, cellProperties) =>
          {
            cellData.Value = workerId;
            cellProperties.PdfFont = PdfRptFont;
            cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
          },
          (cellData, cellProperties) =>
          {
            cellData.Value = "Wage:";
            cellProperties.PdfFont = PdfRptFont;
            cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
            cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
          },
          (cellData, cellProperties) =>
          {
            cellData.Value = wage;
            cellProperties.DisplayFormatFormula = obj => ((decimal)obj).ToString("C2");
            cellProperties.PdfFont = PdfRptFont;
            cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
          },
          (cellData, cellProperties) =>
          {
            cellData.Value = "Salary:";
            cellProperties.PdfFont = PdfRptFont;
            cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
            cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
          },
          (cellData, cellProperties) =>
          {
            cellData.Value = salary;
            cellProperties.DisplayFormatFormula = obj => ((decimal)obj).ToString("C2");
            cellProperties.PdfFont = PdfRptFont;
            cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
          });
      return table.AddBorderToTable(borderColor: BaseColor.LightGray, spacingBefore: 5f);
    }

    public PdfGrid RenderingReportHeader(iTextSharp.text.Document pdfDoc, PdfWriter pdfWriter, IList<SummaryCellData> summaryData)
    {
      var table = new PdfGrid(numColumns: 1) { WidthPercentage = 100 };
      table.AddSimpleRow(
         (cellData, cellProperties) =>
         {
           cellData.Value = title;
           cellProperties.PdfFont = PdfRptFont;
           cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
           cellProperties.HorizontalAlignment = HorizontalAlignment.Center;
         });
      return table.AddBorderToTable();
    }
  }
  #endregion

  #region Private methods
  private PdfReport CreateReport(string title)
  {
    var pdf = new PdfReport();

    pdf.DocumentPreferences(doc =>
    {
      doc.Orientation(PageOrientation.Portrait);
      doc.PageSize(PdfPageSize.A4);
      doc.DocumentMetadata(new DocumentMetadata
      {
        Author = "Viktoriia Myts",
        Application = "RPPP15-WebApp",
        Title = title
      });
      doc.Compression(new CompressionSettings
      {
        EnableCompression = true,
        EnableFullCompression = true
      });
    })
    .DefaultFonts(fonts => {
      fonts.Path(Path.Combine(environment.WebRootPath, "fonts", "verdana.ttf"),
                       Path.Combine(environment.WebRootPath, "fonts", "tahoma.ttf"));
      fonts.Size(9);
      fonts.Color(System.Drawing.Color.Black);
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
        ShowOneGroupPerPage = true,
        SpacingBeforeAllGroupsSummary = 5f,
        NewGroupAvailableSpacingThreshold = 150,
        SpacingAfterAllGroupsSummary = 5f
      });
      table.SpacingAfter(4f);
    });

    return pdf;
  }
  #endregion
}