namespace Movies.Contracts;

public class UpdateMovieRequest
{
    public required string Title { get; set; }
    public required string Director { get; set; }
    public int ReleaseYear { get; set; }
}