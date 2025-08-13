using Microsoft.AspNetCore.Mvc;
using Movies.Contracts;

namespace Movies.Api.Controllers;

[ApiController]
[Route("movies")]
public class MoviesController : ControllerBase
{
    private static readonly List<Movie> Movies =
    [
        new() { Id = 1, Title = "Inception", Director = "Christopher Nolan", ReleaseYear = 2010 },
        new() { Id = 2, Title = "The Matrix", Director = "Lana Wachowski, Lilly Wachowski", ReleaseYear = 1999 },
        new() { Id = 3, Title = "Interstellar", Director = "Christopher Nolan", ReleaseYear = 2014 }
    ];
    
    [HttpGet]
    public ActionResult<IEnumerable<MovieResponse>> GetAllMovies()
    {
        var movies = Movies.Select(m => new MovieResponse
        {
            Id = m.Id,
            Title = m.Title,
            Director = m.Director,
            ReleaseYear = m.ReleaseYear
        }).ToList();
        return Ok(movies);
    }

    [HttpGet("{id:int}")]
    public ActionResult<MovieResponse?> GetMovieById(int id)
    {
        var movie = Movies.FirstOrDefault(m => m.Id == id);
        if (movie is null)
        {
            return NotFound();
        }

        var movieResponse = new MovieResponse
        {
            Id = movie.Id,
            Title = movie.Title,
            Director = movie.Director,
            ReleaseYear = movie.ReleaseYear
        };

        return Ok(movieResponse);
    }

    [HttpPost]
    public ActionResult<MovieResponse> CreateMovie(CreateMovieRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Director))
        {
            return BadRequest("Invalid movie data.");
        }

        var id = Movies.Max(m => m.Id) + 1; // Simple ID generation
        var movie = new Movie
        {
            Id = id,
            Title = request.Title,
            Director = request.Director,
            ReleaseYear = request.ReleaseYear
        };

        Movies.Add(movie);

        var movieResponse = new MovieResponse
        {
            Id = movie.Id,
            Title = movie.Title,
            Director = movie.Director,
            ReleaseYear = movie.ReleaseYear
        };

        return CreatedAtAction(nameof(GetMovieById), new { id = movie.Id }, movieResponse);
    }

    [HttpPut("{id:int}")]
    public ActionResult<MovieResponse> UpdateMovie(int id, UpdateMovieRequest request)
    {
        var movie = Movies.FirstOrDefault(m => m.Id == id);
        if (movie == null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Director))
        {
            return BadRequest("Invalid movie data.");
        }

        movie.Title = request.Title;
        movie.Director = request.Director;
        movie.ReleaseYear = request.ReleaseYear;

        var movieResponse = new MovieResponse
        {
            Id = movie.Id,
            Title = movie.Title,
            Director = movie.Director,
            ReleaseYear = movie.ReleaseYear
        };

        return Ok(movieResponse);
    }

    [HttpDelete("{id:int}")]
    public ActionResult DeleteMovie(int id)
    {
        var movie = Movies.FirstOrDefault(m => m.Id == id);
        if (movie is null)
        {
            return NotFound();
        }

        Movies.Remove(movie);
        return NoContent();
    }
}