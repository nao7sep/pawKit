using System.ComponentModel;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using pawKitLib.Ai.OpenAi.Models;
using pawKitLib.Ai.OpenAi.Services;
using pawKitLib.Models;

namespace pawKitLib.Tests.Ai.OpenAi.Services;

/// <summary>
/// Integration tests for OpenAI services. These tests require valid OpenAI API credentials
/// and will make actual API calls. They test real-world scenarios and API compatibility.
/// </summary>
public class OpenAiIntegrationTests
{
    private readonly OpenAiConfigDto _config;
    private readonly OpenAiChatCompleter _chatCompleter;
    private readonly OpenAiFileManager _fileManager;
    private readonly OpenAiAudioSpeaker _audioSpeaker;
    private readonly OpenAiAudioTranscriber _audioTranscriber;
    private readonly OpenAiImageGenerator _imageGenerator;
    private readonly OpenAiEmbedder _embedder;
    private readonly OpenAiToolCallOrchestrator _toolOrchestrator;

    public OpenAiIntegrationTests()
    {
        // Initialize configuration with user secrets (same as console app)
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<OpenAiIntegrationTests>()
            .Build();

        // Bind configuration to OpenAiConfigDto
        _config = new OpenAiConfigDto();
        configuration.GetSection("pawKit:Ai:OpenAi:Config").Bind(_config);

        // Fallback to environment variable if not in user secrets
        if (string.IsNullOrEmpty(_config.ApiKey))
        {
            _config.ApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? string.Empty;
        }

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
    /// Tests multi-modal chat with text, image, audio, file inputs and JSON response format.
    /// This is the "kitchen sink" test that exercises multiple capabilities in one request.
    /// </summary>
    [Fact]
    public async Task MultiModalChat_WithAllInputTypes_ReturnsStructuredResponse()
    {
        // Skip if no API key
        if (string.IsNullOrEmpty(_config.ApiKey))
        {
            return;
        }

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
    /// Tests tool calling functionality with a simple weather function.
    /// </summary>
    [Fact]
    public async Task ToolCalling_WithWeatherFunction_ExecutesAndReturnsResult()
    {
        if (string.IsNullOrEmpty(_config.ApiKey))
        {
            return;
        }

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

        var finalMessage = response.Choices[0].Message!.Content as string;
        Assert.NotNull(finalMessage);
        Assert.Contains("Tokyo", finalMessage!);
        Assert.Contains("22°C", finalMessage!);
    }

    /// <summary>
    /// Tests streaming chat completion.
    /// </summary>
    [Fact]
    public async Task StreamingChat_WithSimpleMessage_ReturnsCompleteResponse()
    {
        if (string.IsNullOrEmpty(_config.ApiKey))
        {
            return;
        }

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
    }

    /// <summary>
    /// Tests audio generation and transcription round trip.
    /// </summary>
    [Fact]
    public async Task AudioRoundTrip_GenerateAndTranscribe_ReturnsOriginalText()
    {
        if (string.IsNullOrEmpty(_config.ApiKey))
        {
            return;
        }

        // Arrange
        var originalText = "The quick brown fox jumps over the lazy dog.";
        var speechRequest = new OpenAiAudioSpeechRequestDto
        {
            Model = "tts-1",
            Input = originalText,
            Voice = "alloy"
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
    }

    /// <summary>
    /// Tests image generation and analysis round trip.
    /// </summary>
    [Fact]
    public async Task ImageRoundTrip_GenerateAndAnalyze_RecognizesContent()
    {
        if (string.IsNullOrEmpty(_config.ApiKey))
        {
            return;
        }

        // Arrange - Generate image
        var imageRequest = new OpenAiImageGenerationRequestDto
        {
            Model = "dall-e-3",
            Prompt = "A single red rose in a crystal vase",
            ResponseFormat = "b64_json",
            Size = "1024x1024"
        };

        // Act - Generate image
        var imageResponse = await _imageGenerator.GenerateImageAsync(imageRequest);
        Assert.NotEmpty(imageResponse.Data);

        var imageBytes = Convert.FromBase64String(imageResponse.Data[0].B64Json!);

        // Act - Analyze image
        var textPart = OpenAiMultiModalMessageBuilder.CreateTextPart("What flower is shown in this image?");
        var imagePart = OpenAiMultiModalMessageBuilder.CreateImageBase64Part(imageBytes, "image/png");
        var analysisMessage = OpenAiMultiModalMessageBuilder.CreateMultiModalMessage("user", textPart, imagePart);

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
        Assert.Contains("rose", content!.ToLowerInvariant());
    }

    /// <summary>
    /// Tests complete file management CRUD lifecycle.
    /// </summary>
    [Fact]
    public async Task FileManagement_FullCrudLifecycle_WorksCorrectly()
    {
        if (string.IsNullOrEmpty(_config.ApiKey))
        {
            return;
        }

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
    /// Tests embeddings and cosine similarity calculations.
    /// </summary>
    [Fact]
    public async Task Embeddings_WithSemanticallySimilarTexts_RanksCorrectly()
    {
        if (string.IsNullOrEmpty(_config.ApiKey))
        {
            return;
        }

        // Arrange
        var texts = new List<string>
        {
            "The cat sat lazily on the mat.",
            "A feline was resting on the rug.",
            "The dog played fetch in the park.",
            "The New York Stock Exchange is open."
        };
        var targetText = "My kitty is napping on the floor.";

        // Act - Get embeddings
        var textsRequest = new OpenAiEmbeddingRequestDto
        {
            Model = "text-embedding-3-small",
            Input = texts
        };
        var textsResponse = await _embedder.CreateEmbeddingsAsync(textsRequest);

        var targetRequest = new OpenAiEmbeddingRequestDto
        {
            Model = "text-embedding-3-small",
            Input = new List<string> { targetText }
        };
        var targetResponse = await _embedder.CreateEmbeddingsAsync(targetRequest);

        // Calculate similarities
        var targetVector = targetResponse.Data[0].Embedding;
        var similarities = new List<(string Text, double Similarity)>();

        for (int i = 0; i < texts.Count; i++)
        {
            var similarity = CosineSimilarity(targetVector, textsResponse.Data[i].Embedding);
            similarities.Add((texts[i], similarity));
        }

        // Sort by similarity
        var sortedTexts = similarities.OrderByDescending(x => x.Similarity).Select(x => x.Text).ToList();

        // Assert - Just verify that embeddings work and produce reasonable similarity rankings
        // The cat-related texts should be more similar to the target than the unrelated text
        var nycIndex = sortedTexts.IndexOf("The New York Stock Exchange is open.");
        var catIndex = sortedTexts.IndexOf("The cat sat lazily on the mat.");
        var felineIndex = sortedTexts.IndexOf("A feline was resting on the rug.");

        // Cat-related texts should rank higher (lower index) than the unrelated NYC text
        Assert.True(catIndex < nycIndex, "Cat text should be more similar than NYC text");
        Assert.True(felineIndex < nycIndex, "Feline text should be more similar than NYC text");
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

    /// <summary>
    /// Calculates cosine similarity between two vectors.
    /// </summary>
    private static double CosineSimilarity(List<double> v1, List<double> v2)
    {
        var dotProduct = v1.Zip(v2, (a, b) => a * b).Sum();
        var magnitude1 = Math.Sqrt(v1.Sum(x => x * x));
        var magnitude2 = Math.Sqrt(v2.Sum(x => x * x));
        return dotProduct / (magnitude1 * magnitude2);
    }

    /// <summary>
    /// Arguments for the weather tool function.
    /// </summary>
    public class WeatherArgs
    {
        public string Location { get; set; } = string.Empty;
    }
}