using PuppeteerSharp;

namespace TargetProjectManage.Services
{
    public class PdfService : IPdfService, IDisposable
    {
        private IBrowser? _browser;
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public async Task<byte[]> GeneratePdfAsync(string htmlContent, string fileName = "document")
        {
            await _semaphore.WaitAsync();
            try
            {
                // 確保瀏覽器已初始化
                if (_browser == null)
                {
                    await InitializeBrowserAsync();
                }

                using var page = await _browser!.NewPageAsync();
                
                // 設定頁面內容
                await page.SetContentAsync(htmlContent, new NavigationOptions
                {
                    WaitUntil = new[] { WaitUntilNavigation.Networkidle0 }
                });

                // 生成 PDF
                var pdfBytes = await page.PdfDataAsync(new PdfOptions
                {
                    Format = PuppeteerSharp.Media.PaperFormat.A4,
                    PrintBackground = true,
                    MarginOptions = new PuppeteerSharp.Media.MarginOptions
                    {
                        Top = "10mm",
                        Right = "10mm",
                        Bottom = "10mm",
                        Left = "10mm"
                    },
                    DisplayHeaderFooter = true,
                    HeaderTemplate = "<div style='font-size: 9px; text-align: right; width: 100%;'>第 <span class='pageNumber'></span> 頁 / 共 <span class='totalPages'></span> 頁</div>",
                    FooterTemplate = $"<div style='font-size: 9px; text-align: center; width: 100%;'>匯出時間：{DateTime.Now:yyyy-MM-dd HH:mm:ss}</div>"
                });

                return pdfBytes;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task InitializeBrowserAsync()
        {
            // 下載 Chromium（如果尚未下載）
            await new BrowserFetcher().DownloadAsync();

            // 啟動瀏覽器
            _browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                Args = new[]
                {
                    "--no-sandbox",
                    "--disable-setuid-sandbox",
                    "--disable-dev-shm-usage",
                    "--disable-accelerated-2d-canvas",
                    "--no-first-run",
                    "--no-zygote",
                    "--disable-gpu"
                }
            });
        }

        public void Dispose()
        {
            _browser?.Dispose();
            _semaphore?.Dispose();
        }
    }
}
