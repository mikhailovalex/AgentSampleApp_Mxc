using System.ComponentModel;
using AgentSampleApp.Mxc;
using Microsoft.Extensions.AI;

namespace AgentSampleApp.Examples;

/// <summary>
/// Worked examples of how to define agent tools, from simplest to most involved.
///
/// A "tool" is just a C# delegate wrapped by <see cref="AIFunctionFactory.Create"/>. The
/// model reads the <c>name</c>, the <c>description</c>, and each parameter's
/// <see cref="DescriptionAttribute"/> to decide when and how to call it. The
/// function-invocation middleware (see <c>ChatClientFactory</c>) runs the delegate and
/// feeds the return value back to the model automatically.
///
/// To use any of these, pass them to <c>AgentService</c> alongside <c>run_code</c>:
/// <code>
/// var tools = new[] { runCode, ExampleTools.Calculator(), ExampleTools.SearchFiles(runner, workspace) };
/// _agent = new AgentService(chatClient, tools, systemPrompt);
/// </code>
/// </summary>
public static class ExampleTools
{
    /// <summary>
    /// Example 1 — the simplest possible tool: a pure function with typed parameters.
    /// No sandbox, no I/O. Parameter descriptions guide the model's argument choices.
    /// </summary>
    public static AITool Calculator() =>
        AIFunctionFactory.Create(
            ([Description("The arithmetic operation to perform.")] MathOp op,
             [Description("Left operand.")] double a,
             [Description("Right operand.")] double b) => op switch
            {
                MathOp.Add => a + b,
                MathOp.Subtract => a - b,
                MathOp.Multiply => a * b,
                MathOp.Divide => b != 0 ? a / b : double.NaN,
                _ => double.NaN,
            },
            name: "calculator",
            description: "Perform a basic arithmetic operation on two numbers.");

    /// <summary>The model picks one of these by name — enums make choices unambiguous.</summary>
    public enum MathOp { Add, Subtract, Multiply, Divide }

    /// <summary>
    /// Example 2 — a tool that performs real work through the MXC sandbox. It searches the
    /// workspace for a string using <c>findstr</c>. Note how it captures <paramref name="runner"/>
    /// and <paramref name="workspace"/> from the surrounding scope (a closure), so the tool
    /// signature the model sees stays clean (just <c>pattern</c>).
    /// </summary>
    public static AITool SearchFiles(ISandboxRunner runner, string workspace) =>
        AIFunctionFactory.Create(
            async ([Description("Text to search for across files in the workspace.")] string pattern,
                   CancellationToken ct) =>
            {
                // /s = recurse subdirectories, /i = case-insensitive, /n = show line numbers.
                var result = await runner.RunAsync(
                    $"findstr /s /i /n \"{pattern}\" *.*",
                    restricted: true,
                    workspaceDir: workspace,
                    cancellationToken: ct);

                return result.ExitCode == 0
                    ? result.Stdout
                    : $"No matches (exit {result.ExitCode}).";
            },
            name: "search_files",
            description: "Search the workspace for files containing the given text and return "
                       + "matching lines with their file and line number.");

    /// <summary>
    /// Example 3 — a tool that writes a file into the workspace via the sandbox, showing how
    /// a tool can take several parameters and produce a side effect the next tool can use.
    /// </summary>
    public static AITool WriteFile(ISandboxRunner runner, string workspace) =>
        AIFunctionFactory.Create(
            async ([Description("File name to create in the workspace, e.g. notes.txt.")] string fileName,
                   [Description("UTF-8 text content to write.")] string content,
                   CancellationToken ct) =>
            {
                // Use Python (granted via ToolPaths) for safe quoting of arbitrary content.
                var escaped = content.Replace("\\", "\\\\").Replace("\"", "\\\"");
                var result = await runner.RunAsync(
                    $"python -c \"open(r'{fileName}','w',encoding='utf-8').write(\\\"{escaped}\\\")\"",
                    restricted: true,
                    workspaceDir: workspace,
                    cancellationToken: ct);

                return result.ExitCode == 0
                    ? $"Wrote {content.Length} characters to {fileName}."
                    : $"Failed (exit {result.ExitCode}): {result.Stderr}";
            },
            name: "write_file",
            description: "Create or overwrite a text file in the workspace with the given content.");
}
