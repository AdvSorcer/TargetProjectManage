namespace TargetProjectManage.Services
{
    public interface IPdfService
    {
        Task<byte[]> GeneratePdfAsync(string htmlContent, string fileName = "document");
    }
}
