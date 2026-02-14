using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TargetProjectManage.Data;
using TargetProjectManage.Models;

namespace TargetProjectManage.Pages.Analysis
{
    public class EditBenefitItemModel : PageModel
    {
        private readonly ProjectManageContext _context;

        public EditBenefitItemModel(ProjectManageContext context)
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

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(BenefitItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BenefitItemExists(BenefitItem.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./BenefitAnalysis", new { projectId = BenefitItem.ProjectId });
        }

        private bool BenefitItemExists(int id)
        {
            return _context.BenefitItems.Any(e => e.Id == id);
        }
    }
}