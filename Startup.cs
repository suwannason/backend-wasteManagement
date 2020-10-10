using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;


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
            services.AddCors(); // Make sure you call this previous to AddMvc
            services.AddSwaggerGen();

            services.Configure<CompaniesDatabaseSettings>(
                Configuration.GetSection(nameof(CompaniesDatabaseSettings)));

            services.AddSingleton<ICompanieDatabaseSettings>(sp =>
                sp.GetRequiredService<IOptions<CompaniesDatabaseSettings>>().Value);

            services.AddSingleton<CompanyService>();
            services.AddSingleton<RecycleService>();
            services.AddSingleton<CarService>();
            services.AddSingleton<UserService>();
            services.AddSingleton<InvoiceService>();
            services.AddSingleton<WasteTypeService>();
            services.AddSingleton<DisposalService>();
            services.AddSingleton<SubWasteTypeService>();
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/api-fae-part/swagger/v1/swagger.json", "My API V1");
            });
            app.UseCors(
                options => options.WithOrigins("*").AllowAnyMethod().AllowAnyHeader()
            );

            // app.UseMvc();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
