//MIT License
//
//Copyright (c) 2024 Microsoft - Jose Batista-Neto
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

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using SK_Planner.Options;

namespace SK_Planner.Services;

internal static class ConfigurationService
{
    // Build the configuration for the service.
    public static IHostBuilder AddConfigurationService(this IHostBuilder host)
    {
        host.ConfigureAppConfiguration((builderContext, configBuilder) =>
        {
            configBuilder.AddJsonFile(
                path: "appsettings.json",
                optional: false,
                reloadOnChange: true);

            configBuilder.AddJsonFile(
                path: $"appsettings.Development.json",
                optional: true,
                reloadOnChange: true);
        });

        // Add configuration services
        host.ConfigureServices((context, services) =>
        {
            services.Configure<AIServiceOptions>(context.Configuration.GetSection("AIService"));
        });

        return host;
    }
}
