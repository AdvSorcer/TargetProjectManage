using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using TargetProjectManage.Models;
using TargetProjectManage.Data;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using System.Text;

namespace TargetProjectManage.Pages.Proposals
{
    public class IndexModel : PageModel
    {
        private readonly ProjectManageContext _context;

        public IndexModel(ProjectManageContext context)
        {
            _context = context;
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
                worksheet.Cell($"B{summaryRow}").Value = $"NT$ {proposals.Average(p => p.Budget):N0}";
                summaryRow++;

                worksheet.Cell($"A{summaryRow}").Value = "開發時程範圍：";
                worksheet.Cell($"B{summaryRow}").Value = $"{proposals.Min(p => p.DurationMonths)} - {proposals.Max(p => p.DurationMonths)} 個月";
                summaryRow++;

                worksheet.Cell($"A{summaryRow}").Value = "平均開發時程：";
                worksheet.Cell($"B{summaryRow}").Value = $"{proposals.Average(p => p.DurationMonths):0.0} 個月";
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

            // 提案明細資料（先填資料，樣式用範圍統一設定以減少 styles.xml 問題）
            var dataRow = headerRow + 1;
            foreach (var proposal in proposals)
            {
                worksheet.Cell($"A{dataRow}").Value = proposal.VendorName;
                worksheet.Cell($"B{dataRow}").Value = $"NT$ {proposal.Budget:N0}";
                worksheet.Cell($"C{dataRow}").Value = $"{proposal.DurationMonths} 個月";
                worksheet.Cell($"D{dataRow}").Value = proposal.SubmitDate.ToString("yyyy-MM-dd");
                worksheet.Cell($"E{dataRow}").Value = proposal.Comment ?? "";
                dataRow++;
            }

            var lastDataRow = dataRow - 1;
            var tableRange = worksheet.Range(headerRow, 1, lastDataRow, 5);

            // 表頭樣式：粗體、底色（只套表頭列）
            var headerRange = worksheet.Range(headerRow, 1, headerRow, 5);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

            // 整張表邊框一次設定
            tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            // 自動調整欄寬
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var fileName = $"{project.ProjectName}_廠商提案_{DateTime.Now:yyyyMMdd}.xlsx";

            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}
