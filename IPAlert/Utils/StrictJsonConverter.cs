using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace IPAlert.Utils
{   
    /// <summary>
    /// This is a modified version of the JsonConverter that throws JsonExceptions if any property is missing or if any extra properties are found.
    /// I'm not sure why this is not available by default (using options) in System.Text.Json but we do what we must because we can.
    /// </summary>
    /// <typeparam name="T">The type</typeparam>
    public class StrictJsonConverter<T> : JsonConverter<T>
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var safeOptions = new JsonSerializerOptions(options);
            safeOptions.Converters.Remove(this); // Prevent infinite recursion

            var dictionary = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(ref reader, safeOptions);
            if (dictionary == null)
            {
                throw new JsonException("Invalid JSON format.");
            }

            // Get expected property names from the class
            var expectedProperties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                              .Select(p => p.Name)
                                              .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Collect all errors from missing/added properties instead of throwing immediately
            var errors = new List<string>();

            // Find missing properties
            var missingProperties = expectedProperties.Except(dictionary.Keys, StringComparer.OrdinalIgnoreCase);
            if (missingProperties.Any())
            {
                errors.Add($"Missing required Json properties: {string.Join(", ", missingProperties)}");
            }

            // Find extra properties
            var extraProperties = dictionary.Keys.Except(expectedProperties, StringComparer.OrdinalIgnoreCase);
            if (extraProperties.Any())
            {
                errors.Add($"Unexpected Json properties found: {string.Join(", ", extraProperties)}");
            }

            // Throw a single exception with all validation errors
            if (errors.Any())
            {
                throw new JsonException(string.Join("\n", errors));
            }

            // Deserialize normally
            return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(dictionary), safeOptions)!;
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            var safeOptions = new JsonSerializerOptions(options);
            safeOptions.Converters.Remove(this); // Prevent infinite recursion

            JsonSerializer.Serialize(writer, value, safeOptions);
        }
    }
}
