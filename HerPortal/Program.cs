using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using HerPortal.Data;
using HerPortal.Data.Services;

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
            app
                .Services
                .GetService<IRecurringJobManager>()
                .AddOrUpdate<RegularJobsService>(
                    "Send reminder emails",
                    rjs => rjs.SendReminderEmailsAsync(),
                    "0 7 * * *");

            app.Run();
        }
    }
}