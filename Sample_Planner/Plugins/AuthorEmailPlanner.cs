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
using System.ComponentModel;

namespace SK_Planner.Plugins;

public class AuthorEmailPlanner
{
    [KernelFunction]
    [Description("Returns back the required steps necessary to author an email.")]
    [return: Description("The list of steps needed to author an email")]
    public async Task<string> GenerateRequiredStepsAsync(
        Kernel kernel,
        [Description("A 2-3 sentence description of what the email should be about")] string topic,
        [Description("A description of the recipients")] string recipients
    )
    {
        Console.WriteLine("*************GenerateRequiredStepsAsync************");

        // Prompt the LLM to generate a list of steps to complete the task
        var result = await kernel.InvokePromptAsync($"""
        I'm going to write an email to {recipients} about {topic} on behalf of a user.
        Before I do that, can you succinctly recommend the top 3 steps I should take in a numbered list?
        I want to make sure I don't forget anything that would help my user's email sound more professional.
        """, new() {
            { "topic", topic },
            { "recipients", recipients }
        });

        // Return the plan back to the agent
        return result.ToString();
    }
}