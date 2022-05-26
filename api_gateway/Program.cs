using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);
var routesConfig = new ConfigurationBuilder().AddJsonFile("routes.json").Build();
// Add services to the container.
using var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
builder.Services.AddCors(b =>
{
    b.AddDefaultPolicy(c => c.SetIsOriginAllowed(_ => true).AllowCredentials().AllowAnyMethod().AllowAnyHeader());
});
builder.Services.AddOcelot(routesConfig);
var app = builder.Build();

// Configure the HTTP request pipeline.
var logger = loggerFactory.CreateLogger<Program>();
string _refreshPath = builder.Configuration["AuthSettings:RefreshTokenPath"];
string secretKey = builder.Configuration["SecretKey"];
app.UseCors();
app.UseHttpsRedirection();
app.UseWebSockets();
await app.UseOcelot(new OcelotPipelineConfiguration
{
    AuthorizationMiddleware = async (context, next) =>
    {
        bool isAuthorized = false;
        if (context.Request.Headers.Any(h => h.Key == "Role"))
        {
            logger.LogInformation("Request requires authorization: Header contains role");
            string? access_token = context.Request.Cookies["jwt-access"];
            if (!string.IsNullOrEmpty(access_token))
            {
                string _role = context.Request.Headers["Role"];
                logger.LogInformation($"Access token from cookies: {access_token}, role: {_role}");
                var handler = new JwtSecurityTokenHandler();
                if (handler.CanReadToken(access_token))
                {
                    logger.LogInformation("Token is readable");
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
                        logger.LogInformation("Valid token signature");
                        var user = validatedToken as JwtSecurityToken;
                        var role = user?.Claims.FirstOrDefault(x => x.Type == "role")?.Value;
                        if (role != null && role == _role)
                        {
                            logger.LogInformation($"Role is valid from token: {role}");
                            var exp = user?.Claims.FirstOrDefault(x => x.Type == "exp")?.Value;
                            if (exp != null)
                            {
                                logger.LogInformation($"Found expiry date in token: {exp}");
                                var leftExpiryTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(exp)).DateTime;
                                if (DateTime.Now - leftExpiryTime <= TimeSpan.FromMinutes(2))
                                {
                                    logger.LogWarning("Token needs to be refreshed");
                                    using var client = new HttpClient();
                                    var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Post, _refreshPath));
                                    if(response.IsSuccessStatusCode)
                                        logger.LogInformation("Refreshed token");
                                }
                                isAuthorized = true;
                                logger.LogInformation("Successful authorization");
                                await next();
                            } else logger.LogError("Token does not have expiration date");
                        } else logger.LogError("Token role is not authorized");
                    } else logger.LogError("Invalid token signature");
                } else logger.LogError("Token is invalid");
            } else logger.LogError("Token is not found in cookies");
            
            if (isAuthorized == false)
                context.Items.SetError(new UnauthenticatedError(""));
        }
        else
        {
            logger.LogInformation("Request for unauthorized resource");
            await next();
        }
    }
});
app.MapControllers();
app.Run();