using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TargetProjectManage.Models;
using TargetProjectManage.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace TargetProjectManage.Pages.Proposals
{
    public class EditModel : PageModel
    {
        private readonly ProjectManageContext _context;
        
        public EditModel(ProjectManageContext context)
        {
            _context = context;
        }
        
        [BindProperty]
        public Proposal Proposal { get; set; } = new Proposal();
        
        public Project? CurrentProject { get; set; }
        
        public async Task<IActionResult> OnGetAsync(int id)
        {
            var proposal = await _context.Proposals
                .Include(p => p.Project)
                .FirstOrDefaultAsync(p => p.Id == id);
            
            if (proposal == null)
                return RedirectToPage("/Projects/Index");
            
            Proposal = proposal;
            CurrentProject = proposal.Project;
            
            return Page();
        }
        
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                CurrentProject = await _context.Projects.FindAsync(Proposal.ProjectId);
                return Page();
            }
            
            var proposal = await _context.Proposals.FirstOrDefaultAsync(p => p.Id == Proposal.Id);
            if (proposal == null)
                return RedirectToPage("/Projects/Index");
            
            proposal.VendorName = Proposal.VendorName;
            proposal.Budget = Proposal.Budget;
            proposal.DurationMonths = Proposal.DurationMonths;
            proposal.SubmitDate = Proposal.SubmitDate;
            proposal.Comment = Proposal.Comment;
            
            await _context.SaveChangesAsync();
            
            return RedirectToPage("Index", new { projectId = proposal.ProjectId });
        }
    }
}
