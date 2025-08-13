using Microsoft.AspNetCore.Mvc;
using Movies.Api.Clients;

namespace MoviesStock.Api.Controllers;

[ApiController]
[Route("stocks")]
public sealed class StockController : ControllerBase
{
    private readonly IMoviesClient _moviesClient;

    public StockController(IMoviesClient moviesClient)
    {
        _moviesClient = moviesClient;
    }
    
    [HttpGet("movies/{id:int}/stock")]
    public async Task<IActionResult> GetStock(int id)
    {
        var movie = await _moviesClient.GetMovieById(id);

        if (movie is null)
        {
            return NotFound();
        }
        
        // Simulate fetching stock information for a movie
        var stockInfo = new
        {
            MovieId = movie.Id,
            AvailableCopies = 42,
            TotalCopies = 100
        };

        return Ok(stockInfo);
    }
}