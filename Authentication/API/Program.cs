using System.Reflection;

var builder = WebApplication.CreateBuilder(args);


var assembly = Assembly.GetExecutingAssembly();
var assemblyName = assembly.GetName().Name;
var stream = null as Stream;

#if DEBUG
    stream = assembly.GetManifestResourceStream($"{assemblyName}.appsettings.Development.json");
#else
    stream = assembly.GetManifestResourceStream($"{assemblyName}.appsettings.json");
#endif

var configurationBuilder = builder.Configuration.AddJsonStream(stream!);

configurationBuilder.Build().Get<>

builder.WebHost.UseSentry(options =>
{
    // A DSN is required.  You can set it here, or in configuration, or in an environment variable.
    options.Dsn = "https://eb18e953812b41c3aeb042e666fd3b5c@o447951.ingest.sentry.io/5428537";

    // Enable Sentry performance monitoring
    options.TracesSampleRate = 1.0;

#if DEBUG
    // Log debug information about the Sentry SDK
    options.Debug = true;
#endif
});

// Add services to the container.

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
