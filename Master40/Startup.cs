using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Master40.DB.Data.Context;
using Master40.DB.Data.Repository;
using Hangfire;
using Master40.BusinessLogicCentral.MRP;
using Master40.DB.Data.Initializer;
using Master40.MessageSystem.SignalR;
using Master40.MessageSystem.MessageReciever;
using Master40.Simulation.Simulation;

namespace Master40
{
    public class Startup
    {
        private IServiceProvider _serviceProvider;

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
            //services.AddDbContext<MasterDBContext>(options =>
            //    options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            //services.AddDbContext<MasterDBContext>(options => options.UseInMemoryDatabase("InMemeoryMaster"));
            _serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            var dboptions = new DbContextOptionsBuilder<DbContext>();
            dboptions.UseInMemoryDatabase("one");
                //.UseInternalServiceProvider(_serviceProvider);

            services.AddDbContext<MasterDBContext>(op => op.UseInMemoryDatabase("one"));

            //services.AddDbContext<OrderDomainContext>(options =>
            //    options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddDbContext<OrderDomainContext>(op => op.UseInMemoryDatabase("one"));

            //services.AddDbContext<ProductionDomainContext>(options =>
            //    options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            services.AddDbContext<ProductionDomainContext>(op => op.UseInMemoryDatabase("one"));
            //
            //services
            //    .AddEntityFrameworkInMemoryDatabase()
            //    .AddDbContext<ProductionDomainContext>((p, b) => b
            //        .UseInMemoryDatabase("one")
            //        .UseInternalServiceProvider(p));

            services.AddDbContext<CopyContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddDbContext<HangfireDBContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("Hangfire")));

            services.AddHangfire(options => 
                options.UseSqlServerStorage(Configuration.GetConnectionString("Hangfire")));

            services.AddSingleton<IMessageHub, MessageHub>();
            services.AddSingleton<IScheduling, Scheduling>();
            services.AddSingleton<ICapacityScheduling, CapacityScheduling>();
            services.AddSingleton<IProcessMrp, ProcessMrp>();
            services.AddSingleton<ISimulator, Simulator>();
            services.AddSingleton<IProcessMrp, ProcessMrpSim>();
            services.AddSingleton<Client>();



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
                            , ProductionDomainContext productionDomainContext)
        {
            Task.Run((() => { 
                MasterDBInitializerLarge.DbInitialize(context);
                MasterDBInitializerLarge.DbInitialize(productionDomainContext);
                }
            ));
            HangfireDBInitializer.DbInitialize(hangfireContext);
            var options = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
            app.UseRequestLocalization(options.Value);


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

            app.UseStaticFiles();
            app.UseSignalR();

            var serverOptions = new BackgroundJobServerOptions()
            {
                ServerName = "ProcessingUnit",
            };
            app.UseHangfireServer(serverOptions);
            app.UseHangfireDashboard();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });


        }
    }
}
