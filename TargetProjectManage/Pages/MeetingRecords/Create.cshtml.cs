using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TargetProjectManage.Data;
using TargetProjectManage.Models;

namespace TargetProjectManage.Pages.MeetingRecords
{
    public class CreateModel : PageModel
    {
        private readonly ProjectManageContext _context;

        public CreateModel(ProjectManageContext context)
        {
            _context = context;
        }

        [BindProperty]
        public MeetingRecord MeetingRecord { get; set; } = new MeetingRecord();

        public Project? CurrentProject { get; set; }

        public async Task<IActionResult> OnGetAsync(int? projectId)
        {
            if (!projectId.HasValue)
            {
                return RedirectToPage("/Projects/Index");
            }

            CurrentProject = await _context.Projects.FindAsync(projectId.Value);
            if (CurrentProject == null)
            {
                return RedirectToPage("/Projects/Index");
            }

            MeetingRecord.ProjectId = projectId.Value;
            MeetingRecord.MeetingTime = DateTime.Now;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                CurrentProject = await _context.Projects.FindAsync(MeetingRecord.ProjectId);
                return Page();
            }

            MeetingRecord.CreatedAt = DateTime.Now;

            _context.MeetingRecords.Add(MeetingRecord);
            await _context.SaveChangesAsync();

            return RedirectToPage("Index", new { projectId = MeetingRecord.ProjectId });
        }
    }
}
