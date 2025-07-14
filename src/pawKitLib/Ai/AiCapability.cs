namespace pawKitLib.Ai;

/// <summary>
/// Enumerates supported AI capabilities across providers.
/// </summary>
public enum AiCapability
{
    // Core Language & Text Features

    Chat,                   // Multi-turn conversational interaction with an AI assistant
    TextCompletion,         // Single-turn or prompt-based text generation (e.g., autocomplete, summarization)
    TextEmbedding,          // Convert text into high-dimensional vector embeddings for similarity search, clustering, etc.
    TextFineTuning,         // Customize a base model by training it further on domain-specific text or code

    // Visual Features

    ImageGeneration,        // Generate an image from a text prompt
    ImageEditing,           // Modify or inpaint parts of an existing image using text prompts
    VisualUnderstanding,    // Accept images as input to reason about or describe their content
    VisualFineTuning,       // Fine-tune a model using paired image + text datasets

    // Audio Features

    SpeechToText,           // Transcribe spoken audio into written text
    SpeechTranslation,      // Translate audio from one language to another during or after transcription
    TextToSpeech,           // Generate natural-sounding audio from text (TTS)

    // Core AI Capabilities

    ToolCalling,            // Allow the model to call structured functions/tools based on user intent
    ContentModeration,      // Detect and classify unsafe, biased, or harmful content in text or images

    // Infrastructure & Advanced Access

    AsyncBatchProcessing,   // Submit large batches of inputs for asynchronous processing via job APIs
    RealtimeStreaming       // Get live or streaming responses via WebSocket or similar mechanisms
}
