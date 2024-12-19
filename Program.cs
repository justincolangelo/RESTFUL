using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using MongoDB.Driver;
using RESTFUL.Context;
using RESTFUL.Interfaces;
using RESTFUL.Repositories;
using RESTFUL.Settings;
using System.Text.Json;
using System;
using System.Net.Mime;
using Microsoft.AspNetCore.Http;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

IConfiguration configuration = builder.Configuration;

builder.Logging.AddJsonConsole();

BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(BsonType.String));

var mongoDBSettings = configuration.GetSection(nameof(MongoDBSettings)).Get<MongoDBSettings>();

builder.Services.AddSingleton<IMongoClient>(serviceProvider =>
{
    return new MongoClient(mongoDBSettings.ConnectionString);
});


var dbProvider = configuration.GetSection("DBProvider").Get<string>();

// we should only need one instance throughout the app lifetime
// choose which repo to use (mongo, in memory, ...)
if (dbProvider == "mongo")
{
    builder.Services.AddSingleton<IItemsRepository, MongoDBItemsRepository>();

}
else if (dbProvider == "postgres")
{
    var pgsqlSettings = configuration.GetSection(nameof(PGSQLSettings)).Get<PGSQLSettings>();

    builder.Services.AddEntityFrameworkNpgsql()
    .AddDbContext<PGSQLContext>(options =>
        options.UseNpgsql(
            $"Host='{pgsqlSettings.Host}'; Port={pgsqlSettings.Port};Database='{pgsqlSettings.DBName}';Username='{pgsqlSettings.User}';Password='{pgsqlSettings.Password}'"
        )
    );

    // using scoped for services that use db contexts
    builder.Services.AddScoped<IItemsRepository, PGSQLItemsRepository>();
}
else
{
    var mssqlSettings = configuration.GetSection(nameof(MSSQLSettings)).Get<MSSQLSettings>();
    builder.Services.AddDbContext<MSSQLContext>(options =>
        options.UseSqlServer(mssqlSettings.ConnectionString ?? throw new InvalidOperationException("Connection string 'MSSQLContext' not found.")));

    // using scoped for services that use db contexts
    builder.Services.AddScoped<IItemsRepository, MSSQLItemsRepository>();
}

builder.Services.AddControllers(options =>
{
    options.SuppressAsyncSuffixInActionNames = false;
});
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "RESTFUL", Version = "v1" });
});

builder.Services.AddHealthChecks()
    // add healthcheck for the db and others specified here https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
    .AddMongoDb(
        mongoDBSettings.ConnectionString,
        name: "mongodbhealth",
        timeout: TimeSpan.FromSeconds(5),
        tags: new[] { "ready" }
    );


var app = builder.Build();

var env = builder.Environment;

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

app.Run();
