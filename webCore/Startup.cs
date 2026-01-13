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
using webCore.Hubs;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace webCore
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddSignalR();
            services.AddSingleton<IMongoClient>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<Startup>>();

                var mongoConfig = Configuration.GetSection("MongoDB");
                var mongoConnection = mongoConfig["ConnectionString"];
                var databaseName = mongoConfig["DatabaseName"];

                if (string.IsNullOrWhiteSpace(mongoConnection))
                {
                    logger.LogError("MongoDB connection string is missing or empty.");
                    throw new InvalidOperationException("MongoDB connection string is not configured.");
                }

                logger.LogInformation("MongoDB connection string is configured.");
                return new MongoClient(mongoConnection);
            });
            services.AddScoped<IMongoDatabase>(sp =>
            {
                var mongoConfig = Configuration.GetSection("MongoDB");
                var databaseName = mongoConfig["DatabaseName"];

                var client = sp.GetRequiredService<IMongoClient>();

                return client.GetDatabase(databaseName);
            });
            services.AddSingleton<MongoDBService>();

            services.AddSingleton<CloudinaryService>();
            services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 104857600; 
                options.ValueLengthLimit = 104857600;
                options.MultipartHeadersLengthLimit = 16384;
            });
            services.Configure<KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = 104857600; 
                options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(10);
                options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(10);
            });

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
            services.AddScoped<ReviewService>();
            services.AddScoped<SellerOrderService>();
            services.AddScoped<IConversationService, ConversationService>();
            services.AddScoped<IMessageService, MessageService>();
            services.AddScoped<ReturnRequestService>();

            services.AddScoped<ChatService>();
            // Add session management
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.Cookie.Name = ".AspBookCore.Session";
                options.IdleTimeout = TimeSpan.FromMinutes(30); 
                options.Cookie.IsEssential = true; 
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
            });

            services.AddControllersWithViews(options =>
            {
                options.Filters.Add<SetLoginStatusFilter>();
            });

            services.AddScoped<SetLoginStatusFilter>();

            services.AddControllers().AddJsonOptions(opts =>
            {
                opts.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            });

            services.AddHttpContextAccessor();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSession();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapHub<ChatHub>("/chatHub");
                endpoints.MapControllerRoute(
                    name: "detailUser",
                    pattern: "DetailUser/{action=Index}/{id?}");
            });
        }
    }
}
