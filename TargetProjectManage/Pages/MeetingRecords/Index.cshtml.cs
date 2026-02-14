using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TargetProjectManage.Data;
using TargetProjectManage.Models;

namespace TargetProjectManage.Pages.MeetingRecords
{
    public class IndexModel : PageModel
    {
        private readonly ProjectManageContext _context;

        public IndexModel(ProjectManageContext context)
        {
            _context = context;
        }

        public IList<MeetingRecord> MeetingRecords { get; set; } = new List<MeetingRecord>();
        public Project? CurrentProject { get; set; }

        public async Task OnGetAsync(int? projectId)
        {
            if (projectId.HasValue)
            {
                CurrentProject = await _context.Projects.FindAsync(projectId.Value);
                if (CurrentProject != null)
                {
                    MeetingRecords = await _context.MeetingRecords
                        .Where(m => m.ProjectId == projectId.Value)
                        .OrderByDescending(m => m.MeetingTime)
                        .ToListAsync();
                }
            }
            else
            {
                Response.Redirect("/Projects/Index");
            }
        }
    }
}
