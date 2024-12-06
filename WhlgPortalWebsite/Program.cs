using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WhlgPortalWebsite.BusinessLogic.Services;
using WhlgPortalWebsite.Data;
using GlobalConfiguration = WhlgPortalWebsite.BusinessLogic.GlobalConfiguration;

namespace WhlgPortalWebsite
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

            // Migrate the database if it's out of date. Ideally we wouldn't do this on app startup for our deployed
            // environments, because we're risking multiple containers attempting to run the migrations concurrently and
            // getting into a mess. However, we very rarely add migrations at this point, so in practice it's easier to
            // risk it and keep an eye on the deployment: we should be doing rolling deployments anyway which makes it
            // very unlikely we run into concurrency issues. If that changes though we should look at moving migrations
            // to a deployment pipeline step, and only doing the following locally (PC-1151).
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WhlgDbContext>();
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
