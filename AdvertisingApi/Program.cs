using AdvertisingApi.Endpoints;
using Application.Interfaces;
using Application.Services;
using Microsoft.EntityFrameworkCore;
using Persistence.DataAccess;
using Persistence.DataAccess.Repositories;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;

services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("AdvertisingDb"));

services.AddScoped<IAdvertisingPlatformRepository, AdvertisingPlatformRepository>();
services.AddScoped<IAdvertisingService, AdvertisingService>();

var app = builder.Build();

app.MapAdvertisingApi();

app.Run();