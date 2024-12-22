using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


public class SupabaseService
{
    // HttpClient instance for making HTTP requests
    private readonly HttpClient _httpClient;
    private string apiKey = "";
    private string projectURL = "";


    // Constructor initializes the HttpClient with base address and required headers
    public SupabaseService()
    {

        Console.WriteLine("Initializing SupabaseService...");

        // Set up the HttpClient with the base URL of the Supabase REST API
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri($"{projectURL}/rest/v1/")
        };

        // Add required API key and authorization headers for authentication
        _httpClient.DefaultRequestHeaders.Add("apikey", apiKey);
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        Console.WriteLine("HttpClient initialized with base address and headers.");
    }

  

// Method to fetch messages from the "Messages" table in the Supabase database
public async Task<List<Message>> GetMessagesAsync()
    {
        Console.WriteLine("Sending request to fetch messages...");

        // Send a GET request to the "Messages" endpoint
        var response = await _httpClient.GetAsync("Messages");

        // Check if the request was successful
        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("Request successful. Parsing response...");

            // Read the response content as a JSON string
            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response JSON: {json}");

            // Deserialize the JSON string into a list of Message objects
            var messages = System.Text.Json.JsonSerializer.Deserialize<List<Message>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // Allow case-insensitive property matching
            });

            Console.WriteLine($"Parsed {messages?.Count ?? 0} messages.");
            return messages; // Return the list of messages
        }
        else
        {
            // Log the failure status code and return an empty list
            Console.WriteLine($"Request failed with status code: {response.StatusCode}");
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

// Model class representing a message record in the "Messages" table
public class Message
{
    public long Id { get; set; } // Unique identifier for the message
    public string username { get; set; } // Username of the message sender
    public string message { get; set; } // Text content of the message
    public string timestamp { get; set; } // Timestamp when the message was sent
}
