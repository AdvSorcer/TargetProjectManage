using Microsoft.EntityFrameworkCore;
using TargetProjectManage.Models;

namespace TargetProjectManage.Data
{
    /// <summary>
    /// 用於 Demo 的假資料種子，僅在資料庫尚無專案時執行一次。
    /// </summary>
    public static class SeedData
    {
        public static async Task SeedAsync(ProjectManageContext context)
        {
            if (await context.Projects.AnyAsync())
                return;

            var now = DateTime.Now;
            var baseDate = new DateTime(now.Year, 1, 1);

            // ----- 專案 1：智慧辦公系統 -----
            var p1 = new Project
            {
                ProjectName = "智慧辦公系統建置案",
                ProjectCode = "PRJ-2024-001",
                Description = "整合 OA、差勤、請假與報表之內部系統，提升行政效率。",
                CreatedDate = baseDate.AddMonths(-3),
                StartDate = baseDate.AddMonths(-2),
                EndDate = baseDate.AddMonths(4),
                Status = "進行中",
                Owner = "王小明",
                Remark = "第一期以差勤與請假為主。",
                EstimatedCost = 2800000,
                ActualCost = 850000,
                Budget = 3000000,
                Revenue = 0,
                ROI = null,
                ProfitMargin = null,
                CostAnalysis = "已控管在預算內，設備採購延後一季。",
                BenefitAnalysis = "預估可節省 20% 行政作業時間。",
                CostAnalysisDate = baseDate.AddMonths(-1),
                BenefitAnalysisDate = baseDate.AddMonths(-1)
            };
            context.Projects.Add(p1);

            // ----- 專案 2：官網改版 -----
            var p2 = new Project
            {
                ProjectName = "官方網站改版計畫",
                ProjectCode = "PRJ-2024-002",
                Description = "響應式官網改版，含內容管理與數據分析整合。",
                CreatedDate = baseDate.AddMonths(-4),
                StartDate = baseDate.AddMonths(-3),
                EndDate = baseDate.AddMonths(2),
                Status = "進行中",
                Owner = "陳美玲",
                Remark = "需配合資安檢測時程。",
                EstimatedCost = 1500000,
                ActualCost = 1200000,
                Budget = 1600000,
                Revenue = 0,
                ROI = null,
                ProfitMargin = null,
                CostAnalysis = "設計與前端開發已結案。",
                BenefitAnalysis = "預計提升使用者滿意度與停留時間。",
                CostAnalysisDate = baseDate.AddMonths(-2),
                BenefitAnalysisDate = baseDate.AddMonths(-2)
            };
            context.Projects.Add(p2);

            // ----- 專案 3：機房升級 -----
            var p3 = new Project
            {
                ProjectName = "機房電力與空調升級",
                ProjectCode = "PRJ-2023-003",
                Description = "UPS、空調與監控系統更新，確保服務可用性。",
                CreatedDate = baseDate.AddMonths(-8),
                StartDate = baseDate.AddMonths(-6),
                EndDate = baseDate.AddMonths(-2),
                Status = "已結案",
                Owner = "李志偉",
                Remark = "驗收完成，保固兩年。",
                EstimatedCost = 4200000,
                ActualCost = 3980000,
                Budget = 4500000,
                Revenue = 0,
                ROI = null,
                ProfitMargin = null,
                CostAnalysis = "結案時未逾預算。",
                BenefitAnalysis = "停機風險顯著降低。",
                CostAnalysisDate = baseDate.AddMonths(-3),
                BenefitAnalysisDate = baseDate.AddMonths(-3)
            };
            context.Projects.Add(p3);

            await context.SaveChangesAsync();

            // 取得已寫入的專案 Id（SQLite 會自動產生）
            var projects = await context.Projects.OrderBy(x => x.Id).ToListAsync();
            var pid1 = projects[0].Id;
            var pid2 = projects[1].Id;
            var pid3 = projects[2].Id;

            // ----- 時程 (Schedules) -----
            var schedules = new List<Schedule>
            {
                new() { StageName = "需求訪談", StageType = "需求分析", StartDate = baseDate.AddMonths(-2), EndDate = baseDate.AddMonths(-2).AddDays(14), Owner = "王小明", Remark = "各單位訪談完成", ProjectId = pid1 },
                new() { StageName = "系統設計", StageType = "設計", StartDate = baseDate.AddMonths(-1).AddDays(15), EndDate = baseDate, Owner = "王小明", Remark = "UI/API 設計定稿", ProjectId = pid1 },
                new() { StageName = "開發與測試", StageType = "開發", StartDate = baseDate.AddDays(7), EndDate = baseDate.AddMonths(3), Owner = "王小明", Remark = "Sprint 進行中", ProjectId = pid1 },
                new() { StageName = "上線與教育訓練", StageType = "上線", StartDate = baseDate.AddMonths(3).AddDays(7), EndDate = baseDate.AddMonths(4), Owner = "王小明", ProjectId = pid1 },
                new() { StageName = "企劃與設計", StageType = "設計", StartDate = baseDate.AddMonths(-3), EndDate = baseDate.AddMonths(-2), Owner = "陳美玲", ProjectId = pid2 },
                new() { StageName = "前端開發", StageType = "開發", StartDate = baseDate.AddMonths(-2), EndDate = baseDate.AddMonths(-1), Owner = "陳美玲", ProjectId = pid2 },
                new() { StageName = "後端與 CMS", StageType = "開發", StartDate = baseDate.AddMonths(-1), EndDate = baseDate.AddMonths(1), Owner = "陳美玲", ProjectId = pid2 },
                new() { StageName = "電力系統施工", StageType = "施工", StartDate = baseDate.AddMonths(-6), EndDate = baseDate.AddMonths(-4), Owner = "李志偉", ProjectId = pid3 },
                new() { StageName = "空調與監控", StageType = "施工", StartDate = baseDate.AddMonths(-4), EndDate = baseDate.AddMonths(-2), Owner = "李志偉", ProjectId = pid3 }
            };
            context.Schedules.AddRange(schedules);

            // ----- 提案 (Proposals) -----
            context.Proposals.AddRange(
                new Proposal { VendorName = "智聯科技", Budget = 2600000, DurationMonths = 6, SubmitDate = baseDate.AddMonths(-4), Comment = "方案完整，報價合理", ProjectId = pid1 },
                new Proposal { VendorName = "雲端軟體", Budget = 3100000, DurationMonths = 8, SubmitDate = baseDate.AddMonths(-4).AddDays(7), Comment = "含維護一年", ProjectId = pid1 },
                new Proposal { VendorName = "創意數位", Budget = 1400000, DurationMonths = 4, SubmitDate = baseDate.AddMonths(-5), Comment = "採用", ProjectId = pid2 },
                new Proposal { VendorName = "網頁工坊", Budget = 1800000, DurationMonths = 5, SubmitDate = baseDate.AddMonths(-5).AddDays(10), Comment = "備選", ProjectId = pid2 }
            );

            // ----- 成本項目 (CostItems) -----
            context.CostItems.AddRange(
                new CostItem { ItemName = "人力成本", Category = "人力", EstimatedCost = 1200000, ActualCost = 400000, Description = "內部開發與 PM", ProjectId = pid1, ContractNumber = null, PaymentTerms = "月結", TaxStatus = "應稅" },
                new CostItem { ItemName = "伺服器與授權", Category = "設備", EstimatedCost = 800000, ActualCost = 350000, Description = "雲端主機與軟體授權", ProjectId = pid1, PaymentTerms = "年付" },
                new CostItem { ItemName = "委外設計", Category = "其他", EstimatedCost = 450000, ActualCost = 100000, Description = "UI/UX 委外", ProjectId = pid1 },
                new CostItem { ItemName = "設計與前端", Category = "人力", EstimatedCost = 600000, ActualCost = 600000, Description = "已結案", ProjectId = pid2, PaymentTerms = "階段付款" },
                new CostItem { ItemName = "主機與 CDN", Category = "設備", EstimatedCost = 200000, ActualCost = 180000, ProjectId = pid2 },
                new CostItem { ItemName = "UPS 設備", Category = "設備", EstimatedCost = 2200000, ActualCost = 2100000, Description = "含安裝", ProjectId = pid3, ContractNumber = "EQ-2023-001", PaymentTerms = "訂金 30%", PaymentDate = baseDate.AddMonths(-5), TaxStatus = "應稅" },
                new CostItem { ItemName = "空調更新", Category = "設備", EstimatedCost = 1800000, ActualCost = 1750000, ProjectId = pid3, ContractNumber = "EQ-2023-002" }
            );

            // ----- 效益項目 (BenefitItems) -----
            context.BenefitItems.AddRange(
                new BenefitItem { ItemName = "行政作業時間節省", Category = "直接效益", EstimatedValue = 500000, ActualValue = 0, Description = "以人時換算", ProjectId = pid1, BenefitType = "量化", MeasurementMethod = "工時統計", ResponsibleUnit = "行政室" },
                new BenefitItem { ItemName = "流程透明化", Category = "間接效益", EstimatedValue = 200000, ActualValue = 0, Description = "審核流程可追蹤", ProjectId = pid1, BenefitType = "質化" },
                new BenefitItem { ItemName = "官網流量與滿意度", Category = "直接效益", EstimatedValue = 300000, ActualValue = 0, Description = "年度目標", ProjectId = pid2, BenefitType = "量化", MeasurementMethod = "GA 與問卷" },
                new BenefitItem { ItemName = "品牌形象提升", Category = "社會效益", EstimatedValue = 150000, ActualValue = 0, ProjectId = pid2, BenefitType = "質化" },
                new BenefitItem { ItemName = "減少停機損失", Category = "直接效益", EstimatedValue = 800000, ActualValue = 800000, Description = "結案評估", ProjectId = pid3, BenefitType = "量化", MeasurementMethod = "可用性指標", ResponsibleUnit = "資訊室" }
            );

            // ----- 議題 (Issues) -----
            context.Issues.AddRange(
                new Issue { Title = "差勤規則與請假流程確認", Owner = "王小明", Discussion = "與人資確認特休、補休規則後納入規格。", Status = "結束", CreatedAt = baseDate.AddMonths(-2).AddDays(5), ProjectId = pid1 },
                new Issue { Title = "報表權限與個資遮罩", Owner = "王小明", Discussion = "待資安審查通過後實作。", Status = "進行中", CreatedAt = baseDate.AddMonths(-1), ProjectId = pid1 },
                new Issue { Title = "首頁版型 A/B 測試", Owner = "陳美玲", Discussion = "兩版上線後跑兩週再決定。", Status = "等待", CreatedAt = baseDate.AddDays(-10), ProjectId = pid2 },
                new Issue { Title = "機房驗收文件補件", Owner = "李志偉", Discussion = "已補送，結案用。", Status = "結束", CreatedAt = baseDate.AddMonths(-3), ProjectId = pid3 }
            );

            // ----- 會議記錄 (MeetingRecords) -----
            context.MeetingRecords.AddRange(
                new MeetingRecord { MeetingTime = baseDate.AddMonths(-2).AddDays(10), Location = "會議室 A", Participants = "王小明、人資代表、資訊室", Subject = "智慧辦公需求確認", Content = "差勤與請假流程確認，下次提供流程圖。", CreatedAt = baseDate.AddMonths(-2).AddDays(10), ProjectId = pid1 },
                new MeetingRecord { MeetingTime = baseDate.AddMonths(-1).AddDays(15), Location = "線上", Participants = "王小明、開發團隊", Subject = "Sprint 1 檢討", Content = "登入與差勤假單 API 完成，下週進入請假審核。", CreatedAt = baseDate.AddMonths(-1).AddDays(15), ProjectId = pid1 },
                new MeetingRecord { MeetingTime = baseDate.AddMonths(-2).AddDays(20), Location = "會議室 B", Participants = "陳美玲、創意數位", Subject = "官網視覺與動線", Content = "首頁與主要欄位定稿，開始切版。", CreatedAt = baseDate.AddMonths(-2).AddDays(20), ProjectId = pid2 },
                new MeetingRecord { MeetingTime = baseDate.AddMonths(-3), Location = "機房", Participants = "李志偉、廠商、總務", Subject = "機房升級驗收", Content = "電力與空調測試通過，文件補齊後結案。", CreatedAt = baseDate.AddMonths(-3), ProjectId = pid3 }
            );

            // ----- 附件 (ProjectAttachments) - 僅寫入資料，不建立實體檔案 -----
            context.ProjectAttachments.AddRange(
                new ProjectAttachment { OriginalFileName = "智慧辦公需求規格書.pdf", StoredFileName = "spec_prj1.pdf", ContentType = "application/pdf", FilePath = "uploads/attachments/demo/spec_prj1.pdf", Description = "需求規格書", Category = "工作說明", FileSizeBytes = 102400, UploadedAt = baseDate.AddMonths(-2), ProjectId = pid1 },
                new ProjectAttachment { OriginalFileName = "官網設計稿_v2.pdf", StoredFileName = "design_p2_v2.pdf", ContentType = "application/pdf", FilePath = "uploads/attachments/demo/design_p2_v2.pdf", Description = "設計定稿", Category = "圖檔", FileSizeBytes = 2048000, UploadedAt = baseDate.AddMonths(-2), ProjectId = pid2 },
                new ProjectAttachment { OriginalFileName = "機房升級合約.pdf", StoredFileName = "contract_p3.pdf", ContentType = "application/pdf", FilePath = "uploads/attachments/demo/contract_p3.pdf", Description = "工程合約", Category = "合約", FileSizeBytes = 512000, UploadedAt = baseDate.AddMonths(-6), ProjectId = pid3 }
            );

            await context.SaveChangesAsync();
        }
    }
}
