using Client__.Net_.MVVM.Model;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text.Json;
using System.Text;

public class SupabaseService
{
    private readonly HttpClient _httpClient;
    private readonly string _supabaseUrl;
    private readonly string _supabaseApiKey;

    // Event to notify ViewModel when connection fails
    public event Action<string> OnConnectionFailed;

    // Flag to track if the error message has been displayed
    private bool _hasConnectionFailed = false;

    public SupabaseService(SupabaseSettings settings)
    {
        Console.WriteLine("Initializing SupabaseService...");

        // Check if Supabase settings are null or empty
        if (settings == null || string.IsNullOrEmpty(settings.SupabaseUrl) || string.IsNullOrEmpty(settings.SupabaseApiKey))
        {
            // Trigger connection failure event if settings are invalid, only once
            if (!_hasConnectionFailed)
            {
                OnConnectionFailed?.Invoke("Supabase URL or API Key is missing. Please enter the values in settings.");
                _hasConnectionFailed = true; // Set the flag to true after showing the message
            }
            return;
        }

        // Validate the Supabase URL format
        if (!IsValidUrl(settings.SupabaseUrl))
        {
            // Trigger connection failure event if the URL is invalid
            if (!_hasConnectionFailed)
            {
                OnConnectionFailed?.Invoke("The Supabase URL is invalid. Please enter a valid URL.");
                _hasConnectionFailed = true; // Set the flag to true after showing the message
            }
            return;
        }

        _supabaseUrl = settings.SupabaseUrl;
        _supabaseApiKey = settings.SupabaseApiKey;

        _httpClient = new HttpClient
        {
            BaseAddress = new Uri($"{_supabaseUrl}/rest/v1/")
        };

        _httpClient.DefaultRequestHeaders.Add("apikey", _supabaseApiKey);
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_supabaseApiKey}");

        Console.WriteLine("HttpClient initialized with base address and headers.");
    }

    // ✅ Add the Supabase validation method inside SupabaseService
    public async Task<bool> ValidateSupabaseCredentials()
    {
        // Check if URL or API Key is null or empty before making the request
        if (string.IsNullOrEmpty(_supabaseUrl) || string.IsNullOrEmpty(_supabaseApiKey))
        {
            // Trigger connection failure event only once
            if (!_hasConnectionFailed)
            {
                OnConnectionFailed?.Invoke("Supabase URL or API Key is missing. Please enter the values in settings.");
                _hasConnectionFailed = true; // Set the flag to true after showing the message
            }
            return false;
        }

        try
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("apikey", _supabaseApiKey);

                HttpResponseMessage response = await client.GetAsync($"{_supabaseUrl}/auth/v1/settings");

                if (!response.IsSuccessStatusCode)
                {
                    // Trigger connection failure event only once
                    if (!_hasConnectionFailed)
                    {
                        OnConnectionFailed?.Invoke("Failed to connect to Supabase. Please check your credentials.");
                        _hasConnectionFailed = true; // Set the flag to true after showing the message
                    }
                    return false;
                }

                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error validating Supabase credentials: {ex.Message}");
            // Trigger connection failure event only once
            if (!_hasConnectionFailed)
            {
                OnConnectionFailed?.Invoke("Failed to connect to Supabase. Please check your credentials.");
                _hasConnectionFailed = true; // Set the flag to true after showing the message
            }
            return false;
        }
    }

    public async Task<List<Message>> GetMessagesAsync()
    {
        Console.WriteLine("Sending request to fetch messages...");

        // Check if URL or API Key is null or empty before making the request
        if (string.IsNullOrEmpty(_supabaseUrl) || string.IsNullOrEmpty(_supabaseApiKey))
        {
            // Trigger connection failure event only once
            if (!_hasConnectionFailed)
            {
                OnConnectionFailed?.Invoke("Supabase URL or API Key is missing. Please enter the values in settings.");
                _hasConnectionFailed = true; // Set the flag to true after showing the message
            }
            return new List<Message>(); // Return empty list if not connected
        }

        try
        {
            var response = await _httpClient.GetAsync("Messages");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var messages = System.Text.Json.JsonSerializer.Deserialize<List<Message>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return messages?.OrderBy(msg =>
                {
                    try
                    {
                        return DateTime.ParseExact(msg.timestamp, "dd/MM/yyyy HH:mm:ss", null);
                    }
                    catch (FormatException)
                    {
                        Console.WriteLine($"Invalid timestamp format for message: {msg.timestamp}");
                        return DateTime.MinValue;
                    }
                }).ToList() ?? new List<Message>();
            }

            Console.WriteLine($"Error: {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
            return new List<Message>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            return new List<Message>();
        }
    }

    public async Task<bool> SaveMessageAsync(string username, string message, string timestamp)
    {
        Console.WriteLine("Saving message to Supabase...");

        // Check if URL or API Key is null or empty before making the request
        if (string.IsNullOrEmpty(_supabaseUrl) || string.IsNullOrEmpty(_supabaseApiKey))
        {
            // Trigger connection failure event only once
            if (!_hasConnectionFailed)
            {
                OnConnectionFailed?.Invoke("Supabase URL or API Key is missing. Please enter the values in settings.");
                _hasConnectionFailed = true; // Set the flag to true after showing the message
            }
            return false;
        }

        var messageData = new
        {
            username = username,
            message = message,
            timestamp = timestamp
        };

        var content = new StringContent(JsonConvert.SerializeObject(messageData), Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("Messages", content);

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("Message saved successfully.");
            return true;
        }
        else
        {
            Console.WriteLine($"Failed to save message. Status code: {response.StatusCode}");
            return false;
        }

    }

    // Method to validate URL format
    private bool IsValidUrl(string url)
    {
        // Try to create a Uri object and check if it's a valid URI
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}