using Serilog;

namespace WMSSolution
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.Seq("http://localhost:5341")
                .CreateBootstrapLogger();

            try
            {
                Log.Information("--- WMS running ---");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception exception)
            {
                Log.Fatal(exception, "---- exception");
                throw;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseUrls("http://*:5555")
                    .UseStartup<Startup>()
                    .UseKestrel(opt => opt.Limits.MaxRequestBodySize = null);
                }).UseSerilog((context, services, configuration) => configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext().Enrich.WithProperty("Application", "WMSSolution"))
            .UseDefaultServiceProvider(options =>
            {
                options.ValidateScopes = false;
            });
    }
}
