using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TargetProjectManage.Data;
using TargetProjectManage.Models;

namespace TargetProjectManage.Pages.Attachments
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
        public ProjectAttachment Attachment { get; set; } = new ProjectAttachment();

        public Project? CurrentProject { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var att = await _context.ProjectAttachments
                .Include(a => a.Project)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (att == null)
                return RedirectToPage("/Projects/Index");

            Attachment = att;
            CurrentProject = att.Project;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var att = await _context.ProjectAttachments.FirstOrDefaultAsync(a => a.Id == Attachment.Id);
            if (att == null)
                return RedirectToPage("Index", new { projectId = Attachment.ProjectId });

            var projectId = att.ProjectId;

            // 刪除實體檔案
            var fullPath = Path.Combine(_env.WebRootPath ?? "", att.FilePath.Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(fullPath))
            {
                try { System.IO.File.Delete(fullPath); } catch { /* 忽略刪除失敗 */ }
            }

            _context.ProjectAttachments.Remove(att);
            await _context.SaveChangesAsync();

            return RedirectToPage("Index", new { projectId });
        }
    }
}
