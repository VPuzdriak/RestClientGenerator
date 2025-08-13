using System.Text;
using System.Text.Json;
using Movies.Contracts;

namespace Movies.Api.Controllers;

public class MoviesClientSample
{
    private readonly HttpClient _httpClient;

    public MoviesClientSample(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<MovieResponse>> GetAllMoviesAsync()
    {
        var response = await _httpClient.GetAsync("movies");
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadFromJsonAsync<IEnumerable<MovieResponse>>();
        return responseBody!;
    }

    public async Task<MovieResponse?> GetMovieByIdAsync(int id)
    {
        var response = await _httpClient.GetAsync($"movies/{id}");
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadFromJsonAsync<MovieResponse>();
        return responseBody!;
    }

    public async Task<MovieResponse> CreateMovieAsync(CreateMovieRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("movies", request);
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadFromJsonAsync<MovieResponse>();
        return responseBody!;
    }

    public async Task<MovieResponse> UpdateMovieAsync(int id, UpdateMovieRequest request)
    {
        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Put, $"movies/{id}");
        httpRequestMessage.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        httpRequestMessage.Content = JsonContent.Create(request);
        
        var response = await _httpClient.PutAsJsonAsync($"movies/{id}", request);
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadFromJsonAsync<MovieResponse>();
        return responseBody!;
    }

    public async Task<IEnumerable<MovieResponse>> GetMoviesGenAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "movies");
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadFromJsonAsync<IEnumerable<MovieResponse>>();
        return responseBody!;
    }
    
    public async Task<MovieResponse?> GetMovieByIdGenAsync(int id)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"movies/{id}");
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadFromJsonAsync<MovieResponse>();
        return responseBody!;
    }
    
    public async Task DeleteMovieAsync(int id)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"movies/{id}");
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }
}