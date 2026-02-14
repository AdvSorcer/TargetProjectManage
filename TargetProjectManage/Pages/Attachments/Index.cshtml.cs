using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TargetProjectManage.Data;
using TargetProjectManage.Models;

namespace TargetProjectManage.Pages.Attachments
{
    public class IndexModel : PageModel
    {
        private readonly ProjectManageContext _context;

        public IndexModel(ProjectManageContext context)
        {
            _context = context;
        }

        public IList<ProjectAttachment> Attachments { get; set; } = new List<ProjectAttachment>();
        public Project? CurrentProject { get; set; }
        public string? CategoryFilter { get; set; }

        public async Task<IActionResult> OnGetAsync(int? projectId, string? categoryFilter)
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

            CategoryFilter = categoryFilter;
            var query = _context.ProjectAttachments
                .Where(a => a.ProjectId == projectId.Value);

            if (!string.IsNullOrEmpty(CategoryFilter) && CategoryFilter != "全部")
            {
                query = query.Where(a => a.Category == CategoryFilter);
            }

            Attachments = await query
                .OrderByDescending(a => a.UploadedAt)
                .ToListAsync();

            return Page();
        }
    }
}
