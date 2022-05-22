using AppealService.Api;
using AppealService.Api.Config;
using AppealService.Contexts;
using AppealService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddDbContext<AppealsContext>(options => 
    options.UseMySql(builder.Configuration["ConnectionStrings:DefaultConnection"], new MySqlServerVersion(new Version(8, 0, 11))));
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));
builder.Services.AddScoped<AppealsService>();
builder.Services.AddTransient<UsersApi>();
builder.Services.AddTransient<WalletsApi>();
builder.Services.AddTransient<NotificationService>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(opts => { opts.RequireHttpsMetadata = false; });

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
app.UseAuthorization();
app.MapControllers();
app.Run();