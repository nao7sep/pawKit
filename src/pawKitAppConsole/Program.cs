using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using pawKitLib.Ai.OpenAi.Models;
using pawKitLib.Ai.OpenAi.Services;

namespace pawKitAppConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                var services = new ServiceCollection()
                    .AddSingleton<IConfiguration>(config)
                    .AddLogging(builder =>
                    {
                        builder.AddConfiguration(config.GetSection("Logging"));
                        builder.AddConsole();
                    })
                    .AddHttpClient();

                var serviceProvider = services.BuildServiceProvider();

                var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("Logger initialized and ready!");

                try
                {
                    var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
                    var httpClient = httpClientFactory.CreateClient();

                    var openAiConfig = config.GetSection("pawKit:Ai:OpenAi:Config").Get<OpenAiConfigDto>();
                    openAiConfig = openAiConfig ?? throw new InvalidOperationException("OpenAI config section is missing or invalid.");

                    var transcriberLogger = serviceProvider.GetRequiredService<ILogger<OpenAiAudioTranscriber>>();
                    var transcriber = new OpenAiAudioTranscriber(transcriberLogger, openAiConfig, httpClient);
                }
                catch (Exception ex)
                {
                    // LogError will output to the console using the configured log format, including:
                    // - Timestamp
                    // - Log level (Error)
                    // - Logger category (pawKitAppConsole.Program)
                    // - The provided message
                    // - Exception type, message, and stack trace
                    //
                    // Example output:
                    // [2025-07-15T12:34:56.789Z] [Error] pawKitAppConsole.Program
                    // Runtime error occurred.
                    // System.Exception: Something went wrong
                    //    at pawKitAppConsole.Program.Main(String[] args) in Program.cs:line 42
                    //
                    // This makes it easy to identify, filter, and diagnose errors during runtime.
                    logger.LogError(ex, "Runtime error occurred.");
                }
            }
            catch (Exception ex)
            {
                // Use Console.Error.WriteLine to output initialization errors to the standard error stream (stderr).
                // This is different from Console.WriteLine, which writes to the standard output stream (stdout).
                // Sending errors to stderr allows shells, CI systems, and other tools to distinguish error output from normal output,
                // making it easier to filter, redirect, or highlight errors. This is especially important for startup failures,
                // which occur before the logger is available and should be clearly marked as errors.
                Console.Error.WriteLine($"Initialization error: {ex}");
            }
        }
    }
}
