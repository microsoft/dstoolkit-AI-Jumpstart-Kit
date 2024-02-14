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
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

using SK_Planner.Options;
using SK_Planner.Plugins;

namespace SK_Planner.Services;

// Extension methods for registering Semantic Kernel related services.
internal static class SemanticKernelService
{
    // Add Semantic Kernel services
    public static IHostBuilder AddSemanticKernelService(this IHostBuilder host)
    {
        // Semantic Kernel
        host.ConfigureServices((context, services) =>
        {
            services.AddSingleton<Kernel>(sp =>
            {
                // Semantic Kernel Options
                var semanticKernelOptions = sp.GetRequiredService<IOptions<AIServiceOptions>>().Value;

                // Create the kernel builder
                var builder = Kernel.CreateBuilder();

                // Add Logging
                // builder.Services.AddLogging(c => c.AddDebug().AddConsole().SetMinimumLevel(LogLevel.Trace));

                // Add the Completion endpoint
                if (semanticKernelOptions.Type == AIServiceOptions.AIServiceType.AzureOpenAI)
                    builder.AddAzureOpenAIChatCompletion(
                        semanticKernelOptions.Models.Completion,
                        semanticKernelOptions.Endpoint,
                        semanticKernelOptions.ApiKey);
                else
                    builder.AddOpenAIChatCompletion(
                        semanticKernelOptions.Models.Completion,
                        semanticKernelOptions.ApiKey);

                // Add the Plug Ins
                builder.Plugins.AddFromType<AuthorEmailPlanner>();
                builder.Plugins.AddFromType<EmailPlugin>();
                builder.Plugins.AddFromType<ReadTextPlugIn>();

                // Return the kernel
                return builder.Build();
            });
        });
        return host;
    }
}