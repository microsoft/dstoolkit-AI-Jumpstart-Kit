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

#pragma warning disable SKEXP0011, SKEXP0052, SKEXP0003 // SemanticTextMemory is For Evaluation and Testing Purpose Only

using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.Plugins.Memory;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI;
using Microsoft.SemanticKernel.Memory;

using SK_Connector.Options;

namespace SK_Connector.Services;

internal static class SemanticMemoryService
{
    // Add the semantic memory.
    internal static IServiceCollection AddSemanticTextMemoryService(this IServiceCollection services)
    {
        services.AddSingleton<ISemanticTextMemory>(sp =>
        {
            // Semantic Memory Options
            var semanticMemoryOptions = sp.GetRequiredService<IOptions<AIServiceOptions>>().Value;

            // Create the memory builder
            var memoryBuilder = new MemoryBuilder();

            // Add the text embedding generation endpoint
            if (semanticMemoryOptions.Type == AIServiceOptions.AIServiceType.AzureOpenAI)
                memoryBuilder.WithAzureOpenAITextEmbeddingGeneration(
                    semanticMemoryOptions.Models.Embedding,
                    "model-id",
                    semanticMemoryOptions.Endpoint,
                    semanticMemoryOptions.Key);
            else
                memoryBuilder.WithOpenAITextEmbeddingGeneration(
                    semanticMemoryOptions.Models.Embedding,
                    semanticMemoryOptions.Key);

            // Add the memory store
            memoryBuilder.WithMemoryStore(new VolatileMemoryStore());

            // Build the memory
            var memory = memoryBuilder.Build();

            // Load vectordb
            var optionsblob = sp.GetRequiredService<IOptions<ServiceOptions>>().Value;

            // Connect to the storage account containing the list of memories
            var storage = sp.GetRequiredService<BlobStorage>();
            var cont = storage.Connect(optionsblob.Memory.ConnectionString, optionsblob.Memory.ContainerName);

            // Bring the memory from the storage account
            int i = 0;
            var files = storage.GetItemList(cont, "");
            foreach (var file in files)
            {
                string? data = storage.ReadString(cont, file[0]);

                if(data == null)
                    continue;   

                string[] lines = data.Split(Environment.NewLine);
                
                // Iteract thru all lines and add each line as a memory fact
                foreach (string line in lines)
                    memory.SaveInformationAsync("Data", line, $"{i++}");
            }

            return memory;
        });

        return services;
    }
}
