using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Master40.DB.Data.Context;
using Hangfire;
using Master40.DB.Data.Initializer;
using Master40.Simulation;
using Master40.Tools.SignalR;
using Swashbuckle.AspNetCore.Swagger;

namespace Master40
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add Database Context
            //services.AddDbContext<InMemmoryContext>(options =>
            //    options.UseInMemoryDatabase("InMemmoryContext"));
            
            services.AddDbContext<MasterDBContext>
                (options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddDbContext<OrderDomainContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddDbContext<ResultContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("ResultConnection")));

            services.AddDbContext<ProductionDomainContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            // Hangfire
            services.AddDbContext<HangfireDBContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("Hangfire")));

            services.AddHangfire(options => 
                options.UseSqlServerStorage(Configuration.GetConnectionString("Hangfire")));

            services.AddSingleton<IMessageHub, MessageHub>();
            
            //services.AddSingleton<IProcessMrp, ProcessMrpSim>();
            services.AddSingleton<AgentCore>();
            // services.AddSingleton<Client>();

            // Register the Swagger generator, defining one or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "SSPS 4.0 API", Version = "v1" });
            });


            services.Configure<RequestLocalizationOptions>(
                opts =>
                {
                    var supportedCultures = new List<CultureInfo>
                    {
                        new CultureInfo("en-GB"),
                        new CultureInfo("de-DE"),
                    };

                    opts.DefaultRequestCulture = new RequestCulture("de-DE");
                    // Formatting numbers, dates, etc.
                    opts.SupportedCultures = supportedCultures;
                    // UI strings that we have localized.
                    opts.SupportedUICultures = supportedCultures;
                });
                
            // Add Framework Service
            services.AddMvc();
            services.AddSignalR();
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app
                            , IHostingEnvironment env
                            , ILoggerFactory loggerFactory
                            , HangfireDBContext hangfireContext
                            , MasterDBContext context
                            , ResultContext contextResults
                            , ProductionDomainContext productionDomainContext)
        {

                //MasterDBInitializerLarge.DbInitialize(context);
            MasterDBInitializerLarge.DbInitialize(context);
            ResultDBInitializerBasic.DbInitialize(contextResults);

            HangfireDBInitializer.DbInitialize(hangfireContext);
            var options = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
            app.UseRequestLocalization(options.Value);
            GlobalConfiguration.Configuration.UseFilter(new AutomaticRetryAttribute { Attempts = 0 });

            #region Hangfire 
            GlobalConfiguration.Configuration.UseSqlServerStorage(Configuration.GetConnectionString("Hangfire"));

            app.UseHangfireDashboard();
            app.UseHangfireServer();
            #endregion

            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseFileServer();
            app.UseStaticFiles();
            // app.UseSignalR();
            app.UseSignalR(router =>
            {
                router.MapHub<MessageHub>("/MessageHub");
            }) ;

            var serverOptions = new BackgroundJobServerOptions()
            {
                ServerName = "ProcessingUnit",
            };
            app.UseHangfireServer(serverOptions);
            app.UseHangfireDashboard();

            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "API DOC V1");
            });


            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });


        }
    }
}
