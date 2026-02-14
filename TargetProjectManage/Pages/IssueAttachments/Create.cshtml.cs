using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TargetProjectManage.Data;
using TargetProjectManage.Models;

namespace TargetProjectManage.Pages.IssueAttachments
{
    public class CreateModel : PageModel
    {
        private readonly ProjectManageContext _context;
        private readonly IWebHostEnvironment _env;

        public CreateModel(ProjectManageContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [BindProperty]
        public IssueAttachment Attachment { get; set; } = new IssueAttachment();

        [BindProperty]
        [Required(ErrorMessage = "請選擇要上傳的檔案")]
        public IFormFile? UploadedFile { get; set; }

        public Issue? CurrentIssue { get; set; }

        public async Task<IActionResult> OnGetAsync(int? issueId)
        {
            if (!issueId.HasValue)
                return RedirectToPage("/Projects/Index");

            var issue = await _context.Issues
                .Include(i => i.Project)
                .FirstOrDefaultAsync(i => i.Id == issueId.Value);

            if (issue == null)
                return RedirectToPage("/Projects/Index");

            CurrentIssue = issue;
            Attachment.IssueId = issue.Id;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            CurrentIssue = await _context.Issues
                .Include(i => i.Project)
                .FirstOrDefaultAsync(i => i.Id == Attachment.IssueId);

            if (CurrentIssue == null)
                return RedirectToPage("/Projects/Index");

            if (UploadedFile == null || UploadedFile.Length == 0)
            {
                ModelState.AddModelError(nameof(UploadedFile), "請選擇要上傳的檔案");
                return Page();
            }

            const long maxBytes = 50 * 1024 * 1024;
            if (UploadedFile.Length > maxBytes)
            {
                ModelState.AddModelError(nameof(UploadedFile), "檔案大小不可超過 50 MB");
                return Page();
            }

            var extension = Path.GetExtension(UploadedFile.FileName);
            var storedFileName = $"{Guid.NewGuid():N}{extension}";
            var relativePath = Path.Combine("uploads", "attachments", "issue", Attachment.IssueId.ToString(), storedFileName).Replace('\\', '/');
            var fullPath = Path.Combine(_env.WebRootPath ?? "", "uploads", "attachments", "issue", Attachment.IssueId.ToString(), storedFileName);
            var dir = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            await using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await UploadedFile.CopyToAsync(stream);
            }

            Attachment.OriginalFileName = UploadedFile.FileName;
            Attachment.StoredFileName = storedFileName;
            Attachment.ContentType = UploadedFile.ContentType;
            Attachment.FilePath = relativePath;
            Attachment.FileSizeBytes = UploadedFile.Length;
            Attachment.UploadedAt = DateTime.Now;

            _context.IssueAttachments.Add(Attachment);
            await _context.SaveChangesAsync();

            return RedirectToPage("Index", new { issueId = Attachment.IssueId });
        }
    }
}
