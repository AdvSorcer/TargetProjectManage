using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TargetProjectManage.Data;
using TargetProjectManage.Models;

namespace TargetProjectManage.Pages.Issues
{
    public class CreateModel : PageModel
    {
        private readonly ProjectManageContext _context;

        public CreateModel(ProjectManageContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Issue Issue { get; set; } = new Issue();

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

            Issue.ProjectId = projectId.Value;
            Issue.Status = Issue.Status ?? "未開始";

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                CurrentProject = await _context.Projects.FindAsync(Issue.ProjectId);
                return Page();
            }

            Issue.CreatedAt = DateTime.Now;

            _context.Issues.Add(Issue);
            await _context.SaveChangesAsync();

            return RedirectToPage("Index", new { projectId = Issue.ProjectId });
        }
    }
}

