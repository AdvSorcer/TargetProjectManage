using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using TargetProjectManage.Data;
using TargetProjectManage.Models;
using Microsoft.EntityFrameworkCore;

namespace TargetProjectManage.Pages.Schedules
{
    public class TimelineModel : PageModel
    {
        private readonly ProjectManageContext _context;
        
        public TimelineModel(ProjectManageContext context)
        {
            _context = context;
        }

        public List<TimelineItem> TimelineItems { get; set; } = new();
        public Project? CurrentProject { get; set; }

        public async Task OnGetAsync(int? projectId)
        {
            if (projectId.HasValue)
            {
                CurrentProject = await _context.Projects.FindAsync(projectId.Value);
                if (CurrentProject != null)
                {
                    TimelineItems = await _context.Schedules
                        .Where(s => s.ProjectId == projectId.Value)
                        .OrderBy(s => s.EndDate)
                        .Select(s => new TimelineItem
                        {
                            Date = s.EndDate,
                            Title = s.StageName,
                            Description = $"負責人: {s.Owner ?? "-"}"
                        })
                        .ToListAsync();
                }
            }
            else
            {
                // 如果沒有指定 projectId，重定向到 Projects 頁面
                Response.Redirect("/Projects/Index");
            }
        }
    }

    public class TimelineItem
    {
        public DateTime Date { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
