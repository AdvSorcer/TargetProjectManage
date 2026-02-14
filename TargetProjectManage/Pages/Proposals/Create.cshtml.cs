using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TargetProjectManage.Models;
using TargetProjectManage.Data;
using Microsoft.EntityFrameworkCore;

namespace TargetProjectManage.Pages.Proposals
{
    public class CreateModel : PageModel
    {
        private readonly ProjectManageContext _context;
        
        public CreateModel(ProjectManageContext context)
        {
            _context = context;
        }
        
        [BindProperty]
        public Proposal Proposal { get; set; } = new Proposal();
        
        public Project? CurrentProject { get; set; }
        
        public async Task<IActionResult> OnGetAsync(int? projectId)
        {
            if (!projectId.HasValue)
            {
                return RedirectToPage("/Projects/Index");
            }
            
            CurrentProject = await _context.Projects.FindAsync(projectId.Value);
            if (CurrentProject == null)
            {
                return RedirectToPage("/Projects/Index");
            }
            
            Proposal.ProjectId = projectId.Value;
            Proposal.SubmitDate = DateTime.Today;
            
            return Page();
        }
        
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                CurrentProject = await _context.Projects.FindAsync(Proposal.ProjectId);
                return Page();
            }
            
            _context.Proposals.Add(Proposal);
            await _context.SaveChangesAsync();
            
            return RedirectToPage("Index", new { projectId = Proposal.ProjectId });
        }
    }
}
