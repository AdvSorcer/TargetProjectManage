using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TargetProjectManage.Data;
using TargetProjectManage.Models;
using ClosedXML.Excel;
using System.Text;

namespace TargetProjectManage.Pages.Analysis
{
    public class CostAnalysisModel : PageModel
    {
        private readonly ProjectManageContext _context;

        public CostAnalysisModel(ProjectManageContext context)
        {
            _context = context;
        }

        public Project? CurrentProject { get; set; }
        public IList<CostItem> CostItems { get; set; } = new List<CostItem>();
        public Dictionary<string, decimal> CostByCategory { get; set; } = new Dictionary<string, decimal>();
        public decimal TotalEstimatedCost { get; set; }
        public decimal TotalActualCost { get; set; }

        public async Task<IActionResult> OnGetAsync(int? projectId)
        {
            if (projectId == null)
            {
                CurrentProject = null;
                return Page();
            }

            CurrentProject = await _context.Projects
                .Include(p => p.CostItems)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (CurrentProject == null)
            {
                return NotFound();
            }

            CostItems = CurrentProject.CostItems.ToList();

            // 計算各類別成本
            CostByCategory = CostItems
                .GroupBy(c => c.Category)
                .ToDictionary(g => g.Key, g => g.Sum(c => c.ActualCost));

            TotalEstimatedCost = CostItems.Sum(c => c.EstimatedCost);
            TotalActualCost = CostItems.Sum(c => c.ActualCost);

            return Page();
        }

        public async Task<IActionResult> OnGetExportExcelAsync(int? projectId)
        {
            if (projectId == null)
                return NotFound();

            var project = await _context.Projects
                .Include(p => p.CostItems)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
                return NotFound();

            var costItems = project.CostItems.ToList();
            var costByCategory = costItems
                .GroupBy(c => c.Category)
                .ToDictionary(g => g.Key, g => g.Sum(c => c.ActualCost));
            var totalEstimatedCost = costItems.Sum(c => c.EstimatedCost);
            var totalActualCost = costItems.Sum(c => c.ActualCost);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("成本分析");

            // 設定標題
            worksheet.Cell("A1").Value = "成本分析報表";
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

            // 成本摘要
            var summaryRow = 7;
            worksheet.Cell($"A{summaryRow}").Value = "成本摘要";
            worksheet.Cell($"A{summaryRow}").Style.Font.Bold = true;
            worksheet.Cell($"A{summaryRow}").Style.Font.FontSize = 14;
            summaryRow += 2;

            worksheet.Cell($"A{summaryRow}").Value = "總預估成本：";
            worksheet.Cell($"B{summaryRow}").Value = totalEstimatedCost;
            worksheet.Cell($"B{summaryRow}").Style.NumberFormat.Format = "NT$ #,##0";
            summaryRow++;

            worksheet.Cell($"A{summaryRow}").Value = "總實際成本：";
            worksheet.Cell($"B{summaryRow}").Value = totalActualCost;
            worksheet.Cell($"B{summaryRow}").Style.NumberFormat.Format = "NT$ #,##0";
            summaryRow++;

            worksheet.Cell($"A{summaryRow}").Value = "成本差異：";
            worksheet.Cell($"B{summaryRow}").Value = totalActualCost - totalEstimatedCost;
            worksheet.Cell($"B{summaryRow}").Style.NumberFormat.Format = "NT$ #,##0";
            summaryRow++;

            worksheet.Cell($"A{summaryRow}").Value = "差異百分比：";
            worksheet.Cell($"B{summaryRow}").Value = totalEstimatedCost > 0 ? (totalActualCost - totalEstimatedCost) / totalEstimatedCost : 0;
            worksheet.Cell($"B{summaryRow}").Style.NumberFormat.Format = "0.0%";
            summaryRow += 2;

            // 各類別成本
            if (costByCategory.Any())
            {
                worksheet.Cell($"A{summaryRow}").Value = "各類別成本分析";
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

                foreach (var category in costByCategory)
                {
                    var percentage = totalActualCost > 0 ? category.Value / totalActualCost : 0;
                    worksheet.Cell($"A{summaryRow}").Value = category.Key;
                    worksheet.Cell($"B{summaryRow}").Value = category.Value;
                    worksheet.Cell($"B{summaryRow}").Style.NumberFormat.Format = "NT$ #,##0";
                    worksheet.Cell($"C{summaryRow}").Value = percentage;
                    worksheet.Cell($"C{summaryRow}").Style.NumberFormat.Format = "0.0%";
                    summaryRow++;
                }
                summaryRow += 2;
            }

            // 成本明細表頭
            worksheet.Cell($"A{summaryRow}").Value = "成本明細";
            worksheet.Cell($"A{summaryRow}").Style.Font.Bold = true;
            worksheet.Cell($"A{summaryRow}").Style.Font.FontSize = 14;
            summaryRow += 2;

            var headerRow = summaryRow;
            worksheet.Cell($"A{headerRow}").Value = "項目名稱";
            worksheet.Cell($"B{headerRow}").Value = "類別";
            worksheet.Cell($"C{headerRow}").Value = "預估成本";
            worksheet.Cell($"D{headerRow}").Value = "實際成本";
            worksheet.Cell($"E{headerRow}").Value = "差異";
            worksheet.Cell($"F{headerRow}").Value = "備註";

            // 設定表頭樣式
            var headerRange = worksheet.Range($"A{headerRow}:F{headerRow}");
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            // 成本明細資料
            var dataRow = headerRow + 1;
            foreach (var item in costItems)
            {
                var difference = item.ActualCost - item.EstimatedCost;
                worksheet.Cell($"A{dataRow}").Value = item.ItemName;
                worksheet.Cell($"B{dataRow}").Value = item.Category;
                worksheet.Cell($"C{dataRow}").Value = item.EstimatedCost;
                worksheet.Cell($"C{dataRow}").Style.NumberFormat.Format = "NT$ #,##0";
                worksheet.Cell($"D{dataRow}").Value = item.ActualCost;
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
            var fileName = $"{project.ProjectName}_成本分析_{DateTime.Now:yyyyMMdd}.xlsx";

            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}