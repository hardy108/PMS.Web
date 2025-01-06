using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using PMS.EFCore.Model;
using PMS.EFCore.Services;
using AM.EFCore.Services;
using Newtonsoft.Json.Serialization;
using FileStorage.EFCore;

using WF.EFCore.Data;
using WF.EFCore.Services;
using PMS.EFCore.Helper;
using FP.EFCore.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using PMS.EFCore.Services.Utilities;
using PMS.Shared.Models;
using PMS.Shared.Services;
using Microsoft.AspNetCore.Http;
using AM.EFCore.Data;

using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Hangfire;
using Hangfire.MemoryStorage;
using PMS.EFCore.Services.Organization;
using Microsoft.AspNetCore.Http.Features;
using PMS.Shared.Exceptions;

namespace PMS.Web.Services
{
    public class Startup
    {

        public IConfiguration Configuration { get; }
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(env.ContentRootPath);
            builder.AddJsonFile("appsettings.json", false, true);            
            //builder.AddXmlFile("App.config", false, true);
            Configuration = builder.Build();
            JwtTokenRepository.SIGNKEY = Configuration.GetSection("ApplicationSettings")["JwtKey"];
            if (string.IsNullOrWhiteSpace(JwtTokenRepository.SIGNKEY))
                throw new Exception("Invalid Jwt Key");



        }

        

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            string connectionStringEncryptKey = Configuration.GetSection("ApplicationSettings")["CSKey"];
            if (string.IsNullOrWhiteSpace(connectionStringEncryptKey))
                connectionStringEncryptKey = "mps"; // Default Encryption Key

            JwtTokenRepository.SIGNKEY = Configuration.GetSection("ApplicationSettings")["JwtKey"];
            if (string.IsNullOrWhiteSpace(JwtTokenRepository.SIGNKEY))
                throw new ExceptionConfiguration("Jwt Key");


            #region DBContextOptions
            DbContextOptions<PMSContextHO> contextOptionsPMSHO = DBContextOption<PMSContextHO>.GetOptions(Configuration.GetConnectionString("PMSHO"), connectionStringEncryptKey);
            DbContextOptions<PMSContextEstate> contextOptionsPMSEstate = DBContextOption<PMSContextEstate>.GetOptions(Configuration.GetConnectionString("PMS"), connectionStringEncryptKey);
            DbContextOptions<WFContext> contextOptionsWF = DBContextOption<WFContext>.GetOptions(Configuration.GetConnectionString("WF"), connectionStringEncryptKey);
            DbContextOptions<AMContextEstate> contextOptionsAMEstate = DBContextOption<AMContextEstate>.GetOptions(Configuration.GetConnectionString("AM"), connectionStringEncryptKey);
            DbContextOptions<AMContextHO> contextOptionsAMHO = DBContextOption<AMContextHO>.GetOptions(Configuration.GetConnectionString("AMHO"), connectionStringEncryptKey);

            DbContextOptions<FilestorageContext> contextOptionsFileStorage = DBContextOption<FilestorageContext>.GetOptions(Configuration.GetConnectionString("FileStorage"), connectionStringEncryptKey);
            DbContextOptions<FPContext> contextOptionsFP = DBContextOption<FPContext>.GetOptions(Configuration.GetConnectionString("FP"), connectionStringEncryptKey);
            DbContextOptions<AuditContext> contextOptionsAudit = DBContextOption<AuditContext>.GetOptions(Configuration.GetConnectionString("LOG"), connectionStringEncryptKey);
            #endregion

            #region DB Context



            services.AddDbContext<WFContext>(options => options
                .UseSqlServer(DBContextOption<WFContext>.GetConnectionString(Configuration.GetConnectionString("WF"), connectionStringEncryptKey))
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

            services.AddDbContext<AMContextEstate>(options => options
                .UseSqlServer(DBContextOption<AMContextEstate>.GetConnectionString(Configuration.GetConnectionString("AM"), connectionStringEncryptKey))
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

            services.AddDbContext<AMContextHO>(options => options
                .UseSqlServer(DBContextOption<AMContextHO>.GetConnectionString(Configuration.GetConnectionString("AMHO"), connectionStringEncryptKey))
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));





            services.AddDbContext<PMSContextEstate>(options => options
                .UseSqlServer(DBContextOption<PMSContextEstate>.GetConnectionString(Configuration.GetConnectionString("PMS"), connectionStringEncryptKey))
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .EnableSensitiveDataLogging(true)
                
                );

            services.AddDbContext<PMSContextHO>(options => options
                .UseSqlServer(DBContextOption<PMSContextHO>.GetConnectionString(Configuration.GetConnectionString("PMSHO"), connectionStringEncryptKey))
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .EnableSensitiveDataLogging(true)

                );




            services.AddDbContext<FilestorageContext>(options => options
                .UseSqlServer(DBContextOption<FilestorageContext>.GetConnectionString(Configuration.GetConnectionString("FileStorage"), connectionStringEncryptKey))
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

            
            services.AddDbContext<FPContext>(options => options
                .UseSqlServer(DBContextOption<FPContext>.GetConnectionString(Configuration.GetConnectionString("FP"), connectionStringEncryptKey))
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

            services.AddDbContext<AuditContext>(options => options
                .UseSqlServer(DBContextOption<AuditContext>.GetConnectionString(Configuration.GetConnectionString("LOG"), connectionStringEncryptKey))
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

            #endregion

            services.Configure<ConnectionString>(Configuration.GetSection("ConnectionStrings"));
            services.Configure<AppSetting>(Configuration.GetSection("ApplicationSettings"));
            services.Configure<FormOptions>(options =>
            {
                options.ValueCountLimit = int.MaxValue;
            });

            #region JWT Authentication            
            var key = Encoding.ASCII.GetBytes(JwtTokenRepository.SIGNKEY);
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });
            #endregion

            services.AddCors();
            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddJsonOptions(options => options.SerializerSettings.ContractResolver = new DefaultContractResolver())
                .AddJsonOptions(options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

            

            
            services.AddHangfire(config =>
                config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseDefaultTypeSerializer()
                .UseMemoryStorage());

            services.AddHangfireServer();


            // configure DI for application services
            services.AddHostedService<QueuedHostedService>();
            services.AddScoped<IEmailSender, EmailSender>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();            
            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
            services.AddSingleton<IEmailSender, EmailSender>();
            services.AddSingleton<IWebSessionService, WebSessionService>();            
            services.AddSingleton<IDbServerServices, DbServerServices>();
            services.AddScoped<AuthenticationServiceHO, AuthenticationServiceHO>();
            services.AddScoped<AuthenticationServiceEstate, AuthenticationServiceEstate>();            
            services.AddScoped<ScheduledJob, ScheduledJob>();


            #region "Swagger"  	    
            //Swagger API documentation
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "My API", Version = "v1" });
            });
            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, 
            IBackgroundJobClient backgroundJobClient, IRecurringJobManager recurringJobManager, IServiceProvider serviceProvider)
        {

            loggerFactory.AddFile( env.ContentRootPath + "/Logs/log.txt");
            // global cors policy
            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowCredentials()                
                .AllowAnyHeader());

            app.UseAuthentication();

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseMiddleware <WebSessionkMiddleware>();
            //app.UseMiddleware<AuthenticationMiddlware>();
            app.UseFastReport();
            app.UseMvc();


            //Swagger API documentation
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "MyAPI V1");
            });




            PrepareCRONs(app,backgroundJobClient,recurringJobManager,serviceProvider);
        }


        private void PrepareCRONs(IApplicationBuilder app, IBackgroundJobClient backgroundJobClient, IRecurringJobManager recurringJobManager, IServiceProvider serviceProvider)
        {
            app.UseHangfireDashboard();
            backgroundJobClient.Enqueue(() => Console.WriteLine("Scheduled Jobs is running"));


            //Non Active Employee --> Estate Only
            string nonActiveEmployeeCRON = string.Empty;
            try
            {
                nonActiveEmployeeCRON = Configuration.GetSection("ApplicationSettings")["NonActiveEmployeeCRON"];
            }
            catch
            {

            }

            if (!string.IsNullOrWhiteSpace(nonActiveEmployeeCRON))
            {
                recurringJobManager.AddOrUpdate(
                    "NonActiveEmployee",
                    () => serviceProvider.GetService<ScheduledJob>().NonActiveEmployeeBySP3(),
                    nonActiveEmployeeCRON
                    );
            }

            //Clean Trash WF Data --> HO Only
            string cleanTrashWFCRON = string.Empty;
            try
            {
                cleanTrashWFCRON = Configuration.GetSection("ApplicationSettings")["CleanTrashWFCRON"];
            }
            catch
            {

            }

            if (!string.IsNullOrWhiteSpace(cleanTrashWFCRON))
            {
                recurringJobManager.AddOrUpdate(
                    "CleanTrashWFCRON",
                    () => serviceProvider.GetService<ScheduledJob>().CleanWFTrashData(),
                    cleanTrashWFCRON
                    );
            }

            //Ho Only
            string syncWFToEstateCRONS = string.Empty;
            try
            {
                syncWFToEstateCRONS = Configuration.GetSection("ApplicationSettings")["SyncWFToEstateCRONS"];
            }
            catch
            {

            }

            if (!string.IsNullOrWhiteSpace(syncWFToEstateCRONS))
            {
                recurringJobManager.AddOrUpdate(
                    "SyncWFToEstateCRONS",
                    () => serviceProvider.GetService<ScheduledJob>().SubmitToWFByHOCRONS(),
                    syncWFToEstateCRONS
                    );
            }

            //Estate Only
            string submitWFToHOCRONS = string.Empty;
            try
            {
                submitWFToHOCRONS = Configuration.GetSection("ApplicationSettings")["SubmitWFToHOCRONS"];
            }
            catch
            {

            }

            if (!string.IsNullOrWhiteSpace(submitWFToHOCRONS))
            {
                recurringJobManager.AddOrUpdate(
                    "SubmitWFToHOCRONS",
                    () => serviceProvider.GetService<ScheduledJob>().SyncByEstateCRONS(),
                    submitWFToHOCRONS
                    );
            }


            //Estate Only
            string toolsCheckWFAllEstate = string.Empty;
            try
            {
                toolsCheckWFAllEstate = Configuration.GetSection("ApplicationSettings")["ToolsCheckWFAllEstate"];
            }
            catch
            {

            }

            if (!string.IsNullOrWhiteSpace(toolsCheckWFAllEstate))
            {
                recurringJobManager.AddOrUpdate(
                    "ToolsCheckWFAllEstate",
                    () => serviceProvider.GetService<ScheduledJob>().ToolsCheckWFAllEstate(),
                    toolsCheckWFAllEstate
                    );
            }
        }
    }
}
