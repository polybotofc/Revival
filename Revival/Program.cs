using Microsoft.EntityFrameworkCore;
using Revival.Data;
using Revival.Services;
using Revival.Middleware;
using Serilog;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/revival-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Starting Roblox Revival Application");

    var builder = WebApplication.CreateBuilder(args);

    // Use Serilog
    builder.Host.UseSerilog();

    // ==========================================
    // CONFIGURATION
    // ==========================================
    
    // Add configuration
    builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true);
    
    // Configure Kestrel
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(5000);
    });

    // ==========================================
    // SERVICES REGISTRATION
    // ==========================================

    // Configure MySQL with Entity Framework Core
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    var serverVersion = ServerVersion.AutoDetect(connectionString);
    
    builder.Services.AddDbContext<RevivalDbContext>(options =>
    {
        options.UseMySql(connectionString, serverVersion, mysqlOptions =>
        {
            mysqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        });
        options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
        options.EnableDetailedErrors(builder.Environment.IsDevelopment());
    });

    // Register application services
    builder.Services.AddScoped<AuthService>();
    builder.Services.AddScoped<GameService>();
    builder.Services.AddScoped<AssetService>();
    builder.Services.AddScoped<RCCManager>();

    // Configure HttpClient for RCCService communication
    builder.Services.AddHttpClient("RCCService", client =>
    {
        client.Timeout = TimeSpan.FromSeconds(30);
        client.DefaultRequestHeaders.Add("Accept", "application/json");
    })
    .UseHttpMessageHandler< Polly.HttpClientGraduation.Extensions.HttpClientFactory.PollyHttpClientBuilderExtensions>();

    // Add controllers and views
    builder.Services.AddControllersWithViews();
    builder.Services.AddRazorPages();

    // Add session support
    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromHours(24);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.Name = builder.Configuration["Authentication:CookieName"] ?? "RevivalAuth";
        options.Cookie.SecurePolicy = builder.Configuration.GetValue<bool>("Authentication:SecureCookies", false)
            ? CookieSecurePolicy.Always
            : CookieSecurePolicy.None;
    });

    // Add CORS support
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });

    // Add memory cache
    builder.Services.AddMemoryCache();

    // Add response compression
    builder.Services.AddResponseCompression();

    // Add health checks
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<RevivalDbContext>("database");

    // ==========================================
    // APPLICATION BUILD
    // ==========================================

    var app = builder.Build();

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseDatabaseErrorPage();
    }
    else
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    // Enable response compression
    app.UseResponseCompression();

    // Initialize database
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();
        
        try
        {
            var dbContext = services.GetRequiredService<RevivalDbContext>();
            
            // Ensure database exists
            await dbContext.Database.EnsureCreatedAsync();
            logger.LogInformation("Database initialized successfully");
            
            // Seed initial data if needed
            await SeedData.InitializeAsync(dbContext, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database initialization error. The application will continue but database operations may fail.");
        }
    }

    // HTTPS redirect
    if (!app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }

    // Static files
    app.UseStaticFiles();

    // Routing
    app.UseRouting();

    // CORS
    app.UseCors("AllowAll");

    // Session
    app.UseSession();

    // Authentication & Authorization
    app.UseAuthentication();
    app.UseAuthorization();

    // Custom middleware
    app.UseRequestLogging();

    // Map routes
    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    app.MapControllerRoute(
        name: "api",
        pattern: "api/{controller}/{action}/{id?}");

    // Health check endpoint
    app.MapHealthChecks("/health");

    Log.Information("Roblox Revival started on http://localhost:5000");
    Log.Information("Target Roblox Client Version: {Version}", builder.Configuration["AppSettings:RobloxClientVersion"]);

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}