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
            // Add Database Context
            //services.AddDbContext<InMemmoryContext>(options =>
            //    options.UseInMemoryDatabase("InMemmoryContext"));
            
            services.AddDbContext<MasterDBContext>
                (optionsAction: options => options.UseSqlServer(connectionString: Configuration.GetConnectionString(name: "DefaultConnection")));
          
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
                options.UseSqlServerStorage(nameOrConnectionString: Configuration.GetConnectionString(name: "Hangfire")));

            services.AddSingleton<IMessageHub, MessageHub>();
            
            //services.AddSingleton<IProcessMrp, ProcessMrpSim>();
            services.AddSingleton<AgentCore>();
            // services.AddSingleton<Client>();

            // Register the Swagger generator, defining one or more Swagger documents
            services.AddSwaggerGen(setupAction: c =>
            {
                c.SwaggerDoc(name: "v1", info: new Info { Title = "SSPS 4.0 API", Version = "v1" });
            });


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
            //MasterDBInitializerLarge.DbInitialize(context);
            MasterDbInitializerTable.DbInitialize(context: context);

            ResultDBInitializerBasic.DbInitialize(context: contextResults);

            HangfireDBInitializer.DbInitialize(context: hangfireContext);
            var options = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
            app.UseRequestLocalization(options: options.Value);
            GlobalConfiguration.Configuration.UseFilter(filter: new AutomaticRetryAttribute { Attempts = 0 });

            #region Hangfire 
            GlobalConfiguration.Configuration.UseSqlServerStorage(nameOrConnectionString: Configuration.GetConnectionString(name: "Hangfire"));

            app.UseHangfireDashboard();
            app.UseHangfireServer();
            #endregion

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler(errorHandlingPath: "/Home/Error");
            }

            app.UseFileServer();
            app.UseStaticFiles();
            // app.UseSignalR();
            app.UseSignalR(configure: router =>
            {
                router.MapHub<MessageHub>(path: "/MessageHub");
            }) ;

            var serverOptions = new BackgroundJobServerOptions()
            {
                ServerName = "ProcessingUnit",
            };
            app.UseHangfireServer(options: serverOptions);
            app.UseHangfireDashboard();

            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(setupAction: c =>
            {
                c.SwaggerEndpoint(url: "/swagger/v1/swagger.json", name: "API DOC V1");
            });


            app.UseMvc(configureRoutes: routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });


        }
    }
}
