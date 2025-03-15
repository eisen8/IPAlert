using System;
using System.IO;
using System.Text.Json;
using Moq;
using NUnit.Framework;
using IPAlert.Settings;
using System.IO.Abstractions;

namespace IPAlert.Services.Tests
{
    [TestFixture]
    public class AppSettingsTests
    {
        private const string VALID_JSON = @"{
            ""NotificationsEnabled"": true,
            ""NotificationTimeMs"": 5000,
            ""Mode"": ""Timed"",
            ""PollingTimeMs"": 1000
        }";

        private const string INVALID_JSON = @"{{
            ""NotificationsEnabled"": true,
            ""NotificationTimeMs"": 5000,
            ""Mode"": ""Timed"",
            ""PollingTimeMs"": 1000
        }";

        private const string INVALID_MODE = @"{
            ""NotificationsEnabled"": true,
            ""NotificationTimeMs"": 5000,
            ""Mode"": ""InvalidMode"",
            ""PollingTimeMs"": 1000
        }";

        private const string MISSING_FIELD_JSON = @"{
            ""NotificationsEnabled"": true,
            ""Mode"": ""Timed"",
            ""PollingTimeMs"": 1000
        }";


        private const string ADDED_FIELD_JSON = @"{
            ""NotificationsEnabled"": true,
            ""Added_Field"": true,
            ""NotificationTimeMs"": 5000,
            ""Mode"": ""Timed"",
            ""PollingTimeMs"": 1000
        }";

        private const string FILE_PATH = "appsettings.json";

        [Test]
        public void LoadFromFile_ShouldThrowFileNotFoundException_WhenFileDoesNotExist()
        {
            // Arrange
            var mockFileSystem = new Mock<IFileSystem>();
            mockFileSystem.Setup(fs => fs.File.Exists(FILE_PATH)).Returns(false);


            // Act/Assert
            Assert.Throws<FileNotFoundException>(() => AppSettings.LoadFromFile(FILE_PATH, mockFileSystem.Object));
        }

        [Test]
        public void LoadFromFile_ShouldDeserializeCorrectly_WhenFileIsValid()
        {
            // Arrange
            var mockFileSystem = new Mock<IFileSystem>();
            mockFileSystem.Setup(fs => fs.File.Exists(FILE_PATH)).Returns(true);
            mockFileSystem.Setup(fs => fs.File.ReadAllText(FILE_PATH)).Returns(VALID_JSON);

            // Act
            var settings = AppSettings.LoadFromFile(FILE_PATH, mockFileSystem.Object);

            // Assert
            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.NotificationsEnabled, Is.True);
            Assert.That(settings.NotificationTimeMs, Is.EqualTo(5000));
            Assert.That(settings.Mode, Is.EqualTo(IPAlertMode.Timed));
            Assert.That(settings.PollingTimeMs, Is.EqualTo(1000));
        }

        [Test]
        public void LoadFromFile_ShouldThrowException_WhenJsonIsInvalid()
        {
            // Arrange
            var mockFileSystem = new Mock<IFileSystem>();
            mockFileSystem.Setup(fs => fs.File.Exists(FILE_PATH)).Returns(true);
            mockFileSystem.Setup(fs => fs.File.ReadAllText(FILE_PATH)).Returns(INVALID_JSON);

            // Act & Assert
            Assert.Throws<JsonException>(() => AppSettings.LoadFromFile(FILE_PATH, mockFileSystem.Object));
        }

        [Test]
        public void LoadFromFile_ShouldThrowException_WhenHasInvalidMode()
        {
            // Arrange
            var mockFileSystem = new Mock<IFileSystem>();
            mockFileSystem.Setup(fs => fs.File.Exists(FILE_PATH)).Returns(true);
            mockFileSystem.Setup(fs => fs.File.ReadAllText(FILE_PATH)).Returns(INVALID_MODE);

            // Act & Assert
            Assert.Throws<JsonException>(() => AppSettings.LoadFromFile(FILE_PATH, mockFileSystem.Object));
        }

        [Test]
        public void LoadFromFile_ShouldThrowException_WhenFileIsEmpty()
        {
            // Arrange
            var mockFileSystem = new Mock<IFileSystem>();
            mockFileSystem.Setup(fs => fs.File.Exists(FILE_PATH)).Returns(true);
            mockFileSystem.Setup(fs => fs.File.ReadAllText(FILE_PATH)).Returns("");

            // Act & Assert
            Assert.Throws<JsonException>(() => AppSettings.LoadFromFile(FILE_PATH, mockFileSystem.Object));
        }


        [Test]
        public void LoadFromFile_ShouldThrowException_WhenJsonIsMissingFields()
        {
            // Arrange
            var mockFileSystem = new Mock<IFileSystem>();
            mockFileSystem.Setup(fs => fs.File.Exists(FILE_PATH)).Returns(true);
            mockFileSystem.Setup(fs => fs.File.ReadAllText(FILE_PATH)).Returns(MISSING_FIELD_JSON);

            // Act & Assert
            Assert.Throws<JsonException>(() => AppSettings.LoadFromFile(FILE_PATH, mockFileSystem.Object));
        }

        [Test]
        public void LoadFromFile_ShouldThrowException_WhenJsonHasAddedFields()
        {
            // Arrange
            var mockFileSystem = new Mock<IFileSystem>();
            mockFileSystem.Setup(fs => fs.File.Exists(FILE_PATH)).Returns(true);
            mockFileSystem.Setup(fs => fs.File.ReadAllText(FILE_PATH)).Returns(ADDED_FIELD_JSON);

            // Act & Assert
            Assert.Throws<JsonException>(() => AppSettings.LoadFromFile(FILE_PATH, mockFileSystem.Object));
        }
    }
}