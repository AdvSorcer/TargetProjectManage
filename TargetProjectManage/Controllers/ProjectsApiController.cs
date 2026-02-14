using Microsoft.AspNetCore.Mvc;
using TargetProjectManage.Data;
using TargetProjectManage.Models;

namespace TargetProjectManage.Controllers
{
    [ApiController]
    [Route("api/projects")]
    public class ProjectsApiController : ControllerBase
    {
        private readonly ProjectManageContext _context;

        public ProjectsApiController(ProjectManageContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetProjects()
        {
            try
            {
                // 檢查資料庫連線
                if (_context.Projects == null)
                {
                    return StatusCode(500, new { error = "資料庫連線問題" });
                }

                var projects = _context.Projects
                    .OrderByDescending(p => p.CreatedDate)
                    .Select(p => new
                    {
                        id = p.Id,
                        projectName = p.ProjectName,
                        projectCode = p.ProjectCode,
                        status = p.Status,
                        owner = p.Owner
                    })
                    .ToList();

                // 如果沒有專案，返回空陣列而不是錯誤
                return Ok(projects);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "無法載入專案列表", details = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetProject(int id)
        {
            try
            {
                var project = _context.Projects
                    .Where(p => p.Id == id)
                    .Select(p => new
                    {
                        id = p.Id,
                        projectName = p.ProjectName,
                        projectCode = p.ProjectCode,
                        description = p.Description,
                        status = p.Status,
                        owner = p.Owner,
                        startDate = p.StartDate,
                        endDate = p.EndDate,
                        remark = p.Remark
                    })
                    .FirstOrDefault();

                if (project == null)
                {
                    return NotFound(new { error = "找不到指定的專案" });
                }

                return Ok(project);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "無法載入專案資訊", details = ex.Message });
            }
        }
    }
}
