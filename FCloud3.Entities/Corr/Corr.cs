namespace FCloud3.Entities.Corr
{
    public class Corr : IDbModel
    {
        public int Id { get; set; }
        public int A { get; set; }
        public int B { get; set; }
        public int Order { get; set; }
        public CorrType CorrType { get; set; }

        public int CreatorUserId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool Deleted { get; set; }
    }
    public enum CorrType
    {
        UserGroup_User = 10,
        UserFollowed_User = 11,

        WikiItem_WikiDir = 20,
        TextSection_WikiItem = 21,
        FileItem_WikiItem = 22,
        TableItem_WikiItem = 23
    }
}
