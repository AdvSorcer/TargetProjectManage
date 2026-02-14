using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TargetProjectManage.Data;
using TargetProjectManage.Models;

namespace TargetProjectManage.Pages.MeetingRecords
{
    public class EditModel : PageModel
    {
        private readonly ProjectManageContext _context;

        public EditModel(ProjectManageContext context)
        {
            _context = context;
        }

        [BindProperty]
        public MeetingRecord MeetingRecord { get; set; } = new MeetingRecord();

        public Project? CurrentProject { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var record = await _context.MeetingRecords
                .Include(m => m.Project)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (record == null)
                return RedirectToPage("/Projects/Index");

            MeetingRecord = record;
            CurrentProject = record.Project;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                CurrentProject = await _context.Projects.FindAsync(MeetingRecord.ProjectId);
                return Page();
            }

            var record = await _context.MeetingRecords.FirstOrDefaultAsync(m => m.Id == MeetingRecord.Id);
            if (record == null)
                return RedirectToPage("/Projects/Index");

            record.MeetingTime = MeetingRecord.MeetingTime;
            record.Location = MeetingRecord.Location;
            record.Participants = MeetingRecord.Participants;
            record.Subject = MeetingRecord.Subject;
            record.Content = MeetingRecord.Content;

            await _context.SaveChangesAsync();

            return RedirectToPage("Index", new { projectId = record.ProjectId });
        }
    }
}
