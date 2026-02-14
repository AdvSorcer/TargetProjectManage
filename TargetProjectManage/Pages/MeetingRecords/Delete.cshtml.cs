using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TargetProjectManage.Data;
using TargetProjectManage.Models;

namespace TargetProjectManage.Pages.MeetingRecords
{
    public class DeleteModel : PageModel
    {
        private readonly ProjectManageContext _context;

        public DeleteModel(ProjectManageContext context)
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
            var record = await _context.MeetingRecords.FirstOrDefaultAsync(m => m.Id == MeetingRecord.Id);
            var projectId = record?.ProjectId;
            if (record != null)
            {
                _context.MeetingRecords.Remove(record);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("Index", new { projectId });
        }
    }
}
