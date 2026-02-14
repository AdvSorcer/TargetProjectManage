using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TargetProjectManage.Data;
using TargetProjectManage.Models;

namespace TargetProjectManage.Pages.IssueAttachments
{
    public class DeleteModel : PageModel
    {
        private readonly ProjectManageContext _context;
        private readonly IWebHostEnvironment _env;

        public DeleteModel(ProjectManageContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [BindProperty]
        public IssueAttachment Attachment { get; set; } = new IssueAttachment();

        public Issue? CurrentIssue { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var att = await _context.IssueAttachments
                .Include(a => a.Issue)
                .ThenInclude(i => i!.Project)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (att == null)
                return RedirectToPage("/Projects/Index");

            Attachment = att;
            CurrentIssue = att.Issue;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var att = await _context.IssueAttachments.FirstOrDefaultAsync(a => a.Id == Attachment.Id);
            if (att == null)
                return RedirectToPage("Index", new { issueId = Attachment.IssueId });

            var issueId = att.IssueId;

            var fullPath = Path.Combine(_env.WebRootPath ?? "", att.FilePath.Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(fullPath))
            {
                try { System.IO.File.Delete(fullPath); } catch { /* 忽略 */ }
            }

            _context.IssueAttachments.Remove(att);
            await _context.SaveChangesAsync();

            return RedirectToPage("Index", new { issueId });
        }
    }
}
