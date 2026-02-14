using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TargetProjectManage.Models;
using TargetProjectManage.Data;
using Microsoft.EntityFrameworkCore;

namespace TargetProjectManage.Pages.Schedules
{
    public class CreateModel : PageModel
    {
        private readonly ProjectManageContext _context;
        
        public CreateModel(ProjectManageContext context)
        {
            _context = context;
        }
        
        [BindProperty]
        public Schedule Schedule { get; set; } = new Schedule();
        
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
            
            Schedule.ProjectId = projectId.Value;
            Schedule.StartDate = DateTime.Today;
            Schedule.EndDate = DateTime.Today;
            
            return Page();
        }
        
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                CurrentProject = await _context.Projects.FindAsync(Schedule.ProjectId);
                return Page();
            }
            
            _context.Schedules.Add(Schedule);
            await _context.SaveChangesAsync();
            
            return RedirectToPage("Index", new { projectId = Schedule.ProjectId });
        }
    }
}
