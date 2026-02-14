using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using TargetProjectManage.Models;
using TargetProjectManage.Data;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Text;
using ClosedXML.Excel;

namespace TargetProjectManage.Pages.Schedules
{
    public class IndexModel : PageModel
    {
        private readonly ProjectManageContext _context;

        public IndexModel(ProjectManageContext context)
        {
            _context = context;
        }
        
        public IList<Schedule> Schedules { get; set; } = new List<Schedule>();
        public Project? CurrentProject { get; set; }
        
        public async Task OnGetAsync(int? projectId)
        {
            if (projectId.HasValue)
            {
                CurrentProject = await _context.Projects.FindAsync(projectId.Value);
                if (CurrentProject != null)
                {
                    Schedules = await _context.Schedules
                        .Where(s => s.ProjectId == projectId.Value)
                        .OrderBy(s => s.EndDate)
                        .ToListAsync();
                }
            }
            else
            {
                // 如果沒有指定 projectId，重定向到 Projects 頁面
                Response.Redirect("/Projects/Index");
            }
        }


        public async Task<IActionResult> OnGetExportCsvAsync(int? projectId)
        {
            if (!projectId.HasValue)
                return NotFound();

            var project = await _context.Projects.FindAsync(projectId.Value);
            if (project == null)
                return NotFound();

            var schedules = await _context.Schedules
                .Where(s => s.ProjectId == projectId.Value)
                .OrderBy(s => s.EndDate)
                .ToListAsync();

            var csv = new StringBuilder();
            
            // 添加 BOM 以支援中文
            csv.Append('\uFEFF');
            
            // 標題
            csv.AppendLine("專案時程管理報表");
            csv.AppendLine($"專案名稱,{project.ProjectName}");
            csv.AppendLine($"專案代碼,{project.ProjectCode}");
            csv.AppendLine($"匯出時間,{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            csv.AppendLine();
            
            // 表頭
            csv.AppendLine("階段名稱,階段類型,開始日期,結束日期,狀態,負責人,備註");
            
            // 資料
            foreach (var schedule in schedules)
            {
                csv.AppendLine($"{schedule.StageName},{schedule.StageType ?? ""},{schedule.StartDate:yyyy-MM-dd},{schedule.EndDate:yyyy-MM-dd},{schedule.Status ?? ""},{schedule.Owner ?? ""},{schedule.Remark ?? ""}");
            }

            var fileName = $"{project.ProjectName}_時程管理_{DateTime.Now:yyyyMMdd}.csv";
            var fileBytes = Encoding.UTF8.GetBytes(csv.ToString());

            return File(fileBytes, "text/csv; charset=utf-8", fileName);
        }

        public async Task<IActionResult> OnGetExportExcelAsync(int? projectId)
        {
            if (!projectId.HasValue)
                return NotFound();

            var project = await _context.Projects.FindAsync(projectId.Value);
            if (project == null)
                return NotFound();

            var schedules = await _context.Schedules
                .Where(s => s.ProjectId == projectId.Value)
                .OrderBy(s => s.EndDate)
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("時程管理");

            // 設定標題
            worksheet.Cell("A1").Value = "專案時程管理報表";
            worksheet.Range("A1:G1").Merge();
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

            // 表頭
            var headerRow = 7;
            worksheet.Cell($"A{headerRow}").Value = "階段名稱";
            worksheet.Cell($"B{headerRow}").Value = "階段類型";
            worksheet.Cell($"C{headerRow}").Value = "開始日期";
            worksheet.Cell($"D{headerRow}").Value = "結束日期";
            worksheet.Cell($"E{headerRow}").Value = "狀態";
            worksheet.Cell($"F{headerRow}").Value = "負責人";
            worksheet.Cell($"G{headerRow}").Value = "備註";

            // 設定表頭樣式
            var headerRange = worksheet.Range($"A{headerRow}:G{headerRow}");
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            // 資料
            var dataRow = headerRow + 1;
            foreach (var schedule in schedules)
            {
                worksheet.Cell($"A{dataRow}").Value = schedule.StageName;
                worksheet.Cell($"B{dataRow}").Value = schedule.StageType ?? "";
                worksheet.Cell($"C{dataRow}").Value = schedule.StartDate.ToString("yyyy-MM-dd");
                worksheet.Cell($"D{dataRow}").Value = schedule.EndDate.ToString("yyyy-MM-dd");
                worksheet.Cell($"E{dataRow}").Value = schedule.Status ?? "";
                worksheet.Cell($"F{dataRow}").Value = schedule.Owner ?? "";
                worksheet.Cell($"G{dataRow}").Value = schedule.Remark ?? "";
                dataRow++;
            }

            // 設定資料範圍樣式
            var dataRange = worksheet.Range($"A{headerRow}:G{dataRow - 1}");
            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            // 自動調整欄寬
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var fileName = $"{project.ProjectName}_時程管理_{DateTime.Now:yyyyMMdd}.xlsx";

            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}
