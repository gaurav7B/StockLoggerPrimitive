//using Microsoft.EntityFrameworkCore;
//using StockLogger.BackgroundServices;
//using StockLogger.Data;

//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.
//builder.Services.AddControllersWithViews()
//    .AddRazorRuntimeCompilation();

//// Register DbContext with the connection string
//builder.Services.AddDbContext<StockLoggerDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("StockLoggerDbConnection")));


//builder.Services.AddHostedService<StockPriceFetcherService>();

//// Add services to the container.
//builder.Services.AddControllersWithViews();

//// Register HttpClient
//builder.Services.AddHttpClient(); // This adds the HttpClient service

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (!app.Environment.IsDevelopment())
//{
//  app.UseExceptionHandler("/Home/Error");
//  app.UseHsts();
//}

//app.UseHttpsRedirection();
//app.UseStaticFiles();

//app.UseRouting();

//app.UseAuthorization();

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Stock}/{action=GetLatestStockPrice}/{id?}");

//app.Run();

using Microsoft.EntityFrameworkCore;
using StockLogger.BackgroundServices;
using StockLogger.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();

// Register DbContext with the connection string
builder.Services.AddDbContext<StockLoggerDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("StockLoggerDbConnection")));

// Register Hosted Service
builder.Services.AddHostedService<StockPriceFetcherService>();

// Register HttpClient
builder.Services.AddHttpClient();

// Add CORS services
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()    // Allow any origin
                      .AllowAnyMethod()    // Allow any HTTP method (GET, POST, etc.)
                      .AllowAnyHeader());  // Allow any headers
});

var app = builder.Build();

// Apply CORS policy globally
app.UseCors("AllowAll");

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
