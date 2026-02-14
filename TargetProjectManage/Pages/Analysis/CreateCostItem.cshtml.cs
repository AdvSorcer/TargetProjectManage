using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TargetProjectManage.Data;
using TargetProjectManage.Models;

namespace TargetProjectManage.Pages.Analysis
{
    public class CreateCostItemModel : PageModel
    {
        private readonly ProjectManageContext _context;

        public CreateCostItemModel(ProjectManageContext context)
        {
            _context = context;
        }

        [BindProperty]
        public CostItem CostItem { get; set; } = new CostItem();

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

            CostItem.ProjectId = projectId.Value;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                CurrentProject = await _context.Projects.FindAsync(CostItem.ProjectId);
                return Page();
            }

            _context.CostItems.Add(CostItem);
            await _context.SaveChangesAsync();

            return RedirectToPage("./CostAnalysis", new { projectId = CostItem.ProjectId });
        }
    }
}