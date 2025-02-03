using Microsoft.EntityFrameworkCore;
using StockLogger.BackgroundServices;
using StockLogger.BackgroundServices.BackgroundStratergyServices;
using StockLogger.BackgroundServices.BackgroundStratergyServices.HelperMethods;
using StockLogger.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();

builder.Services.AddDbContext<StockLoggerDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("StockLoggerDbConnection"),
    sqlServerOptions => sqlServerOptions.EnableRetryOnFailure()));


//builder.Services.AddHttpClient<Uptrend_Service>();
//builder.Services.AddHostedService<Uptrend_Service>();

//builder.Services.AddHttpClient<DragonflyDojiService>();
//builder.Services.AddHostedService<DragonflyDojiService>();

//builder.Services.AddHostedService<StockPriceFetcherService2>(); //---------Makes the Candels

////builder.Services.AddHostedService<StockPriceFetcherService>();

//builder.Services.AddHostedService<CandelMakerService>();

//builder.Services.AddHttpClient<ThreeWhiteSoilders>();
//builder.Services.AddHostedService<ThreeWhiteSoilders>();

//builder.Services.AddHttpClient<MorningStarService>();
//builder.Services.AddHostedService<MorningStarService>();

//builder.Services.AddHttpClient<BullishEngulfingService>();
//builder.Services.AddHostedService<BullishEngulfingService>();

//builder.Services.AddHttpClient<BullishHaramiService>();
//builder.Services.AddHostedService<BullishHaramiService>();

//builder.Services.AddHttpClient<HammerService>();
//builder.Services.AddHostedService<HammerService>();

//builder.Services.AddHttpClient<PiercingLineService>();
//builder.Services.AddHostedService<PiercingLineService>();

//builder.Services.AddHttpClient<RisingThreeMethodsService>();
//builder.Services.AddHostedService<RisingThreeMethodsService>();

//builder.Services.AddHttpClient<TweezerBottomService>();
//builder.Services.AddHostedService<TweezerBottomService>();

/////////////////////////////

//builder.Services.AddHttpClient<BreakawayService>();
//builder.Services.AddHostedService<BreakawayService>();

//builder.Services.AddHttpClient<AbandonedBabyService>();
//builder.Services.AddHostedService<AbandonedBabyService>();

//builder.Services.AddHttpClient<BeltHoldService>();
//builder.Services.AddHostedService<BeltHoldService>();

//builder.Services.AddHttpClient<MarubozuService>();
//builder.Services.AddHostedService<MarubozuService>();

//////////////////////////

//builder.Services.AddHttpClient<ReporterService>();
//builder.Services.AddHostedService<ReporterService>();

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
