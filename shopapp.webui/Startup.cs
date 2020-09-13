using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using shopapp.business.Abstract;
using shopapp.business.Concrete;
using shopapp.data.Abstract;
using shopapp.data.Concrete.EfCore;
using shopapp.webui.EMailServices;
using shopapp.webui.Identity;

namespace shopapp.webui
{
    public class Startup
    {
            private IConfiguration _Configuration;
            public Startup(IConfiguration configuration)
            {
                _Configuration=configuration;
            }
        public void ConfigureServices(IServiceCollection services)
        {
           
            services.AddDbContext<ApplicationContext>(Options=>Options.UseSqlite("Data Source=shopDb"));
            services.AddIdentity<User,IdentityRole>().AddEntityFrameworkStores<ApplicationContext>().AddDefaultTokenProviders();
            
            services.Configure<IdentityOptions>(Options=>{
            Options.Password.RequireDigit=true;//kullanıcı mutlaka şifre için sayısal değer girmeli
            Options.Password.RequireLowercase=true;//mutlaka küçük harf olmalı
            Options.Password.RequireUppercase=true;//büyük
            Options.Password.RequiredLength=5;//en az 5 karakter
            Options.Password.RequireNonAlphanumeric=true;//@ vb gibi işaretler

            //LOCKOUT kullanıcı hesabının kitlenip kitlenmemesiyle alakalı
            Options.Lockout.MaxFailedAccessAttempts=5;//kullanıcı yanlış parolayı 5 kere girer max
            Options.Lockout.DefaultLockoutTimeSpan=TimeSpan.FromMinutes(5);//zaman bilgisi 5dk sonra kullanıcı giriş yapmaya devam edebilir
            Options.Lockout.AllowedForNewUsers=true;//lockout aktif olması içinse

     //       Options.User.AllowedUserNameCharacters="";//kullanıcı user içinde olasmını istediğin rakamlar veya karakterler
            Options.User.RequireUniqueEmail=true;//her kullanıcının farklı email adresi olması lazım;
            Options.SignIn.RequireConfirmedEmail=true;//kullanıcı üye olur ama mutlaka hesabı onaylaması lazım
            Options.SignIn.RequireConfirmedPhoneNumber=false;//telefon içinde bir onay
            
            services.ConfigureApplicationCookie(Options=>//cooke kullanıcı tarayıcısnda uygulama tarafında bırakılan bilgi 
            {
                 Options.LoginPath="/account/login"; //girdiğim veri yanlışsa
                 Options.LogoutPath="/account/logut";//çıktığım zaman
                 Options.AccessDeniedPath="/account/accessdenied";//her login olan kullanıcı admin sayfasını görüntüleyememeli
                 Options.SlidingExpiration=true;  //cooke süresi varsayılan 20 dk 20 dk ,stek yapmazsan cook silinir ve tekrar login olmak için login sayfasına gönderir
     //          Options.ExpireTimeSpan=TimeSpan.FromDays(365); //sen bana login olursan 365 gün boyunca seni tutarım
                 Options.ExpireTimeSpan=TimeSpan.FromMinutes(60); //20 dk olan varsayılan değeri 60a çıkardım 60 dk boyunca login olabilirsin
                 Options.Cookie=new CookieBuilder
                 {
                     HttpOnly=true,
                     Name=".ShopApp.Security.Cookie",
                     SameSite=SameSiteMode.Strict
                 };
            });


            });

            services.AddScoped<ICategoryRepository,EfCoreCategoryRepository>(); 
            services.AddScoped<IProductRepository,EfCoreProductRepository>(); 

            services.AddScoped<IProductService,ProductManager>(); 
            services.AddScoped<ICategoryService,CategoryManager>(); 

            services.AddScoped<IEmailSender,SmtpEmailSender>(i=> 
                new SmtpEmailSender(
                    _Configuration["EmailSender:Host"],
                    _Configuration.GetValue<int>("EmailSender:Port"),
                    _Configuration.GetValue<bool>("EmailSender:EnableSSL"),
                    _Configuration["EmailSender:UserName"],
                    _Configuration["EmailSender:Password"])
                );

            services.AddControllersWithViews();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseStaticFiles(); // wwwroot

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(),"node_modules")),
                    RequestPath="/modules"                
            });

            if (env.IsDevelopment())
            {
                SeedDatabase.Seed();
                app.UseDeveloperExceptionPage();
            }
            app.UseAuthentication();
            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "adminproducts", 
                    pattern: "admin/products",//bu şekilde talep gelirse
                    defaults: new {controller="Admin",action="ProductList"}//admin product çalıştırılır
                );
                endpoints.MapControllerRoute(
                    name: "adminproductscreate", 
                    pattern: "admin/products/create",
                    defaults: new {controller="Admin",action="CreateProduct"}
                );

                endpoints.MapControllerRoute(
                    name: "adminproductedit", 
                    pattern: "admin/products/{id?}",//admin product sabit id değişken. ? yaptımki bulamazsam hata göndericem.id verilirse edite gider
                    defaults: new {controller="Admin",action="ProductEdit"}
                );     

                endpoints.MapControllerRoute(
                    name: "admincategories", 
                    pattern: "admin/categories",
                    defaults: new {controller="Admin",action="CategoryList"}
                );

                 endpoints.MapControllerRoute(
                    name: "admincategorycreate", 
                    pattern: "admin/categories/create",
                    defaults: new {controller="Admin",action="CategoryCreate"}
                );
               
                endpoints.MapControllerRoute(
                    name: "admincategoryedit", 
                    pattern: "admin/categories/{id?}",//dikkat navbarda hrefte categries burad abir trek var :D
                    defaults: new {controller="Admin",action="CategoryEdit"}
                );     


                // localhost/search    
                endpoints.MapControllerRoute(
                    name: "search", 
                    pattern: "search",
                    defaults: new {controller="Shop",action="search"}
                );

                endpoints.MapControllerRoute(
                    name: "productdetails", 
                    pattern: "{url}",
                    defaults: new {controller="Shop",action="details"}
                );

                endpoints.MapControllerRoute(
                    name: "products", 
                    pattern: "products/{category?}",
                    defaults: new {controller="Shop",action="list"}
                );

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern:"{controller=Home}/{action=Index}/{id?}"
                );
            });
        }
    }
}
