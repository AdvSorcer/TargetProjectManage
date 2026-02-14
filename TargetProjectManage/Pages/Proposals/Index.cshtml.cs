using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using TargetProjectManage.Models;
using TargetProjectManage.Data;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TargetProjectManage.Services;
using ClosedXML.Excel;
using System.Text;

namespace TargetProjectManage.Pages.Proposals
{
    public class IndexModel : PageModel
    {
        private readonly ProjectManageContext _context;
        private readonly IPdfService _pdfService;
        
        public IndexModel(ProjectManageContext context, IPdfService pdfService)
        {
            _context = context;
            _pdfService = pdfService;
        }
        
        public IList<Proposal> Proposals { get; set; } = new List<Proposal>();
        public Project? CurrentProject { get; set; }
        
        public async Task OnGetAsync(int? projectId)
        {
            if (projectId.HasValue)
            {
                CurrentProject = await _context.Projects.FindAsync(projectId.Value);
                if (CurrentProject != null)
                {
                    Proposals = await _context.Proposals
                        .Where(p => p.ProjectId == projectId.Value)
                        .OrderByDescending(p => p.SubmitDate)
                        .ToListAsync();
                }
            }
            else
            {
                // 如果沒有指定 projectId，重定向到 Projects 頁面
                Response.Redirect("/Projects/Index");
            }
        }

        public async Task<IActionResult> OnGetExportPdfAsync(int? projectId)
        {
            if (!projectId.HasValue)
                return NotFound();

            var project = await _context.Projects.FindAsync(projectId.Value);
            if (project == null)
                return NotFound();

            var proposals = await _context.Proposals
                .Where(p => p.ProjectId == projectId.Value)
                .OrderByDescending(p => p.SubmitDate)
                .ToListAsync();

            // 建立 HTML 內容
            var html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html><head>");
            html.AppendLine("<meta charset='utf-8'>");
            html.AppendLine("<style>");
            html.AppendLine("body { font-family: 'Microsoft JhengHei', Arial, sans-serif; margin: 20px; }");
            html.AppendLine("h1 { color: #2c3e50; text-align: center; }");
            html.AppendLine("h2 { color: #34495e; border-bottom: 2px solid #9b59b6; padding-bottom: 5px; }");
            html.AppendLine("table { width: 100%; border-collapse: collapse; margin-top: 20px; }");
            html.AppendLine("th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }");
            html.AppendLine("th { background-color: #f2f2f2; font-weight: bold; }");
            html.AppendLine("tr:nth-child(even) { background-color: #f9f9f9; }");
            html.AppendLine(".info { background-color: #f8f0ff; padding: 10px; margin: 10px 0; border-left: 4px solid #9b59b6; }");
            html.AppendLine(".summary { background-color: #f0f8ff; padding: 15px; margin: 15px 0; border: 1px solid #3498db; border-radius: 5px; }");
            html.AppendLine("</style>");
            html.AppendLine("</head><body>");

            html.AppendLine($"<h1>廠商提案管理報表</h1>");
            html.AppendLine($"<div class='info'>");
            html.AppendLine($"<strong>專案名稱：</strong>{project.ProjectName}<br>");
            html.AppendLine($"<strong>專案代碼：</strong>{project.ProjectCode}<br>");
            html.AppendLine($"<strong>匯出時間：</strong>{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            html.AppendLine($"</div>");

            // 提案摘要
            if (proposals.Any())
            {
                var totalBudget = proposals.Sum(p => p.Budget);
                var avgBudget = proposals.Average(p => p.Budget);
                var totalDuration = proposals.Sum(p => p.DurationMonths);
                var avgDuration = proposals.Average(p => p.DurationMonths);

                html.AppendLine("<div class='summary'>");
                html.AppendLine("<h3>提案摘要</h3>");
                html.AppendLine($"<p><strong>提案廠商數量：</strong>{proposals.Count} 家</p>");
                html.AppendLine($"<p><strong>總預算範圍：</strong>NT$ {proposals.Min(p => p.Budget):N0} - NT$ {proposals.Max(p => p.Budget):N0}</p>");
                html.AppendLine($"<p><strong>平均預算：</strong>NT$ {avgBudget:N0}</p>");
                html.AppendLine($"<p><strong>總開發時程：</strong>{proposals.Min(p => p.DurationMonths)} - {proposals.Max(p => p.DurationMonths)} 個月</p>");
                html.AppendLine($"<p><strong>平均開發時程：</strong>{avgDuration:F1} 個月</p>");
                html.AppendLine("</div>");
            }

            // 提案明細
            html.AppendLine("<h2>廠商提案明細</h2>");
            html.AppendLine("<table>");
            html.AppendLine("<tr><th>廠商名稱</th><th>預算</th><th>開發時程</th><th>提出日期</th><th>評語</th></tr>");

            foreach (var proposal in proposals)
            {
                html.AppendLine($"<tr>");
                html.AppendLine($"<td>{proposal.VendorName}</td>");
                html.AppendLine($"<td>NT$ {proposal.Budget:N0}</td>");
                html.AppendLine($"<td>{proposal.DurationMonths} 個月</td>");
                html.AppendLine($"<td>{proposal.SubmitDate:yyyy-MM-dd}</td>");
                html.AppendLine($"<td>{proposal.Comment ?? ""}</td>");
                html.AppendLine($"</tr>");
            }

            html.AppendLine("</table>");
            html.AppendLine("</body></html>");

            // 使用 PuppeteerSharp 轉換為 PDF
            var pdfBytes = await _pdfService.GeneratePdfAsync(html.ToString(), $"{project.ProjectName}_廠商提案");
            var fileName = $"{project.ProjectName}_廠商提案_{DateTime.Now:yyyyMMdd}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }

        public async Task<IActionResult> OnGetExportExcelAsync(int? projectId)
        {
            if (!projectId.HasValue)
                return NotFound();

            var project = await _context.Projects.FindAsync(projectId.Value);
            if (project == null)
                return NotFound();

            var proposals = await _context.Proposals
                .Where(p => p.ProjectId == projectId.Value)
                .OrderByDescending(p => p.SubmitDate)
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("廠商提案");

            // 設定標題
            worksheet.Cell("A1").Value = "廠商提案管理報表";
            worksheet.Range("A1:E1").Merge();
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

            // 提案摘要
            if (proposals.Any())
            {
                var summaryRow = 7;
                worksheet.Cell($"A{summaryRow}").Value = "提案摘要";
                worksheet.Cell($"A{summaryRow}").Style.Font.Bold = true;
                worksheet.Cell($"A{summaryRow}").Style.Font.FontSize = 14;
                summaryRow += 2;

                worksheet.Cell($"A{summaryRow}").Value = "提案廠商數量：";
                worksheet.Cell($"B{summaryRow}").Value = proposals.Count;
                worksheet.Cell($"B{summaryRow}").Value = $"{proposals.Count} 家";
                summaryRow++;

                worksheet.Cell($"A{summaryRow}").Value = "預算範圍：";
                worksheet.Cell($"B{summaryRow}").Value = $"NT$ {proposals.Min(p => p.Budget):N0} - NT$ {proposals.Max(p => p.Budget):N0}";
                summaryRow++;

                worksheet.Cell($"A{summaryRow}").Value = "平均預算：";
                worksheet.Cell($"B{summaryRow}").Value = proposals.Average(p => p.Budget);
                worksheet.Cell($"B{summaryRow}").Style.NumberFormat.Format = "NT$ #,##0";
                summaryRow++;

                worksheet.Cell($"A{summaryRow}").Value = "開發時程範圍：";
                worksheet.Cell($"B{summaryRow}").Value = $"{proposals.Min(p => p.DurationMonths)} - {proposals.Max(p => p.DurationMonths)} 個月";
                summaryRow++;

                worksheet.Cell($"A{summaryRow}").Value = "平均開發時程：";
                worksheet.Cell($"B{summaryRow}").Value = proposals.Average(p => p.DurationMonths);
                worksheet.Cell($"B{summaryRow}").Style.NumberFormat.Format = "0.0 個月";
                summaryRow += 2;
            }

            // 提案明細表頭
            var headerRow = proposals.Any() ? 15 : 7;
            worksheet.Cell($"A{headerRow}").Value = "廠商提案明細";
            worksheet.Cell($"A{headerRow}").Style.Font.Bold = true;
            worksheet.Cell($"A{headerRow}").Style.Font.FontSize = 14;
            headerRow += 2;

            worksheet.Cell($"A{headerRow}").Value = "廠商名稱";
            worksheet.Cell($"B{headerRow}").Value = "預算";
            worksheet.Cell($"C{headerRow}").Value = "開發時程";
            worksheet.Cell($"D{headerRow}").Value = "提出日期";
            worksheet.Cell($"E{headerRow}").Value = "評語";

            // 設定表頭樣式
            var headerRange = worksheet.Range($"A{headerRow}:E{headerRow}");
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            // 提案明細資料
            var dataRow = headerRow + 1;
            foreach (var proposal in proposals)
            {
                worksheet.Cell($"A{dataRow}").Value = proposal.VendorName;
                worksheet.Cell($"B{dataRow}").Value = proposal.Budget;
                worksheet.Cell($"B{dataRow}").Style.NumberFormat.Format = "NT$ #,##0";
                worksheet.Cell($"C{dataRow}").Value = proposal.DurationMonths;
                worksheet.Cell($"C{dataRow}").Style.NumberFormat.Format = "0 個月";
                worksheet.Cell($"D{dataRow}").Value = proposal.SubmitDate.ToString("yyyy-MM-dd");
                worksheet.Cell($"E{dataRow}").Value = proposal.Comment ?? "";
                dataRow++;
            }

            // 設定資料範圍樣式
            var dataRange = worksheet.Range($"A{headerRow}:E{dataRow - 1}");
            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            // 自動調整欄寬
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var fileName = $"{project.ProjectName}_廠商提案_{DateTime.Now:yyyyMMdd}.xlsx";

            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}
