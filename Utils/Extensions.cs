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

#pragma warning disable  SKEXP0052, SKEXP0003  // SemanticTextMemory is For Evaluation and Testing Purpose Only

using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Plugins.Memory;
using Microsoft.SemanticKernel;
using SK_Connector.Options;
using SK_Connector.Services;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace SK_Connector.Utils;

public static class Extensions
{
    // Register the skills with the kernel.
    public static Task RegisterKernelSkills(IServiceProvider sp, Kernel kernel)
    {
        // Add Skills from Storage Account
        KernelPromptTemplateFactory factory = new();

        // Services  Options
        var serviceOptions = sp.GetRequiredService<IOptions<ServiceOptions>>().Value;

        // BlobStorage
        var storage = sp.GetRequiredService<BlobStorage>();

        // Connect to the storage account
        var storageConnection = storage.Connect(serviceOptions.SemanticSkills.ConnectionString, serviceOptions.SemanticSkills.ContainerName);

        // Get the file tree from the storage account
        List<string[]> filetree = storage.GetItemList(storageConnection, "");

        // Create a list of plugins
        List<KernelPlugin> plugins = new();

        // Iterate and register the skills with the kernel.
        foreach (var item in filetree!)
        {
            // The file tree is a list of string arrays with the following structure:
            var pluginName = item[0];   // Directory Name
            var functionName = item[1];
            var fileName = item[2];

            //only process the entry for skprompt
            if (fileName != "skprompt.txt") { continue; }

            // Validate skill name
            ValidSkillName(pluginName);

            // Prepare the path to the prompt file
            var promptPath = Path.Combine(pluginName, functionName, "skprompt.txt");

            // Prepare the path to load prompt configuration. Note: the configuration is optional.
            var configPath = Path.Combine(pluginName, functionName, "config.json");

            PromptTemplateConfig promptConfig = new();
            if (storage.FileExists(storageConnection, configPath))
            {
                var confjson = storage.ReadString(storageConnection, configPath);
                if (!string.IsNullOrWhiteSpace(confjson))
                    promptConfig = PromptTemplateConfig.FromJson(confjson);
            }
            promptConfig.Name = functionName;

            // Load prompt template
            string? promptTemplate = storage.ReadString(storageConnection, promptPath);

            // If the prompt template is empty, skip this skill.
            if (string.IsNullOrWhiteSpace(promptTemplate))
                continue;

            promptConfig.Template = promptTemplate;

            // Create the prompt template instance
            var promptTemplateInstance = factory.Create(promptConfig);

            // Add to the list of plugins to register with the kernel.
            if (promptTemplateInstance != null)
            {
                if (!plugins.Any(p => p.Name == pluginName))
                    plugins.Add(new(pluginName));

                plugins.Where(p => p.Name == pluginName).FirstOrDefault()?.AddFunction(KernelFunctionFactory.CreateFromPrompt(promptTemplateInstance, promptConfig));
            }
        }

        // Clear the plugins from the kernel.
        kernel.Plugins.Clear();

        // Register the plugins with the kernel.
        foreach (var plugin in plugins)
            kernel.Plugins.Add(plugin);

        // Add the semantic text memory plugin
        var memory = sp.GetRequiredService<ISemanticTextMemory>();
        kernel.ImportPluginFromObject(new TextMemoryPlugin(memory));

        return Task.CompletedTask;
    }

    // Validate skill name Regular Expression
    private static readonly Regex s_asciiLettersDigitsUnderscoresRegex = new("^[0-9A-Za-z_]*$");

    // Validate skill name
    private static void ValidSkillName([NotNull] string? skillName)
    {
        if (string.IsNullOrWhiteSpace(skillName) || !s_asciiLettersDigitsUnderscoresRegex.IsMatch(skillName))
            throw new Exception($"Invalid skill name {skillName}");
    }
}
