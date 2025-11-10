
using DemoProject.API.Data;
using DemoProject.API.Data.Models;
using DemoProject.API.Middleware;
using DemoProject.API.Services.Implementation;
using DemoProject.API.Services.Interface;
using DemoProject.DataModels.Dto.System;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OpenAI;
using OpenAI.Chat;
using Serilog;
using System.ClientModel;
using System.Security.Claims;
using System.Text;

namespace DemoProject.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.AddServiceDefaults();


            builder.Host.UseSerilog((context, configuration) =>
            {
                configuration.ReadFrom.Configuration(context.Configuration);
            });

            builder.Services.AddScoped<IUserEmailStore<ApplicationUser>>(sp =>
          (IUserEmailStore<ApplicationUser>)sp.GetRequiredService<IUserStore<ApplicationUser>>());

            builder.Services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Description = "Enter  your token in the text input below.\n\nExample: '12345abcdef'",
                });

                c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
             {
                 {
                     new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                     {
                         Reference = new Microsoft.OpenApi.Models.OpenApiReference
                         {
                             Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                             Id = "Bearer"
                         }
                     },
                     new string[] { }
                 }
             });
            });

            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));

            builder.Services.AddOptions<EmailOptions>()
            .Bind(builder.Configuration.GetSection("Email"))
    .ValidateDataAnnotations();

            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IManageService, ManageService>();
            builder.Services.AddScoped<IEmailSender, EmailSenderService>();
            builder.Services.AddScoped<IClaimsService, ClaimsService>();
            builder.Services.AddScoped<IPdfService, PdfService>();


            var githubToken = builder.Configuration["GitHubModels:Token"]
                ?? throw new InvalidOperationException("Missing configuration: GitHubModels:Token");

            var endpoint = new Uri("https://models.inference.ai.azure.com");

            // Register Scoped IChatClient
            builder.Services.AddScoped<IChatClient>(sp =>
            {
                var credential = new ApiKeyCredential(githubToken);
                var clientOptions = new OpenAIClientOptions { Endpoint = endpoint };
                var openAiClient = new OpenAIClient(credential, clientOptions);

                // Use GitHub-hosted GPT model
                var chatClient = openAiClient.GetChatClient("gpt-4o-mini").AsIChatClient();

                // Add useful middleware
                //chatClient.UseFunctionInvocation();
                //chatClient.UseLogging();

                return chatClient;
            });

            // Register Scoped IEmbeddingGenerator
            builder.Services.AddScoped<IEmbeddingGenerator>(sp =>
            {
                var credential = new ApiKeyCredential(githubToken);
                var clientOptions = new OpenAIClientOptions { Endpoint = endpoint };
                var openAiClient = new OpenAIClient(credential, clientOptions);

                var embeddingClient = openAiClient.GetEmbeddingClient("text-embedding-3-small").AsIEmbeddingGenerator();
                return embeddingClient;
            });


            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;


            builder.Services.AddHttpContextAccessor();



            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddScoped<INoteService , NoteService >();
            builder.Services.AddScoped<ITokenService, TokenService>();
            
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.SignIn.RequireConfirmedAccount = true;
                options.SignIn.RequireConfirmedEmail = true;
            }).AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            builder.Services.AddScoped<IUserEmailStore<ApplicationUser>>(sp =>
          (IUserEmailStore<ApplicationUser>)sp.GetRequiredService<IUserStore<ApplicationUser>>());

            var jwtSettings = new JwtSettings();
            builder.Configuration.GetSection(JwtSettings.SectionName).Bind(jwtSettings);

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
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    ClockSkew = TimeSpan.Zero,
                    RoleClaimType = ClaimTypes.Role,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
                };
            });

            builder.Services.AddAuthorization();

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();
            
            builder.Services.AddCors(x => {
                x.AddPolicy("DemoClient",
                    y => {
                        y.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                    });

            });

            var app = builder.Build();

            app.MapDefaultEndpoints();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwagger();
                app.UseSwaggerUI(app =>
                {
                    app.SwaggerEndpoint("/swagger/v1/swagger.json", "Demo API V1");
                });
            }

            app.UseHttpsRedirection();

            app.UseCors("DemoClient");

            app.UseMiddleware<ExceptionHandlingMiddleware>();
            
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.UseCors("DemoClient");

            app.Run();
        }
    }
}
