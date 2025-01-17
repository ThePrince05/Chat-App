using Chat_App.MVVM.Model;
using Chat_App.Net;
using Client__.Net_.MVVM.Model;
using System;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Windows.Media;

public class SQLiteDBService
{
    private readonly string _databaseFolderPath;
    private readonly string _databaseFilePath;

    public SQLiteDBService()
    {
        // Set the folder and database file paths to AppData
        _databaseFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Chat App");
        _databaseFilePath = Path.Combine(_databaseFolderPath, "ChatAppDatabase.db");
    }

    public void InitializeDatabase()
    {
        try
        {
            // Ensure the folder exists
            if (!Directory.Exists(_databaseFolderPath))
            {
                Debug.WriteLine($"Creating folder: {_databaseFolderPath}");
                Directory.CreateDirectory(_databaseFolderPath);
                Debug.WriteLine("Folder created successfully.");
            }
            else
            {
                Debug.WriteLine("Folder already exists.");
            }

            // Check if the database file exists
            if (!File.Exists(_databaseFilePath))
            {
                Debug.WriteLine($"Creating database file: {_databaseFilePath}");
                SQLiteConnection.CreateFile(_databaseFilePath);
                Debug.WriteLine("Database file created successfully.");

                // Initialize the database with necessary tables
                using (var connection = new SQLiteConnection($"Data Source={_databaseFilePath};Version=3;"))
                {
                    connection.Open();

                    CreateTables(connection);

                    Debug.WriteLine("Tables created successfully.");
                }
            }
            else
            {
                Debug.WriteLine("Database file already exists. No changes made.");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error initializing the database: {ex.Message}");
            throw;
        }
    }

    private void CreateTables(SQLiteConnection connection)
    {
        // Create User table
        string createUserTableQuery = @"
            CREATE TABLE IF NOT EXISTS User (
                UserID INTEGER PRIMARY KEY AUTOINCREMENT,
                Username TEXT NOT NULL,
                Colour TEXT NOT NULL
            );";

        // Create Settings table
        string createSettingsTableQuery = @"
            CREATE TABLE IF NOT EXISTS Settings (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            SupabaseUrl TEXT,
            SupabaseApiKey TEXT,
            IsDedicatedServerEnabled BOOLEAN,
            ServerIp TEXT,
            ServerPort INTEGER
        );";

        using (var command = new SQLiteCommand(createUserTableQuery, connection))
        {
            command.ExecuteNonQuery();
            Debug.WriteLine("Table 'User' created successfully.");
        }

        using (var command = new SQLiteCommand(createSettingsTableQuery, connection))
        {
            command.ExecuteNonQuery();
            Debug.WriteLine("Table 'Settings' created successfully.");
        }
    }

    public void SaveUser(string username, string color)
    {
        try
        {
            using (var connection = new SQLiteConnection($"Data Source={_databaseFilePath};Version=3;"))
            {
                connection.Open();

                // Check if there's already a user in the database
                string checkUserQuery = "SELECT COUNT(*) FROM User";
                using (var command = new SQLiteCommand(checkUserQuery, connection))
                {
                    long userCount = (long)command.ExecuteScalar();

                    // If there's a user already in the table, delete them
                    if (userCount > 0)
                    {
                        string deleteQuery = "DELETE FROM User";
                        using (var deleteCommand = new SQLiteCommand(deleteQuery, connection))
                        {
                            deleteCommand.ExecuteNonQuery();
                            Debug.WriteLine("Existing user deleted.");
                        }
                    }

                    // Insert the new user
                    string insertQuery = "INSERT INTO User (Username, Colour) VALUES (@Username, @Colour)";
                    using (var insertCommand = new SQLiteCommand(insertQuery, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@Username", username);
                        insertCommand.Parameters.AddWithValue("@Colour", color);
                        insertCommand.ExecuteNonQuery();
                        Debug.WriteLine("New user saved successfully.");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error saving user: {ex.Message}");
            throw;
        }
    }


    public void SaveSettings(string supabaseUrl, string supabaseApiKey, bool isDedicatedServerEnabled, string serverIp, int serverPort)
    {
        try
        {
            using (var connection = new SQLiteConnection($"Data Source={_databaseFilePath};Version=3;"))
            {
                connection.Open();

                // Delete existing record to ensure only one record remains in the table
                var deleteCommand = new SQLiteCommand("DELETE FROM Settings", connection);
                deleteCommand.ExecuteNonQuery();

                // Insert the new settings
                var insertCommand = new SQLiteCommand("INSERT INTO Settings (SupabaseUrl, SupabaseApiKey, IsDedicatedServerEnabled, ServerIp, ServerPort) VALUES (@SupabaseUrl, @SupabaseApiKey, @IsDedicatedServerEnabled, @ServerIp, @ServerPort)", connection);
                insertCommand.Parameters.AddWithValue("@SupabaseUrl", supabaseUrl);
                insertCommand.Parameters.AddWithValue("@SupabaseApiKey", supabaseApiKey);
                insertCommand.Parameters.AddWithValue("@IsDedicatedServerEnabled", isDedicatedServerEnabled);
                insertCommand.Parameters.AddWithValue("@ServerIp", serverIp);
                insertCommand.Parameters.AddWithValue("@ServerPort", serverPort);

                insertCommand.ExecuteNonQuery();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error saving settings: {ex.Message}");
            throw;
        }
    }

public UserModel LoadUser()
    {
        try
        {
            using (var connection = new SQLiteConnection($"Data Source={_databaseFilePath};Version=3;"))
            {
                connection.Open();

                // Fetch the first user record (assuming there is only one user for now)
                string query = "SELECT Username, Colour FROM User LIMIT 1";
                using (var command = new SQLiteCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string username = reader["Username"].ToString();
                            string colorHex = reader["Colour"].ToString();

                            // Convert colorHex to SolidColorBrush
                            var colorBrush = (SolidColorBrush)new BrushConverter().ConvertFromString(colorHex);

                            return new UserModel
                            {
                                Username = username,
                                SelectedColor = colorBrush
                            };
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading user data: {ex.Message}");
            throw;
        }

        // Return a default user model if no data is found
        return new UserModel
        {
            Username = string.Empty,
            SelectedColor = Brushes.White // Default color
        };
    }

    public (ServerSettings, SupabaseSettings) LoadSettings()
    {
        try
        {
            var serverSettings = new ServerSettings();
            var supabaseSettings = new SupabaseSettings();

            using (var connection = new SQLiteConnection($"Data Source={_databaseFilePath};Version=3;"))
            {
                connection.Open();

                // Query to fetch the settings
                string query = "SELECT ServerIp, ServerPort, IsDedicatedServerEnabled, SupabaseUrl, SupabaseApiKey FROM Settings LIMIT 1"; // Adjust query for your schema
                using (var command = new SQLiteCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Initialize the ServerSettings
                            serverSettings.ServerIp = reader["ServerIp"].ToString();
                            serverSettings.ServerPort = reader["ServerPort"].ToString();
                            serverSettings.IsDedicatedServerEnabled = Convert.ToBoolean(reader["IsDedicatedServerEnabled"]);

                            // Initialize the SupabaseSettings
                            supabaseSettings.SupabaseUrl = reader["SupabaseUrl"].ToString();
                            supabaseSettings.SupabaseApiKey = reader["SupabaseApiKey"].ToString();
                        }
                    }
                }
            }

            return (serverSettings, supabaseSettings);
        }
        catch (Exception ex)
        {
            // Handle exceptions (log, show message, etc.)
            Console.WriteLine($"Error loading settings: {ex.Message}");
            return (null, null); // Return null or default values in case of error
        }
    }


    public bool TableHasData(string tableName)
    {
        try
        {
            using (var connection = new SQLiteConnection($"Data Source={_databaseFilePath};Version=3;"))
            {
                connection.Open();

                string query = $"SELECT EXISTS (SELECT 1 FROM {tableName} LIMIT 1)";
                using (var command = new SQLiteCommand(query, connection))
                {
                    return Convert.ToBoolean(command.ExecuteScalar());
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error checking data in table '{tableName}': {ex.Message}");
            return false; // Assume the table has no data in case of an error
        }
    }

    public (bool IsUserDataPresent, bool IsSettingsDataPresent) CheckInitializationState()
    {
        bool isUserDataPresent = TableHasData("User");
        bool isSettingsDataPresent = TableHasData("Settings");

        return (isUserDataPresent, isSettingsDataPresent);
    }


    public string GetDatabaseFilePath()
    {
        return _databaseFilePath;
    }
}
