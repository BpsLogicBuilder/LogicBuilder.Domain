namespace LogicBuilder.Domain.Json
{
    public class ModelConverter : JsonTypeConverter<BaseModel>
    {
        public override string TypePropertyName => nameof(BaseModel.TypeString);
    }
}
