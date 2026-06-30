namespace AgentSampleApp.Configuration;

/// <summary>
/// Agent settings bound from the "Agent" section of appsettings.json.
/// The API key itself is never stored here — it is read at runtime from the
/// environment variable named by <see cref="ApiKeyEnvVar"/>.
/// </summary>
public sealed class AgentConfig
{
    /// <summary>"OpenAI" or "AzureOpenAI".</summary>
    public string Provider { get; set; } = "OpenAI";

    /// <summary>Model id (OpenAI) or deployment name (Azure OpenAI).</summary>
    public string Model { get; set; } = "Demo";

    /// <summary>
    /// Azure OpenAI endpoint. Ignored for the OpenAI provider. If left empty for the
    /// AzureOpenAI provider, the AZURE_OPENAI_ENDPOINT environment variable is used.
    /// </summary>
    public string? Endpoint { get; set; }

    /// <summary>
    /// Environment variable that holds the API key. For the AzureOpenAI provider, if this
    /// variable is unset, AZURE_OPENAI_API_KEY is used as a fallback.
    /// </summary>
    public string ApiKeyEnvVar { get; set; } = "OPENAI_API_KEY";

    public string SystemPrompt { get; set; } =
        "You are a helpful assistant embedded in a desktop app.";

    /// <summary>
    /// Extra host directories the sandbox may read (for example, a Python or Node.js
    /// install). At the AppContainer tier the sandbox runs on the host filesystem with
    /// access gated by DACLs, so an interpreter outside the workspace is invisible until
    /// its folder is granted here. Each path is mounted read-only and prepended to PATH so
    /// its executables resolve by name (e.g. the model can just call <c>python</c>).
    /// </summary>
    public string[] ToolPaths { get; set; } = [];
}
