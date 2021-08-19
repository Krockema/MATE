using System.Collections.Generic;
using System.Globalization;
using Hangfire;
using Hangfire.Console;
using Mate.DataCore;
using Mate.DataCore.Data.Context;
using Mate.DataCore.Data.Initializer;
using Mate.Models;
using Mate.Production.CLI;
using Mate.Production.CLI.HangfireConfiguration;
using Mate.Production.Core.SignalR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Mate
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
            services.AddDbContext<MateDb>
            (optionsAction: options =>
                options.UseSqlServer(connectionString: Dbms.GetMateDataBase(dbName: DataBaseConfiguration.MateDb).ConnectionString.Value));

            services.AddDbContext<MateResultDb>(optionsAction: options =>
                options.UseSqlServer(connectionString: Dbms.GetMateDataBase(dbName: DataBaseConfiguration.MateResultDb).ConnectionString.Value));

            services.AddDbContext<MateProductionDb>(optionsAction: options =>
                options.UseSqlServer(connectionString: Dbms.GetMateDataBase(dbName: DataBaseConfiguration.MateDb).ConnectionString.Value));
            services.AddLogging(builder => { builder.AddFilter("Microsoft", LogLevel.Error); });


            // Hangfire
            services.AddDbContext<HangfireDBContext>(optionsAction: options =>
                options.UseSqlServer(connectionString: Dbms.GetHangfireDataBase(DataBaseConfiguration.MateHangfireDb).ConnectionString.Value));

            services.AddHangfire(configuration: options =>
                options.UseSqlServerStorage(Dbms.GetHangfireDataBase(DataBaseConfiguration.MateHangfireDb).ConnectionString.Value,
                                            StorageOptions.Default));


            services.AddSingleton<IMessageHub, MessageHub>();

            services.AddSingleton<AgentCore>();

            services.AddTransient<HangfireJob>();



            services.AddCors(options => options.AddDefaultPolicy(builder =>
            {
                // Not recommended for productive use
                builder
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowAnyOrigin()
                    .WithOrigins("https://www2.htw-dresden.de").AllowCredentials();

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
            , MateDb context
            , MateResultDb contextResults
            , MateProductionDb productionDomainContext)
        {
            MasterDbInitializerTable.DbInitialize(context: context);
            ResultDBInitializerBasic.DbInitialize(context: contextResults);

            #region Hangfire

            HangfireDBInitializer.DbInitialize(context: hangfireContext);
            GlobalConfiguration.Configuration
                .UseFilter(filter: new AutomaticRetryAttribute {Attempts = 0})
                .UseSqlServerStorage(nameOrConnectionString: Dbms.GetHangfireDataBase(DataBaseConfiguration.MateHangfireDb).ConnectionString.Value)
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

            app.UseCors();
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