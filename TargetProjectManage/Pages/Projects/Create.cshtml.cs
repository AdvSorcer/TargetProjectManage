using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TargetProjectManage.Models;
using TargetProjectManage.Data;

namespace TargetProjectManage.Pages.Projects
{
    public class CreateModel : PageModel
    {
        private readonly ProjectManageContext _context;

        public CreateModel(ProjectManageContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public Project Project { get; set; } = new Project();

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Projects.Add(Project);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
