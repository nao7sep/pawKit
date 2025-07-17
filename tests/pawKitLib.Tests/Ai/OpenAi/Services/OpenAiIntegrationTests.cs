using System.ComponentModel;
using System.Numerics.Tensors;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using pawKitLib.Ai;
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

        // Output the final message for inspection
        _output.WriteLine($"Final message: {finalMessage}");
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
    /// Tests complete file management CRUD lifecycle.
    /// </summary>
    [Fact]
    public async Task FileManagement_FullCrudLifecycle_WorksCorrectly()
    {
        // OpenAI API supports several file formats for upload, including .jsonl (JSON Lines), which is required for fine-tune files.
        // Each line in a .jsonl file must be a valid JSON object.
        var testContent = "{\"prompt\":\"Say this is a test!\",\"completion\":\"This is a test.\"}";
        var tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".jsonl");

        try
        {
            // Create
            await File.WriteAllTextAsync(tempFile, testContent);
            var uploadRequest = new OpenAiFileUploadRequestDto
            {
                File = new FilePathReferenceDto { FilePath = tempFile },
                // We tried different extensions, file contents, and Purpose values (e.g., "vision", "assistants").
                // As of 2025-07, "fine-tune" just happened to be the first Purpose that passed for this test.
                // This is solely for testing file CRUD, so any working purpose is acceptable.
                Purpose = "fine-tune"
            };
            var uploadedFile = await _fileManager.UploadAsync(uploadRequest);
            Assert.NotNull(uploadedFile.Id);

            // Read - Retrieve
            var retrievedFile = await _fileManager.RetrieveAsync(uploadedFile.Id);
            Assert.Equal(uploadedFile.Id, retrievedFile.Id);

            // Read - List
            var fileList = await _fileManager.ListAsync();
            _output.WriteLine($"File list ({fileList.Data.Count}):");
            foreach (var f in fileList.Data)
            {
                // We've observed that .txt files with the "assistants" purpose also upload successfully and appear in the file list.
                // Currently, this test uses "fine-tune" and sends a .jsonl file, but other extensions and purposes (such as .txt with "assistants") may also work.
                _output.WriteLine($"  Id: {f.Id}, Filename: {f.Filename}, Purpose: {f.Purpose}, Status: {f.Status}");

                // Delete all files in the list (for cleanup/demo purposes)
                // This is solely for convenience, as there is currently no official page or interface to delete uploaded files.
                // By setting this to true, we will indiscriminately attempt to delete all files, which may cause the rest of the code to break and the test to fail.
                if (false)
                {
                    _output.WriteLine($"  Deleting file: {f.Id}");
                    await _fileManager.DeleteAsync(f.Id);
                }
            }
            Assert.Contains(fileList.Data, f => f.Id == uploadedFile.Id);

            // Read - Download
            var downloadedBytes = await _fileManager.DownloadContentAsync(uploadedFile.Id);
            var downloadedContent = Encoding.UTF8.GetString(downloadedBytes);
            Assert.Equal(testContent, downloadedContent);

            // Delete
            var deleteResponse = await _fileManager.DeleteAsync(uploadedFile.Id);
            Assert.True(deleteResponse.Deleted);

            // Verify deletion
            // The catch-all mechanism (e.g., using Exception or a base type) did not work here; by changing the type to AiServiceException, the test started passing.
            await Assert.ThrowsAsync<AiServiceException>(() => _fileManager.RetrieveAsync(uploadedFile.Id));
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    /// Multi-modal test: generates related image and audio content, uses gpt-4o for image analysis,
    /// gpt-4o-audio-preview for audio processing, then generates a response image based on the answer.
    /// Note: This test has been simplified to a straightforward question and answer flow due to frequent
    /// policy violation errors encountered by OpenAI when the previous version involved risky situations
    /// and their predictable outcomes as comedic content.
    /// </summary>
    [Fact]
    public async Task MultiModalChat_QuestionAnswer_InteractionTest()
    {
        // Generate a creative image prompt and speech text using the AI
        var creativePrompt = "Generate a JSON object with two fields: 'image_prompt' and 'speech_text'. 'image_prompt' should describe a specific, detailed scene that could be interpreted in multiple creative ways (like a person looking at something, holding an object, or in a particular setting). 'speech_text' should be an intriguing, imaginative statement or question that could lead to a completely different visual interpretation - like 'What if this was actually...', 'Imagine if instead...', or 'This reminds me of...' The audio should serve as a creative bridge to transform the first image concept into something entirely different but thematically connected.";
        var genRequest = new OpenAiChatCompletionRequestDto
        {
            Model = "gpt-4o",
            Messages = [new OpenAiChatMessageDto { Role = "user", Content = creativePrompt }],
            ResponseFormat = new OpenAiResponseFormatDto { Type = "json_object" }
        };
        var genResponse = await _chatCompleter.CompleteAsync(genRequest);
        var genJson = genResponse.Choices[0].Message!.Content as string;
        Assert.NotNull(genJson);
        _output.WriteLine($"Scene/Question JSON: {genJson}");

        string imagePrompt, questionText;
        using (var doc = JsonDocument.Parse(genJson!))
        {
            imagePrompt = doc.RootElement.GetProperty("image_prompt").GetString()!;
            questionText = doc.RootElement.GetProperty("speech_text").GetString()!;
        }

        // Generate the scene image
        var imageRequest = new OpenAiImageGenerationRequestDto
        {
            Model = "dall-e-3",
            Prompt = imagePrompt,
            ResponseFormat = "b64_json",
            Size = "1024x1024"
        };
        var imageResponse = await _imageGenerator.GenerateImageAsync(imageRequest);
        Assert.NotEmpty(imageResponse.Data);
        var imageBytes = Convert.FromBase64String(imageResponse.Data[0].B64Json!);

        // Generate the question audio
        var speechRequest = new OpenAiAudioSpeechRequestDto
        {
            Model = "tts-1",
            Input = questionText,
            Voice = "alloy",
            ResponseFormat = "mp3"
        };
        var audioBytes = await _audioSpeaker.GenerateSpeechAsync(speechRequest);
        Assert.NotEmpty(audioBytes);

        // Save the initial files to Desktop
        var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        var ticks = DateTime.UtcNow.Ticks.ToString();
        var imageFileName = $"openai-qa-{ticks}-scene.png";
        var audioFileName = $"openai-qa-{ticks}-question.mp3";
        var imageFilePath = Path.Combine(desktopPath, imageFileName);
        var audioFilePath = Path.Combine(desktopPath, audioFileName);
        await File.WriteAllBytesAsync(imageFilePath, imageBytes);
        await File.WriteAllBytesAsync(audioFilePath, audioBytes);
        _output.WriteLine($"Scene image saved to: {imageFilePath}");
        _output.WriteLine($"Question audio saved to: {audioFilePath}");

        // Use gpt-4o to analyze the image
        var imageAnalysisTextPart = OpenAiMultiModalMessageBuilder.CreateTextPart(
            "Analyze this image in detail. Focus on the key elements, composition, mood, and any symbolic or metaphorical potential. What story does this image tell? What emotions or ideas does it evoke?");
        var imagePart = OpenAiMultiModalMessageBuilder.CreateImageBase64Part(imageBytes, "image/png");
        var imageAnalysisMessage = OpenAiMultiModalMessageBuilder.CreateMultiModalMessage("user", imageAnalysisTextPart, imagePart);

        var imageAnalysisRequest = new OpenAiChatCompletionRequestDto
        {
            // gpt-4o is a multi-modal model with advanced vision capabilities, but it does not support audio input or output as of now.
            Model = "gpt-4o",
            Messages = [imageAnalysisMessage]
        };

        var imageAnalysisResponse = await _chatCompleter.CompleteAsync(imageAnalysisRequest);
        var imageAnalysis = imageAnalysisResponse.Choices[0].Message!.Content as string;
        Assert.NotNull(imageAnalysis);
        _output.WriteLine($"Image analysis: {imageAnalysis}");

        // Use gpt-4o-audio-preview with both image analysis and audio to create a creative transformation
        var promptTextPart = OpenAiMultiModalMessageBuilder.CreateTextPart(
            $"Based on this image analysis: '{imageAnalysis}', and the creative prompt you'll hear in the audio, create a detailed description for a completely different but thematically connected scene. The audio will suggest a creative transformation or reinterpretation. Describe this new scene vividly, focusing on how it relates to the original while being visually distinct. Be imaginative and creative in your transformation.");
        var audioPart = OpenAiMultiModalMessageBuilder.CreateAudioInputPart(audioBytes, "mp3");
        var questionAnswerMessage = OpenAiMultiModalMessageBuilder.CreateMultiModalMessage("user", promptTextPart, audioPart);

        var questionAnswerRequest = new OpenAiChatCompletionRequestDto
        {
            // gpt-4o-audio-preview does not support image input. Note: the 'modalities' parameter can be misleading—
            // if you include "audio" in the list, the model expects to generate audio output, not just accept audio input.
            // It is not a declaration of supported input types. If "audio" is specified, you must provide the required audio fields for generation.
            // If 'modalities' is omitted, the model defaults to text responses.
            Model = "gpt-4o-audio-preview",
            Messages = [questionAnswerMessage]
        };

        var questionAnswerResponse = await _chatCompleter.CompleteAsync(questionAnswerRequest);
        var answer = questionAnswerResponse.Choices[0].Message!.Content as string;
        Assert.NotNull(answer);
        _output.WriteLine($"Answer: {answer}");

        // Generate response image based on the creative transformation
        var responseImageRequest = new OpenAiImageGenerationRequestDto
        {
            Model = "dall-e-3",
            Prompt = $"Create a detailed, visually striking image based on this description: {answer}. Make it artistic and engaging, with rich visual details that clearly show the creative transformation from the original concept.",
            ResponseFormat = "b64_json",
            Size = "1024x1024"
        };
        var responseImageResponse = await _imageGenerator.GenerateImageAsync(responseImageRequest);
        Assert.NotEmpty(responseImageResponse.Data);
        var responseImageBytes = Convert.FromBase64String(responseImageResponse.Data[0].B64Json!);

        // Save the response image
        var responseImageFileName = $"openai-qa-{ticks}-response.png";
        var responseImageFilePath = Path.Combine(desktopPath, responseImageFileName);
        await File.WriteAllBytesAsync(responseImageFilePath, responseImageBytes);
        _output.WriteLine($"Response image saved to: {responseImageFilePath}");

        // Save a markdown report summarizing the test
        var reportFileName = $"openai-qa-{ticks}-report.md";
        var reportFilePath = Path.Combine(desktopPath, reportFileName);
        var reportContent = new StringBuilder();
        reportContent.AppendLine("# OpenAI Question & Answer Interaction Test Report");
        reportContent.AppendLine();
        reportContent.AppendLine("## Initial Setup");
        reportContent.AppendLine();
        reportContent.AppendLine("**Generation Prompt:**");
        reportContent.AppendLine(creativePrompt);
        reportContent.AppendLine();
        reportContent.AppendLine("**Scene Description:**");
        reportContent.AppendLine(imagePrompt);
        reportContent.AppendLine();
        reportContent.AppendLine("**Creative Transformation Prompt:**");
        reportContent.AppendLine(questionText);
        reportContent.AppendLine();
        reportContent.AppendLine("## AI Analysis and Creative Process");
        reportContent.AppendLine();
        reportContent.AppendLine("**Original Image Analysis:**");
        reportContent.AppendLine(imageAnalysis);
        reportContent.AppendLine();
        reportContent.AppendLine("**Creative Transformation Description:**");
        reportContent.AppendLine(answer);
        reportContent.AppendLine();
        reportContent.AppendLine("## Generated Files");
        reportContent.AppendLine();
        reportContent.AppendLine($"- Scene image: `{imageFilePath}`");
        reportContent.AppendLine($"- Transformation prompt audio: `{audioFilePath}`");
        reportContent.AppendLine($"- Transformed scene image: `{responseImageFilePath}`");
        await File.WriteAllTextAsync(reportFilePath, reportContent.ToString());
        _output.WriteLine($"Markdown report saved to: {reportFilePath}");
    }
}
