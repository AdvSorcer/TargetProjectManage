using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TargetProjectManage.Data;
using TargetProjectManage.Models;

namespace TargetProjectManage.Pages.Dashboard
{
    public class IndexModel : PageModel
    {
        private readonly ProjectManageContext _context;

        public IndexModel(ProjectManageContext context)
        {
            _context = context;
        }

        public Project? CurrentProject { get; set; }
        /// <summary>未結案議題數（狀態不是「結束」）</summary>
        public int UnresolvedIssueCount { get; set; }
        /// <summary>時程進度：狀態為「已完成」的階段數 / 總階段數 * 100，無時程時為 0</summary>
        public double ScheduleProgressPercent { get; set; }
        /// <summary>近期時程：狀態為「已完成」的時程，依結束日由新到舊，最多 5 筆</summary>
        public IList<Schedule> RecentSchedules { get; set; } = new List<Schedule>();
        /// <summary>即將到期的時程（結束日 &gt;= 今天，依結束日由近到遠，最多 5 筆）</summary>
        public IList<Schedule> UpcomingSchedules { get; set; } = new List<Schedule>();
        /// <summary>已逾期：結束日 &lt; 今天且狀態非「已完成」，依結束日由近到遠，最多 5 筆</summary>
        public IList<Schedule> OverdueSchedules { get; set; } = new List<Schedule>();
        /// <summary>下一場會議（MeetingTime &gt;= 現在，取一筆）</summary>
        public MeetingRecord? NextMeeting { get; set; }
        public int AttachmentCount { get; set; }
        public int MeetingRecordCount { get; set; }

        public async Task OnGetAsync(int? projectId)
        {
            if (!projectId.HasValue)
            {
                Response.Redirect("/Projects/Index");
                return;
            }

            CurrentProject = await _context.Projects.FindAsync(projectId.Value);
            if (CurrentProject == null)
            {
                Response.Redirect("/Projects/Index");
                return;
            }

            var today = DateTime.Today;

            // 未結案議題數
            UnresolvedIssueCount = await _context.Issues
                .CountAsync(i => i.ProjectId == projectId.Value && i.Status != "結束");

            // 時程進度：狀態為「已完成」的階段數 / 總階段數
            var allSchedules = await _context.Schedules
                .Where(s => s.ProjectId == projectId.Value)
                .ToListAsync();
            var totalSchedules = allSchedules.Count;
            var completedSchedules = allSchedules.Count(s => s.Status == "已完成");
            ScheduleProgressPercent = totalSchedules > 0
                ? Math.Round((double)completedSchedules / totalSchedules * 100, 1)
                : 0;

            // 近期時程：狀態為「已完成」，依結束日由新到舊，取 5 筆
            RecentSchedules = await _context.Schedules
                .Where(s => s.ProjectId == projectId.Value && s.Status == "已完成")
                .OrderByDescending(s => s.EndDate)
                .Take(5)
                .ToListAsync();

            // 即將到期：結束日 >= 今天，依結束日由近到遠，取 5 筆
            UpcomingSchedules = await _context.Schedules
                .Where(s => s.ProjectId == projectId.Value && s.EndDate >= today)
                .OrderBy(s => s.EndDate)
                .Take(5)
                .ToListAsync();

            // 已逾期：結束日 < 今天且狀態非「已完成」，依結束日由近到遠，取 5 筆
            OverdueSchedules = await _context.Schedules
                .Where(s => s.ProjectId == projectId.Value && s.EndDate < today && s.Status != "已完成")
                .OrderByDescending(s => s.EndDate)
                .Take(5)
                .ToListAsync();

            // 下一場會議（MeetingTime >= 現在）
            var now = DateTime.Now;
            NextMeeting = await _context.MeetingRecords
                .Where(m => m.ProjectId == projectId.Value && m.MeetingTime >= now)
                .OrderBy(m => m.MeetingTime)
                .FirstOrDefaultAsync();

            AttachmentCount = await _context.ProjectAttachments
                .CountAsync(a => a.ProjectId == projectId.Value);

            MeetingRecordCount = await _context.MeetingRecords
                .CountAsync(m => m.ProjectId == projectId.Value);
        }
    }
}
