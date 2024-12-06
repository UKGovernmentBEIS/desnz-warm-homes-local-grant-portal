using System;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using GovUkDesignSystem.ModelBinders;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using HerPublicWebsite.BusinessLogic.Services.S3ReferralFileKeyGenerator;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using WhlgPortalWebsite.BusinessLogic;
using WhlgPortalWebsite.BusinessLogic.ExternalServices.EmailSending;
using WhlgPortalWebsite.BusinessLogic.ExternalServices.S3FileReader;
using WhlgPortalWebsite.BusinessLogic.Services;
using WhlgPortalWebsite.BusinessLogic.Services.CsvFileService;
using WhlgPortalWebsite.Data;
using WhlgPortalWebsite.ErrorHandling;
using WhlgPortalWebsite.Middleware;
using WhlgPortalWebsite.Services;
using GlobalConfiguration = WhlgPortalWebsite.BusinessLogic.GlobalConfiguration;

namespace WhlgPortalWebsite
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
            services.AddScoped<UserService>();
            services.AddScoped<IDataAccessProvider, DataAccessProvider>();
            services.AddScoped<ICsvFileService, CsvFileService>();
            services.AddSingleton<StaticAssetsVersioningService>();
            // This allows encrypted cookies to be understood across multiple web server instances
            services.AddDataProtection().PersistKeysToDbContext<HerDbContext>();

            ConfigureGlobalConfiguration(services);

            ConfigureGovUkNotify(services);
            ConfigureDatabaseContext(services);
            ConfigureS3Client(services);
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
            .AddCookie(options =>
            {
                options.ExpireTimeSpan = TimeSpan.FromMinutes(15);
                options.SlidingExpiration = true;
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Lax;
            })
            .AddOpenIdConnect(options =>
            {
                options.NonceCookie.SameSite = SameSiteMode.Lax;
                options.CorrelationCookie.SameSite = SameSiteMode.Lax;
                
                options.ResponseType = OpenIdConnectResponseType.Code;
                options.MetadataAddress = configuration["Authentication:Cognito:MetadataAddress"];
                options.ClientId = configuration["Authentication:Cognito:ClientId"];
                options.ClientSecret = configuration["Authentication:Cognito:ClientSecret"];
                options.Scope.Add("email");
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.SaveTokens = true; // Save tokens issued to encrypted cookies
                options.UseTokenLifetime = false; // Don't override the cookie lifetime set above
                options.NonceCookie.HttpOnly = true;
                options.NonceCookie.SecurePolicy = CookieSecurePolicy.Always;
                options.CorrelationCookie.HttpOnly = true;
                options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;

                // We see relatively frequent errors where the user doesn't have a valid correlation cookie.
                // This may be for a number of reasons:
                // - The cookie expires after 15 minutes
                // - Landing on the login screen without first hitting the app (therefore missing the cookie)
                // - Some other unknown cause, e.g. the browser handling SameSite cookie settings incorrectly
                //
                // If we detect a correlation error, we redirect to the homepage where the user will be
                // re-authenticated with a fresh correlation cookie. This introduces a small risk of an
                // infinite redirect loop upon misconfiguration, but we expect this to be rare.
                options.Events.OnRemoteFailure = context =>
                {
                    if (context.Failure?.Message.Contains("Correlation failed") is true)
                    {
                        context.Response.Redirect(Constants.BASE_PATH);
                        context.HandleResponse();
                    }

                    return Task.CompletedTask;
                };
            });

            services.AddHsts(options =>
            {
                // Recommendation for MaxAge is at least one year, and a maximum of 2 years
                // If Preload is enabled, IncludeSubdomains should be set to true, and MaxAge should be set to 2 years
                options.MaxAge = TimeSpan.FromDays(365);
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
        
        private void ConfigureS3Client(IServiceCollection services)
        {
            var s3Config = new S3Configuration();
            configuration.GetSection(S3Configuration.ConfigSection).Bind(s3Config);
            
            if (webHostEnvironment.IsDevelopment())
            {
                services.AddScoped(_ =>
                {
                    // For local development connect to a local instance of Minio
                    var clientConfig = new AmazonS3Config
                    {
                        AuthenticationRegion = s3Config.Region,
                        ServiceURL = s3Config.LocalDevOnly_ServiceUrl,
                        ForcePathStyle = true,
                    };
                    return new AmazonS3Client(s3Config.LocalDevOnly_AccessKey, s3Config.LocalDevOnly_SecretKey, clientConfig);
                });
            }
            else
            {
                services.AddScoped(_ => new AmazonS3Client(RegionEndpoint.GetBySystemName(s3Config.Region)));
            }
        }

        private void ConfigureS3FileReader(IServiceCollection services)
        {
            services.Configure<S3Configuration>(
                configuration.GetSection(S3Configuration.ConfigSection));
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
                app.UseHsts();
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
