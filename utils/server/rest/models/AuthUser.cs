
using SQLite;

namespace Game.Rest
{
    [Table("auth")]
    public class AuthUser
    {
        [PrimaryKey, AutoIncrement]
        [Column("id")]
        public int Id { get; set; }

        [Column("token")]
        public string Token { get; set; }

        [Column("salt")]
        public string Salt { get; set; }

        [Column("username")]
        public string Username { get; set; }

        [Column("password")]
        public string Password { get; set; }
    }
}