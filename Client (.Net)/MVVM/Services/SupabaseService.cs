using Client__.Net_.MVVM.Model;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Diagnostics;
using Client__.Net_.MVVM.View;
using Chat_App.MVVM.Model;

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

    public async Task<bool> InitializeDatabaseSchemaAsync()
    {
        // Check if _supabaseUrl is null or empty before proceeding
        if (string.IsNullOrEmpty(_supabaseUrl))
        {
            Debug.WriteLine("Supabase URL is null or empty. Initialization aborted.");
            return false;
        }

        // The SQL to initialize the schema, using SERIAL for auto-incrementing primary keys
        var sql = @"
            CREATE TABLE IF NOT EXISTS Users (
                UserID SERIAL PRIMARY KEY,
                Username TEXT UNIQUE NOT NULL,
                UserPassword TEXT NOT NULL,
                SelectedColour TEXT NOT NULL,
                CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Groups (
                GroupID SERIAL PRIMARY KEY,
                GroupName TEXT NOT NULL,
                CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL
            );

            CREATE TABLE IF NOT EXISTS GroupMembers (
                MembershipID SERIAL PRIMARY KEY,
                UserID INTEGER NOT NULL,
                GroupID INTEGER NOT NULL,
                JoinedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
                FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE CASCADE,
                FOREIGN KEY (GroupID) REFERENCES Groups(GroupID) ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS UserMessages (
                MessageID SERIAL PRIMARY KEY,
                GroupID INTEGER NOT NULL,
                UserID INTEGER NOT NULL,
                Content TEXT NOT NULL,
                SentAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
                FOREIGN KEY (GroupID) REFERENCES Groups(GroupID) ON DELETE CASCADE,
                FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE CASCADE
            );
        ";

        // Ensure SQL string is not null or empty
        if (string.IsNullOrEmpty(sql))
        {
            Debug.WriteLine("SQL schema is null or empty. Initialization aborted.");
            return false;
        }

        // Prepare the body for the POST request
        var requestBody = new
        {
            sql_statement = sql
        };

        var jsonRequestBody = JsonConvert.SerializeObject(requestBody);

        // Create the StringContent with Content-Type set
        var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

        try
        {
            // Call the execute_sql function via the Supabase API
            var response = await _httpClient.PostAsync(
                $"rpc/execute_sql", content // Use the rpc endpoint to execute SQL directly
            );

            // Ensure the request was successful
            if (response.IsSuccessStatusCode)
            {
                Debug.WriteLine("Database schema initialized successfully.");
                return true;
            }
            else
            {
                // Log error response if not successful
                Debug.WriteLine($"Error: {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception: {ex.Message}");
            return false;
        }
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
            var response = await _httpClient.GetAsync("Messages"); // Make sure the table name is correct (user_messages)

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

        try
        {
            var response = await _httpClient.PostAsync("Messages", content); // Check if 'user_messages' is the correct table

            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            Console.WriteLine($"Error: {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            return false;
        }
    }
    public async Task<bool> InsertUserAsync(string username, string password, string selectedColor)
    {
        Console.WriteLine("Inserting user into Supabase...");

        // Validate the Supabase URL and API Key before making the request
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

        // First, check if the username already exists
        var checkUsernameResponse = await _httpClient.GetAsync($"users?username=eq.{username}");

        if (checkUsernameResponse.IsSuccessStatusCode)
        {
            var responseContent = await checkUsernameResponse.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(responseContent) && responseContent != "[]")
            {
                // Username already exists
                Debug.WriteLine("Username already exists.");
                return false;
            }
        }
        else
        {
            Console.WriteLine($"Error checking username: {checkUsernameResponse.StatusCode}, {await checkUsernameResponse.Content.ReadAsStringAsync()}");
            return false;
        }

        // If the username is unique, proceed with the insertion
        var userData = new
        {
            username = username,
            userpassword = password,
            selectedcolour = selectedColor
        };

        var content = new StringContent(JsonConvert.SerializeObject(userData), Encoding.UTF8, "application/json");

        try
        {
            // Perform a POST request to insert the user into the Users table
            var response = await _httpClient.PostAsync("users", content); // Ensure "users" matches your Supabase table name

            // Check if the request was successful
            if (response.IsSuccessStatusCode)
            {
                Debug.WriteLine("User inserted successfully.");
                return true;
            }

            // If not successful, log the error
            Debug.WriteLine($"Error: {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
            return false;
        }
        catch (Exception ex)
        {
            // Log any exceptions that occur
            Debug.WriteLine($"An error occurred while inserting the user: {ex.Message}");
            return false;
        }
    }

    public async Task<User> GetUserByUsernameAsync(string username)
    {
        if (_httpClient == null)
        {
            Debug.WriteLine("Error: HTTP client is not initialized.");
            return null;
        }

        try
        {
            var response = await _httpClient.GetAsync($"users?username=eq.{username}");

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"Error fetching user: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(json))
            {
                Debug.WriteLine("Error: API returned an empty response.");
                return null;
            }

            var users = JsonConvert.DeserializeObject<List<User>>(json);
            return users?.FirstOrDefault();
        }
        catch (HttpRequestException ex)
        {
            Debug.WriteLine($"HTTP request error: {ex.Message}");
        }
        catch (System.Text.Json.JsonException ex)
        {
            Debug.WriteLine($"JSON parsing error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Unexpected error: {ex.Message}");
        }

        return null;
    }

    public async Task<bool> UpdateUserAsync(string username, string encryptedPassword, string selectedColorHex)
    {
        try
        {
            Debug.WriteLine($"Starting UpdateUserAsync for username: {username}");

            var requestBody = new
            {
                userpassword = encryptedPassword,  // Ensure lowercase "userpassword"
                selectedcolour = selectedColorHex  // Ensure lowercase "selectedcolour"
            };

            string jsonData = JsonConvert.SerializeObject(requestBody);
            Debug.WriteLine($"Request JSON: {jsonData}");

            var jsonContent = new StringContent(jsonData, Encoding.UTF8, "application/json");

            Debug.WriteLine($"Sending PATCH request to Supabase for user: {username}");

            var response = await _httpClient.PatchAsync($"users?username=eq.{username}", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                Debug.WriteLine("User update successful.");
                return true;
            }
            else
            {
                string errorResponse = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"User update failed. Status Code: {response.StatusCode}, Response: {errorResponse}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in UpdateUserAsync: {ex.Message}");
            return false;
        }
    }



    // ✅ URL validation helper method
    private bool IsValidUrl(string url)
    {
        Uri uriResult;
        return Uri.TryCreate(url, UriKind.Absolute, out uriResult)
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
