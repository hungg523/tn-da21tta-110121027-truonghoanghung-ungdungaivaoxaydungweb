using AppleShop.API.DependencyInjection.Extension;
using AppleShop.API.DependencyInjection.Middleware;
using AppleShop.Application.DependencyInjection.Extension;
using AppleShop.Application.Hubs;
using AppleShop.Infrastructure.DependencyInjection.Extensions;
using AppleShop.Share.Constant;
using AppleShop.Share.DependencyInjection.Extensions;
using Hangfire;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.ConfigureRequest(builder.Configuration);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    var key = Encoding.UTF8.GetBytes(builder.Configuration.GetSection(Const.AUTHEN_KEY).Value!);
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = false,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
.AddGoogle(Const.GOOGLE, options =>
{
    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.ClientId = builder.Configuration.GetSection(Const.GOOGLE_CLIENT_ID).Value!;
    options.ClientSecret = builder.Configuration.GetSection(Const.GOOGLE_CLIENT_SECRET).Value!;
    options.SaveTokens = true;
    options.ClaimActions.MapJsonKey(Const.GOOGLE_PICTURE, Const.PICTURE, Const.PICTURE);
})
.AddFacebook(FacebookDefaults.AuthenticationScheme, options =>
{
    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.AppId = builder.Configuration.GetSection(Const.FACEBOOK_APP_ID).Value!;
    options.AppSecret = builder.Configuration.GetSection(Const.FACEBOOK_SECRET_KEY).Value!;
    options.Fields.Add(Const.EMAIL);
    options.Scope.Add(Const.EMAIL);
    options.Fields.Add(Const.NAME);
    options.SaveTokens = true;
    options.ClaimActions.MapJsonKey(Const.EMAIL, Const.EMAIL, Const.EMAIL);
    options.ClaimActions.MapJsonKey(Const.NAME, Const.NAME, Const.NAME);
});

builder.Services.AddApplication();
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder =>
        builder.WithOrigins(
                "https://localhost:4200",
                "http://localhost:4200",
                "https://localhost:4201",
                "http://localhost:4201",
                "http://thh.id.vn",
                "https://thh.id.vn",
                "http://admin.thh.id.vn",
                "https://admin.thh.id.vn")
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials());
});
builder.Services.AddAuthorization(options => { options.AddPolicy(Const.ROLE_USER_OR_ADMIN, policy => policy.RequireClaim(ClaimTypes.Role, Const.ROLE_USER, Const.ROLE_ADMIN)); });
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPresentation();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSignalR();
builder.Services.AddHttpClient();
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/all-version/swagger.json", "AppleShop API"); });
}
app.UseCors("CorsPolicy");
//app.UseHangfireDashboard(Const.HANGFIRE_ROUTE);
app.UseMiddleware<GlobalExceptionHandler>();
//app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();
app.MapPresentationEndpoint();
app.MapHub<ChatHub>("/chathub");
app.MapHub<NotificationHub>("/admin/notification");
//RecurringJob.AddOrUpdate<ExpiredVoucherCleanupJob>(Const.CLEAN_VOUCHER_JOB, job => job.Run(), Cron.Daily);
//RecurringJob.AddOrUpdate<OrderCleanupJob>(Const.CANCEL_UNPAID_ORDERS, job => job.CancelUnpaidOrdersAsync(), "*/10 * * * *");
app.Run();