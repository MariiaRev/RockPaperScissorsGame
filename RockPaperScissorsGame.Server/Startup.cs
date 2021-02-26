using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RockPaperScissorsGame.Server.Services;
using RockPaperScissorsGame.Server.Options;
using Microsoft.OpenApi.Models;
using RockPaperScissorsGame.Server.Hubs;
using RockPaperScissorsGame.Server.Services.Abstractions;
using RockPaperScissorsGame.Server.Services.Implementations;

namespace RockPaperScissorsGame.Server
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
            services.AddSignalR();
            services.AddControllers();

            services.AddSingleton(typeof(IStorage<>), typeof(Storage<>))
                    .AddSingleton(typeof(JsonDataService<>))
                    .AddSingleton<IStatisticsService, StatisticsService>()
                    .AddSingleton<IUsersService, UsersService>()
                    .AddTransient<IBotGameService, BotGameService>()
                    .Configure<JsonPathsOptions>(Configuration.GetSection("JsonPaths"))
                    .Configure<StatisticsOptions>(Configuration.GetSection("StatisticsSettings"));
            
            services.AddSingleton<IGameStoringService, GameStoringService>();
            services.AddTransient<IGameService, GameService>();
            
            /*services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "RockPaperScissorsGame.Server", Version = "v1" });
            });*/
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var statisticsService = app.ApplicationServices.GetRequiredService<IStatisticsService>();
            var usersService = app.ApplicationServices.GetRequiredService<IUsersService>();

            statisticsService.SetupStorage();
            usersService.SetupStorage();

            //app.UseSwagger();
            //app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Contrllrs.Notes v1"));

            app.UseRouting();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<GameHub>("/GameHub");
            });
        }
    }
}
