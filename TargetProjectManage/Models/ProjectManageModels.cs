using System;
using System.ComponentModel.DataAnnotations;

namespace TargetProjectManage.Models
{
    public class Project
    {
        public int Id { get; set; }
        [Required]
        public string ProjectName { get; set; } = string.Empty;
        [Required]
        public string ProjectCode { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Status { get; set; } = "進行中";
        public string? Owner { get; set; }
        public string? Remark { get; set; }
        
        // 成本分析相關欄位
        public decimal? EstimatedCost { get; set; }        // 預估成本
        public decimal? ActualCost { get; set; }           // 實際成本
        public decimal? Budget { get; set; }               // 預算上限
        public decimal? Revenue { get; set; }              // 預期收入
        public decimal? ActualRevenue { get; set; }        // 實際收入
        
        // 效益分析相關欄位
        public decimal? ROI { get; set; }                  // 投資報酬率
        public decimal? ProfitMargin { get; set; }         // 利潤率
        public string? CostAnalysis { get; set; }          // 成本分析備註
        public string? BenefitAnalysis { get; set; }      // 效益分析備註
        public DateTime? CostAnalysisDate { get; set; }   // 成本分析日期
        public DateTime? BenefitAnalysisDate { get; set; } // 效益分析日期
        
        // 導航屬性
        public ICollection<CostItem> CostItems { get; set; } = new List<CostItem>();
        public ICollection<BenefitItem> BenefitItems { get; set; } = new List<BenefitItem>();
        public ICollection<ProjectAttachment> ProjectAttachments { get; set; } = new List<ProjectAttachment>();
        public ICollection<MeetingRecord> MeetingRecords { get; set; } = new List<MeetingRecord>();
    }

    /// <summary>
    /// 專案附件：合約、工作說明、圖檔等檔案紀錄
    /// </summary>
    public class ProjectAttachment
    {
        public int Id { get; set; }

        [Required]
        public string OriginalFileName { get; set; } = string.Empty;  // 使用者上傳的檔名

        /// <summary>儲存用的檔名（含副檔名，避免重複）</summary>
        public string StoredFileName { get; set; } = string.Empty;

        public string? ContentType { get; set; }

        /// <summary>相對 wwwroot 的儲存路徑，例如 uploads/attachments/1/xxx.pdf</summary>
        public string FilePath { get; set; } = string.Empty;

        public string? Description { get; set; }  // 說明（如：合約書、工作說明書）

        public string? Category { get; set; }    // 類別：合約、工作說明、圖檔、其他

        public long FileSizeBytes { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.Now;

        public int ProjectId { get; set; }
        public Project? Project { get; set; }
    }

    public class Schedule
    {
        public int Id { get; set; }
        [Required]
        public string StageName { get; set; } = string.Empty;
        public string? StageType { get; set; }  // 階段類型
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        public string? Owner { get; set; }
        public string? Remark { get; set; }
        /// <summary>階段狀態：未開始、進行中、已完成（用於區分「近期時程」與「已逾期」）</summary>
        public string? Status { get; set; } = "未開始";

        // 外鍵關聯到 Project
        public int? ProjectId { get; set; }
        public Project? Project { get; set; }
    }

    public class Proposal
    {
        public int Id { get; set; }
        [Required]
        public string VendorName { get; set; } = string.Empty;
        [Required]
        public decimal Budget { get; set; }
        [Required]
        public int DurationMonths { get; set; }
        [Required]
        public DateTime SubmitDate { get; set; }
        public string? Comment { get; set; }
        
        // 外鍵關聯到 Project
        public int? ProjectId { get; set; }
        public Project? Project { get; set; }
    }

    public class CostItem
    {
        public int Id { get; set; }
        [Required]
        public string ItemName { get; set; } = string.Empty;  // 成本項目名稱
        [Required]
        public string Category { get; set; } = string.Empty;  // 成本類別（人力、設備、材料、管理費、其他）
        [Required]
        public decimal EstimatedCost { get; set; }             // 預估成本
        public decimal ActualCost { get; set; }                // 實際成本
        public string? Description { get; set; }               // 說明
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        // 政府專案特有欄位
        public string? ContractNumber { get; set; }        // 合約編號
        public string? PaymentTerms { get; set; }         // 付款條件
        public DateTime? PaymentDate { get; set; }        // 付款日期
        public string? TaxStatus { get; set; }            // 稅務狀態
        
        // 外鍵關聯到 Project
        public int ProjectId { get; set; }
        public Project? Project { get; set; }
    }

    public class BenefitItem
    {
        public int Id { get; set; }
        [Required]
        public string ItemName { get; set; } = string.Empty;  // 效益項目名稱
        [Required]
        public string Category { get; set; } = string.Empty;  // 效益類別（直接效益、間接效益、社會效益）
        [Required]
        public decimal EstimatedValue { get; set; }           // 預估價值
        public decimal ActualValue { get; set; }              // 實際價值
        public string? Description { get; set; }              // 說明
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        // 政府專案特有欄位
        public string? BenefitType { get; set; }          // 效益類型（量化/質化）
        public string? MeasurementMethod { get; set; }   // 衡量方法
        public string? ResponsibleUnit { get; set; }      // 負責單位
        
        // 外鍵關聯到 Project
        public int ProjectId { get; set; }
        public Project? Project { get; set; }
    }

    public class Issue
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;  // 議題

        public string? Owner { get; set; }                 // 負責人/Issue Owner

        public string? Discussion { get; set; }            // 討論內容

        [Required]
        public string Status { get; set; } = "未開始";      // 狀態：未開始、進行中、等待、結束

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // 外鍵關聯到 Project
        public int ProjectId { get; set; }
        public Project? Project { get; set; }

        /// <summary>議題附件（一對多）</summary>
        public ICollection<IssueAttachment> IssueAttachments { get; set; } = new List<IssueAttachment>();
    }

    /// <summary>
    /// 議題附件：每個議題可有多個附件
    /// </summary>
    public class IssueAttachment
    {
        public int Id { get; set; }

        [Required]
        public string OriginalFileName { get; set; } = string.Empty;

        public string StoredFileName { get; set; } = string.Empty;

        public string? ContentType { get; set; }

        /// <summary>相對 wwwroot 的儲存路徑，例如 uploads/attachments/issue/1/xxx.pdf</summary>
        public string FilePath { get; set; } = string.Empty;

        public string? Description { get; set; }

        public long FileSizeBytes { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.Now;

        public int IssueId { get; set; }
        public Issue? Issue { get; set; }
    }

    /// <summary>
    /// 會議記錄：開會時間、地點、人員、主題、內容
    /// </summary>
    public class MeetingRecord
    {
        public int Id { get; set; }

        [Required]
        public DateTime MeetingTime { get; set; }   // 開會時間

        public string? Location { get; set; }      // 地點

        public string? Participants { get; set; }  // 與會人員

        [Required]
        public string Subject { get; set; } = string.Empty;  // 主題

        public string? Content { get; set; }        // 會議內容

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int ProjectId { get; set; }
        public Project? Project { get; set; }
    }
}