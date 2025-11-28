namespace SolarMonitor.Api.Models;

/// <summary>
/// Standard API response wrapper for success and error cases.
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Error { get; set; }

    public static ApiResponse<T> Ok(T data) => new()
    {
        Success = true,
        Data = data
    };

    public static ApiResponse<T> Fail(string error) => new()
    {
        Success = false,
        Error = error
    };
}
