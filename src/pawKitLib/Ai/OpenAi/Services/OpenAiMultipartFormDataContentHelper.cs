using System.Collections;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;
using pawKitLib.Conversion;
using pawKitLib.Models;

namespace pawKitLib.Ai.OpenAi.Services;

public static class OpenAiMultipartFormDataContentHelper
{
    // Streams and HttpContent objects added to MultipartFormDataContent are automatically disposed
    // when the MultipartFormDataContent instance is disposed. Therefore, AddFile does not need to
    // return the created streams for manual disposal by the caller. This behavior is implemented in
    // MultipartContent.Dispose, which disposes all nested HttpContent objects and their underlying streams.
    //
    // References:
    // https://github.com/microsoft/referencesource/blob/main/System/net/System/Net/Http/MultipartFormDataContent.cs
    // https://github.com/microsoft/referencesource/blob/main/System/net/System/Net/Http/MultipartContent.cs

    public static void AddFile(MultipartFormDataContent form, FilePathReferenceDto file)
    {
        var stream = File.OpenRead(file.FilePath);
        var fileName = Path.GetFileName(file.FilePath);
        var contentType = MimeTypeHelper.GetMimeType(fileName, fallbackToDefault: true);
        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType!);
        form.Add(fileContent, "file", fileName);
    }

    public static void AddFile(MultipartFormDataContent form, FileContentDto file)
    {
        var stream = new MemoryStream(file.Bytes);
        var contentType = MimeTypeHelper.GetMimeType(file.FileName, fallbackToDefault: true);
        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType!);
        form.Add(fileContent, "file", file.FileName);
    }

    public static void AddFile(MultipartFormDataContent form, object file)
    {
        switch (file)
        {
            case FilePathReferenceDto filePathDto:
                AddFile(form, filePathDto);
                break;
            case FileContentDto fileContentDto:
                AddFile(form, fileContentDto);
                break;
            default:
                throw new ArgumentException($"Unsupported file type: {file?.GetType().FullName}", nameof(file));
        }
    }

    // Currently, this method uses string.Empty to represent null values in multipart form data.
    // The optimal approach for handling nulls in this context is still under consideration and may evolve.
    // Further refinement may be needed to ensure correct and predictable null serialization for all consumers.
    private static void AddNull(MultipartFormDataContent form, string name)
    {
        form.Add(new StringContent(string.Empty), name);
    }

    // This method exists separately from AddValue to avoid unnecessary conversion:
    // If a string is passed to AddValue, it would be processed by ValueTypeConverter,
    // resulting in redundant conversion and potential overhead. AddString ensures that
    // plain strings are added directly and efficiently to the form data.
    private static void AddString(MultipartFormDataContent form, string name, string content)
    {
        form.Add(new StringContent(content), name);
    }

    // Adds a name/content pair to the multipart form data, where content is dynamic.
    //
    // Why use dynamic?
    // - Accepts any primitive, value type, or object at runtime, providing maximum flexibility for DTO serialization.
    // - Enables runtime dispatch to ValueTypeConverter, which covers all major .NET value types and enums for roundtrippable, culture-safe conversion.
    //
    // Responsibility and error handling:
    // - This method does not handle null or unsupported types.
    // - If content is null or not supported by ValueTypeConverter, an exception will be thrown.
    // - This enforces single responsibility: callers must ensure content is valid and supported.
    // - Handling or fallback for null/unsupported types should be done by the caller, not here.
    //
    // Why not use Convert.ToString(value, CultureInfo.InvariantCulture)?
    // - Convert.ToString provides a generic string representation, but may not guarantee roundtrip safety for all types (e.g., DateTime, Guid, enums).
    // - ValueTypeConverter ensures consistent, predictable formatting for each supported type, making it possible to restore the original value reliably.
    // - This approach avoids subtle bugs from locale differences and ambiguous string formats.
    //
    // Summary:
    // - This method is designed for wide coverage and reliable serialization/deserialization of .NET value types.
    // - Prefer ValueTypeConverter for known types; do not handle errors or fallbacks here.
    // - Use with care in strongly-typed codebases; dynamic is a tradeoff for flexibility and coverage.

    private static void AddValue(MultipartFormDataContent form, string name, dynamic content)
    {
        form.Add(new StringContent(ValueTypeConverter.ToString(content)), name);
    }

    /// <summary>
    /// Adds an enumerable to the multipart form data as repeated key[] entries.
    /// Each item is handled according to its type:
    /// - null: added using AddNull
    /// - string: added using AddString
    /// - value type: added using AddValue
    /// - nested arrays/collections: throws NotSupportedException
    /// - objects: throws NotSupportedException
    /// Note: In .NET, string is the only type that implements IEnumerable but is not an array or a list.
    /// This ensures strings are treated as atomic values, not collections.
    /// Arrays of objects are not confirmed to be supported by OpenAI multipart form data.
    /// </summary>
    private static void AddEnumerable(MultipartFormDataContent form, string name, IEnumerable enumerable)
    {
        var fullName = $"{name}[]";
        foreach (var item in enumerable)
        {
            if (item is null)
            {
                AddNull(form, fullName);
            }
            else if (item is string str)
            {
                AddString(form, fullName, str);
            }
            else if (item.GetType().IsValueType)
            {
                AddValue(form, fullName, item);
            }
            else if (item is IEnumerable && item is not string)
            {
                // Important: strings are also IEnumerable, but are handled above.
                // Only nested arrays or collections reach here, which are not supported.
                throw new NotSupportedException("Nested arrays or collections are not supported for OpenAI multipart form data.");
            }
            else
            {
                // The item is not guaranteed to be an object; unsupported types will reach here.
                throw new NotSupportedException("Arrays of objects or other unsupported types are not supported for OpenAI multipart form data.");
            }
        }
    }

    /// <summary>
    /// Adds a JsonElement to the multipart form data, handling primitives, arrays, and objects.
    /// Throws for Undefined value kind.
    /// </summary>
    private static void AddJsonElement(MultipartFormDataContent form, string name, JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Undefined:
                throw new NotSupportedException("JsonElement.ValueKind.Undefined is not supported.");
            case JsonValueKind.Null:
                AddNull(form, name);
                break;
            case JsonValueKind.True:
            case JsonValueKind.False:
                AddString(form, name, element.GetRawText());
                break;
            case JsonValueKind.Number:
                AddString(form, name, element.GetRawText());
                break;
            case JsonValueKind.String:
                var strValue = element.GetString();
                if (strValue is null)
                {
                    AddNull(form, name);
                }
                else
                {
                    AddString(form, name, strValue);
                }
                break;
            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                {
                    AddJsonElement(form, $"{name}[]", item);
                }
                break;
            case JsonValueKind.Object:
                foreach (var prop in element.EnumerateObject())
                {
                    AddJsonElement(form, $"{name}.{prop.Name}", prop.Value);
                }
                break;
            default:
                throw new NotSupportedException($"Unsupported JsonElement ValueKind: {element.ValueKind}");
        }
    }

    /// <summary>
    /// Adds the child properties of a DTO to the multipart form data, skipping properties with DtoOutputIgnoreAttribute.
    /// Handles nulls, strings, value types, arrays/enumerables, and objects.
    /// </summary>
    public static void AddDto(MultipartFormDataContent form, object dto, string? namePrefix = null)
    {
        var type = dto.GetType();
        var properties = type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetCustomAttribute(typeof(DtoOutputIgnoreAttribute)) == null);

        foreach (var prop in properties)
        {
            var fullName = namePrefix != null ? $"{namePrefix}{prop.Name}" : prop.Name;
            var content = prop.GetValue(dto);

            if (content is null)
            {
                AddNull(form, fullName);
            }
            else if (content is string str)
            {
                AddString(form, fullName, str);
            }
            else if (content.GetType().IsValueType)
            {
                AddValue(form, fullName, content);
            }
            else if (content is IEnumerable && content is not string)
            {
                AddEnumerable(form, fullName, (IEnumerable)content);
            }
            else if (content.GetType().IsClass && content is not string)
            {
                AddDto(form, content, fullName + ".");
            }
            else
            {
                throw new NotSupportedException($"Unsupported property type: {content.GetType().FullName}");
            }
        }

        if (dto is DynamicDto dynamicDto)
        {
            foreach (var kvp in dynamicDto.ExtraProperties)
            {
                var extraFullName = namePrefix != null ? $"{namePrefix}{kvp.Key}" : kvp.Key;
                var element = kvp.Value;
                AddJsonElement(form, extraFullName, element);
            }
        }
    }
}
