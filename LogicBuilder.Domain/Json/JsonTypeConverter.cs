using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LogicBuilder.Domain.Json
{
    abstract public class JsonTypeConverter<T> : JsonConverter<T>
    {
        private readonly IReadOnlyDictionary<string, Type> knownTypes;
        private readonly ITypeNameHelper typeNameHelper;
        private readonly ITypeHelper typeHelper;

        protected JsonTypeConverter() : this(typeof(T).Assembly)
        {
        }

        protected JsonTypeConverter(params Assembly[] assemblies)
            : this((IEnumerable<Assembly>)assemblies)
        {
        }

        protected JsonTypeConverter(IEnumerable<Assembly> assemblies)
        {
            typeNameHelper = new TypeNameHelper();
            typeHelper = new TypeHelper(typeNameHelper);
            if (assemblies == null)
                throw new ArgumentNullException(nameof(assemblies));

            Assembly[] assemblyList = [.. assemblies.Where(a => a != null).Distinct()];

            knownTypes = typeHelper.BuildKnownTypes<T>(assemblyList.SelectMany(typeHelper.LoadTypesFromAssembly));
        }

        protected JsonTypeConverter(IEnumerable<Type> allowedTypes)
        {
            typeNameHelper = new TypeNameHelper();
            typeHelper = new TypeHelper(typeNameHelper);
            if (allowedTypes == null)
                throw new ArgumentNullException(nameof(allowedTypes));

            Type[] typeList = [.. allowedTypes];
            Type? disallowedType = typeList.FirstOrDefault(type => !typeHelper.IsAllowedType<T>(type));
            if (disallowedType != null)
                throw new ArgumentException($"{disallowedType.FullName} must be a concrete type assignable to {typeof(T).FullName}.", nameof(allowedTypes));

            knownTypes = typeHelper.BuildKnownTypes<T>(typeList);
        }

        #region Properties
        abstract public string TypePropertyName { get; }
        #endregion Properties

        #region Methods
        public override bool CanConvert(Type typeToConvert)
            => typeToConvert == typeof(T);

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException();

            using var jsonDocument = JsonDocument.ParseValue(ref reader);
            JsonProperty jsonProperty = GetJsonProperty();
            if (jsonProperty.Equals(default(JsonProperty)))
                throw new JsonException();

            if (jsonProperty.Value.ValueKind != JsonValueKind.String)
                throw new JsonException($"The {TypePropertyName} property must be a string.");

            string objectType = jsonProperty.Value.GetString()!;//does not return null if JsonValueKind is not JsonValueKind.Null

            Type type = typeHelper.ResolveType(objectType, knownTypes)
                ?? throw new JsonException($"Type \"{objectType}\" is not an allowed type for {typeof(T).FullName}.");

            return (T)jsonDocument.RootElement.Deserialize(type, options)!;//never null because only valid JSON like "null" can return null.  For this method, the JsonTokenType is always JsonTokenType.StartObject.

            JsonProperty GetJsonProperty()
                => jsonDocument.RootElement.EnumerateObject().FirstOrDefault(e => e.Name.ToLowerInvariant() == TypePropertyName.ToLowerInvariant());
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value), "Value cannot be null.");

            Type type = value.GetType() ?? throw new InvalidOperationException("Type cannot be null");
            if (!knownTypes.TryGetValue(typeNameHelper.GetKey(type), out Type? knownType) || knownType != type)
                throw new JsonException($"Type \"{type.AssemblyQualifiedName}\" is not an allowed type for {typeof(T).FullName}.");

            JsonSerializer.Serialize(writer, value, value.GetType(), options);
        }
        #endregion Methods
    }
}
