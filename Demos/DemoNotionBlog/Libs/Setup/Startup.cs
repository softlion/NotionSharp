using DemoNotionBlog.Libs.Services;
using Hangfire;
using Hangfire.MemoryStorage;
using Toolbelt.Blazor.Extensions.DependencyInjection;
using VapoliaFr.Blazorr;

namespace DemoNotionBlog
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddHeadElementHelper();

            services.AddHangfire(options =>
                {
                    //Le format de la connectionString ne lui conviens pas
                    //options.UseSqlServerStorage(connectionString);
                    options.UseStorage(new MemoryStorage());
                });

            services.Configure<NotionOptions>(Configuration.GetSection("Notion"));
            services.AddSingleton<NotionCmsService>();
        }

        public void Configure(IApplicationBuilder app, IHostEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                //app.UseHsts(); //already handled by the reverse proxy
            }

            //app.UseHttpsRedirection(); //already handled by the reverse proxy
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub(); //default to _Blazor
                endpoints.MapFallbackToPage("/_Host");
            });

            //Use Hangfire to manage background tasks
            GlobalConfiguration.Configuration.UseActivator(new HangfireActivator(serviceProvider));
            app.UseHangfireServer();
        }
    }
}
