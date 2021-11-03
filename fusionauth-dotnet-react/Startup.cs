using System;
using System.Threading.Tasks;
using fusionauth_dotnet_react.Helpers;
using fusionauth_dotnet_react.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace fusionauth_dotnet_react
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
            // To be used with Fiddler
            //HttpClient.DefaultProxy = new WebProxy(new Uri("http://localhost:8888"));

            services.AddCors();
            services.AddControllers();
            services.Configure<FusionAuth>(Configuration.GetSection(FusionAuth.ConfigName));
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "fusionauth_dotnet_react", Version = "v1" });
            });

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = Configuration.GetValue<string>("FusionAuth:Authority");
                    options.Audience = Configuration["FusionAuth:ClientId"];
                    options.RequireHttpsMetadata = false;
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            context.Token = context.HttpContext.Session.GetString(SessionKeys.Token);

                            return Task.CompletedTask;
                        },
                    };
                })
                .AddOpenIdConnect("oidc", options =>
                {
                    options.Authority = Configuration["FusionAuth:Authority"];
                    options.ClientId = Configuration["FusionAuth:ClientId"];
                    options.ClientSecret = Configuration["FusionAuth:ClientSecret"];

                    options.UsePkce = true;
                    options.ResponseType = "code";
                    options.RequireHttpsMetadata = false;
                    options.Events = new OpenIdConnectEvents
                    {
                        OnMessageReceived = context =>
                        {
                            context.Token = context.HttpContext.Session.GetString(SessionKeys.Token);

                            return Task.CompletedTask;
                        },
                    };
                });
            services.AddAuthorization();

            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(60 * 24);
            });
            services.AddHttpClient();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "fusionauth_dotnet_react v1"));
            }

            app.UseHttpsRedirection();
            app.UseSession();
            app.UseRouting();

            // global cors policy
            app.UseCors(x => x
                .AllowAnyMethod()
                .AllowAnyHeader()
                .SetIsOriginAllowed(origin => true) // allow any origin
                .AllowCredentials()); // allow credentials

            app.UseSecurityMiddleware();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseProxyToSpaDevelopmentServer(Configuration.GetValue<Uri>("SpaDevelopmentServer"));
                }
            });
        }
    }
}