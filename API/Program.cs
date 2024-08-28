using API.Configuration;
using API.Interfaces;
using API.Services;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);


var appsettingsFile = null as string;

#if DEBUG
appsettingsFile = "appsettings.Development.json";
#else
    appsettingsFile= "appsettings.json";
#endif

var configurationBuilder = builder.Configuration.AddJsonFile(appsettingsFile);

var authConfiguration = configurationBuilder.Build().Get<AuthConfiguration>();

builder.WebHost.UseSentry(options =>
{
    // A DSN is required.  You can set it here, or in configuration, or in an environment variable.
    options.Dsn = authConfiguration!.Sentry.Dsn;

    // Enable Sentry performance monitoring
    options.TracesSampleRate = 1.0;

#if DEBUG
    // Log debug information about the Sentry SDK
    options.Debug = true;
#endif
});

// Add services to the container.

builder.Services.AddSingleton<IDatabaseService, DatabaseService>();
builder.Services.AddSingleton<RefreshTokenService>();
builder.Services.AddSingleton<AuthTokenService>();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
