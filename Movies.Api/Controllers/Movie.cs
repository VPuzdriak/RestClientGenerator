namespace Movies.Api.Controllers;

public class Movie
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Director { get; set; }
    public int ReleaseYear { get; set; }
}