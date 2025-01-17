using Client__.Net_.MVVM.Model;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text.Json;
using System.Text;

public class SupabaseService
{
    // HttpClient instance for making HTTP requests
    private readonly HttpClient _httpClient;

    public SupabaseService(SupabaseSettings settings)
    {
        Console.WriteLine("Initializing SupabaseService...");

        if (settings == null)
            throw new ArgumentNullException(nameof(settings), "Supabase settings cannot be null.");

        // Set up the HttpClient with the base URL of the Supabase REST API
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri($"{settings.SupabaseUrl}/rest/v1/")
        };

        // Add required API key and authorization headers for authentication
        _httpClient.DefaultRequestHeaders.Add("apikey", settings.SupabaseApiKey);
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {settings.SupabaseApiKey}");

        Console.WriteLine("HttpClient initialized with base address and headers.");
    }

    public async Task<List<Message>> GetMessagesAsync()
    {
        Console.WriteLine("Sending request to fetch messages...");

        try
        {
            var response = await _httpClient.GetAsync("Messages");

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Request successful. Parsing response...");
                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response JSON: {json}");

                var messages = System.Text.Json.JsonSerializer.Deserialize<List<Message>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                Console.WriteLine($"Parsed {messages?.Count ?? 0} messages.");

                // Sort messages by timestamp (using DateTime.ParseExact for the specific format)
                var sortedMessages = messages?
                    .OrderBy(msg =>
                    {
                        try
                        {
                            return DateTime.ParseExact(msg.timestamp, "dd/MM/yyyy HH:mm:ss", null);
                        }
                        catch (FormatException)
                        {
                            Console.WriteLine($"Invalid timestamp format for message: {msg.timestamp}");
                            return DateTime.MinValue; // Fallback for invalid timestamps
                        }
                    })
                    .ToList();

                return sortedMessages ?? new List<Message>();
            }

            // Log error response
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

        // Create the message object to be serialized and sent
        var messageData = new
        {
            username = username,
            message = message,
            timestamp = timestamp
        };

        // Serialize the message object into JSON format
        var content = new StringContent(JsonConvert.SerializeObject(messageData), Encoding.UTF8, "application/json");

        // Send a POST request to the "Messages" endpoint
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
}
