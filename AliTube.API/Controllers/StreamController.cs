using AliTube.API;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using System;

namespace AliTube.Web
{
    [ApiController]
    [Route("[controller]")]
    public class StreamController : ControllerBase
    {
		private readonly AppDbContext _context;

		public StreamController(AppDbContext context)
		{
			_context = context;
		}

		[HttpGet]
        public async Task Get(int movie_id)
        {
            Console.WriteLine("Stream");
            long startPosition = 0;

            if (!string.IsNullOrEmpty(Request.Headers["Range"]))
            {
                string[] range = Request.Headers["Range"].ToString().Split(new char[] { '=', '-' });
                startPosition = long.Parse(range[1]);
            }

            var torrent = await _context.Torrents
				.FirstOrDefaultAsync(m => m.MovieId == movie_id);

            if (torrent == null)
            {
                Console.WriteLine("torrent not found");
                return;
            }

            Console.WriteLine("Get movie : " + movie_id);
            Console.WriteLine("magnet : " + torrent.TorrentData);

			Console.WriteLine("read at position " + startPosition);

            await TorrentStream.TorrentStreamManager.StreamPartOfTorrent(this.HttpContext, movie_id, torrent.TorrentData, startPosition, startPosition + (2048 * 2048));
        }
    }
}
