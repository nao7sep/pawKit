using System.ComponentModel;
using System.Numerics.Tensors;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using pawKitLib.Ai.OpenAi.Models;
using pawKitLib.Ai.OpenAi.Services;
using pawKitLib.Models;
using Xunit.Abstractions;

namespace pawKitLib.Tests.Ai.OpenAi.Services;

/// <summary>
/// Integration tests for OpenAI services. These tests require valid OpenAI API credentials
/// and will make actual API calls. They test real-world scenarios and API compatibility.
/// </summary>
public class OpenAiIntegrationTests
{
    private readonly ITestOutputHelper _output;
    private readonly OpenAiConfigDto _config;
    private readonly OpenAiChatCompleter _chatCompleter;
    private readonly OpenAiFileManager _fileManager;
    private readonly OpenAiAudioSpeaker _audioSpeaker;
    private readonly OpenAiAudioTranscriber _audioTranscriber;
    private readonly OpenAiImageGenerator _imageGenerator;
    private readonly OpenAiEmbedder _embedder;
    private readonly OpenAiToolCallOrchestrator _toolOrchestrator;

    public OpenAiIntegrationTests(ITestOutputHelper output)
    {
        _output = output;

        // Initialize configuration with user secrets (same as console app)
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<OpenAiIntegrationTests>()
            .Build();

        // Bind configuration to OpenAiConfigDto
        _config = new OpenAiConfigDto();
        configuration.GetSection("pawKit:Ai:OpenAi:Config").Bind(_config);

        // Set default base URL if not configured
        if (string.IsNullOrEmpty(_config.BaseUrl))
        {
            _config.BaseUrl = "https://api.openai.com/v1";
        }

        // Initialize services with proper dependency injection
        var httpClient = new HttpClient();
        var options = Options.Create(_config);

        _chatCompleter = new OpenAiChatCompleter(new NullLogger<OpenAiChatCompleter>(), options, httpClient);
        _fileManager = new OpenAiFileManager(new NullLogger<OpenAiFileManager>(), options, httpClient);
        _audioSpeaker = new OpenAiAudioSpeaker(new NullLogger<OpenAiAudioSpeaker>(), options, httpClient);
        _audioTranscriber = new OpenAiAudioTranscriber(new NullLogger<OpenAiAudioTranscriber>(), options, httpClient);
        _imageGenerator = new OpenAiImageGenerator(new NullLogger<OpenAiImageGenerator>(), options, httpClient);
        _embedder = new OpenAiEmbedder(new NullLogger<OpenAiEmbedder>(), options, httpClient);

        var toolCallHandler = new OpenAiToolCallHandler(new NullLogger<OpenAiToolCallHandler>());
        _toolOrchestrator = new OpenAiToolCallOrchestrator(new NullLogger<OpenAiToolCallOrchestrator>(), _chatCompleter, toolCallHandler);
    }

    /// <summary>
    /// Tests that the API key is properly retrieved from user secrets and contains meaningful content.
    /// This should be the first test to ensure basic configuration is working.
    /// </summary>
    [Fact]
    public void ApiKey_WhenRetrievedFromUserSecrets_HasMeaningfulContent()
    {
        // Assert - API key should not be null or empty
        Assert.False(string.IsNullOrEmpty(_config.ApiKey), "API key should be configured in user secrets");

        // Assert - API key should have reasonable length (OpenAI API keys are typically 51+ characters)
        Assert.True(_config.ApiKey.Length >= 20, $"API key should have meaningful length, but was {_config.ApiKey.Length} characters");

        // Assert - API key should contain alphanumeric characters (basic sanity check)
        Assert.True(_config.ApiKey.Any(char.IsLetterOrDigit), "API key should contain alphanumeric characters");

        // Assert - API key should not be a placeholder value
        Assert.False(_config.ApiKey.Equals("your-api-key-here", StringComparison.OrdinalIgnoreCase), "API key should not be a placeholder value");
        Assert.False(_config.ApiKey.Equals("sk-placeholder", StringComparison.OrdinalIgnoreCase), "API key should not be a placeholder value");

        // Output the first half of the API key (masked)
        if (!string.IsNullOrEmpty(_config.ApiKey))
        {
            int halfLength = _config.ApiKey.Length / 2;
            string halfKey = _config.ApiKey.Substring(0, halfLength);
            _output.WriteLine($"API Key (first half): {halfKey}... (truncated)");
        }
    }

    /// <summary>
    /// Tests audio generation and transcription round trip.
    /// </summary>
    [Fact]
    public async Task AudioRoundTrip_GenerateAndTranscribe_ReturnsOriginalText()
    {
        // Arrange
        var originalText = "The quick brown fox jumps over the lazy dog.";
        var speechRequest = new OpenAiAudioSpeechRequestDto
        {
            Model = "tts-1",
            Input = originalText,
            Voice = "alloy",
            // Explicitly set to the default value ("mp3") for clarity and future-proofing
            ResponseFormat = "mp3"
        };

        // Act - Generate speech
        var audioBytes = await _audioSpeaker.GenerateSpeechAsync(speechRequest);
        Assert.NotEmpty(audioBytes);

        // Act - Transcribe audio
        var transcribeRequest = new OpenAiAudioTranscribeRequestDto
        {
            Model = "whisper-1",
            File = new FileContentDto { Bytes = audioBytes, FileName = "test.mp3" }
        };
        var transcription = await _audioTranscriber.TranscribeAsync(transcribeRequest);

        // Assert
        Assert.NotNull(transcription.Text);
        Assert.Contains("quick brown fox", transcription.Text.ToLowerInvariant());

        // Output the transcription content for inspection
        _output.WriteLine($"Transcription: {transcription.Text}");
    }

    /// <summary>
    /// Tests image generation and analysis round trip.
    /// </summary>
    [Fact]
    public async Task ImageRoundTrip_GenerateAndAnalyze_RecognizesContent()
    {
        // Arrange - Generate two images: one as base64, one as URL
        var prompt1 = "A single red rose in a crystal vase";
        var prompt2 = "A blue sports car on a mountain road";

        var imageRequestBase64 = new OpenAiImageGenerationRequestDto
        {
            Model = "dall-e-3",
            Prompt = prompt1,
            ResponseFormat = "b64_json",
            Size = "1024x1024"
        };
        var imageRequestUrl = new OpenAiImageGenerationRequestDto
        {
            Model = "dall-e-3",
            Prompt = prompt2,
            ResponseFormat = "url",
            Size = "1024x1024"
        };

        // Act - Generate both images
        var imageResponseBase64 = await _imageGenerator.GenerateImageAsync(imageRequestBase64);
        var imageResponseUrl = await _imageGenerator.GenerateImageAsync(imageRequestUrl);
        Assert.NotEmpty(imageResponseBase64.Data);
        Assert.NotEmpty(imageResponseUrl.Data);

        var imageBytes = Convert.FromBase64String(imageResponseBase64.Data[0].B64Json!);
        var imageUrl = imageResponseUrl.Data[0].Url!;

        // Save images to Desktop with UTC ticks in filename
        var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        var ticks = DateTime.UtcNow.Ticks.ToString();
        var base64FileName = $"openai-{ticks}z-base64.png";
        var urlFileName = $"openai-{ticks}z-url.png";
        var base64FilePath = Path.Combine(desktopPath, base64FileName);
        var urlFilePath = Path.Combine(desktopPath, urlFileName);

        await File.WriteAllBytesAsync(base64FilePath, imageBytes);

        // Note: Creating HttpClient directly here is poor design and should be avoided in production code.
        // This is done solely for test/demo purposes in this context.
        using (var httpClient = new HttpClient())
        {
            var urlBytes = await httpClient.GetByteArrayAsync(imageUrl);
            await File.WriteAllBytesAsync(urlFilePath, urlBytes);
        }

        // Output the file paths for inspection
        _output.WriteLine($"Base64 image saved to: {base64FilePath}");
        _output.WriteLine($"URL image saved to: {urlFilePath}");

        // Act - Analyze both images in a single request
        var textPart = OpenAiMultiModalMessageBuilder.CreateTextPart(
            "Describe each image separately. What do you see in the first image (base64) and the second image (URL)? Respond with two distinct answers: 'image1' and 'image2'.");
        var imagePart1 = OpenAiMultiModalMessageBuilder.CreateImageBase64Part(imageBytes, "image/png");
        var imagePart2 = OpenAiMultiModalMessageBuilder.CreateImageUrlPart(imageUrl);
        var analysisMessage = OpenAiMultiModalMessageBuilder.CreateMultiModalMessage("user", textPart, imagePart1, imagePart2);

        var chatRequest = new OpenAiChatCompletionRequestDto
        {
            Model = "gpt-4o",
            Messages = [analysisMessage]
        };

        var analysisResponse = await _chatCompleter.CompleteAsync(chatRequest);

        // Assert
        Assert.NotNull(analysisResponse.Choices[0].Message);
        var content = analysisResponse.Choices[0].Message!.Content as string;
        Assert.NotNull(content);
        Assert.Contains("image1", content!.ToLowerInvariant());
        Assert.Contains("image2", content!.ToLowerInvariant());

        // Output the AI's response for inspection
        _output.WriteLine($"AI analysis response: {content}");
    }

    /// <summary>
    /// Tests embeddings and cosine similarity calculations.
    /// </summary>
    [Fact]
    public async Task Embeddings_WithSemanticallySimilarTexts_RanksCorrectly()
    {
        // Arrange: Ask OpenAI to generate 1 target sentence and 10 comparison sentences, one of which is intentionally similar to the target
        var prompt = "Generate 1 target sentence about a specific topic, and 10 other sentences about different topics. Make sure that exactly one of the 10 sentences is very similar in meaning to the target sentence, while the other 9 are unrelated. Return a JSON object with the following structure: { \"target\": <target sentence>, \"candidates\": [<sentence1>, <sentence2>, ..., <sentence10>], \"most_similar_index\": <index of the most similar sentence in the candidates array> }";
        var chatRequest = new OpenAiChatCompletionRequestDto
        {
            Model = "gpt-4o",
            Messages = [new OpenAiChatMessageDto { Role = "user", Content = prompt }],
            ResponseFormat = new OpenAiResponseFormatDto { Type = "json_object" }
        };
        var chatResponse = await _chatCompleter.CompleteAsync(chatRequest);
        var json = chatResponse.Choices[0].Message!.Content as string;
        Assert.NotNull(json);

        string targetText;
        List<string> candidates;
        int mostSimilarIndex;
        using (var doc = JsonDocument.Parse(json!))
        {
            targetText = doc.RootElement.GetProperty("target").GetString()!;
            candidates = doc.RootElement.GetProperty("candidates").EnumerateArray().Select(e => e.GetString()!).ToList();
            mostSimilarIndex = doc.RootElement.GetProperty("most_similar_index").GetInt32();
        }

        // Act: Get embeddings for all candidates and the target
        List<string> allInputs = new List<string>(candidates);
        allInputs.Add(targetText);
        var embedRequest = new OpenAiEmbeddingRequestDto
        {
            Model = "text-embedding-3-large",
            Input = allInputs
        };
        var embedResponse = await _embedder.CreateEmbeddingsAsync(embedRequest);
        var targetVector = embedResponse.Data[^1].Embedding;
        var similarities = new List<(string Text, int Index, double Similarity)>();
        for (int i = 0; i < candidates.Count; i++)
        {
            var arr1 = targetVector.ToArray();
            var arr2 = embedResponse.Data[i].Embedding.ToArray();
            var similarity = TensorPrimitives.CosineSimilarity<double>(arr1, arr2);
            similarities.Add((candidates[i], i, similarity));
        }

        // Sort by similarity (descending)
        List<(string Text, int Index, double Similarity)> sorted = similarities.OrderByDescending(x => x.Similarity).ToList();

        // Output the similarity table before assertions
        _output.WriteLine($"Target: {targetText}");
        _output.WriteLine("Similarity Table (sorted):");
        for (int i = 0; i < sorted.Count; i++)
        {
            _output.WriteLine($"Index: {sorted[i].Index}, Text: '{sorted[i].Text}', Similarity: {sorted[i].Similarity}");
        }

        // Assert: The most similar candidate (highest similarity) should be the one OpenAI marked as most similar
        int topIndex = sorted[0].Index;
        Assert.Equal(mostSimilarIndex, topIndex);
    }

    /// <summary>
    /// Tests streaming chat completion.
    /// </summary>
    [Fact]
    public async Task StreamingChat_WithSimpleMessage_ReturnsCompleteResponse()
    {
        // Arrange
        var request = new OpenAiChatCompletionRequestDto
        {
            Model = "gpt-4o",
            Messages = [new OpenAiChatMessageDto { Role = "user", Content = "Tell me a very short story in exactly 3 sentences." }],
            Stream = true
        };

        // Act
        var responseBuilder = new StringBuilder();
        await foreach (var chunk in _chatCompleter.CompleteStreamAsync(request))
        {
            if (chunk.Choices?.Count > 0 && chunk.Choices[0].Delta?.Content != null)
            {
                responseBuilder.Append(chunk.Choices[0].Delta!.Content!);
            }
        }

        // Assert
        var fullResponse = responseBuilder.ToString();
        Assert.NotEmpty(fullResponse);
        Assert.True(fullResponse.Length > 10); // Should be a meaningful response

        _output.WriteLine($"Full streaming response: {fullResponse}");
    }

    /// <summary>
    /// Tests tool calling functionality with a simple weather function.
    /// </summary>
    [Fact]
    public async Task ToolCalling_WithWeatherFunction_ExecutesAndReturnsResult()
    {
        // Arrange - Register weather tool with the existing orchestrator
        var functionDef = OpenAiToolDefinitionBuilder.CreateFromMethod(typeof(OpenAiIntegrationTests).GetMethod(nameof(GetWeather))!);

        // Create a new orchestrator with a fresh tool handler for this test
        var toolCallHandler = new OpenAiToolCallHandler(new NullLogger<OpenAiToolCallHandler>());
        toolCallHandler.RegisterTool<WeatherArgs>("GetWeather", GetWeather, functionDef);
        var orchestrator = new OpenAiToolCallOrchestrator(new NullLogger<OpenAiToolCallOrchestrator>(), _chatCompleter, toolCallHandler);

        var toolDto = new OpenAiToolDto
        {
            Type = "function",
            Function = functionDef
        };

        // Maintain a conversation history suitable for continuing with the AI
        var conversationHistory = new List<object>();

        // User message
        var userMessage = new
        {
            role = "user",
            content = "What's the weather like in Tokyo?"
        };
        conversationHistory.Add(userMessage);

        var request = new OpenAiChatCompletionRequestDto
        {
            Model = "gpt-4o",
            Messages = [new OpenAiChatMessageDto { Role = "user", Content = "What's the weather like in Tokyo?" }],
            Tools = [toolDto]
        };

        // Act
        var response = await orchestrator.CompleteWithToolsAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.NotEmpty(response.Choices);
        Assert.NotNull(response.Choices[0].Message);

        // Add assistant/tool messages to conversation history
        // If the response contains tool calls, add them as tool messages
        var assistantMessage = response.Choices[0].Message;
        if (assistantMessage != null && assistantMessage.ToolCalls != null && assistantMessage.ToolCalls.Count > 0)
        {
            foreach (var toolCall in assistantMessage.ToolCalls)
            {
                conversationHistory.Add(new
                {
                    role = "assistant",
                    content = null as string,
                    tool_calls = new[]
                    {
                        new {
                            id = toolCall.Id,
                            type = toolCall.Type,
                            function = toolCall.Function
                        }
                    }
                });

                // Add the tool response as a tool message
                conversationHistory.Add(new
                {
                    role = "tool",
                    tool_call_id = toolCall.Id,
                    name = toolCall.Function?.Name,
                    content = JsonSerializer.Serialize(GetWeather(new WeatherArgs { Location = "Tokyo" }))
                });
            }
        }

        // Add the final assistant message
        var finalMessage = response.Choices[0].Message!.Content as string;
        Assert.NotNull(finalMessage);
        Assert.Contains("Tokyo", finalMessage!);
        Assert.Contains("22°C", finalMessage!);

        conversationHistory.Add(new
        {
            role = "assistant",
            content = finalMessage
        });

        // Output the final message for inspection
        _output.WriteLine($"Final message: {finalMessage}");

        // Save the conversation history as indented JSON on the desktop
        var conversationJson = JsonSerializer.Serialize(conversationHistory, new JsonSerializerOptions { WriteIndented = true });
        var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        var ticks = DateTime.UtcNow.Ticks.ToString();
        var jsonFileName = $"openai-{ticks}z-tools.json";
        var jsonFilePath = Path.Combine(desktopPath, jsonFileName);
        await File.WriteAllTextAsync(jsonFilePath, conversationJson);
        _output.WriteLine($"Conversation JSON saved to: {jsonFilePath}");
    }

    /// <summary>
    /// Mock weather function for tool calling tests.
    /// </summary>
    [Description("Gets the current weather for a specified location.")]
    public static object GetWeather(WeatherArgs args)
    {
        return new { temperature = "22°C", condition = "Sunny", location = args.Location };
    }

    /// <summary>
    /// Arguments for the weather tool function.
    /// </summary>
    public class WeatherArgs
    {
        public string Location { get; set; } = string.Empty;
    }

    /// <summary>
    /// Tests multi-modal chat with text, image, audio, file inputs and JSON response format.
    /// This is the "kitchen sink" test that exercises multiple capabilities in one request.
    /// </summary>
    [Fact]
    public async Task MultiModalChat_WithAllInputTypes_ReturnsStructuredResponse()
    {
        // Arrange - Create test assets
        var testImageBytes = CreateTestImageBytes();
        var testAudioBytes = CreateTestAudioBytes();
        var testFileContent = "Test analysis data: User engagement increased by 15% this quarter.";

        // Upload test file
        var tempFile = Path.GetTempFileName();
        await File.WriteAllTextAsync(tempFile, testFileContent);

        var uploadRequest = new OpenAiFileUploadRequestDto
        {
            File = new FilePathReferenceDto { FilePath = tempFile },
            Purpose = "vision"
        };
        var uploadedFile = await _fileManager.UploadAsync(uploadRequest);
        File.Delete(tempFile);

        try
        {
            // Build multi-modal message using static methods
            var textPart = OpenAiMultiModalMessageBuilder.CreateTextPart("Analyze the provided image, audio, and file. Return your findings as JSON with 'image_analysis', 'audio_transcription', and 'file_summary' fields.");
            var imagePart = OpenAiMultiModalMessageBuilder.CreateImageBase64Part(testImageBytes, "image/png");
            var audioPart = OpenAiMultiModalMessageBuilder.CreateAudioInputPart(testAudioBytes, "mp3");
            var filePart = OpenAiMultiModalMessageBuilder.CreateFilePart(uploadedFile.Id);

            var message = OpenAiMultiModalMessageBuilder.CreateMultiModalMessage("user", textPart, imagePart, audioPart, filePart);

            var request = new OpenAiChatCompletionRequestDto
            {
                Model = "gpt-4o",
                Messages = [message],
                ResponseFormat = new OpenAiResponseFormatDto { Type = "json_object" }
            };

            // Act
            var response = await _chatCompleter.CompleteAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.NotEmpty(response.Choices);
            Assert.NotNull(response.Choices[0].Message);

            var content = response.Choices[0].Message!.Content as string;
            Assert.NotNull(content);

            // Verify it's valid JSON
            var jsonDoc = JsonDocument.Parse(content!);
            Assert.True(jsonDoc.RootElement.TryGetProperty("image_analysis", out _));
            Assert.True(jsonDoc.RootElement.TryGetProperty("audio_transcription", out _));
            Assert.True(jsonDoc.RootElement.TryGetProperty("file_summary", out _));
        }
        finally
        {
            // Cleanup
            await _fileManager.DeleteAsync(uploadedFile.Id);
        }
    }

    /// <summary>
    /// Tests complete file management CRUD lifecycle.
    /// </summary>
    [Fact]
    public async Task FileManagement_FullCrudLifecycle_WorksCorrectly()
    {
        var testContent = "Test file content for CRUD operations.";
        var tempFile = Path.GetTempFileName();

        try
        {
            // Create
            await File.WriteAllTextAsync(tempFile, testContent);
            var uploadRequest = new OpenAiFileUploadRequestDto
            {
                File = new FilePathReferenceDto { FilePath = tempFile },
                Purpose = "vision"
            };
            var uploadedFile = await _fileManager.UploadAsync(uploadRequest);
            Assert.NotNull(uploadedFile.Id);

            // Read - Retrieve
            var retrievedFile = await _fileManager.RetrieveAsync(uploadedFile.Id);
            Assert.Equal(uploadedFile.Id, retrievedFile.Id);

            // Read - List
            var fileList = await _fileManager.ListAsync();
            Assert.Contains(fileList.Data, f => f.Id == uploadedFile.Id);

            // Read - Download
            var downloadedBytes = await _fileManager.DownloadContentAsync(uploadedFile.Id);
            var downloadedContent = Encoding.UTF8.GetString(downloadedBytes);
            Assert.Equal(testContent, downloadedContent);

            // Delete
            var deleteResponse = await _fileManager.DeleteAsync(uploadedFile.Id);
            Assert.True(deleteResponse.Deleted);

            // Verify deletion
            await Assert.ThrowsAsync<Exception>(() => _fileManager.RetrieveAsync(uploadedFile.Id));
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    /// Creates test image bytes (simple PNG).
    /// </summary>
    private static byte[] CreateTestImageBytes()
    {
        // Simple 1x1 PNG - minimal valid PNG file
        return Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8/5+hHgAHggJ/PchI7wAAAABJRU5ErkJggg==");
    }

    /// <summary>
    /// Creates test audio bytes (minimal MP3).
    /// </summary>
    private static byte[] CreateTestAudioBytes()
    {
        // Minimal MP3 header - this won't play but will be accepted by the API
        return new byte[] { 0xFF, 0xFB, 0x90, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
    }


}
