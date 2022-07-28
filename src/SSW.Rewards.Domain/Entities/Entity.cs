namespace SSW.Rewards.Domain.Entities
{
    public abstract class Entity
    {
        public int Id { get; set; }
        public System.DateTime CreatedUtc { get; set; }
    }
}


