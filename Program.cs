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

builder.Services.AddDbContext<StockLoggerDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("StockLoggerDbConnection"),
    sqlServerOptions => sqlServerOptions.EnableRetryOnFailure()));

//builder.Services.AddHostedService<CandelAnalyzerService>();
//builder.Services.AddHostedService<CandelMakerService>();

//builder.Services.AddHostedService<StockPriceFetcherService>();

builder.Services.AddHostedService<CandelMakerWithPriceCallEvery30sec>();
//builder.Services.AddHostedService<_2Service>();

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
