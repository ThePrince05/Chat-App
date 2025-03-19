using Client__.Net_.MVVM.Model;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Diagnostics;
using Client__.Net_.MVVM.View;
using Chat_App.Core.Model;
using Newtonsoft.Json.Linq;

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

    // ✅ URL validation helper method
    private bool IsValidUrl(string url)
    {
        Uri uriResult;
        return Uri.TryCreate(url, UriKind.Absolute, out uriResult)
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
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

    public async Task<List<Message>> GetMessagesAsync(int groupId)
    {
        var messages = new List<Message>();

        try
        {
            var response = await _httpClient.GetAsync($"usermessages?groupid=eq.{groupId}&order=sentat.asc");

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"Error fetching messages: {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
                return messages;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(responseContent);

            if (result == null || result.Count == 0)
            {
                Debug.WriteLine("No messages retrieved.");
                return messages;
            }

            foreach (var item in result)
            {
                int userId = Convert.ToInt32(item["userid"]);
                string username = await GetUsernameByUserIdAsync(userId); // Fetch username from Users table

                messages.Add(new Message
                {
                    Id = Convert.ToInt32(item["messageid"]),
                    username = username, // Assign retrieved username
                    message = item["content"].ToString(),
                    timestamp = item["sentat"].ToString()
                });
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in GetMessagesAsync: {ex.Message}");
        }

        return messages;
    }
    public async Task<string> GetUsernameByUserIdAsync(int userId)
    {
        if (_httpClient == null)
        {
            Debug.WriteLine("HTTP client is not initialized.");
            return "Unknown";
        }

        try
        {
            var response = await _httpClient.GetAsync($"users?userid=eq.{userId}");

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"Error fetching username: {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
                return "Unknown";
            }

            var json = await response.Content.ReadAsStringAsync();
            var users = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(json);

            if (users != null && users.Count > 0 && users.First().ContainsKey("username"))
            {
                return users.First()["username"].ToString();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in GetUsernameByUserIdAsync: {ex.Message}");
        }

        return "Unknown";
    }


    public async Task<bool> SaveMessageAsync(string username, string message, int groupId)
    {
        Debug.WriteLine("Saving message to Supabase...");

        if (string.IsNullOrEmpty(_supabaseUrl) || string.IsNullOrEmpty(_supabaseApiKey))
        {
            if (!_hasConnectionFailed)
            {
                OnConnectionFailed?.Invoke("Supabase URL or API Key is missing. Please enter the values in settings.");
                _hasConnectionFailed = true;
            }
            return false;
        }

        // Get UserID from Username
        int userId = await GetUserIdByUsernameAsync(username);
        if (userId <= 0)
        {
            Debug.WriteLine("User ID not found.");
            return false;
        }

        // Prepare message data (do not include messageid so the database can auto-generate it)
        var messageData = new
        {
            userid = userId,
            groupid = groupId,
            content = message,
            sentat = DateTime.UtcNow // Use proper timestamp format
        };

        var contentPayload = new StringContent(JsonConvert.SerializeObject(messageData), Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync("usermessages", contentPayload); // Correct table name

            if (response.IsSuccessStatusCode)
            {
                Debug.WriteLine("Message saved successfully!");
                return true;
            }

            string errorContent = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"Error: {response.StatusCode}, {errorContent}");

            // Check for duplicate key error (conflict)
            if (errorContent.Contains("duplicate key value violates unique constraint"))
            {
                Debug.WriteLine("Duplicate key error: This likely means that the sequence for messageid is out-of-sync. " +
                    "Please run the following SQL on your database to update the sequence:\n" +
                    "SELECT setval('usermessages_messageid_seq', (SELECT MAX(messageid) FROM usermessages)+1);");
            }
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"An error occurred: {ex.Message}");
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

    public async Task<List<string>> GetAllUsernamesAsync(string currentUsername)
    {
        Console.WriteLine("Fetching all usernames except the current logged-in user...");

        if (string.IsNullOrEmpty(_supabaseUrl) || string.IsNullOrEmpty(_supabaseApiKey))
        {
            if (!_hasConnectionFailed)
            {
                OnConnectionFailed?.Invoke("Supabase URL or API Key is missing. Please enter the values in settings.");
                _hasConnectionFailed = true;
            }
            return new List<string>();
        }

        try
        {
            // Fetch only usernames excluding the current user
            var response = await _httpClient.GetAsync($"users?username=neq.{currentUsername}&select=username");

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var users = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(jsonResponse);

                return users?.Select(u => u["username"]).ToList() ?? new List<string>();
            }
            else
            {
                Console.WriteLine($"Error fetching usernames: {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"An error occurred while fetching usernames: {ex.Message}");
        }

        return new List<string>();
    }

    public async Task<int> InsertGroup(string groupName)
    {
        var groupData = new { groupname = groupName };
        var content = new StringContent(JsonConvert.SerializeObject(groupData), Encoding.UTF8, "application/json");

        Debug.WriteLine($"Sending request to create group: {JsonConvert.SerializeObject(groupData)}");

        var response = await _httpClient.PostAsync("groups", content);
        var jsonResponse = await response.Content.ReadAsStringAsync();
        Debug.WriteLine($"API Raw Response: {jsonResponse}");

        if (!response.IsSuccessStatusCode)
        {
            Debug.WriteLine($"Error creating group: {response.StatusCode}, Content: {jsonResponse}");
            return -1; // Return -1 to indicate failure
        }

        // Since Supabase isn't returning the groupid, fetch it manually
        return await GetGroupIdByNameAsync(groupName);
    }

    private async Task<int> GetGroupIdByNameAsync(string groupName)
    {
        Debug.WriteLine($"Fetching group ID for group: {groupName}");

        var response = await _httpClient.GetAsync($"groups?groupname=eq.{groupName}&select=groupid");
        var jsonResponse = await response.Content.ReadAsStringAsync();
        Debug.WriteLine($"API Response for group ID lookup: {jsonResponse}");

        if (!response.IsSuccessStatusCode)
        {
            Debug.WriteLine($"Error fetching group ID: {response.StatusCode}, Content: {jsonResponse}");
            return -1;
        }

        try
        {
            var result = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(jsonResponse);
            if (result != null && result.Count > 0 && result.First().ContainsKey("groupid"))
            {
                int groupId = Convert.ToInt32(result.First()["groupid"]);
                Debug.WriteLine($"Fetched Group ID: {groupId}");
                return groupId;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error parsing group ID response: {ex.Message}");
        }

        Debug.WriteLine("Error: 'groupid' not found in the fetched response.");
        return -1;
    }



    // Helper method to get UserID by username
    private async Task<int> GetUserIdByUsernameAsync(string username)
    {
        if (_httpClient == null)
        {
            Debug.WriteLine("Error: HTTP client is not initialized.");
            return -1;
        }

        try
        {
            var response = await _httpClient.GetAsync($"users?username=eq.{username}");

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"Error fetching user ID: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                return -1;
            }

            var responseContent = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(responseContent))
            {
                Debug.WriteLine("Error: API returned an empty response.");
                return -1;
            }

            var result = JsonConvert.DeserializeObject<JArray>(responseContent);
            if (result != null && result.Count > 0)
            {
                return Convert.ToInt32(result.First()["userid"]); // 'userid' in lowercase
            }
            else
            {
                Debug.WriteLine("Warning: User not found in database.");
            }
        }
        catch (HttpRequestException ex)
        {
            Debug.WriteLine($"Network error: {ex.Message}");
        }
        catch (System.Text.Json.JsonException ex)
        {
            Debug.WriteLine($"JSON parsing error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Unexpected error: {ex.Message}");
        }

        return -1; // Return -1 if any issue occurs
    }


    public async Task AddGroupMembersAsync(int groupId, List<string> selectedUsernames, string currentUserUsername)
    {
        // Combine the selected usernames with the current user
        var allUsernames = new List<string>(selectedUsernames) { currentUserUsername };

        foreach (var username in allUsernames)
        {
            // Get the userId from the username
            var userId = await GetUserIdByUsernameAsync(username);

            // If the user ID is valid (greater than 0), add them to the GroupMembers table
            if (userId > 0)
            {
                // Ensure JSON keys match column names in the database (lowercase)
                var groupMemberData = new { userid = userId, groupid = groupId };
                var groupMemberContent = new StringContent(JsonConvert.SerializeObject(groupMemberData), Encoding.UTF8, "application/json");

                // Insert the user into the GroupMembers table
                var memberResponse = await _httpClient.PostAsync("groupmembers", groupMemberContent);

                if (memberResponse.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"User {username} added to group {groupId}.");
                }
                else
                {
                    // Log error if adding user fails
                    string memberErrorContent = await memberResponse.Content.ReadAsStringAsync();
                    Debug.WriteLine($"Error adding user {username} to group {groupId}: {memberResponse.StatusCode}, {memberErrorContent}");
                }
            }
            else
            {
                Debug.WriteLine($"User {username} not found. Skipping...");
            }
        }
    }
    public async Task<List<Group>> GetUserGroupsAsync(string username)
    {
        var groups = new List<Group>();

        // Step 1: Get the userId from the username
        int userId = await GetUserIdByUsernameAsync(username);
        if (userId <= 0)
        {
            Debug.WriteLine("User ID not found.");
            return groups;
        }

        // Step 2: Fetch group IDs from groupmembers table
        var response = await _httpClient.GetAsync($"groupmembers?userid=eq.{userId}");
        if (!response.IsSuccessStatusCode)
        {
            Debug.WriteLine($"Error fetching group memberships: {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
            return groups;
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var groupMemberships = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(responseContent);
        if (groupMemberships == null || groupMemberships.Count == 0)
        {
            Debug.WriteLine("User is not in any groups.");
            return groups;
        }

        var groupIds = groupMemberships.Select(g => Convert.ToInt32(g["groupid"])).ToList();

        // Step 3: Fetch group names and last message from the database
        foreach (var groupId in groupIds)
        {
            var groupResponse = await _httpClient.GetAsync($"groups?groupid=eq.{groupId}");
            if (!groupResponse.IsSuccessStatusCode)
            {
                Debug.WriteLine($"Error fetching group info for group ID {groupId}: {groupResponse.StatusCode}, {await groupResponse.Content.ReadAsStringAsync()}");
                continue;
            }

            var groupContent = await groupResponse.Content.ReadAsStringAsync();
            var groupData = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(groupContent);
            if (groupData == null || groupData.Count == 0)
            {
                continue;
            }

            string groupName = groupData.First()["groupname"].ToString();

            // Step 4: Fetch the most recent message for the group
            string lastMessage = "No Messages";
            var messageResponse = await _httpClient.GetAsync($"usermessages?groupid=eq.{groupId}&order=sentat.desc&limit=1");
            if (messageResponse.IsSuccessStatusCode)
            {
                var messageContent = await messageResponse.Content.ReadAsStringAsync();
                var messages = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(messageContent);
                if (messages != null && messages.Count > 0)
                {
                    lastMessage = messages.First()["content"].ToString();
                }
            }

            // Step 5: Add group with last message
            groups.Add(new Group
            {
                Id = groupId,
                GroupName = groupName,
                Messages = lastMessage,
                ImageSource = "https://img.freepik.com/free-photo/people-posing-together-registration-day_23-2149096794.jpg"
            });
        }

        return groups;
    }



    public async Task<List<Message>> GetMessagesByGroupIdAsync(int groupId)
    {
        if (_httpClient == null)
        {
            Debug.WriteLine("Error: HTTP client is not initialized.");
            return new List<Message>();
        }

        try
        {
            var response = await _httpClient.GetAsync($"usermessages?groupid=eq.{groupId}&order=sentat.asc");

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"Error fetching messages: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                return new List<Message>();
            }

            var json = await response.Content.ReadAsStringAsync();
            var messages = JsonConvert.DeserializeObject<List<Message>>(json);

            return messages ?? new List<Message>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Unexpected error fetching messages: {ex.Message}");
            return new List<Message>();
        }
    }

}
