using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TargetProjectManage.Models;
using TargetProjectManage.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace TargetProjectManage.Pages.Schedules
{
    public class EditModel : PageModel
    {
        private readonly ProjectManageContext _context;
        
        public EditModel(ProjectManageContext context)
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
            if (!ModelState.IsValid)
            {
                CurrentProject = await _context.Projects.FindAsync(Schedule.ProjectId);
                return Page();
            }
            
            var schedule = await _context.Schedules.FirstOrDefaultAsync(s => s.Id == Schedule.Id);
            if (schedule == null)
                return RedirectToPage("/Projects/Index");
            
            schedule.StageName = Schedule.StageName;
            schedule.StageType = Schedule.StageType;
            schedule.StartDate = Schedule.StartDate;
            schedule.EndDate = Schedule.EndDate;
            schedule.Owner = Schedule.Owner;
            schedule.Remark = Schedule.Remark;
            
            await _context.SaveChangesAsync();
            
            return RedirectToPage("Index", new { projectId = schedule.ProjectId });
        }
    }
}
