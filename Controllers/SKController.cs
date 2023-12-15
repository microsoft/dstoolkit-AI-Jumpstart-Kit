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


using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web.Resource;
using Microsoft.SemanticKernel;
using System.Text.Json;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Plugins.Memory;

using SK_Connector.Options;
using SK_Connector.Services;

namespace SK_Connector.Controllers;

#pragma warning disable SKEXP0052, SKEXP0003 // SemanticTextMemory is For Evaluation and Testing Purpose Only
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

[Authorize]  // Comment this to remove authentication
[ApiController]
[Route("[controller]")]
[RequiredScope(RequiredScopesConfigurationKey = "AzureAd:Scopes")]
public class EndPoints : ControllerBase
{
    private readonly ILogger<EndPoints> _logger;
    private readonly IOptions<AIServiceOptions> _aiserviceoptions;
    private readonly IOptions<ServiceOptions> _serviceoptions;
    private readonly Kernel _kernel;
    private readonly BlobStorage _blobStorage;
    private readonly ISemanticTextMemory _semanticTextMemory;
    private readonly IServiceProvider _sp;


    public EndPoints(IServiceProvider sp, IOptions<AIServiceOptions> aiserviceoptions, IOptions<ServiceOptions> serviceoptions, ILogger<EndPoints> logger, [FromServices] Kernel kernel, BlobStorage blobStorage, ISemanticTextMemory semanticTextMemory)
    {
        _logger = logger;
        _aiserviceoptions = aiserviceoptions;
        _serviceoptions = serviceoptions;
        _kernel = kernel;
        _blobStorage = blobStorage;
        _semanticTextMemory = semanticTextMemory;
        _sp = sp;
    }

    [HttpPost]
    [Tags("OpenAI")]
    [Route("/Function/Execute")]
    [ProducesResponseType(typeof(String), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(String), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> SKFunctionExecute(string pluginName, string functionName, string input)
    {
        try
        {
            // Get the function
            var sf = _kernel.Plugins.GetFunction(pluginName, functionName);

            // Prepare the arguments
            var arguments = new KernelArguments()
            {
                ["input"] = input
            };

            // Process the user message and get an answer
            var answer = await _kernel.InvokeAsync(sf, arguments);

            // Return the answer
            return Ok(answer.ToString());
        }
        catch (Exception ex)
        {
            // Return the error
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    [Tags("OpenAI")]
    [Route("/Function/ExecuteWithMemory")]
    [ProducesResponseType(typeof(String), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(String), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(String), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> SKFunctionExecuteWithMemory(string pluginName, string functionName, double Relevance, string input)
    {
        try
        {
            // Get the function
            var sf = _kernel.Plugins.GetFunction(pluginName, functionName);

            // Search the memory for the input
            var res = _semanticTextMemory.SearchAsync("Data", input).ToBlockingEnumerable().First();

            // Check if the input was found
            var memoryrelevance = $"Memory Relevance: {res.Relevance:P} Result: {res.Metadata.Text}";
            if (res.Relevance < Relevance)
                return NotFound("Not found within relevance threshhold. Please try again.");

            // Prepare the arguments
            var arguments = new KernelArguments();
            arguments["fact1"] = res.Metadata.Text;
            arguments["input"] = input;
            arguments["limit"] = 10;
            arguments[TextMemoryPlugin.CollectionParam] = "Data";
            arguments[TextMemoryPlugin.RelevanceParam] = Relevance.ToString();

            // Process the user message and get an answer
            var answer = await sf.InvokeAsync(_kernel, arguments);

            // Return the answer
            return Ok(answer.ToString());
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    private class PluginInfo
    {
        public string PluginName { get; set; } = string.Empty;
        public string? FunctionName { get; set; }
        public string? Description { get; set; }
    }

    [HttpGet]
    [Tags("CRUD")]
    [Route("/SK/ListPlugins")]
    [ProducesResponseType(typeof(JsonDocument), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(String), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<JsonDocument>> SKListPlugins()
    {
        try
        {
            // Get the list of plugins
            List<PluginInfo> plugins = new();
            foreach (var item in _kernel.Plugins)
                foreach (var function in item)
                    plugins.Add(new PluginInfo { PluginName = item.Name, FunctionName = function.Name, Description = function.Description });

            // sort the list
            plugins.Sort((x, y) => x.PluginName.CompareTo(y.PluginName));

            // Serialize the list
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(plugins, options);

            // Return the list
            return Ok(JsonDocument.Parse(json));
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet]
    [Tags("CRUD")]
    [Route("/SK/Get")]
    [ProducesResponseType(typeof(String), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(String), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(String), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> SkGet(string pluginName, string functionName)
    {
        try
        {
            // Connect to the storage account
            var cont = _blobStorage.Connect(_serviceoptions.Value.SemanticSkills.ConnectionString, _serviceoptions.Value.SemanticSkills.ContainerName);

            // Prepare the path to the prompt file
            var SKpromptPath = Path.Combine(pluginName, functionName, "skprompt.txt");

            // Check if the prompt file exists
            if (_blobStorage.FileExists(cont, SKpromptPath))
            {
                // Read the prompt file
                var SKPrompt = _blobStorage.ReadString(cont, SKpromptPath);

                // Prepare the path to the config file
                var SkConfigPath = Path.Combine(pluginName, functionName, "config.json");

                var SkConfig = string.Empty;

                // Check if the config file exists
                if (_blobStorage.FileExists(cont, SkConfigPath))
                    SkConfig = _blobStorage.ReadString(cont, SkConfigPath);

                // Create a JSON containig the prompt and config
                var json = $"{{\"prompt\":\n\"{SKPrompt}\",\n\"config\":\n\"{SkConfig}\"\n}}";

                // Return the JSON
                return Ok(json);
            }

            // Prompt file not found
            return NotFound($"'{SKpromptPath}' not found");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    [Tags("CRUD")]
    [Route("/SK/Post")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(String), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SKInsert(string pluginName, string functionName, string SKPrompt, string? SkConfigjson = null)
    {
        try
        {
            // Connect to the storage account
            var cont = _blobStorage.Connect(_serviceoptions.Value.SemanticSkills.ConnectionString, _serviceoptions.Value.SemanticSkills.ContainerName);

            // Prepare the path to the prompt file
            var SKpromptPath = Path.Combine(pluginName, functionName, "skprompt.txt");

            // Write the prompt file
            _blobStorage.WriteString(cont, SKpromptPath, SKPrompt);

            if (!string.IsNullOrEmpty(SkConfigjson))
            {
                // Prepare the path to the config.json file
                var SKConfigPath = Path.Combine(pluginName, functionName, "config.json");

                // Write the config.json file
                _blobStorage.WriteString(cont, SKConfigPath, SkConfigjson);
            }

            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete]
    [Tags("CRUD")]
    [Route("/SK/Delete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(String), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(String), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> SKDelete(string pluginName, string functionName)
    {
        try
        {
            // Connect to the storage account
            var cont = _blobStorage.Connect(_serviceoptions.Value.SemanticSkills.ConnectionString, _serviceoptions.Value.SemanticSkills.ContainerName);

            // Prepare the path to the prompt file
            var SKpromptPath = Path.Combine(pluginName, functionName, "skprompt.txt");

            // Check if the prompt file exists
            if (_blobStorage.FileExists(cont, SKpromptPath))
            {
                // delete the file
                _blobStorage.DeleteFile(cont, SKpromptPath);

                // Prepare the path to the config file
                var SkConfigPath = Path.Combine(pluginName, functionName, "config.json");

                // Check if the config file exists
                if (_blobStorage.FileExists(cont, SkConfigPath))
                    _blobStorage.DeleteFile(cont, SkConfigPath);

                // Check if all functions are deleted so we also delete the skill folder
                var functions = _blobStorage.GetItemList(cont, Path.Combine(pluginName));

                if (functions.Count() == 0)
                    _blobStorage.DeleteFile(cont, Path.Combine(pluginName));

                return Ok();
            }

            // Prompt file not found
            return NotFound($"'{pluginName}/{functionName}' not found");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet]
    [Route("/Memory/List")]
    [Tags("Memory")]
    [ProducesResponseType(typeof(String), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(String), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<string>> MemoryList()
    {
        try
        {
            List<string> l = new();
            int i = 0;
            while (true && i < 10000)
            {
                var r = await _semanticTextMemory.GetAsync("Data", $"{i++}");
                if (r == null)
                    break;
                l.Add(r.Metadata.Text);
            }

            return Ok(JsonSerializer.Serialize(l, new JsonSerializerOptions() { WriteIndented = true }));
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    [Route("/Memory/Search/")]
    [Tags("Memory")]
    [ProducesResponseType(typeof(String), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(String), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> MemorySearch(string Input)
    {
        try
        {
            var res = _semanticTextMemory.SearchAsync("Data", Input).ToBlockingEnumerable().First();
            return Ok($"Memory Search Relevance: {res.Relevance:P} Search Result: '{res.Metadata.Text}'");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}

