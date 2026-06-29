using AgentSampleApp.Configuration;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using OpenAI;

namespace AgentSampleApp.Agent;

/// <summary>
/// Builds a Microsoft.Extensions.AI <see cref="IChatClient"/> for the configured
/// provider and wraps it with the function-invocation pipeline, so tool calls the
/// model requests are dispatched to our C# methods automatically.
/// </summary>
public static class ChatClientFactory
{
    /// <summary>Standard Azure OpenAI environment variables (Azure SDK convention).</summary>
    private const string AzureApiKeyEnvVar = "AZURE_OPENAI_API_KEY";
    private const string AzureEndpointEnvVar = "AZURE_OPENAI_ENDPOINT";

    public static IChatClient Create(AgentConfig config)
    {
        // NOTE: In recent Microsoft.Extensions.AI.OpenAI versions the adapter is
        // AsIChatClient(); older previews exposed it as AsChatClient(). If these lines
        // do not compile, switch to the name your restored package version uses.
        IChatClient inner = config.Provider.Trim().ToLowerInvariant() switch
        {
            "azureopenai" => CreateAzure(config),
            _ => CreateOpenAI(config),
        };

        return inner
            .AsBuilder()
            .UseFunctionInvocation()   // auto-dispatch tool calls
            .Build();
    }

    private static IChatClient CreateOpenAI(AgentConfig config)
    {
        var apiKey = Environment.GetEnvironmentVariable(config.ApiKeyEnvVar)
            ?? throw new InvalidOperationException(
                $"API key not found. Set the '{config.ApiKeyEnvVar}' environment variable.");

        return new OpenAIClient(apiKey)
            .GetChatClient(config.Model)
            .AsIChatClient();
    }

    private static IChatClient CreateAzure(AgentConfig config)
    {
        // Key: honor an explicitly configured env var name, otherwise fall back to the
        // standard AZURE_OPENAI_API_KEY.
        var apiKey = Environment.GetEnvironmentVariable(config.ApiKeyEnvVar)
            ?? Environment.GetEnvironmentVariable(AzureApiKeyEnvVar, EnvironmentVariableTarget.User)
            ?? throw new InvalidOperationException(
                $"Azure OpenAI API key not found. Set the '{AzureApiKeyEnvVar}' environment variable " +
                $"(or the '{config.ApiKeyEnvVar}' variable named in appsettings.json).");

        // Endpoint: an explicit appsettings.json value wins, otherwise read the standard
        // AZURE_OPENAI_ENDPOINT environment variable.
        var endpoint = !string.IsNullOrWhiteSpace(config.Endpoint)
            ? config.Endpoint
            : Environment.GetEnvironmentVariable(AzureEndpointEnvVar, EnvironmentVariableTarget.User)
              ?? throw new InvalidOperationException(
                  $"Azure OpenAI endpoint not found. Set 'Agent:Endpoint' in appsettings.json " +
                  $"or the '{AzureEndpointEnvVar}' environment variable.");

        return new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey))
            .GetChatClient(config.Model)
            .AsIChatClient();
    }
}
