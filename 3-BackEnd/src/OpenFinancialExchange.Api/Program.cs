using OpenFinancialExchange.Api.Middleware;
using OpenFinancialExchange.Application;
using OpenFinancialExchange.Infrastructure;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Serilog
    builder.Host.UseSerilog((context, services, configuration) =>
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File(
                path: "logs/ofx-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 14));

    // Application layers
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    // MVC
    builder.Services.AddControllers();

    // OpenAPI
    builder.Services.AddOpenApi();
    builder.Services.AddEndpointsApiExplorer();

    // HttpContext access
    builder.Services.AddHttpContextAccessor();

    // CORS — allow any origin for development
    builder.Services.AddCors(options =>
        options.AddDefaultPolicy(policy =>
            policy
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()));

    var app = builder.Build();

    app.UseMiddleware<ExceptionHandlingMiddleware>();

    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    // UseHttpsRedirection disabled — running on HTTP in development
    // app.UseHttpsRedirection();

    app.UseCors();

    app.MapControllers();

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
