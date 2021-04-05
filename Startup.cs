
using backend.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace backend
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
            services.AddSwaggerGen();

            services.Configure<CompaniesDatabaseSettings>(
                Configuration.GetSection(nameof(CompaniesDatabaseSettings)));

            services.Configure<Endpoint>(
                Configuration.GetSection(nameof(Endpoint))
            );

            services.AddSingleton<ICompanieDatabaseSettings>(sp =>
                sp.GetRequiredService<IOptions<CompaniesDatabaseSettings>>().Value);

            services.AddSingleton<IEndpoint>(sp =>
                sp.GetRequiredService<IOptions<Endpoint>>().Value
            );

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(option =>
            {
                option.RequireHttpsMetadata = false;
                option.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = Configuration["Jwt:Issuer"],
                    ValidAudience = Configuration["Jwt:Issuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]))
                };
                option.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Add("Token-Expired", "true");
                        }
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        JwtSecurityToken accessToken = context.SecurityToken as JwtSecurityToken;
                        if (accessToken != null)
                        {
                            ClaimsIdentity identity = context.Principal.Identity as ClaimsIdentity;
                            if (identity != null)
                            {
                                var handler = new JwtSecurityTokenHandler();
                                if (handler.CanReadToken(accessToken.RawData))
                                {
                                    var jwtToken = handler.ReadJwtToken(accessToken.RawData);
                                    var claimed = jwtToken.Claims.FirstOrDefault(claim => claim.Type == "user");

                                    JObject data = JObject.Parse("{" + claimed.ToString() + "}");
                                    var username = data["user"]["username"];
                                    var permission = data["user"]["permission"];
                                    var name = data["user"]["name"];
                                    var email = data["user"]["email"];
                                    var div = data["user"]["div"];
                                    var dept = data["user"]["dept"];
                                    var tel = data["user"]["tel"];
                                    var band = data["user"]["band"];

                                    identity.AddClaim(new Claim("access_token", accessToken.RawData));
                                    identity.AddClaim(new Claim("username", username.ToString()));
                                    identity.AddClaim(new Claim("name", name.ToString()));
                                    identity.AddClaim(new Claim("email", email.ToString()));
                                    identity.AddClaim(new Claim("div", div.ToString()));
                                    identity.AddClaim(new Claim("dept", dept.ToString()));
                                    identity.AddClaim(new Claim("tel", tel.ToString()));
                                    identity.AddClaim(new Claim("band", band.ToString()));
                                    identity.AddClaim(new Claim("permission", permission.ToString()));
                                    // identity.AddClaim(new Claim("position", position.ToString()));
                                }
                            }
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            services.AddSingleton<CompanyService>();
            services.AddSingleton<RecycleService>();
            services.AddSingleton<CarService>();
            services.AddSingleton<UserService>();
            services.AddSingleton<InvoiceService>();
            services.AddSingleton<QuotationService>();
            services.AddSingleton<WasteNameService>();
            services.AddSingleton<wasteGroupService>();
            services.AddSingleton<HazadousService>();
            services.AddSingleton<InfectionsService>();
            services.AddSingleton<ScrapMatrialImoService>();
            services.AddSingleton<ScrapMatrialPMDService>();
            services.AddSingleton<PricingService>();
            services.AddSingleton<prepareLotService>();
            services.AddSingleton<ITC_IMO_DB_service>();
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseCors(
                options =>
                options.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowAnyOrigin()
                // .AllowCredentials()
          
            );

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                // c.SwaggerEndpoint("/api-fae-part/swagger/v1/swagger.json", "My API V1");
            });
            // app.UseMvc();

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
