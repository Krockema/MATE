using System;
using Hangfire;
using Master40.DB.Data.Context;
using Master40.DB.Data.Initializer;
using Master40.Simulation;
using Master40.Tools.SignalR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using Hangfire.Console;
using Hangfire.SqlServer;
using Master40.Models;
using Master40.Simulation.HangfireConfiguration;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;


namespace Master40
{
    public class Startup
    {
        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(basePath: env.ContentRootPath)
                .AddJsonFile(path: "appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile(path: $"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<MasterDBContext>
            (optionsAction: options =>
                options.UseSqlServer(connectionString: Configuration.GetConnectionString(name: "DefaultConnection")));

            services.AddDbContext<OrderDomainContext>(optionsAction: options =>
                options.UseSqlServer(connectionString: Configuration.GetConnectionString(name: "DefaultConnection")));

            services.AddDbContext<ResultContext>(optionsAction: options =>
                options.UseSqlServer(connectionString: Configuration.GetConnectionString(name: "ResultConnection")));

            services.AddDbContext<ProductionDomainContext>(optionsAction: options =>
                options.UseSqlServer(connectionString: Configuration.GetConnectionString(name: "DefaultConnection")));
            services.AddLogging(builder => { builder.AddFilter("Microsoft", LogLevel.Error); });


            // Hangfire
            services.AddDbContext<HangfireDBContext>(optionsAction: options =>
                options.UseSqlServer(connectionString: Configuration.GetConnectionString(name: "Hangfire")));

            services.AddHangfire(configuration: options =>
                options.UseSqlServerStorage(Configuration.GetConnectionString(name: "Hangfire"),
                                            StorageOptions.Default));


            services.AddSingleton<IMessageHub, MessageHub>();

            services.AddSingleton<AgentCore>();

            services.AddTransient<HangfireJob>();

            services.AddCors(options => options.AddPolicy("CorsPolicy", builder =>
            {
                // Not recommended for productive use
                builder
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowAnyOrigin();
            }));

            GlobalJobFilters.Filters.Add(new KeepJobInStore());
            services.Configure<RequestLocalizationOptions>(
                configureOptions: opts =>
                {
                    var supportedCultures = new List<CultureInfo>
                    {
                        new CultureInfo(name: "en-GB"),
                        new CultureInfo(name: "de-DE"),
                    };

                    opts.DefaultRequestCulture = new RequestCulture(culture: "de-DE");
                    // Formatting numbers, dates, etc.
                    opts.SupportedCultures = supportedCultures;
                    // UI strings that we have localized.
                    opts.SupportedUICultures = supportedCultures;
                });

            // Add Framework Service
            services.AddMvc(option => option.EnableEndpointRouting = false)
                .AddNewtonsoftJson()
                .AddRazorRuntimeCompilation();
            services.AddSignalR();
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app
            , IWebHostEnvironment env
            , HangfireDBContext hangfireContext
            , MasterDBContext context
            , ResultContext contextResults
            , ProductionDomainContext productionDomainContext)
        {
            MasterDbInitializerTable.DbInitialize(context: context);
            ResultDBInitializerBasic.DbInitialize(context: contextResults);

            #region Hangfire

            HangfireDBInitializer.DbInitialize(context: hangfireContext);
            GlobalConfiguration.Configuration
                .UseFilter(filter: new AutomaticRetryAttribute {Attempts = 0})
                .UseSqlServerStorage(nameOrConnectionString: Configuration.GetConnectionString(name: "Hangfire"))
                .UseConsole(); 
            app.UseHangfireDashboard();

            #endregion

            var options = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
            app.UseRequestLocalization(options: options.Value);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler(errorHandlingPath: "/Home/Error");
            }

            app.UseCors("CorsPolicy");
            app.UseFileServer();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseEndpoints(router => { router.MapHub<MessageHub>("/MessageHub"); });

            app.UseMvc(configureRoutes: routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}