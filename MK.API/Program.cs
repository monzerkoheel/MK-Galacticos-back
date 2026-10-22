using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MK.API.Errors;
using MK.API.Middleware;
using MK.Core.Contracts.RepositoriesContracts;
using MK.Core.Contracts.ServicesContracts;
using MK.Core.Localization;
using MK.Core.Models;
using MK.Repository.Data;
using MK.Repository.Data.Interceptors;
using MK.Repository.Repositories;
using MK.Service.BackgroundJobs;
using MK.Service.BackgroundServices;
using MK.Service.HelperServices;
using MK.Service.HelperServices.EmailSevrices;
using MK.Service.HelperServices.Localization;
using MK.Service.Services;
using QuestPDF.Drawing;
using System.Globalization;
using System.Text;
using System.Text.Json.Serialization;

namespace MK.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            // Add services to the container.

            builder.Services.AddLocalization(options =>
            {
                options.ResourcesPath = "Localization/Resources";
            });
            var supportedCultures = new[]
            {
                SupportedLanguages.English,
                SupportedLanguages.Arabic,
                SupportedLanguages.Spanish
            };
            var localizationOptions = new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture(SupportedLanguages.Default),
                SupportedCultures = supportedCultures.Select(c => new CultureInfo(c)).ToList(),
                SupportedUICultures = supportedCultures.Select(c => new CultureInfo(c)).ToList()
            };


            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(
                    new JsonStringEnumConverter());
                options.JsonSerializerOptions.ReferenceHandler =
                    ReferenceHandler.IgnoreCycles;
            });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<MKGalacticosDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.AddDbContext<MKGalacticosDbContext>((serviceProvider, options) =>
            {
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection"));

                options.AddInterceptors(
                    serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>());
            });


            builder.Services.AddMemoryCache();

            builder.Services.AddScoped<ICurrentLanguage, CurrentLanguage>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IUserManagementService, UserManagementService>();
            builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
            builder.Services.AddScoped<IUserInformationService, UserInformationService>();
            builder.Services.AddAutoMapper(typeof(MappingProfile));
            builder.Services.AddScoped<IFileService, FileService>();
            builder.Services.AddScoped<IRequestContext, RequestContext>();
            builder.Services.AddScoped<AuditSaveChangesInterceptor>();
            builder.Services.AddScoped<ICouponService, CouponService>();
            builder.Services.AddScoped<ICollectionService, CollectionService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<ILegendCardService, LegendCardService>();
            builder.Services.AddScoped<IShippingMethodService, ShippingMethodService>();
            builder.Services.AddScoped<IPaymentMethodsService, PaymentMethodsService>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<IInvoiceService, InvoiceService>();

            builder.Services.AddHttpClient();

            builder.Services.AddScoped<IFanRequestService, FanRequestService>();
            builder.Services.AddScoped<IFanRequestNotificationService, FanRequestNotificationService>();
            builder.Services.AddHostedService<FanRequestAvailabilityEmailWorker>();


            /*builder.Services.Configure<PaymobOptions>(builder.Configuration.GetSection("Paymob"));
            builder.Services.AddHttpClient<IPaymobService, PaymobService>();
            builder.Services.AddHttpClient<IPaymobService, PaymobService>((serviceProvider, client) =>
            {
                var options = serviceProvider
                    .GetRequiredService<IOptions<PaymobOptions>>()
                    .Value;

                client.BaseAddress = new Uri(options.BaseUrl);
            });*/




            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var response = new ApiValidationErrorResponse();

                    foreach (var error in context.ModelState.Values
                        .SelectMany(v => v.Errors))
                    {
                        response.Errors.Add(error.ErrorMessage);
                    }

                    return new BadRequestObjectResult(response);
                };
            });

            builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = builder.Configuration["JwtSettings:ValidIssuer"],
                    ValidAudience = builder.Configuration["JwtSettings:ValidAudience"],

                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecurityKey"]!))
                };
            })
            .AddCookie("External")
            .AddGoogle(options =>
            {
                options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;

                options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;

                options.SignInScheme = "External";
            });


            builder.Services.AddHangfire(config =>
            {
                config.UseSimpleAssemblyNameTypeSerializer()
                      .UseRecommendedSerializerSettings()
                      .UseSqlServerStorage(
                          builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            builder.Services.AddHangfireServer();
            builder.Services.AddScoped<AccountDeletionJob>();

            var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            var app = builder.Build();

            using var scope = app.Services.CreateScope();
            var service = scope.ServiceProvider;

            var _dbContext = service.GetRequiredService<MKGalacticosDbContext>();
            var userManager = service.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = service.GetRequiredService<RoleManager<IdentityRole>>();
            var loggerF = service.GetRequiredService<ILoggerFactory>();
            try
            {
                await DbContextSeeding.SeedAsync(_dbContext, userManager, roleManager);
                await _dbContext.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                var logger = loggerF.CreateLogger<Program>();
                logger.LogError(ex, "El Error fe el Migration");
            }
            var fontPath = Path.Combine(builder.Environment.ContentRootPath, "Fonts", "NotoSansArabic-Regular.ttf");

            FontManager.RegisterFontFromFile(fontPath);
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Evaluation;
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseStaticFiles();

            app.UseRequestLocalization(localizationOptions);

            app.UseMiddleware<ExceptionMiddelware>();

            app.UseHttpsRedirection();

            app.UseCors("AllowFrontend");

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseHangfireDashboard();
            var recurringJobManager = app.Services.GetRequiredService<IRecurringJobManager>();

            recurringJobManager.AddOrUpdate<AccountDeletionJob>(
                "account-deletion-job",
                job => job.ExecuteAsync(),
                "0 0 1 * *");


            app.MapControllers();

            app.Run();
        }
    }
}
