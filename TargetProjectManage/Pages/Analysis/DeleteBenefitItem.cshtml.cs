using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TargetProjectManage.Data;
using TargetProjectManage.Models;

namespace TargetProjectManage.Pages.Analysis
{
    public class DeleteBenefitItemModel : PageModel
    {
        private readonly ProjectManageContext _context;

        public DeleteBenefitItemModel(ProjectManageContext context)
        {
            _context = context;
        }

        [BindProperty]
        public BenefitItem BenefitItem { get; set; } = new BenefitItem();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var benefitItem = await _context.BenefitItems
                .Include(b => b.Project)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (benefitItem == null)
            {
                return NotFound();
            }

            BenefitItem = benefitItem;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var benefitItem = await _context.BenefitItems.FindAsync(id);

            if (benefitItem != null)
            {
                var projectId = benefitItem.ProjectId;
                _context.BenefitItems.Remove(benefitItem);
                await _context.SaveChangesAsync();
                return RedirectToPage("./BenefitAnalysis", new { projectId = projectId });
            }

            return RedirectToPage("./BenefitAnalysis");
        }
    }
}