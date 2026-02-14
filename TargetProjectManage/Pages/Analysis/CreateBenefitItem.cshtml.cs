using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TargetProjectManage.Data;
using TargetProjectManage.Models;

namespace TargetProjectManage.Pages.Analysis
{
    public class CreateBenefitItemModel : PageModel
    {
        private readonly ProjectManageContext _context;

        public CreateBenefitItemModel(ProjectManageContext context)
        {
            _context = context;
        }

        [BindProperty]
        public BenefitItem BenefitItem { get; set; } = new BenefitItem();

        public Project? CurrentProject { get; set; }

        public async Task<IActionResult> OnGetAsync(int? projectId)
        {
            if (projectId == null)
            {
                return RedirectToPage("/Projects/Index");
            }

            CurrentProject = await _context.Projects.FindAsync(projectId);
            if (CurrentProject == null)
            {
                return NotFound();
            }

            BenefitItem.ProjectId = projectId.Value;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                CurrentProject = await _context.Projects.FindAsync(BenefitItem.ProjectId);
                return Page();
            }

            _context.BenefitItems.Add(BenefitItem);
            await _context.SaveChangesAsync();

            return RedirectToPage("./BenefitAnalysis", new { projectId = BenefitItem.ProjectId });
        }
    }
}