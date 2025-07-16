using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using pawKitLib.Ai.OpenAi.Models;
using pawKitLib.Ai.OpenAi.Services;
using pawKitLib.Models;
using System.Text.Json;

namespace pawKitAppConsole
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddUserSecrets<Program>()
                    .Build();

                var services = new ServiceCollection()
                    .AddSingleton<IConfiguration>(config)
                    .AddLogging(builder =>
                    {
                        builder.AddConfiguration(config.GetSection("Logging"));
                        builder.AddConsole();
                    })
                    .AddHttpClient();

                // For security, never store the ApiKey property in appsettings.json or any file under source control.
                // Instead, use the .NET user secrets feature during development to keep your API key safe.
                // If you haven't already, initialize user secrets for your project with:
                //   dotnet user-secrets init
                // Then set your API key with:
                //   dotnet user-secrets set "pawKit:Ai:OpenAi:Config:ApiKey" "your-api-key-here"
                // This keeps your secret local and ensures it is loaded at runtime just like a config value, but never checked into source control.

                // The following is the recommended built-in way to bind and validate OpenAiConfigDto:
                // This approach does everything the previous Configure did, and also ensures that ApiKey is validated at startup.
                services.AddOptions<OpenAiConfigDto>()
                    .Bind(config.GetSection("pawKit:Ai:OpenAi:Config"))
                    .Validate(o => !string.IsNullOrWhiteSpace(o.ApiKey), "OpenAI ApiKey must be provided in configuration or user secrets.")
                    .ValidateOnStart();

                // services.Configure<OpenAiConfigDto>(...) does NOT register OpenAiConfigDto itself as a singleton instance.
                // It registers configuration binding for OpenAiConfigDto using the options pattern.
                // When you inject IOptions<OpenAiConfigDto>, you get a new instance bound from config each time you access .Value.
                // If you want a singleton OpenAiConfigDto instance, use:
                //   var openAiConfig = config.GetSection("pawKit:Ai:OpenAi:Config").Get<OpenAiConfigDto>();
                //   services.AddSingleton(openAiConfig);
                // services.Configure<OpenAiConfigDto>(config.GetSection("pawKit:Ai:OpenAi:Config"));

                // PostConfigure can be used to perform additional setup after configuration binding.
                // For example, you can normalize values, set derived properties, or log configuration state for diagnostics.
                // Validation of ApiKey is already handled by .Validate above, so avoid duplicating validation here.
                // services.PostConfigure<OpenAiConfigDto>(config =>
                // {
                // });

                // Register OpenAiAudioTranscriber as transient because it is stateless and may depend on services (like HttpClient) that are themselves transient or scoped.
                // Using transient ensures a fresh instance per request and avoids issues with shared state or improper disposal that could occur with singleton or scoped lifetimes.
                services.AddTransient<OpenAiAudioTranscriber>();

                using var serviceProvider = services.BuildServiceProvider();

                var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("Logger initialized and ready!");

                try
                {
                    await RunTranscriptionTest(serviceProvider, config, logger);
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

        private static async Task RunTranscriptionTest(IServiceProvider serviceProvider, IConfiguration config, ILogger logger)
        {
            try
            {
                string audioFilePath = PromptForInput("Enter the path to an audio file: ");
                if (!File.Exists(audioFilePath))
                {
                    Console.WriteLine("File does not exist. Aborting transcription test.");
                    return;
                }

                var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
                // Do NOT dispose httpClient when using IHttpClientFactory.
                // The factory manages the lifetime and pooling of HttpClient instances.
                // Disposing here would break connection pooling and can cause performance issues in long-running apps.
                var httpClient = httpClientFactory.CreateClient();
                var transcriber = serviceProvider.GetRequiredService<OpenAiAudioTranscriber>();

                var request = new OpenAiAudioTranscribeRequestDto
                {
                    File = new FilePathReferenceDto { FilePath = audioFilePath },
                    // The model is set as a literal here to ensure explicit user control.
                    // See the DTO's comment for why there is no fallback to config or default.
                    // Note: As of 2025-07-16, "gpt-4o-transcribe" is the latest OpenAI transcription model.
                    // However, only "whisper-1" currently supports the "verbose_json" response format, which is required for this test.
                    Model = "whisper-1"
                };
                // Explicitly request the "verbose_json" response format to test detailed output parsing.
                // Only the "whisper-1" model currently supports this format, which includes additional metadata in the response.
                request.SetString("response_format", "verbose_json");
                var response = await transcriber.TranscribeAsync(request);
                Console.WriteLine($"Transcribed text: {response.Text}");

                // Display the full response DTO to verify correct parsing of the verbose_json format.
                // This outputs all properties, including those in ExtraProperties, in a structured, indented format using [JsonExtensionData].
                var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
                Console.WriteLine($"Full Response DTO: {JsonSerializer.Serialize(response, jsonOptions)}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Transcription failed.");
            }
        }

        /// <summary>
        /// Prompts the user for a single line of input after displaying a message.
        /// Trims the input. If allowEmpty is false, keeps prompting until non-empty input is provided.
        /// If allowCancel is true, user can cancel by pressing Ctrl+C (throws OperationCanceledException), Ctrl+Z, or entering 'cancel'.
        /// By default, cancellation is not allowed.
        /// </summary>
        /// <param name="prompt">The message to display before input.</param>
        /// <param name="allowEmpty">Whether to accept an empty string as valid input.</param>
        /// <param name="allowCancel">Whether to allow the user to cancel input. Default is false.</param>
        /// <returns>The trimmed user input, or throws if cancelled and allowed.</returns>
        private static string PromptForInput(string prompt, bool allowEmpty = false, bool allowCancel = false)
        {
            while (true)
            {
                Console.Write(prompt);
                string? input = Console.ReadLine();

                // Handle end-of-input (Ctrl+Z/Ctrl+D or redirected input)
                if (input == null)
                {
                    if (allowCancel)
                        throw new OperationCanceledException("Input cancelled by user (end of input).");
                    Console.WriteLine("Input cannot be cancelled. Please try again.");
                    continue;
                }

                input = input.Trim();

                // Handle typed cancellation
                if (allowCancel && input.Equals("cancel", StringComparison.OrdinalIgnoreCase))
                    throw new OperationCanceledException("Input cancelled by user (typed 'cancel').");

                // Accept input if allowed
                if (allowEmpty || input.Length > 0)
                    return input;

                // Prompt again if input is empty
                if (allowCancel)
                    Console.WriteLine("Input cannot be empty. Please try again or type 'cancel' to abort.");
                else
                    Console.WriteLine("Input cannot be empty. Please try again.");
            }
        }
    }
}
