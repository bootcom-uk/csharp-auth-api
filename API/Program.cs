using API.Configuration;
using API.Interfaces;
using API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


var appsettingsFile = null as string;

#if DEBUG
appsettingsFile = "appsettings.Development.json";
#else
    appsettingsFile= "appsettings.json";
#endif

var configurationBuilder = builder.Configuration.AddJsonFile(appsettingsFile);

var apiConfiguration = configurationBuilder.Build().Get<APIConfiguration>();

builder.WebHost.UseSentry(options =>
{
    // A DSN is required.  You can set it here, or in configuration, or in an environment variable.
    options.Dsn = apiConfiguration!.SentryConfigurationSection.Dsn;

    // Enable Sentry performance monitoring
    options.TracesSampleRate = 1.0;

#if DEBUG
    // Log debug information about the Sentry SDK
    options.Debug = true;
#endif
});

// Add services to the container.
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var connectionString = apiConfiguration!.MongoConfigurationSection.Connectionstring;
    return new MongoClient(connectionString);
});
builder.Services.AddSingleton<MongoDatabaseService>();
builder.Services.AddScoped<EmailProviderService>();



var publicKey = RSA.Create();
publicKey.ImportSubjectPublicKeyInfo(Convert.FromBase64String(apiConfiguration!.TokenConfigurationSection.PublicKey!), out _);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = "YourIssuer",
        IssuerSigningKey = new RsaSecurityKey(publicKey), // Use the public key for validation
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero
    };
});

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
