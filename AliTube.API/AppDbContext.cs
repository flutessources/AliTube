using System.Collections.Generic;
using AliTube.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace AliTube.API
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
        {
            Console.WriteLine("Create App DbContext");
            this.Database.EnsureCreated();
        }


        public DbSet<MovieData> Movies { get; set; } = null;
        public DbSet<TorrentMovieData> Torrents { get; set; } = null;
    }
}
