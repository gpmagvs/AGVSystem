using AGVSystem.UserManagers;
using Microsoft.EntityFrameworkCore;

namespace AGVSystem.DATABASE
{
    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options)
            : base(options)
        {
        }

        public DbSet<UserEntity> Users { get; set; }
    }
}
