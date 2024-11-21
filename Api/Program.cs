using Api;
using Api.Data;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Linq;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<Context>(options =>
{
  //for defaultconnection see appsettings.Development.json
  options.UseSqlServer(builder.Configuration.GetConnectionString("Defaultconnection"));
});

// be able to inject JWT service class inside the Controllers
builder.Services.AddScoped<JWTService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<ContextSeedService>();

//newly add to cater role management
//builder.Services.AddScoped<RoleManagementService>();
//builder.Services.AddTransient<DatabaseSeeder>();

// Add User Secrets
builder.Configuration.AddUserSecrets<Program>();

var configuration = builder.Configuration;

// Access the secrets
var mailjetApiKey = configuration["Mailjet:ApiKey"];
var mailjetSecretKey = configuration["Mailjet:SecretKey"];

// defining our IdentityCore Service
builder.Services.AddIdentityCore<User>(options =>
{
  options.Password.RequiredLength = 6;
  options.Password.RequireDigit = false;
  options.Password.RequireLowercase = false;
  options.Password.RequireUppercase = false;
  options.Password.RequireNonAlphanumeric = false;

  //for email confirmation
  options.SignIn.RequireConfirmedEmail = true;
})
  //other services
  .AddRoles<IdentityRole>() //be able to add roles
  .AddRoleManager<RoleManager<IdentityRole>>() // be able to make use of rolemanager
  .AddEntityFrameworkStores<Context>() //providing our context
  .AddSignInManager<SignInManager<User>>() //make use of signin manager
  .AddUserManager<UserManager<User>>() // make use of usermanager to create users
  .AddDefaultTokenProviders();// be able to create tokens for email confirmation

// be able to authenticate users using JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
  .AddJwtBearer(options =>
  {
    options.TokenValidationParameters = new TokenValidationParameters
    {
      // validate the token based on  the key we have provided inside appsettings.development.json JWT:Key
      ValidateIssuerSigningKey = true,
      //the issuer signing key based on JWT:Key
      IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"])),
      // the issuer which in here is the api project url we are using
      ValidIssuer = builder.Configuration["JWT:Issuer"],
      //validate the issuer (who ever issuing the JWT)
      ValidateIssuer = true,
      // don't validate audience (angular side)
      ValidateAudience = false
    };
  });

builder.Services.AddCors();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
  options.InvalidModelStateResponseFactory = actionContext =>
  {
    var errors = actionContext.ModelState
    .Where(x => x.Value.Errors.Count > 0)
    .SelectMany(x => x.Value.Errors)
    .Select(x => x.ErrorMessage).ToArray();

    var toReturn = new
    {
      Errors = errors
    };

    return new BadRequestObjectResult(toReturn);
  };
});

//Roles policy
builder.Services.AddAuthorization(opt =>
{
  opt.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
  opt.AddPolicy("ManagerPolicy", policy => policy.RequireRole("Manager"));
  opt.AddPolicy("PlayerPolicy", policy => policy.RequireRole("User"));

  //multiple role for admin and manager
  opt.AddPolicy("AdminOrManagerPolicy", policy => policy.RequireRole("Admin", "Manager"));
  //can be access for those who have admin and manager policy
  opt.AddPolicy("AdminAndManagerPolicy", policy => policy.RequireRole("Admin").RequireRole("Manager"));
  //all role policy
  opt.AddPolicy("AllRolePolicy", policy => policy.RequireRole("Admin", "Manager", "User"));

  opt.AddPolicy("AdminEmailPolicy", policy => policy.RequireClaim(ClaimTypes.Email, SD.AdminUserName));
  opt.AddPolicy("GuadaSurnamePolicy", policy => policy.RequireClaim(ClaimTypes.Surname, "guada"));
  opt.AddPolicy("ManagerEmailAndGuadaSurnamePolicy", policy => policy.RequireClaim(ClaimTypes.Surname, "guada")
      .RequireClaim(ClaimTypes.Email, "manager@sample.com"));
  opt.AddPolicy("VIPPolicy", policy => policy.RequireAssertion(context => SD.VIPPolicy(context)));
});


var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseCors(opt =>
{
  opt.AllowAnyHeader().AllowAnyMethod().AllowCredentials().WithOrigins(builder.Configuration["JWT:ClientUrl"]);
});

if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// adding UseAuthentication into our pipeline and this should come before UseAuthorization
// Authentication verifies the identity of a user of service, and authorization determines their access rights.
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

#region ContextSeed
// insert service to program.cs

using var scope = app.Services.CreateScope(); //service scope

try
{
  var contextSeedService = scope.ServiceProvider.GetService<ContextSeedService>();
  await contextSeedService.InitializeContextAsync(); // call this method " InitializeContextAsync() "
}
catch (Exception ex)
{
  var logger = scope.ServiceProvider.GetService<ILogger<Program>>();
  //logger.LogError(ex.Message, "Failed to initialize and seed the database.");
  logger.LogError("Failed to initialize and seed the database. Exception: {0}", ex.Message);

}
#endregion 

app.Run();
