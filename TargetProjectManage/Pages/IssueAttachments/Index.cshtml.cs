using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TargetProjectManage.Data;
using TargetProjectManage.Models;

namespace TargetProjectManage.Pages.IssueAttachments
{
    public class IndexModel : PageModel
    {
        private readonly ProjectManageContext _context;

        public IndexModel(ProjectManageContext context)
        {
            _context = context;
        }

        public IList<IssueAttachment> Attachments { get; set; } = new List<IssueAttachment>();
        public Issue? CurrentIssue { get; set; }

        public async Task<IActionResult> OnGetAsync(int? issueId)
        {
            if (!issueId.HasValue)
                return RedirectToPage("/Projects/Index");

            var issue = await _context.Issues
                .Include(i => i.Project)
                .Include(i => i.IssueAttachments)
                .FirstOrDefaultAsync(i => i.Id == issueId.Value);

            if (issue == null)
                return RedirectToPage("/Projects/Index");

            CurrentIssue = issue;
            Attachments = issue.IssueAttachments
                .OrderByDescending(a => a.UploadedAt)
                .ToList();

            return Page();
        }
    }
}
