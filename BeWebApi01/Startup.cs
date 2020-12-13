using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.WindowsServer.TelemetryChannel;
using CommonLibrary.Logging;

using BeWebApi01.Context;


namespace BeWebApi01
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            // Application Insightsへのログ出力設定
            //
            // NOTE: https://docs.microsoft.com/ja-jp/azure/azure-monitor/app/asp-net-core#configuration-recommendation-for-microsoftapplicationinsightsaspnetcore-sdk-2150--above
            // 
            //services.AddSingleton(typeof(ITelemetryChannel), new ServerTelemetryChannel() { StorageFolder = "/tmp/" });
            //var telemetryConfigutaion = new TelemetryClientConfigure(Configuration.GetValue<string>("ApplicationInsights_InstrumentationKey")).aiOptions;


            //services.AddApplicationInsightsTelemetry(telemetryConfigutaion);


            string appInsightsKey = Configuration.GetValue<string>("ApplicationInsights_InstrumentationKey");
            string logLevel = Configuration.GetValue<string>("Log_Level");

            services = TelemetryClientConfigure.ConfigureServerTelemetryChannel(services, appInsightsKey);
            services = myILoggerProvider.Congfigure(services, appInsightsKey, logLevel);

            services.AddControllers();
            // DBコンテキストの登録。(テーブルがなければ作成する。) 
            services.AddDbContext<DatabaseContext>(options => options.UseSqlServer(
               Configuration.GetValue<string>("dbConnectionString")));

            //services.AddDbContext<DatabaseContext>(options => options.UseInMemoryDatabase(databaseName: "sample"));
                
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            // DBにテーブルがなければ作成する。
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var dbContext = serviceScope.ServiceProvider.GetRequiredService<DatabaseContext>();
                dbContext.Database.EnsureCreated();
                //dbContext.Database.Migrate();

            }
            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
