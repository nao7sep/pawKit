using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using pawKitLib.Ai.OpenAi.Services;
using pawKitLib.Conversion;
using pawKitLib.Models;

namespace pawKitLib.Tests.Ai.OpenAi.Services;

/// <summary>
/// Contains unit tests for the <see cref="OpenAiMultipartFormDataContentHelper"/> class.
/// </summary>
public class OpenAiMultipartFormDataContentHelperTests
{
    #region Test DTOs

    private class NestedTestDto
    {
        public string NestedProperty { get; set; } = string.Empty;
    }

    private class ComplexTestDto : DynamicDto
    {
        public string StringProperty { get; set; } = string.Empty;
        public int IntProperty { get; set; }
        public bool BoolProperty { get; set; }
        public string? NullableStringProperty { get; set; }
        public List<string> StringList { get; set; } = [];
        public int[] IntArray { get; set; } = [];
        public NestedTestDto NestedObject { get; set; } = new();

        [JsonPropertyName("custom_name")]
        public string RenamedProperty { get; set; } = string.Empty;

        [DtoOutputIgnore]
        public string IgnoredProperty { get; set; } = "should-be-ignored";
    }

    #endregion

    /// <summary>
    /// Verifies that AddDto correctly flattens a complex DTO with both declared properties
    /// and dynamic ExtraProperties into the multipart form data.
    /// </summary>
    [Fact]
    public async Task AddDto_WithComplexDtoAndExtraProperties_FlattensAndAddsAllPartsCorrectly()
    {
        // Arrange
        var dto = new ComplexTestDto
        {
            StringProperty = "hello world",
            IntProperty = 123,
            BoolProperty = true,
            NullableStringProperty = null,
            StringList = ["one", "two"],
            IntArray = [4, 5],
            NestedObject = new NestedTestDto { NestedProperty = "nested value" },
            RenamedProperty = "renamed value"
        };

        // Populate ExtraProperties from a JSON string to easily create various JsonElement types.
        var extraDataJson = """
        {
            "extra_string": "extra value",
            "extra_number": 99.9,
            "extra_bool": false,
            "extra_null": null,
            "extra_array": ["a", "b"],
            "extra_object": { "child": "child value" }
        }
        """;
        using var doc = JsonDocument.Parse(extraDataJson);
        foreach (var prop in doc.RootElement.EnumerateObject())
        {
            // Clone the element as it's owned by the JsonDocument which will be disposed.
            dto.ExtraProperties[prop.Name] = prop.Value.Clone();
        }

        using var form = new MultipartFormDataContent();

        // Act
        OpenAiMultipartFormDataContentHelper.AddDto(form, dto);

        // Assert
        var data = await GetFormDataAsDictionary(form);

        // --- Validate strongly-typed properties ---
        Assert.Equal("hello world", data["StringProperty"].Single());
        Assert.Equal(ValueTypeConverter.ToString(123), data["IntProperty"].Single());
        Assert.Equal(ValueTypeConverter.ToString(true), data["BoolProperty"].Single());
        Assert.Equal(string.Empty, data["NullableStringProperty"].Single()); // Nulls become empty strings
        Assert.Equal(["one", "two"], data["StringList[]"]);
        Assert.Equal([ValueTypeConverter.ToString(4), ValueTypeConverter.ToString(5)], data["IntArray[]"]);
        Assert.Equal("nested value", data["NestedObject.NestedProperty"].Single());
        Assert.Equal("renamed value", data["custom_name"].Single());
        Assert.DoesNotContain("IgnoredProperty", data.Keys); // Ignored property should not be present

        // --- Validate dynamic ExtraProperties ---
        Assert.Equal("extra value", data["extra_string"].Single());
        Assert.Equal("99.9", data["extra_number"].Single());
        Assert.Equal("false", data["extra_bool"].Single());
        Assert.Equal(string.Empty, data["extra_null"].Single());
        Assert.Equal(["a", "b"], data["extra_array[]"]);
        Assert.Equal("child value", data["extra_object.child"].Single());

        // Verify the total number of unique keys
        Assert.Equal(14, data.Count);
    }

    /// <summary>
    /// Helper method to convert MultipartFormDataContent into a dictionary for easy assertions.
    /// Handles repeated keys by storing values in a list.
    /// </summary>
    private static async Task<Dictionary<string, List<string>>> GetFormDataAsDictionary(MultipartFormDataContent form)
    {
        var formData = new Dictionary<string, List<string>>();
        foreach (var part in form)
        {
            // The name is stored in the ContentDisposition header.
            var name = part.Headers.ContentDisposition?.Name?.Trim('"');
            if (name is null)
            {
                continue;
            }

            var value = await part.ReadAsStringAsync();

            if (formData.TryGetValue(name, out var values))
            {
                values.Add(value);
            }
            else
            {
                formData[name] = [value];
            }
        }
        return formData;
    }
}