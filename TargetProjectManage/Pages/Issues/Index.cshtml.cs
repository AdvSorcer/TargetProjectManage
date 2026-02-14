using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TargetProjectManage.Data;
using TargetProjectManage.Models;

namespace TargetProjectManage.Pages.Issues
{
    public class IndexModel : PageModel
    {
        private readonly ProjectManageContext _context;

        public IndexModel(ProjectManageContext context)
        {
            _context = context;
        }

        public IList<Issue> Issues { get; set; } = new List<Issue>();
        public Project? CurrentProject { get; set; }
        public string? StatusFilter { get; set; }

        public async Task OnGetAsync(int? projectId, string? statusFilter)
        {
            if (projectId.HasValue)
            {
                CurrentProject = await _context.Projects.FindAsync(projectId.Value);
                if (CurrentProject != null)
                {
                    StatusFilter = statusFilter;

                    var query = _context.Issues
                        .Include(i => i.IssueAttachments)
                        .Where(i => i.ProjectId == projectId.Value);

                    if (!string.IsNullOrEmpty(StatusFilter) && StatusFilter != "全部")
                    {
                        query = query.Where(i => i.Status == StatusFilter);
                    }

                    Issues = await query
                        .OrderByDescending(i => i.CreatedAt)
                        .ToListAsync();
                }
            }
            else
            {
                // 如果沒有指定 projectId，重定向到 Projects 頁面
                Response.Redirect("/Projects/Index");
            }
        }

        // Index 僅負責顯示清單，新增/編輯/刪除由其他頁面處理
    }
}

