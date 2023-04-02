using AliTube.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AliTube.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MoviesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MoviesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Movies
        [HttpGet]
        public async Task<IActionResult> Get([FromBody] GetMoviesMessage message)
        {
            if (message == null)
            {
				return NotFound();
			}

			int skip = (message.Page - 1) * message.Limit;
            //var movies = _context.Movies;
            IQueryable<MovieData> movies = _context.Movies.AsQueryable();

			if (message.Order != null)
            {
				switch (message.Order.OrderType)
				{
					case EMovieOrder.Title:
						movies = message.Order.Descendent ? movies.OrderByDescending(m => m.Title) : movies.OrderBy(m => m.Title);
						break;
					case EMovieOrder.Year:
						movies = message.Order.Descendent ? movies.OrderByDescending(m => m.Year) : movies.OrderBy(m => m.Year);
						break;
					case EMovieOrder.Rating:
						movies = message.Order.Descendent ? movies.OrderByDescending(m => m.Rating) : movies.OrderBy(m => m.Rating);
						break;
				}
			}

            if (message.Filters != null)
            {
                foreach(var filter in message.Filters)
                {
					switch (filter.Filter)
					{
						case EMovieFilter.Title:
							movies = movies.Where(m => m.Title.Contains(filter.Value));
							break;
						case EMovieFilter.YearMin:
							movies = movies.Where(m => m.Year >= int.Parse(filter.Value));
							break;
						case EMovieFilter.YearMax:
							movies = movies.Where(m => m.Year <= int.Parse(filter.Value));
							break;
						case EMovieFilter.Genre:
							movies = movies.Where(m => m.Genres.Contains(filter.Value.ToLower()));
							break;
					}
				}
            }

			return new JsonResult(await movies.Skip(skip).Take(message.Limit).ToListAsync());
			//return View(await _context.Movies.ToListAsync());
		}

		// GET: Movies
		[HttpGet("All")]
		public async Task<IActionResult> GetAll()
		{
			return new JsonResult(await _context.Movies.ToListAsync());
        }

        // GET: Movies/Details/5
        [HttpGet("Details")]
        public async Task<IActionResult> GetDetails(int? id)
        {
            if (id == null || _context.Movies == null)
            {
                return NotFound(id);
            }

            var movie = await _context.Movies
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (movie == null)
            {
                return NotFound(id);
            }

            return new JsonResult(movie);
            //return View(movie);
        }

        // POST: Movies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Post([FromBody][Bind("Id, Owner,OwnerId,Title,Year,Description,ImageUrl,Rating")] MovieData movie)
        {
            if (ModelState.IsValid)
            {
                Console.WriteLine("Post movie start");
                _context.Add(movie);
                await _context.SaveChangesAsync();


				Console.WriteLine("Post movie end");

				return new JsonResult(movie);
            }

            return new JsonResult(movie);
            //return View(movie);
        }
    }
}
