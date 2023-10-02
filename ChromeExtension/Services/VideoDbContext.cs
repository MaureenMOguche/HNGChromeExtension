using ChromeExtension.Model;
using Microsoft.EntityFrameworkCore;

namespace ChromeExtension.Services
{
    public class VideoDbContext : DbContext
    {
        private readonly IConfiguration _config;

        public VideoDbContext(IConfiguration config)
        {
            _config = config;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(_config.GetConnectionString("PostgresConn"));
        }

        public DbSet<VideoData> VideoDatas { get; set; }
    }
}
