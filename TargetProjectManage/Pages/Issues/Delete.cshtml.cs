using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TargetProjectManage.Data;
using TargetProjectManage.Models;

namespace TargetProjectManage.Pages.Issues
{
    public class DeleteModel : PageModel
    {
        private readonly ProjectManageContext _context;

        public DeleteModel(ProjectManageContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Issue Issue { get; set; } = new Issue();

        public Project? CurrentProject { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var issue = await _context.Issues
                .Include(i => i.Project)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (issue == null)
                return RedirectToPage("/Projects/Index");

            Issue = issue;
            CurrentProject = issue.Project;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var issue = await _context.Issues.FirstOrDefaultAsync(i => i.Id == Issue.Id);
            if (issue != null)
            {
                _context.Issues.Remove(issue);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("Index", new { projectId = issue?.ProjectId });
        }
    }
}

