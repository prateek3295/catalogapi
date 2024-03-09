using Amazon.S3;
using Catalog.API.Data;
using Catalog.API.Data.Interfaces;
using Catalog.API.Repositories;
using Catalog.API.Repositories.Interfaces;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System.Linq;

namespace Catalog.API
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
            services.AddScoped<ICatalogContext, CatalogContext>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddSingleton<IAmazonS3>(provider =>
            {
                var configuration = provider.GetRequiredService<IConfiguration>();
                var accessKeyId = configuration["AWS:AccessKeyId"];
                var secretAccessKey = configuration["AWS:SecretAccessKey"];
                var region = configuration["AWS:Region"];

                return new AmazonS3Client(accessKeyId, secretAccessKey, Amazon.RegionEndpoint.GetBySystemName(region));
            });

            services.AddSingleton<AWSS3Service>();

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Catalog.API", Version = "v1" });
            });

            services.AddHealthChecks();
                   // .AddMongoDb(Configuration["DatabaseSettings:ConnectionString"], "MongoDb Health", HealthStatus.Degraded);

            services.AddCors(options =>
            {
                options.AddPolicy("AllowOrigin", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Catalog.API v1"));
            }

            app.UseRouting();

            app.UseCors("AllowOrigin");

            //app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health", new HealthCheckOptions
                {
                    ResponseWriter = async (context, report) =>
                    {
                        var result = System.Text.Json.JsonSerializer.Serialize(
                            new
                            {
                                status = report.Status.ToString(),
                                checks = report.Entries.Select(entry => new
                                {
                                    name = entry.Key,
                                    status = entry.Value.Status.ToString(),
                                    description = entry.Value.Description
                                })
                            });

                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync(result);
                    }
                });
            });
        }
    }
}
