using LogicBuilder.Domain.Json;
using System;
using System.Linq;
using System.Reflection;
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
            var exception = Assert.Throws<JsonException>(() =>
            {
                JsonSerializer.Deserialize<InvalidTypeModel>(json);
            });
            Assert.Equal($"Type \"{typeof(InvalidTypeChildModel).Name}\" is not an allowed type for {typeof(InvalidTypeModelBase).FullName}.", exception.Message);
        }

        [Fact]
        public void ModelConverterDoesNotInstantiate_TypeOutsideAllowlist()
        {
            // Arrange
            GadgetType.Instantiated = false;
            string json = "{\"TypeString\":\"" + typeof(GadgetType).AssemblyQualifiedName + "\"}";

            // Act & Assert
            Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<BaseModel>(json));
            Assert.False(GadgetType.Instantiated);
        }

        [Fact]
        public void ModelConverterRejects_DescriptorSubtypeFromUnregisteredAssembly()
        {
            // Arrange
            string json = "{\"TypeString\":\"" + typeof(ExternalModel).AssemblyQualifiedName + "\"}";

            // Act & Assert
            Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<BaseModel>(json));
            Assert.Throws<JsonException>(() => JsonSerializer.Serialize<BaseModel>(new ExternalModel()));
        }

        [Fact]
        public void ModelConverterAccepts_DescriptorSubtypeFromRegisteredAssembly()
        {
            // Arrange
            string json = JsonSerializer.Serialize<BaseModel>(new ExternalModel { Name = "A" }, TestSerializationOptions.ExternalModelOptions);

            // Act
            BaseModel result = JsonSerializer.Deserialize<BaseModel>(json, TestSerializationOptions.ExternalModelOptions)!;

            // Assert
            Assert.Equal("A", Assert.IsType<ExternalModel>(result).Name);
        }

        [Fact]
        public void ModelConverterAccepts_DescriptorSubtypeFromRegisteredAssembly_UsingTypesListConstructor()
        {
            // Arrange
            JsonSerializerOptions options = new();
            options.Converters.Add(new TestModelConverter(typeof(ExternalModel).Assembly.GetTypes().Where(t => typeof(BaseModel).IsAssignableFrom(t)).ToArray()));
            string json = JsonSerializer.Serialize<BaseModel>(new ExternalModel { Name = "A" }, options);

            // Act
            BaseModel result = JsonSerializer.Deserialize<BaseModel>(json, options)!;

            // Assert
            Assert.Equal("A", Assert.IsType<ExternalModel>(result).Name);
        }

        [Fact]
        public void DescriptorThrowsJsonException_WhenJsonTpePropertyNameIsNotAString()
        {
            // Arrange
            JsonSerializerOptions options = new();
            options.Converters.Add(new TestModelConverterWithInvalidPropertyName(typeof(ExternalModel).Assembly.GetTypes().Where(t => typeof(BaseModel).IsAssignableFrom(t)).ToArray()));
            string json = JsonSerializer.Serialize<BaseModel>(new ExternalModel { Name = "A" }, options);

            // Act Assert
            Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<BaseModel>(json, options)!);
        }

        [Fact]
        public void ModelConverterAccepts_TypeStringWithDifferentAssemblyVersion()
        {
            // Arrange
            string typeString = $"{typeof(ExternalModel).FullName}, {typeof(ExternalModel).Assembly.GetName().Name}, Version=0.0.0.1, Culture=neutral, PublicKeyToken=null";
            string json = "{\"TypeString\":\"" + typeString + "\",\"Constant\":1}";

            // Act & Assert
            Assert.IsType<ExternalModel>(JsonSerializer.Deserialize<BaseModel>(json, TestSerializationOptions.ExternalModelOptions));
        }

        [Fact]
        public void CreateConverterThrows_WhenTypesListContainsInvalidTypes()
        {
            // Act Assert
            Assert.Throws<ArgumentException>(() =>
            {
                new TestModelConverter(typeof(ExternalModel).Assembly.GetTypes().ToArray());
            });
        }

        [Fact]
        public void CreateConverterThrows_WhenTypesListIsNull()
        {
            // Act Assert
            Assert.Throws<ArgumentNullException>(() =>
            {
                new TestModelConverter((Type[])null!);
            });
        }

        [Fact]
        public void CreateConverterThrows_WhenAssemblyListIsNull()
        {
            // Act Assert
            Assert.Throws<ArgumentNullException>(() =>
            {
                new TestModelConverter((Assembly[])null!);
            });
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

        public class GadgetType
        {
            public static bool Instantiated { get; set; }
            protected GadgetType() => Instantiated = true;
        }

        public class ExternalModel : BaseModel
        {
            public int ID { get; set; }
            public string? Name { get; set; }
        }

        internal class TestModelConverter : JsonTypeConverter<BaseModel>
        {
            public TestModelConverter()
            {
            }

            public TestModelConverter(params Assembly[] additionalAssemblies)
                : base(additionalAssemblies)
            {
            }

            public TestModelConverter(params Type[] types)
                : base(types)
            {
            }

            public override string TypePropertyName => nameof(BaseModel.TypeString);
        }

        internal class TestModelConverterWithInvalidPropertyName : JsonTypeConverter<BaseModel>
        {
            public TestModelConverterWithInvalidPropertyName()
            {
            }

            public TestModelConverterWithInvalidPropertyName(params Assembly[] additionalAssemblies)
                : base(additionalAssemblies)
            {
            }

            public TestModelConverterWithInvalidPropertyName(params Type[] types)
                : base(types)
            {
            }

            public override string TypePropertyName => nameof(ExternalModel.ID);
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

            private static JsonSerializerOptions? _externalModelOptions;
            public static JsonSerializerOptions ExternalModelOptions
            {
                get
                {
                    if (_externalModelOptions != null)
                        return _externalModelOptions;

                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                    options.Converters.Add(new ModelConverter(typeof(ExternalModel).Assembly));

                    _externalModelOptions = options;

                    return _externalModelOptions;
                }
            }
        }
    }
}
