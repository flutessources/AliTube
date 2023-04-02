using AliTube.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AliTube.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TorrentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TorrentsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Torrents
        [HttpGet()]
        public async Task<IActionResult> Get()
        {
            return new JsonResult(await _context.Movies.ToListAsync());
        }

        // GET: Torrents/MovieDetails/5

        [HttpGet("Details")]
        public async Task<IActionResult> GetDetails(int? movie_id)
        {
            if (movie_id == null || _context.Torrents == null)
            {
                return NotFound();
            }

            var movies =  _context.Torrents
                .Where(m => m.MovieId == movie_id);

            if (movies == null)
            {
                return NotFound();
            }

            return new JsonResult(await movies.ToListAsync());
        }

        // POST: Torrents/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Post([FromBody][Bind("MovieId,TorrentData,Size,Quality")] TorrentMovieData torrent)
        {
            if (ModelState.IsValid)
            {
                _context.Add(torrent);
                await _context.SaveChangesAsync();
                return new JsonResult(torrent);
            }
            return new JsonResult(torrent);
        }
    }
}
