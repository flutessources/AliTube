using Microsoft.EntityFrameworkCore;

namespace AliTube.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();


            // Replace with your connection string.
            var connectionString = "server=localhost;user=root;password=;database=alitubes";

            // Replace with your server version and type.
            // Use 'MariaDbServerVersion' for MariaDB.
            // Alternatively, use 'ServerVersion.AutoDetect(connectionString)'.
            // For common usages, see pull request #1233.
            var serverVersion = new MySqlServerVersion(new Version(8, 0, 31));

            // Replace 'YourDbContext' with the name of your own DbContext derived class.
            builder.Services.AddDbContext<AppDbContext>(
                dbContextOptions => dbContextOptions
                    .UseMySql(connectionString, serverVersion)
                    // The following three options help with debugging, but should
                    // be changed or removed for production.
                    .LogTo(Console.WriteLine, LogLevel.Information)
                    .EnableSensitiveDataLogging()
                    .EnableDetailedErrors()
            );



            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            SetupTorrentStream();

            app.Run();
        }


        private static void SetupTorrentStream()
        {
            var engine = new MonoTorrent.Client.ClientEngine(new MonoTorrent.Client.EngineSettings()
            {
                
            });
            
            TorrentStream.TorrentStreamManager.Init(engine);
        }
    }
}