using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using System.Globalization;
using Wms.Theme.Web.Services.Asn;
using Wms.Theme.Web.Services.AsnMaster;
using Wms.Theme.Web.Services.Authen;
using Wms.Theme.Web.Services.Category;
using Wms.Theme.Web.Services.Customer;
using Wms.Theme.Web.Services.Dashboard;
using Wms.Theme.Web.Services.Dispatch;
using Wms.Theme.Web.Services.GoodLocations;
using Wms.Theme.Web.Services.GoodsOwner;
using Wms.Theme.Web.Services.InboundPallet;
using Wms.Theme.Web.Services.OutboundGateway;
using Wms.Theme.Web.Services.Pallet;
using Wms.Theme.Web.Services.Planning;
using Wms.Theme.Web.Services.PurchaseOrder;
using Wms.Theme.Web.Services.RBAC;
using Wms.Theme.Web.Services.Receipt;
using Wms.Theme.Web.Services.Reports;
using Wms.Theme.Web.Services.Sku;
using Wms.Theme.Web.Services.Spu;
using Wms.Theme.Web.Services.Stock;
using Wms.Theme.Web.Services.StockAdjust;
using Wms.Theme.Web.Services.StockMove;
using Wms.Theme.Web.Services.StockProcess;
using Wms.Theme.Web.Services.StockTaking;
using Wms.Theme.Web.Services.Supplier;
using Wms.Theme.Web.Services.Unit;
using Wms.Theme.Web.Services.Warehouse;

var builder = WebApplication.CreateBuilder(args);

// Add HttpClient
// Register AuthHeaderHandler
builder.Services.AddTransient<AuthHeaderHandler>();

// Add HttpClient with AuthHeaderHandler
builder.Services.AddHttpClient("Auth", client =>
{
    client.Timeout = TimeSpan.FromMinutes(5);
})
.AddHttpMessageHandler<AuthHeaderHandler>();
// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Add API Services to DI container
ConfigureServices(builder.Services);

// Authentication and Authorization
builder.Services.AddAuthentication("MyCookieAuth")
    .AddCookie("MyCookieAuth", options =>
    {
        options.Cookie.Name = "WMS_Session";
        options.LoginPath = "/Auth/Index";
        options.ExpireTimeSpan = TimeSpan.FromDays(1);
        options.SlidingExpiration = true;
    });

// Add services
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services
    .AddRazorPages()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();
builder.Services.AddControllers();


// Localization options
var supportedCultures = new[] { new CultureInfo("en-US"), new CultureInfo("vi-VN") };
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("vi-VN");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
// Use localization early in the pipeline
var localizationOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;
app.UseRequestLocalization(localizationOptions);
app.UseStaticFiles();

app.UseStatusCodePagesWithReExecute("/Error/{0}");
app.UseRouting();

app.Use(async (context, next) =>
{
    var path = context.Request.Path;

    var listOfExcludedPaths = new List<string>
    {
        "/Auth/Index",
        "/Auth/Logout",
        "/Auth/Register",
        "/LandingPage",
        "/Orders",
        "/SalesOrders"
    };

    bool isExcludedPath = listOfExcludedPaths.Any(p => path.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase));

    if (!isExcludedPath)
    {
        var token = context.Request.Cookies["access_token"];

        if (string.IsNullOrEmpty(token))
        {
            context.Response.Redirect("/Auth/Index");
            return;
        }
    }

    await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();
app.Run();

// Method to register services in the DI container
static void ConfigureServices(IServiceCollection services)
{
    services.AddScoped<IAuthenService, AuthenService>();
    services.AddScoped<IAsnService, AsnService>();
    services.AddScoped<ISpuService, SpuService>();
    services.AddScoped<IAsnMasterService, AsnMasterService>();
    services.AddScoped<IGoodLocationService, GoodLocationService>();
    services.AddScoped<IStockService, StockService>();
    services.AddScoped<IGoodOwnerService, GoodsOwnerService>();
    services.AddScoped<ISupplierService, SupplierService>();
    services.AddScoped<IDispatchService, DispatchService>();
    services.AddScoped<ICustomerService, CustomerService>();
    services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
    services.AddScoped<IWarehouseService, WarehouseService>();
    services.AddScoped<ICategoryService, CategoryService>();
    services.AddScoped<IStockAdjustServices, StockAdjustService>();
    services.AddScoped<IStockProcessService, StockProcessService>();
    services.AddScoped<IStockMoveService, StockMoveService>();
    services.AddScoped<IStockTakingService, StockTakingService>();
    services.AddScoped<IPalletService, PalletService>();
    services.AddScoped<IReceiptService, ReceiptService>();
    services.AddScoped<ISkuService, SkuService>();
    services.AddScoped<IOutboundReceiptService, OutboundReceiptService>();
    services.AddScoped<IOutboundGatewayService, OutboundGatewayService>();
    services.AddScoped<IDashboardService, DashboardService>();
    services.AddScoped<IUserService, UserService>();
    services.AddScoped<IUnitService, UnitService>();
    services.AddScoped<ISpecificationService, SpecificationService>();
    services.AddScoped<IIntegrationService, IntegrationService>();
    services.AddScoped<IBeginMerchandiseService, BeginMerchandiseService>();
    services.AddScoped<IPlanningService, PlanningService>();
    services.AddScoped<IReportService, ReportService>();
    services.AddScoped<IInboundPalletService, InboundPalletService>();
}


