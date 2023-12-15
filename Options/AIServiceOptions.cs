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
using System.ComponentModel.DataAnnotations;

namespace SK_Connector.Options;

// Configuration options for AI services, such as Azure OpenAI and OpenAI.
public sealed class AIServiceOptions
{
    public const string PropertyName = "AIService";

    // Supported types of AI services.
    public enum AIServiceType
    {
        AzureOpenAI, // Azure OpenAI https://learn.microsoft.com/en-us/azure/cognitive-services/openai/
        OpenAI         // OpenAI https://openai.com/
    }

    // AI models to use.
    public class ModelTypes
    {
        // Azure OpenAI deployment name or OpenAI model name to use for completions.
        [Required]
        public string Completion { get; set; } = string.Empty;

        // Azure OpenAI deployment name or OpenAI model name to use for embeddings.
        [Required]
        public string Embedding { get; set; } = string.Empty;
    }

    // Type of AI service.
    [Required]
    public AIServiceType Type { get; set; } = AIServiceType.AzureOpenAI;

    // Models/deployment names to use.
    [Required]
    public ModelTypes Models { get; set; } = new();

    // (Azure OpenAI only) Azure OpenAI endpoint.
    [Required]
    public string Endpoint { get; set; } = string.Empty;

    // Key to access the AI service.
    [Required]
    public string Key { get; set; } = string.Empty;
}
