
using BlooTusk.Business.Implementation;
using BlooTusk.Business.Interface;
using BlooTusk.Entity.BlooTuskModel;
using BlooTusk.Model.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BlooTusk.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddCors();
            //services.AddCors(options =>
            //{
            //    options.AddPolicy("AllowOriginPolicy",
            //        builder =>
            //        {
            //            builder.AllowAnyOrigin()
            //                   .AllowAnyMethod()
            //                   .AllowAnyHeader();
            //        });
            //});


            services.AddDbContext<BlooTuskContext>(options => options
                                                 .UseLazyLoadingProxies()
                                                 .UseMySQL(Configuration["ConnectionStrings:DefaultConnection"]));
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Latest);

           
            services.Configure<ConfigurationModel>(Configuration.GetSection("ConfigurationModel"));
            //services.AddTransient<IEmailSenderService, EmailSenderService>();
            services.Configure<EmailSettings>(Configuration.GetSection("EmailSettings"));
            // services.AddScoped<IAuthService, AuthService>();

            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IAuthenticateService, AuthenticateService>();
            services.AddScoped<IMerchantService, MerchantService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ISignupRequestService, SignupRequestService>();
            services.AddScoped<IAuditlogService, AuditlogService>();
            services.AddScoped<IMerchantUserService , MerchantUserService>();
            services.AddScoped<ISMSTemplateService,SMSTemplateService>();
            services.AddScoped<IRewardPointService, RewardPointService>();
            services.AddScoped<ICouponService, CouponService>();
            services.AddScoped<INoteMasterService, NoteMasterService>();
            services.AddScoped<IAdminDashboardService, AdminDashboardService>();
            services.AddScoped<ICouponredeemtionService, CouponredeemtionService>();          

            // Adding jwtBearer Authentication  


            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
             .AddJwtBearer(options =>
             {
                 options.SaveToken = true;
                 options.RequireHttpsMetadata = false;
                 options.TokenValidationParameters = new TokenValidationParameters()
                 {
                     ValidateIssuer = true,
                     ValidateAudience = true,
                     ValidAudience = Configuration["JWT:ValidAudience"],
                     ValidIssuer = Configuration["JWT:ValidIssuer"],
                     IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:Secret"]))
                 };
             });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "BlooTusk.API", Version = "v1" });

                // To Enable authorization using Swagger (JWT)

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = Configuration["APISecurityDefinition:Description"],
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                          new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            new string[] {}
                    }
                });
              });
            }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {          
            if (env.IsDevelopment() || env.IsProduction())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(builder => builder.AllowAnyOrigin()
                           .AllowAnyMethod().AllowAnyHeader());

            //app.UseCors(builder => builder.AllowAnyOrigin()
            //               .AllowAnyMethod()
            //               .WithHeaders("authorization", "accept", "content-type", "origin"));

            //app.UseCors(builder => builder.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin()
            //   .WithExposedHeaders("content-disposition"));
            //app.UseCors("AllowOriginPolicy");

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
            });

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "BlooTusk.API v1"));
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}


//. user: root / pwd: admin123
//2. user: blootusk / pwd: admin123



//mySQL Instance Name: MySQLblootusk
//IP: 34.66.214.227