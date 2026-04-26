using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using mmrdaconsent.API.Data;
using mmrdaconsent.API.Interfaces;
using mmrdaconsent.API.Middleware;
using mmrdaconsent.API.Services;


var builder = WebApplication.CreateBuilder(args);

// ── Database ──────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.CommandTimeout(60)
    ));

// ── Settings ──────────────────────────────────────────────────────────────
builder.Services.Configure<TataSmsSettings>(
    builder.Configuration.GetSection("TataSms"));

// ── Memory Cache (for OTP) ────────────────────────────────────────────────
builder.Services.AddMemoryCache();

// ── HttpClient (for SMS gateway) ──────────────────────────────────────────
builder.Services.AddHttpClient<IOtpService, OtpService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(15);
});

// ── Services ──────────────────────────────────────────────────────────────
builder.Services.AddScoped<IApplicationService, ApplicationService>();
builder.Services.AddScoped<ILocationService, LocationService>();

// ── CORS ──────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// ── Swagger ───────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MMRDA Consent API",
        Version = "v1"
    });
});

// ── Document Storage ──────────────────────────────────────────────────────
builder.Services.Configure<DocumentStorageSettings>(
    builder.Configuration.GetSection("DocumentStorage"));

builder.Services.AddScoped<IDocumentService, DocumentService>();

builder.Services.AddControllers();

// ─────────────────────────────────────────────────────────────────────────
var app = builder.Build();
// ─────────────────────────────────────────────────────────────────────────

// ── Middleware Pipeline (ORDER MATTERS) ───────────────────────────────────

app.UseMiddleware<ExceptionMiddleware>();
// 1. Global error handler — FIRST
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MMRDA Consent API V1");
        c.RoutePrefix = "swagger";
    });
}

app.UseCors("AllowAll");                   // 2. CORS
app.UseHttpsRedirection();                 // 3. HTTPS redirect
app.UseAuthorization();                    // 4. Authorization
app.MapControllers();                      // 5. Route to controllers
app.Run();