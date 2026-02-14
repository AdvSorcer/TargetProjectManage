using Microsoft.AspNetCore.Mvc.RazorPages;
using TargetProjectManage.Models;
using TargetProjectManage.Data;
using System.Collections.Generic;
using System.Linq;

namespace TargetProjectManage.Pages.Projects
{
    public class IndexModel : PageModel
    {
        private readonly ProjectManageContext _context;
        
        public IndexModel(ProjectManageContext context)
        {
            _context = context;
        }
        
        public IList<Project> Projects { get; set; } = new List<Project>();
        
        public void OnGet()
        {
            Projects = _context.Projects
                .OrderByDescending(p => p.CreatedDate)
                .ToList();
        }
    }
}
