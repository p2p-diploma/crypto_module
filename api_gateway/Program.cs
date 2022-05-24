using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);
var routesConfig = new ConfigurationBuilder().AddJsonFile("routes.json").Build();
// Add services to the container.

builder.Services.AddOcelot(routesConfig);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
{
    o.RequireHttpsMetadata = false;
});
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

string _refreshPath = builder.Configuration["AuthSettings:RefreshTokenPath"];
string secretKey = builder.Configuration["SecretKey"];
app.UseHttpsRedirection();
await app.UseOcelot(new OcelotPipelineConfiguration
{
    AuthorizationMiddleware = async (context, next) =>
    {
        bool isAuthorized = false;
        if (context.Request.Headers.Any(h => h.Key == "Role"))
        {
            string? access_token = context.Request.Cookies["jwt-access"];
            if (!string.IsNullOrEmpty(access_token))
            {
                string _role = context.Request.Headers["Role"];
                var handler = new JwtSecurityTokenHandler();
                if (handler.CanReadToken(access_token))
                {
                   var parameters = new TokenValidationParameters
                   {
                       ValidateActor = false, ValidateIssuer = false,
                       ValidateLifetime = false, ValidateAudience = false,
                       ValidateIssuerSigningKey = true,
                       IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                   };
                   SecurityToken? validatedToken;
                   try
                   {
                       handler.ValidateToken(access_token, parameters, out validatedToken);
                   }
                   catch
                   {
                       validatedToken = null;
                   }
                   if (validatedToken != null)
                   {
                       var user = validatedToken as JwtSecurityToken;
                       var role = user?.Claims.FirstOrDefault(x => x.Type == "role")?.Value;
                       if (role != null && role == _role)
                       {
                           var exp = user?.Claims.FirstOrDefault(x => x.Type == "exp")?.Value;
                           if (exp != null)
                           {
                               var leftExpiryTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(exp)).DateTime;
                               if (DateTime.Now - leftExpiryTime <= TimeSpan.FromMinutes(2))
                               {
                                   using var client = new HttpClient();
                                   await client.SendAsync(new HttpRequestMessage(HttpMethod.Post, _refreshPath));
                               }
                               isAuthorized = true;
                               await next();
                           }
                       }
                   }
                }
                if(isAuthorized == false)
                    context.Items.SetError(new UnauthenticatedError(""));
            } else await next();
        }
    }
});
app.MapControllers();
app.Run();