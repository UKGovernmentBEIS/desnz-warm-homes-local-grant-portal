using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using HerPortal.Data;
using HerPortal.Data.Services;
using Microsoft.Extensions.Options;
using GlobalConfiguration = HerPortal.BusinessLogic.GlobalConfiguration;

namespace HerPortal
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            
            // Hide that we are using Kestrel for security reasons
            builder.WebHost.ConfigureKestrel(serverOptions => serverOptions.AddServerHeader = false);
            
            var startup = new Startup(builder.Configuration, builder.Environment);
            startup.ConfigureServices(builder.Services);

            var app = builder.Build();

            startup.Configure(app, app.Environment);

            // Migrate the database if it's out of date
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<HerDbContext>();
            dbContext.Database.Migrate();
            
            // Run nightly tasks at 07:00 UTC daily
            // This code to get the config is odd, but it's in the documentation:
            //   https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-7.0#access-options-in-programcs
            var crontab = app.Services.GetRequiredService<IOptionsMonitor<GlobalConfiguration>>()
                .CurrentValue.ReferralReminderCrontab;
            app
                .Services
                .GetService<IRecurringJobManager>()
                .AddOrUpdate<RegularJobsService>(
                    "Send reminder emails",
                    rjs => rjs.SendReminderEmailsAsync(),
                    crontab);

            app.Run();
        }
    }
}
