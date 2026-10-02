using System.Linq;
using System.Reflection;

namespace LogicBuilder.Domain.Json
{
    public class ModelConverter : JsonTypeConverter<BaseModel>
    {
        /// <summary>
        /// Allows only the descriptors defined in LogicBuilder.Structures.
        /// Used when the converter is applied through the <see cref="System.Text.Json.Serialization.JsonConverterAttribute"/> on <see cref="BaseModel"/>.
        /// </summary>
        public ModelConverter()
        {
        }

        /// <summary>
        /// Allows the descriptors defined in LogicBuilder.Structures plus concrete <see cref="BaseModel"/> subtypes defined in <paramref name="additionalAssemblies"/>.
        /// Register with <see cref="System.Text.Json.JsonSerializerOptions.Converters"/> - converters added to the options take precedence over the attribute on <see cref="BaseModel"/>.
        /// </summary>
        public ModelConverter(params Assembly[] additionalAssemblies)
            : base(new[] { typeof(BaseModel).Assembly }.Concat(additionalAssemblies ?? []))
        {
        }

        public override string TypePropertyName => nameof(BaseModel.TypeString);
    }
}
