using Microsoft.EntityFrameworkCore;

namespace Master40.DB.Data.Context
{
    public class HangfireDBContext : DbContext
    {
        private DbContextOptions<HangfireDBContext> options;

        public HangfireDBContext(DbContextOptions<HangfireDBContext> options) : base(options)
        {
        }

    }

    public static class HangfireDBInitializer
    {
        public static void DbInitialize(HangfireDBContext context)
        {
            //context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }
    }
}