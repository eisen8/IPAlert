using IPAlert.Utils;
using System.IO.Abstractions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IPAlert.Settings
{
    public class AppSettings
    {
        public bool NotificationsEnabled { get; set; }
        public int NotificationTimeMs { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public IPAlertMode Mode { get; set; }
        public int PollingTimeMs { get; set; }


        /// <summary>
        /// Loads the AppSettings from the appsettings.json file
        /// </summary>
        /// <param name="filePath">The filepath to the appsettings.json file</param>
        /// <param name="fileSystem">The filesystem</param>
        /// <returns>The AppSettings</returns>
        public static AppSettings LoadFromFile(string filePath, IFileSystem fileSystem)
        {
            if (!fileSystem.File.Exists(filePath))
                throw new FileNotFoundException($"Configuration file not found at {filePath}.", filePath);

            var options = new JsonSerializerOptions();
            options.Converters.Add(new StrictJsonConverter<AppSettings>());

            string json = fileSystem.File.ReadAllText(filePath);
            AppSettings? result = JsonSerializer.Deserialize<AppSettings>(json, options);
            if (result == null)
            {
                throw new JsonException("Could not deserialize appsettings Configuration file");
            }

            return result;

        }
    }

    public enum IPAlertMode
    {
        Auto,
        Timed
    }
}
