using System.Text;
using pawKitLib.Ai.OpenAi.Models;
using pawKitLib.Models;
using pawKitLib.Conversion;

namespace pawKitLib.Ai.OpenAi.Services;

/// <summary>
/// Helper service for building multi-modal chat messages with text, images, audio, and files.
/// Provides convenient methods to construct complex message content without manual DTO manipulation.
/// </summary>
public static class OpenAiMultiModalMessageBuilder
{
    /// <summary>
    /// Creates a simple text-only message.
    /// </summary>
    public static OpenAiChatMessageDto CreateTextMessage(string role, string text)
    {
        return new OpenAiChatMessageDto
        {
            Role = role,
            Content = text
        };
    }

    /// <summary>
    /// Creates a multi-modal message with multiple content parts.
    /// </summary>
    public static OpenAiChatMessageDto CreateMultiModalMessage(string role, params OpenAiChatMessageContentPartDto[] parts)
    {
        return new OpenAiChatMessageDto
        {
            Role = role,
            Content = parts.ToList()
        };
    }

    /// <summary>
    /// Creates a text content part for multi-modal messages.
    /// </summary>
    public static OpenAiChatMessageContentPartDto CreateTextPart(string text)
    {
        return new OpenAiChatMessageContentPartDto
        {
            Type = "text",
            Text = text
        };
    }

    /// <summary>
    /// Creates an image content part from a URL with optional detail level.
    /// </summary>
    public static OpenAiChatMessageContentPartDto CreateImageUrlPart(string imageUrl, string? detail = null)
    {
        return new OpenAiChatMessageContentPartDto
        {
            Type = "image_url",
            ImageUrl = new OpenAiImageUrlDto
            {
                Url = imageUrl,
                Detail = detail
            }
        };
    }

    /// <summary>
    /// Creates an image content part from base64-encoded image data with optional detail level.
    /// </summary>
    public static OpenAiChatMessageContentPartDto CreateImageBase64Part(byte[] imageBytes, string mimeType, string? detail = null)
    {
        var base64Data = Convert.ToBase64String(imageBytes);
        var dataUrl = $"data:{mimeType};base64,{base64Data}";

        return new OpenAiChatMessageContentPartDto
        {
            Type = "image_url",
            ImageUrl = new OpenAiImageUrlDto
            {
                Url = dataUrl,
                Detail = detail
            }
        };
    }

    /// <summary>
    /// Creates an image content part from a FileContentDto with optional detail level.
    /// </summary>
    public static OpenAiChatMessageContentPartDto CreateImageFromFileContent(FileContentDto file, string? detail = null)
    {
        // Use MimeTypeHelper to determine MIME type from file extension
        var mimeType = MimeTypeHelper.GetMimeType(file.FileName, fallbackToDefault: true)!;
        return CreateImageBase64Part(file.Bytes, mimeType, detail);
    }

    /// <summary>
    /// Creates an audio input content part from base64-encoded audio data.
    /// </summary>
    public static OpenAiChatMessageContentPartDto CreateAudioInputPart(byte[] audioBytes, string format)
    {
        var base64Data = Convert.ToBase64String(audioBytes);

        return new OpenAiChatMessageContentPartDto
        {
            Type = "input_audio",
            InputAudio = new OpenAiInputAudioDto
            {
                Data = base64Data,
                Format = format
            }
        };
    }

    /// <summary>
    /// Creates an audio input content part from a FileContentDto.
    /// </summary>
    public static OpenAiChatMessageContentPartDto CreateAudioFromFileContent(FileContentDto file, string format)
    {
        return CreateAudioInputPart(file.Bytes, format);
    }

    /// <summary>
    /// Creates a file reference content part using an uploaded file ID.
    /// </summary>
    public static OpenAiChatMessageContentPartDto CreateFilePart(string fileId)
    {
        return new OpenAiChatMessageContentPartDto
        {
            Type = "file",
            File = new OpenAiFileDto
            {
                Id = fileId
            }
        };
    }

    /// <summary>
    /// Creates a user message with text and an image from URL.
    /// </summary>
    public static OpenAiChatMessageDto CreateUserMessageWithImage(string text, string imageUrl, string? detail = null)
    {
        return CreateMultiModalMessage("user",
            CreateTextPart(text),
            CreateImageUrlPart(imageUrl, detail)
        );
    }

    /// <summary>
    /// Creates a user message with text and an image from file content.
    /// </summary>
    public static OpenAiChatMessageDto CreateUserMessageWithImageFile(string text, FileContentDto imageFile, string? detail = null)
    {
        return CreateMultiModalMessage("user",
            CreateTextPart(text),
            CreateImageFromFileContent(imageFile, detail)
        );
    }

    /// <summary>
    /// Creates a user message with text and audio input.
    /// </summary>
    public static OpenAiChatMessageDto CreateUserMessageWithAudio(string text, FileContentDto audioFile, string format)
    {
        return CreateMultiModalMessage("user",
            CreateTextPart(text),
            CreateAudioFromFileContent(audioFile, format)
        );
    }

    /// <summary>
    /// Creates a user message with text and a file reference.
    /// </summary>
    public static OpenAiChatMessageDto CreateUserMessageWithFile(string text, string fileId)
    {
        return CreateMultiModalMessage("user",
            CreateTextPart(text),
            CreateFilePart(fileId)
        );
    }
}
