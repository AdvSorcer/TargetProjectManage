using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TargetProjectManage.Models;
using TargetProjectManage.Data;
using Microsoft.EntityFrameworkCore;

namespace TargetProjectManage.Pages.Projects
{
    public class DeleteModel : PageModel
    {
        private readonly ProjectManageContext _context;

        public DeleteModel(ProjectManageContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Project Project { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects.FirstOrDefaultAsync(m => m.Id == id);

            if (project == null)
            {
                return NotFound();
            }
            else
            {
                Project = project;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects.FindAsync(id);

            if (project != null)
            {
                Project = project;
                
                // 刪除相關的 Schedule 和 Proposal
                var schedules = await _context.Schedules.Where(s => s.ProjectId == id).ToListAsync();
                var proposals = await _context.Proposals.Where(p => p.ProjectId == id).ToListAsync();
                
                _context.Schedules.RemoveRange(schedules);
                _context.Proposals.RemoveRange(proposals);
                _context.Projects.Remove(Project);
                
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
