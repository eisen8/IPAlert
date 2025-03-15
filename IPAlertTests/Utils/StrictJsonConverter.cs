using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Moq;
using NUnit.Framework;
using IPAlert.Utils;

namespace IPAlert.Tests.Utils
{
    [TestFixture]
    public class StrictJsonConverterTests
    {
        private JsonSerializerOptions _options;
        internal class TestClass
        {
            public string Property1 { get; set; }
            public int Property2 { get; set; }

            public bool Property3 { get; set; }
        }

        [SetUp]
        public void SetUp()
        {
            _options = new JsonSerializerOptions
            {
                Converters = { new StrictJsonConverter<TestClass>() }
            };
        }

        [Test]
        public void Read_ShouldDeserializeCorrectly_WhenJsonIsValid()
        {
            // Arrange
            var json = @"{ ""Property1"": ""Value1"", ""Property2"": 123, ""Property3"": true }";

            // Act
            var result = JsonSerializer.Deserialize<TestClass>(json, _options);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Property1, Is.EqualTo("Value1"));
            Assert.That(result.Property2, Is.EqualTo(123));
        }

        [Test]
        public void Read_ShouldThrowJsonException_WhenJsonHasMissingProperties()
        {
            // Arrange
            var json = @"{ ""Property1"": ""Value1"", ""Property2"": 123 }";

            // Act & Assert
            var ex = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<TestClass>(json, _options));
            Assert.That(ex.Message, Does.Contain("Missing required Json properties: Property3"));
        }

        [Test]
        public void Read_ShouldThrowJsonException_WhenJsonHasMultipleMissingProperties()
        {
            // Arrange
            var json = @"{ ""Property1"": ""Value1"" }";

            // Act & Assert
            var ex = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<TestClass>(json, _options));
            Assert.That(ex.Message, Does.Contain("Missing required Json properties: Property2, Property3"));
        }

        [Test]
        public void Read_ShouldThrowJsonException_WhenJsonHasExtraProperties()
        {
            // Arrange
            var json = @"{ ""Property1"": ""Value1"", ""Property2"": 123, ""Property3"": true, ""Extra1"": 321 }";

            // Act & Assert
            var ex = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<TestClass>(json, _options));
            Assert.That(ex.Message, Does.Contain("Unexpected Json properties found: Extra1"));
        }

        [Test]
        public void Read_ShouldThrowJsonException_WhenJsonHasMultipleExtraProperties()
        {
            // Arrange
            var json = @"{ ""Property1"": ""Value1"", ""Property2"": 123, ""Property3"": true, ""Extra1"": 321, ""Extra2"": 412 }";

            // Act & Assert
            var ex = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<TestClass>(json, _options));
            Assert.That(ex.Message, Does.Contain("Unexpected Json properties found: Extra1, Extra2"));
        }

        [Test]
        public void Read_ShouldThrowJsonException_WhenJsonHasMissingAndExtraProperties()
        {
            // Arrange
            var json = @"{ ""Property1"": ""Value1"", ""Extra1"": 321, ""Extra2"": 412 }";

            // Act & Assert
            var ex = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<TestClass>(json, _options));
            Assert.That(ex.Message, Does.Contain("Missing required Json properties: Property2, Property3"));
            Assert.That(ex.Message, Does.Contain("Unexpected Json properties found: Extra1, Extra2"));
        }

        [Test]
        public void Write_ShouldSerializeCorrectly()
        {
            // Arrange
            var testClass = new TestClass
            {
                Property1 = "Value1",
                Property2 = 123,
                Property3 = true
            };
            var expectedJson = @"{""Property1"":""Value1"",""Property2"":123,""Property3"":true}";

            // Act
            var json = JsonSerializer.Serialize(testClass, _options);

            // Assert
            Assert.That(json, Is.EqualTo(expectedJson));
        }
    }
}