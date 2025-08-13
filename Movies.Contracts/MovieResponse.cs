namespace Movies.Contracts;

public class MovieResponse
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Director { get; set; }
    public int ReleaseYear { get; set; }
}