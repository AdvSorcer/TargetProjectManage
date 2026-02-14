using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TargetProjectManage.Data;

namespace TargetProjectManage.Pages.Attachments
{
    public class DownloadModel : PageModel
    {
        private readonly ProjectManageContext _context;
        private readonly IWebHostEnvironment _env;

        public DownloadModel(ProjectManageContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (!id.HasValue)
                return RedirectToPage("/Projects/Index");

            var att = await _context.ProjectAttachments.FirstOrDefaultAsync(a => a.Id == id.Value);
            if (att == null)
                return NotFound();

            var fullPath = Path.Combine(_env.WebRootPath ?? "", att.FilePath.Replace('/', Path.DirectorySeparatorChar));
            if (!System.IO.File.Exists(fullPath))
                return NotFound();

            var contentType = att.ContentType ?? "application/octet-stream";
            return PhysicalFile(fullPath, contentType, att.OriginalFileName);
        }
    }
}
