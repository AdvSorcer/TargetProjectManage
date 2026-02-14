using Microsoft.EntityFrameworkCore;
using TargetProjectManage.Models;

namespace TargetProjectManage.Data
{
    public class ProjectManageContext : DbContext
    {
        public ProjectManageContext(DbContextOptions<ProjectManageContext> options)
            : base(options)
        {
        }


        public DbSet<Project> Projects { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<Proposal> Proposals { get; set; }
        public DbSet<CostItem> CostItems { get; set; }
        public DbSet<BenefitItem> BenefitItems { get; set; }
        public DbSet<Issue> Issues { get; set; }
        public DbSet<ProjectAttachment> ProjectAttachments { get; set; }
        public DbSet<IssueAttachment> IssueAttachments { get; set; }
        public DbSet<MeetingRecord> MeetingRecords { get; set; }
    }
}