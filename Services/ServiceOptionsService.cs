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

using SK_Connector.Options;

namespace SK_Connector.Services;

internal static class ServiceOptionsService
{
    // Parse configuration into options.
    internal static IServiceCollection AddOptionService(this IServiceCollection services, ConfigurationManager configuration)
    {

        // Default AI service configurations for Semantic Kernel
        AddOptions<AIServiceOptions>(AIServiceOptions.PropertyName);

        // General configuration
        AddOptions<ServiceOptions>(ServiceOptions.PropertyName);

        return services;

        // Local functions
        void AddOptions<TOptions>(string propertyName)
            where TOptions : class
        {
            services.AddOptions<TOptions>(configuration.GetSection(propertyName));
        }
    }

    // Validate options on startup.
    private static void AddOptions<TOptions>(this IServiceCollection services, IConfigurationSection section)
        where TOptions : class
    {
        services.AddOptions<TOptions>()
            .Bind(section)
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }
}