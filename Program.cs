//MIT License
//
//Copyright (c) 2023 Microsoft - Jose Batista-Neto
//
//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.
//

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;

using SK_Connector.Services;

namespace SK_Connector;

public class Program
{
    public static void Main(string[] args)
    {
        // Create the web application.
        var builder = WebApplication.CreateBuilder(args);

        // Add CORS policy // this should be corrected for production environements
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll",
                builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
        });

        // Add authentication.
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

        // Load in configuration settings from appsettings.json, user-secrets, key vaults, etc...
        builder.Host.AddConfigurationService();

        // Add custom services.
        builder.Services
            .AddOptionService(builder.Configuration)
            .AddBlobStorageService()
            .AddSemanticTextMemoryService()
            .AddSemanticKernelService();

        // Add endpoints.
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "SKConnector API",
                Description = "API for speeding up OpenAI development"
            });
        });

        // Add health checks.
        builder.Services.AddHealthChecks();

        // Build the web application.
        var app = builder.Build();

        // Add the swagger
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // Configure the HTTP request pipeline for the Web API.
        app.UseHttpsRedirection();

        // Authentication oauth2
        app.UseAuthentication();
        app.UseAuthorization();

        // Map Endpoints
        app.MapControllers();
        app.MapHealthChecks("/");

        // Run the web application.
        app.Run();
    }
}