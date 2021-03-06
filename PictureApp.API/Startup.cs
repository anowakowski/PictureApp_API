using System;
using System.Reflection;
using System.Text;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using PictureApp.API.Data;
using PictureApp.API.Data.Repositories;
using PictureApp.API.SeedData;
using PictureApp.API.Helpers;
using PictureApp.API.Models;
using PictureApp.API.Providers;
using PictureApp.API.Services;
using PictureApp.Messaging;

namespace PictureApp.API
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
            services.AddCors();
            services.AddAutoMapper();
            services.AddMediatR(typeof(UserRegisteredNotificationHandler).GetTypeInfo().Assembly);            
            services.AddDbContext<DataContext>(x => x.UseSqlite(Configuration.GetConnectionString("DefaultConnection"), b => b.MigrationsAssembly(typeof(Startup).Namespace)));
            services.AddScoped<DbContext>(sp => sp.GetRequiredService<DataContext>());
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddJsonOptions(opt =>
                {
                    opt.SerializerSettings.ReferenceLoopHandling =
                        Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                });         
            services.AddScoped<IAuthTokenProvider, JwtTokenProvider>();
            services.AddScoped<ITokenProvider, TokenProvider>();
            services.AddScoped<IPasswordProvider, PasswordProvider>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IPhotoService, PhotoService>();
            services.AddScoped<IPhotoServiceScoped, PhotoServiceScoped>();
            services.AddSingleton(provider =>
                new Func<IServiceScope, IPhotoService>((scope) =>
                {
                    var dbContext = scope.ServiceProvider.GetService<DbContext>();
                    return new PhotoService(new Repository<Photo>(dbContext), new UnitOfWork(dbContext),
                        scope.ServiceProvider.GetService<IMapper>());
                }));
            services.AddScoped<INotificationService, EmailNotificationService>();
            services.AddScoped<IEmailClientProvider, MailKitEmailClientProvider>();
            services.AddScoped<INotificationTemplateService, NotificationTemplateService>();
            services.AddScoped<IFollowerService, FollowerService>();
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IRepositoryFactory, RepositoryFactory>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IFilesStorageProvider, AzureBlobStorageProvider>();
            services.AddScoped<IPhotoStorageProvider, CloudinaryPhotoStorageProvider>();
            services.AddScoped<IFileFormatInspectorProvider, FileFormatInspectorProvider>();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII
                            .GetBytes(Configuration.GetSection("AppSettings:Token").Value)),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };
                });      
            services.AddTransient<Seed>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, Seed seed)
        {
            void Seed()
            {
                //seed.SeedUsers();
                //seed.SeedNotificationTemplates();
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(builder =>
                {
                    builder.Run(async context =>
                    {
                        context.Response.StatusCode = (int)System.Net.HttpStatusCode.InternalServerError;

                        var error = context.Features.Get<IExceptionHandlerFeature>();
                        if (error != null)
                        {
                            context.Response.AddApplicationError(error.Error.Message);
                            await context.Response.WriteAsync(error.Error.Message);
                        }
                    });
                });
            }

            Seed();
            app.UseCors(x => x.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
            app.UseAuthentication();
            app.UseMvc();
        }
    }
}