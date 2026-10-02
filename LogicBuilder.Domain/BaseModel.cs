using LogicBuilder.Domain.Json;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
[assembly: InternalsVisibleTo("LogicBuilder.Domain.Tests, Publickey=002400000480000094000000060200000024000052534131000400000100010059b59302e7303accd5cc84fd482cae54dea8d8b8de7faaef37abbac4b08e3d91283087f48ae04c4fdd117752a3fcafcda61cd2099e2d5432b9bce70e5fe083b15e43cd652617b06dc1422d347ffe7b2aeb7b466e567c6988f26dccbf9723b4b57b1aeaa0a2dbd00478d7135da9bb04a6138d5f29e54ac7e9ac9ae3b7956cf6c2")]
namespace LogicBuilder.Domain
{
    [JsonConverter(typeof(ModelConverter))]
    abstract public class BaseModel : IBaseModel
    {
        public EntityStateType EntityState { get; set; }
        public string TypeString => this.GetType().AssemblyQualifiedName;
    }
}
