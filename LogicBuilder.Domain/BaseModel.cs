namespace LogicBuilder.Domain
{
    abstract public class BaseModel : IBaseModel
    {
        public EntityStateType EntityState { get; set; }
    }
}
