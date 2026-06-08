using LogicBuilder.Domain.Json;
using System.Text.Json.Serialization;

namespace LogicBuilder.Domain
{
    [JsonConverter(typeof(ModelConverter))]
    abstract public class BaseModel : IBaseModel
    {
        public EntityStateType EntityState { get; set; }
        public string TypeString => this.GetType().AssemblyQualifiedName;
    }
}
