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

        public static AppSettings LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Configuration file not found at {filePath}.", filePath);

            string json = File.ReadAllText(filePath);
            AppSettings result = JsonSerializer.Deserialize<AppSettings>(json);
            if (result == null)
            {
                throw new Exception("Could not deserialize Configuration file");
            }

            return result;

        }
    }

    public enum IPAlertMode
    {
        OnNetworkChanges,
        Timed
    }
}
