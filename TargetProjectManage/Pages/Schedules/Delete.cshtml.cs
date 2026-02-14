using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TargetProjectManage.Models;
using TargetProjectManage.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace TargetProjectManage.Pages.Schedules
{
    public class DeleteModel : PageModel
    {
        private readonly ProjectManageContext _context;
        
        public DeleteModel(ProjectManageContext context)
        {
            _context = context;
        }
        
        [BindProperty]
        public Schedule Schedule { get; set; } = new Schedule();
        
        public Project? CurrentProject { get; set; }
        
        public async Task<IActionResult> OnGetAsync(int id)
        {
            var schedule = await _context.Schedules
                .Include(s => s.Project)
                .FirstOrDefaultAsync(s => s.Id == id);
            
            if (schedule == null)
                return RedirectToPage("/Projects/Index");
            
            Schedule = schedule;
            CurrentProject = schedule.Project;
            
            return Page();
        }
        
        public async Task<IActionResult> OnPostAsync()
        {
            var schedule = await _context.Schedules.FirstOrDefaultAsync(s => s.Id == Schedule.Id);
            if (schedule != null)
            {
                _context.Schedules.Remove(schedule);
                await _context.SaveChangesAsync();
            }
            
            return RedirectToPage("Index", new { projectId = schedule?.ProjectId });
        }
    }
}
