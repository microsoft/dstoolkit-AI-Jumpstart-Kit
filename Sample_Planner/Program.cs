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

using Microsoft.SemanticKernel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using SK_Planner.Services;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace SK_Planner;
internal class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Semantic Kernel Planner Test!");

        // Build the host and add configuration service
        var host = Host.CreateDefaultBuilder(args)
            .AddConfigurationService()
            .AddSemanticKernelService()
            .Build();

        // Get the Kernel
        var kernel = host.Services.GetRequiredService<Kernel>();

        // List the installed Plugins
        Console.WriteLine("Installed Plugins");
        foreach (var plugin in kernel.Plugins)
            Console.WriteLine(plugin.Name);

        // Create the chat history system prompt
        ChatHistory history = new ChatHistory("""
    You are a friendly assistant who likes to follow the rules. You will complete required steps
    and request approval before taking any consequential actions. If the user doesn't provide
    enough information for you to complete a task, you will keep asking questions until you have
    enough information to complete the task.
    """);

        // Get chat completion service
        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        // Configure the ToolCallBehavior to automatically call Kernel Functions
        OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
        {
            // Enable auto function calling
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
        };

        // Start the conversation
        while (true)
        {
            // Get user input
            Console.Write("User > ");
            history.AddUserMessage(Console.ReadLine()!);

            // Get the response from the AI
            var result = chatCompletionService.GetStreamingChatMessageContentsAsync(
                history,
                executionSettings: openAIPromptExecutionSettings,
                kernel: kernel);

            // Stream the results
            string fullMessage = "";
            var first = true;
            await foreach (var content in result)
            {
                if (content.Role.HasValue && first)
                {
                    Console.Write("Assistant > ");
                    first = false;
                }
                Console.Write(content.Content);
                fullMessage += content.Content;
            }
            Console.WriteLine();

            // Add the message from the agent to the chat history
            history.AddAssistantMessage(fullMessage);
        }
    }
}
