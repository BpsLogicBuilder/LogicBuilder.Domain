using LogicBuilder.Domain.Json;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LogicBuilder.Domain.Tests.Json
{
    public class ModelConverterTest
    {
        [Fact]
        public void ModelConverterThrows_WhenJsonPropertyIsDefault()
        {
            // Arrange
            string json = JsonSerializer.Serialize(new { Name = "John" });//Serialize anonymous type so JsonProperty of Start object is default

            // Act & Assert
            Assert.Throws<JsonException>(() =>
            {
                JsonSerializer.Deserialize<object>(json, TestSerializationOptions.Default);
            });
        }

        [Fact]
        public void ModelConverterThrows_WhenJsonTokenTypeIsNotStartObject()
        {
            // Arrange
            string json = JsonSerializer.Serialize((object)"MyString");//Use a string so JsonTokenType is not StartObject

            // Act & Assert
            Assert.Throws<JsonException>(() =>
            {
                JsonSerializer.Deserialize<object>(json, TestSerializationOptions.Default);
            });
        }

        [Fact]
        public void ModelConverterThrows_WhenValueIsNull()
        {
            // Arrange
            TestModel nullValue = new(null);

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
            {
                JsonSerializer.Serialize(nullValue);
            });
            Assert.Equal("Value cannot be null. (Parameter 'value')", exception.Message);
        }

        [Fact]
        public void ModelConverterThrows_WhenTypeStringIsInvalid()
        {
            // Arrange
            InvalidTypeModel invalidTypeModel = new(new InvalidTypeChildModel());
            string json = JsonSerializer.Serialize(invalidTypeModel);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                JsonSerializer.Deserialize<InvalidTypeModel>(json);
            });
            Assert.Equal($"Type cannot be loaded for {typeof(InvalidTypeChildModel).Name}.", exception.Message);
        }

        private static JsonSerializerOptions? _options;
        public static JsonSerializerOptions Options
        {
            get
            {
                if (_options != null)
                    return _options;

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                options.Converters.Add(new ModelConverter());
                _options = options;
                return _options;
            }
        }

        internal class TestObjectConverter : JsonTypeConverter<object>
        {
            public override string TypePropertyName => "";

            public override bool CanConvert(Type typeToConvert)
                => typeToConvert == typeof(object);
        }

        internal class TestModelConverterWithNullHandling : JsonTypeConverter<TestModelBase>
        {
            public override string TypePropertyName => "";

            public override bool HandleNull => true;
        }

        [JsonConverter(typeof(TestModelConverterWithNullHandling))]
        public abstract class TestModelBase
        {
            public string TypeString => this.GetType().AssemblyQualifiedName!;
        }

        internal class TestModel(TestModelBase? constant) : TestModelBase
        {
            public TestModelBase? Constant { get; set; } = constant;
        }

        internal class InvalidTypeModeConverter : JsonTypeConverter<InvalidTypeModelBase>
        {
            public override string TypePropertyName => nameof(InvalidTypeModelBase.TypeString);
        }

        internal class InvalidTypeModel(InvalidTypeModelBase? constant) : InvalidTypeModelBase
        {
            public InvalidTypeModelBase? Constant { get; set; } = constant;
        }

        internal class InvalidTypeChildModel() : InvalidTypeModelBase
        {
        }

        [JsonConverter(typeof(InvalidTypeModeConverter))]
        public abstract class InvalidTypeModelBase
        {
            public string TypeString => this.GetType().Name;
        }

        static class TestSerializationOptions
        {
            private static JsonSerializerOptions? _default;
            public static JsonSerializerOptions Default
            {
                get
                {
                    if (_default != null)
                        return _default;

                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                    options.Converters.Add(new TestObjectConverter());

                    _default = options;

                    return _default;
                }
            }
        }
    }
}
