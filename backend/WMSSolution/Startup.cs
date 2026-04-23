using WMSSolution.Core.Extentions;
namespace WMSSolution;

/// <summary>
/// startup
/// </summary>
/// <param name="configuration">Config</param>
public class Startup(IConfiguration configuration)
{
    public IConfiguration Configuration { get; } = configuration;

    /// <summary>
    ///  register service 
    /// </summary>
    /// <param name="services">services</param>
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddExtensionsService(Configuration);
    }

    /// <summary>
    /// configure
    /// </summary>
    /// <param name="app">app</param>
    /// <param name="env">env</param>
    /// <param name="serviceProvider">serviceProvider</param>
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider service_provider)
    {
        app.UseExtensionsConfigure(env, service_provider, Configuration);
    }
}
