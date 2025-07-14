namespace pawKitLib.Ai;

/// <summary>
/// Enumerates supported AI capabilities across providers.
/// </summary>
// Note: This enum is not currently linked to any service selection logic or abstraction within the codebase.
// It serves as a personal reference for cataloging and classifying potential AI capabilities that may be implemented.
// Type names elsewhere in the code are not necessarily consistent with the enum item names.
// This list is maintained to help avoid confusion and ensure clarity when considering different AI features.
// No direct mapping or dependency exists between this enum and other types.
// If, in the future, we implement many capabilities for multiple AI providers, we might consider introducing
// abstraction layers where this enum could become useful. This is not contrary to YAGNI: as an enum, I do not
// strictly need it now, but as a curated list of potential capabilities requiring implementation, it is already
// valuable. Its presence does not cause harm, as it is not merely a text file but a structured reference.
public enum AiCapability
{
    // Core Language & Text Features

    /// <summary>
    /// Multi-turn conversational interaction with an AI assistant
    /// </summary>
    Chat,
    /// <summary>
    /// Single-turn or prompt-based text generation (e.g., autocomplete, summarization)
    /// </summary>
    TextCompletion,
    /// <summary>
    /// Convert text into high-dimensional vector embeddings for similarity search, clustering, etc.
    /// </summary>
    TextEmbedding,
    /// <summary>
    /// Customize a base model by training it further on domain-specific text or code
    /// </summary>
    TextFineTuning,

    // Visual Features

    /// <summary>
    /// Generate an image from a text prompt
    /// </summary>
    ImageGeneration,
    /// <summary>
    /// Modify or inpaint parts of an existing image using text prompts
    /// </summary>
    ImageEditing,
    /// <summary>
    /// Accept images as input to reason about or describe their content
    /// </summary>
    VisualUnderstanding,
    /// <summary>
    /// Fine-tune a model using paired image + text datasets
    /// </summary>
    VisualFineTuning,

    // Audio Features

    /// <summary>
    /// Transcribe spoken audio into written text
    /// </summary>
    SpeechToText,
    /// <summary>
    /// Translate audio from one language to another during or after transcription
    /// </summary>
    SpeechTranslation,
    /// <summary>
    /// Generate natural-sounding audio from text (TTS)
    /// </summary>
    TextToSpeech,

    // Core AI Capabilities

    /// <summary>
    /// Allow the model to call structured functions/tools based on user intent
    /// </summary>
    ToolCalling,
    /// <summary>
    /// Detect and classify unsafe, biased, or harmful content in text or images
    /// </summary>
    ContentModeration,

    // Infrastructure & Advanced Access

    /// <summary>
    /// Submit large batches of inputs for asynchronous processing via job APIs
    /// </summary>
    AsyncBatchProcessing,
    /// <summary>
    /// Get live or streaming responses via WebSocket or similar mechanisms
    /// </summary>
    RealtimeStreaming
}
