using System.Reflection;

namespace DemoNotionBlog
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host
                .CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var env = hostingContext.HostingEnvironment;

                    config
                        //This second settings file contains only secrets, and can be put in a kubernetes secret store.
                        .AddJsonFile("appsettings-secrets.json", optional: false, reloadOnChange: true)
                        .AddJsonFile($"appsettings-secrets.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

                    //Persistent storage folder on target, when a query string is used to refresh notion tokens
                    var persistFolder = Path.Combine(env.ContentRootPath, "persist");
                    Directory.CreateDirectory(persistFolder); //If directory is missing, AddJsonFile fails
                    config.AddJsonFile(Path.Combine(persistFolder, "notionKeys.json"), optional: true, reloadOnChange: true);

                    //User secrets should still override appsettings-secrets
                    if (env.IsDevelopment() && !string.IsNullOrEmpty(env.ApplicationName))
                    {
                        var appAssembly = Assembly.Load(new AssemblyName(env.ApplicationName));
                        config.AddUserSecrets(appAssembly, optional: true);
                    }
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .ConfigureKestrel(options =>
                        {
                            options.AddServerHeader = false;
                        })
                        .UseStartup<Startup>();
                });
    }
}
