using TargetProjectManage.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();

// 從環境變數或配置檔案取得連線字串
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Data Source=Data/ProjectManage.db";

builder.Services.AddDbContext<ProjectManageContext>(options =>
    options.UseSqlite(connectionString, sqliteOptions =>
    {
        sqliteOptions.CommandTimeout(30);
    }));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
}
// 開發環境使用 http profile 時未設定 HTTPS 埠，若啟用 UseHttpsRedirection 會出現
// "Failed to determine the https port for redirect" 警告，故僅在正式環境啟用

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();
app.MapControllers();

// 確保資料庫已建立並啟用外鍵約束
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ProjectManageContext>();
    
    // 啟用 SQLite 外鍵約束
    context.Database.ExecuteSqlRaw("PRAGMA foreign_keys = ON;");
    
    // 套用所有遷移；若資料庫不存在會自動建立並包含所有欄位
    context.Database.Migrate();

    // Demo 用：若尚無專案則填入假資料（僅執行一次）
    await SeedData.SeedAsync(context);
}

app.Run();