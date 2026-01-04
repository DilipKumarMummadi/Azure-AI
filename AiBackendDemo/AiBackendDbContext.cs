using Microsoft.EntityFrameworkCore;

namespace AiBackendDemo
{
    public class AiBackendDbContext : DbContext
    {
        public AiBackendDbContext(DbContextOptions<AiBackendDbContext> options)
            : base(options)
        {
        }

        public DbSet<Models.Action> Actions { get; set; }
        public DbSet<Models.ActionMemory> ActionMemories { get; set; }
    }
}
