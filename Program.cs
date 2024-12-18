using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using StockLogger.BackgroundServices;
using StockLogger.BackgroundServices.BackGroundServiceForEach;
using StockLogger.BackgroundServices.BackgroundStratergyServices;
using StockLogger.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();

// Register DbContext with the connection string
//builder.Services.AddDbContext<StockLoggerDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("StockLoggerDbConnection")));

builder.Services.AddDbContext<StockLoggerDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("StockLoggerDbConnection"),
    sqlServerOptions => sqlServerOptions.EnableRetryOnFailure()));  // Connection Pooling and Retry Logic

//builder.Services.AddHostedService<CandelAnalyzerService>();
//builder.Services.AddHostedService<CandelMakerService>();

builder.Services.AddHostedService<StockPriceFetcherPerSecService>();

//builder.Services.AddHostedService<StockPriceFetcherService>();

//builder.Services.AddHostedService<CandelMakerWithPriceCallEvery30sec>();
builder.Services.AddHostedService<_1Service>();
//builder.Services.AddHostedService<_2Service>();
//builder.Services.AddHostedService<_3Service>();
//builder.Services.AddHostedService<_4Service>();

//builder.Services.AddHostedService<_5Service>();
//builder.Services.AddHostedService<_6Service>();
//builder.Services.AddHostedService<_7Service>();
//builder.Services.AddHostedService<_8Service>();

//builder.Services.AddHostedService<_9Service>();
//builder.Services.AddHostedService<_10Service>();
//builder.Services.AddHostedService<_11Service>();
//builder.Services.AddHostedService<_12Service>();

builder.Services.AddHostedService<_3WhiteSoildersService>();
//builder.Services.AddHostedService<CupAndHandelService>();

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register HttpClient
builder.Services.AddHttpClient(); // This adds the HttpClient service

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Stock}/{action=GetLatestStockPrice}/{id?}");

app.Run();
