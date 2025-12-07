using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using webCore.MongoHelper;
using webCore.Services;
using Microsoft.AspNetCore.Http;

namespace webCore
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
            // Add controllers and views
            services.AddControllersWithViews();

            // Add MongoDB Client (singleton because it is thread-safe)
            services.AddSingleton<IMongoClient>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<Startup>>();

                // Read MongoDB configuration
                var mongoConfig = Configuration.GetSection("MongoDB");
                var mongoConnection = mongoConfig["ConnectionString"];
                var databaseName = mongoConfig["DatabaseName"];

                // Log and validate the connection string
                if (string.IsNullOrWhiteSpace(mongoConnection))
                {
                    logger.LogError("MongoDB connection string is missing or empty.");
                    throw new InvalidOperationException("MongoDB connection string is not configured.");
                }

                logger.LogInformation("MongoDB connection string is configured.");
                return new MongoClient(mongoConnection);
            });

            // Register MongoDBService with DI container
            services.AddSingleton<MongoDBService>();

            // Register Cloudinary service for image upload
            services.AddSingleton<CloudinaryService>();

            // Register services that will be used for the application
            services.AddScoped<ProductService>();
            services.AddScoped<CategoryService>();
            services.AddScoped<DetailProductService>();
            services.AddScoped<CartService>();
            services.AddScoped<OrderService>();
            services.AddScoped<VoucherClientService>();
            services.AddScoped<UserService>();
            services.AddScoped<VoucherService>();
            services.AddScoped<AccountService>();
            services.AddScoped<CategoryProduct_adminService>();
            services.AddScoped<Order_adminService>();
            services.AddScoped<User_adminService>();
            services.AddScoped<ForgotPasswordService>();
            services.AddScoped<RoleService>();
            services.AddScoped<ShopService>();
            services.AddScoped<SellerOrderService>();

            // Add session management
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.Cookie.Name = ".AspBookCore.Session"; // Session cookie name
                options.IdleTimeout = TimeSpan.FromMinutes(30); // Session expiration time
                options.Cookie.IsEssential = true; // Cookie is required for the session
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
            });

            // Register a global action filter
            services.AddControllersWithViews(options =>
            {
                options.Filters.Add<SetLoginStatusFilter>();
            });

            services.AddScoped<SetLoginStatusFilter>();

            // Configure JSON options
            services.AddControllers().AddJsonOptions(opts =>
            {
                opts.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            });

            // Access HTTP context for session management
            services.AddHttpContextAccessor();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Use session
            app.UseSession();

            // Configure error handling and environment settings
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            // Enable HTTPS redirection and static file serving
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            // Routing configuration
            app.UseRouting();

            // Authorization middleware (if needed)
            app.UseAuthorization();

            // Configure endpoints for the application
            app.UseEndpoints(endpoints =>
            {
                // Default route
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                // Custom route for DetailUserController
                endpoints.MapControllerRoute(
                    name: "detailUser",
                    pattern: "DetailUser/{action=Index}/{id?}");
            });
        }
    }
}
