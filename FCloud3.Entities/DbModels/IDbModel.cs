namespace FCloud3.Entities.DbModels
{
    public interface IDbModel
    {
        public int Id { get; set; }
        public int CreatorUserId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool Deleted { get; set; }
    }
}
