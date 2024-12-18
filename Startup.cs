using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using RESTFUL.Interfaces;
using RESTFUL.Repositories;
using RESTFUL.Settings;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Text.Json;
using System.Net.Mime;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RESTFUL.Context;

namespace RESTFUL
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
            BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
            BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(BsonType.String));

            var mongoDBSettings = Configuration.GetSection(nameof(MongoDBSettings)).Get<MongoDBSettings>();

            services.AddSingleton<IMongoClient>(serviceProvider =>
            {
                return new MongoClient(mongoDBSettings.ConnectionString);
            });

          
            var dbProvider = Configuration.GetSection("DBProvider").Get<string>();

            // we should only need one instance throughout the app lifetime
            // choose which repo to use (mongo, in memory, ...)
            if (dbProvider == "mongo")
            {
                services.AddSingleton<IItemsRepository, MongoDBItemsRepository>();

            }
            else if (dbProvider == "postgres")
            {
                var pgsqlSettings = Configuration.GetSection(nameof(PGSQLSettings)).Get<PGSQLSettings>();

                services.AddEntityFrameworkNpgsql()
                .AddDbContext<PGSQLContext>(options =>
                    options.UseNpgsql(
                        $"Host='{pgsqlSettings.Host}'; Port={pgsqlSettings.Port};Database='{pgsqlSettings.DBName}';Username='{pgsqlSettings.User}';Password='{pgsqlSettings.Password}'"
                    )
                );

                // using scoped for services that use db contexts
                services.AddScoped<IItemsRepository, PGSQLItemsRepository>();
            }
            else
            {
                var mssqlSettings = Configuration.GetSection(nameof(MSSQLSettings)).Get<MSSQLSettings>();
                services.AddDbContext<MSSQLContext>(options =>
                    options.UseSqlServer(mssqlSettings.ConnectionString ?? throw new InvalidOperationException("Connection string 'MSSQLContext' not found.")));

                // using scoped for services that use db contexts
                services.AddScoped<IItemsRepository, MSSQLItemsRepository>();
            }

            services.AddControllers(options =>
            {
                options.SuppressAsyncSuffixInActionNames = false;
            });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "RESTFUL", Version = "v1" });
            });

            services.AddHealthChecks()
                // add healthcheck for the db and others specified here https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
                .AddMongoDb(
                    mongoDBSettings.ConnectionString,
                    name: "mongodbhealth",
                    timeout: TimeSpan.FromSeconds(5),
                    tags: new[] { "ready" }
                );
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "RESTFUL v1"));
            }

            // we will use docker in production and won't need https internally
            if (env.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                // database can take requests
                endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
                {
                    Predicate = (check) => check.Tags.Contains("ready"),
                    ResponseWriter = async (context, report) =>
                    {
                        var result = JsonSerializer.Serialize(
                            new
                            {
                                status = report.Status.ToString(),
                                checks = report.Entries.Select(x => new
                                {
                                    name = x.Key,
                                    status = x.Value.Status.ToString(),
                                    exception = x.Value.Exception != null ? x.Value.Exception.Message : "none",
                                    duration = x.Value.Duration.ToString()
                                })
                            });

                        context.Response.ContentType = MediaTypeNames.Application.Json;
                        await context.Response.WriteAsync(result);
                    }
                });

                // service is running
                endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
                {
                    Predicate = (_) => false
                });
            });
        }
    }
}
