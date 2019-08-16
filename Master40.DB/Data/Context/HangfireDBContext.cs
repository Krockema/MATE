using System;
using Microsoft.EntityFrameworkCore;

namespace Master40.DB.Data.Context
{
    public class HangfireDBContext : DbContext
    {
        private DbContextOptions<HangfireDBContext> options;

        public HangfireDBContext(DbContextOptions<HangfireDBContext> options) : base(options: options)
        {
        }

    }

    public static class HangfireDBInitializer
    {
        public static void DbInitialize(HangfireDBContext context)
        {
            //if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Azure")
                //   context.Database.EnsureDeleted();
            // else
            context.Database.EnsureCreated();
        }
    }
}