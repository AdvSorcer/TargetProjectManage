using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TargetProjectManage.Data;
using TargetProjectManage.Models;
using TargetProjectManage.Services;
using ClosedXML.Excel;
using System.Text;

namespace TargetProjectManage.Pages.Analysis
{
    public class BenefitAnalysisModel : PageModel
    {
        private readonly ProjectManageContext _context;
        private readonly IPdfService _pdfService;

        public BenefitAnalysisModel(ProjectManageContext context, IPdfService pdfService)
        {
            _context = context;
            _pdfService = pdfService;
        }

        public Project? CurrentProject { get; set; }
        public IList<BenefitItem> BenefitItems { get; set; } = new List<BenefitItem>();
        public Dictionary<string, decimal> BenefitByCategory { get; set; } = new Dictionary<string, decimal>();
        public decimal TotalEstimatedValue { get; set; }
        public decimal TotalActualValue { get; set; }

        public async Task<IActionResult> OnGetAsync(int? projectId)
        {
            if (projectId == null)
            {
                CurrentProject = null;
                return Page();
            }

            CurrentProject = await _context.Projects
                .Include(p => p.BenefitItems)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (CurrentProject == null)
            {
                return NotFound();
            }

            BenefitItems = CurrentProject.BenefitItems.ToList();

            // 計算各類別效益
            BenefitByCategory = BenefitItems
                .GroupBy(b => b.Category)
                .ToDictionary(g => g.Key, g => g.Sum(b => b.ActualValue));

            TotalEstimatedValue = BenefitItems.Sum(b => b.EstimatedValue);
            TotalActualValue = BenefitItems.Sum(b => b.ActualValue);

            return Page();
        }

        public async Task<IActionResult> OnGetExportPdfAsync(int? projectId)
        {
            if (projectId == null)
                return NotFound();

            var project = await _context.Projects
                .Include(p => p.BenefitItems)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
                return NotFound();

            var benefitItems = project.BenefitItems.ToList();
            var benefitByCategory = benefitItems
                .GroupBy(b => b.Category)
                .ToDictionary(g => g.Key, g => g.Sum(b => b.ActualValue));
            var totalEstimatedValue = benefitItems.Sum(b => b.EstimatedValue);
            var totalActualValue = benefitItems.Sum(b => b.ActualValue);

            // 建立 HTML 內容
            var html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html><head>");
            html.AppendLine("<meta charset='utf-8'>");
            html.AppendLine("<style>");
            html.AppendLine("body { font-family: 'Microsoft JhengHei', Arial, sans-serif; margin: 20px; }");
            html.AppendLine("h1 { color: #2c3e50; text-align: center; }");
            html.AppendLine("h2 { color: #34495e; border-bottom: 2px solid #27ae60; padding-bottom: 5px; }");
            html.AppendLine("table { width: 100%; border-collapse: collapse; margin-top: 20px; }");
            html.AppendLine("th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }");
            html.AppendLine("th { background-color: #f2f2f2; font-weight: bold; }");
            html.AppendLine("tr:nth-child(even) { background-color: #f9f9f9; }");
            html.AppendLine(".info { background-color: #f0f9f0; padding: 10px; margin: 10px 0; border-left: 4px solid #27ae60; }");
            html.AppendLine(".summary { background-color: #f0f8ff; padding: 15px; margin: 15px 0; border: 1px solid #3498db; border-radius: 5px; }");
            html.AppendLine(".category-item { margin: 5px 0; padding: 5px; background-color: #f8f9fa; border-left: 3px solid #27ae60; }");
            html.AppendLine("</style>");
            html.AppendLine("</head><body>");

            html.AppendLine($"<h1>效益分析報表</h1>");
            html.AppendLine($"<div class='info'>");
            html.AppendLine($"<strong>專案名稱：</strong>{project.ProjectName}<br>");
            html.AppendLine($"<strong>專案代碼：</strong>{project.ProjectCode}<br>");
            html.AppendLine($"<strong>匯出時間：</strong>{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            html.AppendLine($"</div>");

            // 效益摘要
            html.AppendLine("<div class='summary'>");
            html.AppendLine("<h3>效益摘要</h3>");
            html.AppendLine($"<p><strong>總預估效益：</strong>NT$ {totalEstimatedValue:N0}</p>");
            html.AppendLine($"<p><strong>總實際效益：</strong>NT$ {totalActualValue:N0}</p>");
            html.AppendLine($"<p><strong>效益差異：</strong>NT$ {(totalActualValue - totalEstimatedValue):N0}</p>");
            html.AppendLine($"<p><strong>差異百分比：</strong>{((totalActualValue - totalEstimatedValue) / totalEstimatedValue * 100):F1}%</p>");
            html.AppendLine("</div>");

            // 各類別效益
            if (benefitByCategory.Any())
            {
                html.AppendLine("<h2>各類別效益分析</h2>");
                foreach (var category in benefitByCategory)
                {
                    var percentage = totalActualValue > 0 ? (category.Value / totalActualValue * 100) : 0;
                    html.AppendLine($"<div class='category-item'>");
                    html.AppendLine($"<strong>{category.Key}：</strong>NT$ {category.Value:N0} ({percentage:F1}%)");
                    html.AppendLine($"</div>");
                }
            }

            // 效益明細
            html.AppendLine("<h2>效益明細</h2>");
            html.AppendLine("<table>");
            html.AppendLine("<tr><th>項目名稱</th><th>類別</th><th>預估效益</th><th>實際效益</th><th>差異</th><th>備註</th></tr>");

            foreach (var item in benefitItems)
            {
                var difference = item.ActualValue - item.EstimatedValue;
                html.AppendLine($"<tr>");
                html.AppendLine($"<td>{item.ItemName}</td>");
                html.AppendLine($"<td>{item.Category}</td>");
                html.AppendLine($"<td>NT$ {item.EstimatedValue:N0}</td>");
                html.AppendLine($"<td>NT$ {item.ActualValue:N0}</td>");
                html.AppendLine($"<td>NT$ {difference:N0}</td>");
                html.AppendLine($"<td>{item.Description ?? ""}</td>");
                html.AppendLine($"</tr>");
            }

            html.AppendLine("</table>");
            html.AppendLine("</body></html>");

            // 使用 PuppeteerSharp 轉換為 PDF
            var pdfBytes = await _pdfService.GeneratePdfAsync(html.ToString(), $"{project.ProjectName}_效益分析");
            var fileName = $"{project.ProjectName}_效益分析_{DateTime.Now:yyyyMMdd}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }

        public async Task<IActionResult> OnGetExportExcelAsync(int? projectId)
        {
            if (projectId == null)
                return NotFound();

            var project = await _context.Projects
                .Include(p => p.BenefitItems)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
                return NotFound();

            var benefitItems = project.BenefitItems.ToList();
            var benefitByCategory = benefitItems
                .GroupBy(b => b.Category)
                .ToDictionary(g => g.Key, g => g.Sum(b => b.ActualValue));
            var totalEstimatedValue = benefitItems.Sum(b => b.EstimatedValue);
            var totalActualValue = benefitItems.Sum(b => b.ActualValue);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("效益分析");

            // 設定標題
            worksheet.Cell("A1").Value = "效益分析報表";
            worksheet.Range("A1:F1").Merge();
            worksheet.Cell("A1").Style.Font.Bold = true;
            worksheet.Cell("A1").Style.Font.FontSize = 16;
            worksheet.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // 專案資訊
            worksheet.Cell("A3").Value = "專案名稱：";
            worksheet.Cell("B3").Value = project.ProjectName;
            worksheet.Cell("A4").Value = "專案代碼：";
            worksheet.Cell("B4").Value = project.ProjectCode;
            worksheet.Cell("A5").Value = "匯出時間：";
            worksheet.Cell("B5").Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            // 效益摘要
            var summaryRow = 7;
            worksheet.Cell($"A{summaryRow}").Value = "效益摘要";
            worksheet.Cell($"A{summaryRow}").Style.Font.Bold = true;
            worksheet.Cell($"A{summaryRow}").Style.Font.FontSize = 14;
            summaryRow += 2;

            worksheet.Cell($"A{summaryRow}").Value = "總預估效益：";
            worksheet.Cell($"B{summaryRow}").Value = totalEstimatedValue;
            worksheet.Cell($"B{summaryRow}").Style.NumberFormat.Format = "NT$ #,##0";
            summaryRow++;

            worksheet.Cell($"A{summaryRow}").Value = "總實際效益：";
            worksheet.Cell($"B{summaryRow}").Value = totalActualValue;
            worksheet.Cell($"B{summaryRow}").Style.NumberFormat.Format = "NT$ #,##0";
            summaryRow++;

            worksheet.Cell($"A{summaryRow}").Value = "效益差異：";
            worksheet.Cell($"B{summaryRow}").Value = totalActualValue - totalEstimatedValue;
            worksheet.Cell($"B{summaryRow}").Style.NumberFormat.Format = "NT$ #,##0";
            summaryRow++;

            worksheet.Cell($"A{summaryRow}").Value = "差異百分比：";
            worksheet.Cell($"B{summaryRow}").Value = totalEstimatedValue > 0 ? (totalActualValue - totalEstimatedValue) / totalEstimatedValue : 0;
            worksheet.Cell($"B{summaryRow}").Style.NumberFormat.Format = "0.0%";
            summaryRow += 2;

            // 各類別效益
            if (benefitByCategory.Any())
            {
                worksheet.Cell($"A{summaryRow}").Value = "各類別效益分析";
                worksheet.Cell($"A{summaryRow}").Style.Font.Bold = true;
                worksheet.Cell($"A{summaryRow}").Style.Font.FontSize = 14;
                summaryRow += 2;

                worksheet.Cell($"A{summaryRow}").Value = "類別";
                worksheet.Cell($"B{summaryRow}").Value = "金額";
                worksheet.Cell($"C{summaryRow}").Value = "百分比";
                var categoryHeaderRange = worksheet.Range($"A{summaryRow}:C{summaryRow}");
                categoryHeaderRange.Style.Font.Bold = true;
                categoryHeaderRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                summaryRow++;

                foreach (var category in benefitByCategory)
                {
                    var percentage = totalActualValue > 0 ? category.Value / totalActualValue : 0;
                    worksheet.Cell($"A{summaryRow}").Value = category.Key;
                    worksheet.Cell($"B{summaryRow}").Value = category.Value;
                    worksheet.Cell($"B{summaryRow}").Style.NumberFormat.Format = "NT$ #,##0";
                    worksheet.Cell($"C{summaryRow}").Value = percentage;
                    worksheet.Cell($"C{summaryRow}").Style.NumberFormat.Format = "0.0%";
                    summaryRow++;
                }
                summaryRow += 2;
            }

            // 效益明細表頭
            worksheet.Cell($"A{summaryRow}").Value = "效益明細";
            worksheet.Cell($"A{summaryRow}").Style.Font.Bold = true;
            worksheet.Cell($"A{summaryRow}").Style.Font.FontSize = 14;
            summaryRow += 2;

            var headerRow = summaryRow;
            worksheet.Cell($"A{headerRow}").Value = "項目名稱";
            worksheet.Cell($"B{headerRow}").Value = "類別";
            worksheet.Cell($"C{headerRow}").Value = "預估效益";
            worksheet.Cell($"D{headerRow}").Value = "實際效益";
            worksheet.Cell($"E{headerRow}").Value = "差異";
            worksheet.Cell($"F{headerRow}").Value = "備註";

            // 設定表頭樣式
            var headerRange = worksheet.Range($"A{headerRow}:F{headerRow}");
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            // 效益明細資料
            var dataRow = headerRow + 1;
            foreach (var item in benefitItems)
            {
                var difference = item.ActualValue - item.EstimatedValue;
                worksheet.Cell($"A{dataRow}").Value = item.ItemName;
                worksheet.Cell($"B{dataRow}").Value = item.Category;
                worksheet.Cell($"C{dataRow}").Value = item.EstimatedValue;
                worksheet.Cell($"C{dataRow}").Style.NumberFormat.Format = "NT$ #,##0";
                worksheet.Cell($"D{dataRow}").Value = item.ActualValue;
                worksheet.Cell($"D{dataRow}").Style.NumberFormat.Format = "NT$ #,##0";
                worksheet.Cell($"E{dataRow}").Value = difference;
                worksheet.Cell($"E{dataRow}").Style.NumberFormat.Format = "NT$ #,##0";
                worksheet.Cell($"F{dataRow}").Value = item.Description ?? "";
                dataRow++;
            }

            // 設定資料範圍樣式
            var dataRange = worksheet.Range($"A{headerRow}:F{dataRow - 1}");
            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            // 自動調整欄寬
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var fileName = $"{project.ProjectName}_效益分析_{DateTime.Now:yyyyMMdd}.xlsx";

            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}