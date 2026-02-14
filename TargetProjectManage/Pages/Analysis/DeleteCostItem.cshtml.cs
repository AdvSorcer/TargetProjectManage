using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TargetProjectManage.Data;
using TargetProjectManage.Models;

namespace TargetProjectManage.Pages.Analysis
{
    public class DeleteCostItemModel : PageModel
    {
        private readonly ProjectManageContext _context;

        public DeleteCostItemModel(ProjectManageContext context)
        {
            _context = context;
        }

        [BindProperty]
        public CostItem CostItem { get; set; } = new CostItem();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var costItem = await _context.CostItems
                .Include(c => c.Project)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (costItem == null)
            {
                return NotFound();
            }

            CostItem = costItem;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var costItem = await _context.CostItems.FindAsync(id);

            if (costItem != null)
            {
                var projectId = costItem.ProjectId;
                _context.CostItems.Remove(costItem);
                await _context.SaveChangesAsync();
                return RedirectToPage("./CostAnalysis", new { projectId = projectId });
            }

            return RedirectToPage("./CostAnalysis");
        }
    }
}