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

using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;

using SK_Connector.Options;
using SK_Connector.Utils;

namespace SK_Connector.Services;

// Extension methods for registering Semantic Kernel related services.
internal static class SemanticKernelService
{
    // Add Semantic Kernel services
    internal static IServiceCollection AddSemanticKernelService(this IServiceCollection services)
    {
        // Semantic Kernel
        services.AddSingleton<Kernel>(sp =>
        {
            // Semantic Kernel Options
            var semanticKernelOptions = sp.GetRequiredService<IOptions<AIServiceOptions>>().Value;

            // Create the kernel builder
            KernelBuilder builder = new();

            // Add the Completion endpoint
            if (semanticKernelOptions.Type == AIServiceOptions.AIServiceType.AzureOpenAI)
                builder.AddAzureOpenAIChatCompletion(
                    semanticKernelOptions.Models.Completion,
                    "model-id",
                    semanticKernelOptions.Endpoint,
                    semanticKernelOptions.Key);
            else
                builder.AddOpenAIChatCompletion(
                    semanticKernelOptions.Models.Completion,
                    semanticKernelOptions.Key);

            // Build the kernel
            var kernel = builder.Build();

            // Register the skills from the storage account with the kernel.
            Extensions.RegisterKernelSkills(sp, kernel);

            // Return the kernel
            return kernel;
        });

        return services;
    }
}