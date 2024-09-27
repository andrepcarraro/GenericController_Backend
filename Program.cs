using GenericController_Backend.Entity;
using GenericController_Backend.Hubs;
using GenericController_Backend.Services;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GenericController_Backend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            // Register PIDController and SimulatorService
            builder.Services.AddSingleton<ControlParameters>();  // Singleton for control parameters
            builder.Services.AddSingleton<PIDController>();
            builder.Services.AddSingleton<SimulatorService>();

            // Add SignalR
            builder.Services.AddSignalR()
              .AddJsonProtocol(options =>
              {
                  options.PayloadSerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;
                  options.PayloadSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
              });

            // Add services to the container
            builder.Services.AddControllers();

            // Swagger/OpenAPI configuration
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // CORS policy
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAngularApp", builder =>
                {
                    builder
                        .WithOrigins("http://localhost:4200") // Your Angular app running at localhost
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials(); // This is important if you're using SignalR with credentials
                });
            });

            var app = builder.Build();

            // Use CORS policy
            app.UseCors("AllowAngularApp");

            // Developer exception page and Swagger in development
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                app.UseDeveloperExceptionPage();
            }

            // Uncomment if you want to enforce HTTPS redirection
            // app.UseHttpsRedirection();

            // Map SignalR hubs and controllers
            app.MapHub<ControlHub>("controlHub");

            app.MapControllers();

            // Run the application
            app.Run();
        }
    }
}
