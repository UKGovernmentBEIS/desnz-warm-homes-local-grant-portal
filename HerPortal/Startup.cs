using System;
using GovUkDesignSystem.ModelBinders;
using Hangfire;
using Hangfire.PostgreSql;
using HerPortal.BusinessLogic.ExternalServices.S3FileReader;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using HerPortal.Data;
using HerPortal.DataStores;
using HerPortal.ErrorHandling;
using HerPortal.ExternalServices.CsvFiles;
using HerPortal.ExternalServices.EmailSending;
using HerPortal.Middleware;
using HerPortal.Services;
using HerPublicWebsite.BusinessLogic.Services.S3ReferralFileKeyGenerator;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.HttpOverrides;
using GlobalConfiguration = HerPortal.BusinessLogic.GlobalConfiguration;

namespace HerPortal
{
    public class Startup
    {
        private readonly IConfiguration configuration;
        private readonly IWebHostEnvironment webHostEnvironment;
        
        public Startup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            this.configuration = configuration;
            this.webHostEnvironment = webHostEnvironment;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            ConfigureHangfire(services);
            
            services.AddMemoryCache();
            services.AddScoped<CsvFileDownloadDataStore>();
            services.AddScoped<UserDataStore>();
            services.AddScoped<IDataAccessProvider, DataAccessProvider>();
            services.AddScoped<ICsvFileGetter, CsvFileGetter>();
            services.AddSingleton<StaticAssetsVersioningService>();
            // This allows encrypted cookies to be understood across multiple web server instances
            services.AddDataProtection().PersistKeysToDbContext<HerDbContext>();

            ConfigureGlobalConfiguration(services);
            
            ConfigureGovUkNotify(services);
            ConfigureDatabaseContext(services);
            ConfigureS3FileReader(services);

            services.AddControllersWithViews(options =>
            {
                options.Filters.Add<ErrorHandlingFilter>();
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
                options.ModelMetadataDetailsProviders.Add(new GovUkDataBindingErrorTextProvider());
            });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie()
            .AddOpenIdConnect(options =>
            {
                options.ResponseType = "code";
                options.MetadataAddress = configuration["Authentication:Cognito:MetadataAddress"];
                options.ClientId = configuration["Authentication:Cognito:ClientId"];
                options.ClientSecret = configuration["Authentication:Cognito:ClientSecret"];
                options.Scope.Add("email");
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.SaveTokens = true;
            });

            services.AddHttpContextAccessor();
        }

        private void ConfigureGlobalConfiguration(IServiceCollection services)
        {
            services.Configure<GlobalConfiguration>
            (
                configuration.GetSection(GlobalConfiguration.ConfigSection)
            );
        }

        private void ConfigureHangfire(IServiceCollection services)
        {
            // Add Hangfire services.
            services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(configuration.GetConnectionString("PostgreSQLConnection")));

            // Add the Hangfire processing server as IHostedService
            services.AddHangfireServer();
        }

        private void ConfigureDatabaseContext(IServiceCollection services)
        {
            var databaseConnectionString = configuration.GetConnectionString("PostgreSQLConnection");
            services.AddDbContext<HerDbContext>(opt =>
                opt.UseNpgsql(databaseConnectionString));
        }

        private void ConfigureGovUkNotify(IServiceCollection services)
        {
            services.AddScoped<IEmailSender, GovUkNotifyApi>();
            services.Configure<GovUkNotifyConfiguration>(
                configuration.GetSection(GovUkNotifyConfiguration.ConfigSection));
        }
        
        private void ConfigureS3FileReader(IServiceCollection services)
        {
            services.Configure<S3FileReaderConfiguration>(
                configuration.GetSection(S3FileReaderConfiguration.ConfigSection));
            services.AddScoped<IS3FileReader, S3FileReader>();
            services.AddScoped<S3ReferralFileKeyService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UsePathBase(Constants.BASE_PATH);

            if (!webHostEnvironment.IsDevelopment())
            {
                app.UseExceptionHandler(new ExceptionHandlerOptions
                {
                    ExceptionHandlingPath = "/error"
                });
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                // app.UseHsts();
            }

            // Use forwarded headers, so we know which URL to use in our auth redirects
            // AWS ALB will automatically add `X-Forwarded-For` and `X-Forwarded-Proto`
            var forwardedHeaderOptions = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            };

            // TODO: We know all traffic to the container is from AWS, but ideally we
            // would still specify the IP and networks of the ALB here
            forwardedHeaderOptions.KnownNetworks.Clear();
            forwardedHeaderOptions.KnownProxies.Clear();

            app.UseForwardedHeaders(forwardedHeaderOptions);

            // This solves an issue with casting DateTime objects to the database
            //   https://stackoverflow.com/questions/69961449/net6-and-datetime-problem-cannot-write-datetime-with-kind-utc-to-postgresql-ty
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            app.UseStatusCodePagesWithReExecute("/error/{0}");

            if (webHostEnvironment.IsDevelopment())
            {
                // In production we terminate TLS at the load balancer and redirect there
                app.UseHttpsRedirection();
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseMiddleware<SecurityHeadersMiddleware>();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints
                    .MapControllers()
                    .RequireAuthorization();  // makes all endpoints require auth by default
            });
        }
    }
}
