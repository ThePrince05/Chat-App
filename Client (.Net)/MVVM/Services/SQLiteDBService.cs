using Chat_App.MVVM.Model;
using Client__.Net_.MVVM.Model;
using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Windows.Media;
using User = Chat_App.MVVM.Model.User;

public class SQLiteDBService
{
    private readonly string _databaseFolderPath;
    private readonly string _databaseFilePath;

    public SQLiteDBService()
    {
        _databaseFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Chat App");
        _databaseFilePath = Path.Combine(_databaseFolderPath, "ChatAppDatabase.db");
    }

    public void InitializeDatabase()
    {
        try
        {
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

            if (!File.Exists(_databaseFilePath))
            {
                Debug.WriteLine($"Creating database file: {_databaseFilePath}");
                SQLiteConnection.CreateFile(_databaseFilePath);
                Debug.WriteLine("Database file created successfully.");

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
        string createUserTableQuery = @"
        CREATE TABLE IF NOT EXISTS User (
            UserID INTEGER PRIMARY KEY AUTOINCREMENT,
            Username TEXT NOT NULL,
            SelectedColour TEXT NOT NULL
        );";

        string createSettingsTableQuery = @"
        CREATE TABLE IF NOT EXISTS Settings (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            SupabaseUrl TEXT,
            SupabaseApiKey TEXT
        );";

        string createLoginTableQuery = @"
        CREATE TABLE IF NOT EXISTS Login (
            LoginID INTEGER PRIMARY KEY AUTOINCREMENT,
            UserID INTEGER NOT NULL,
            UserLogin BOOLEAN NOT NULL DEFAULT 0,
            FOREIGN KEY (UserID) REFERENCES users(UserID) ON DELETE CASCADE
        );";

        using (var command = new SQLiteCommand(createUserTableQuery, connection))
        {
            command.ExecuteNonQuery();
            Debug.WriteLine("Table 'users' created successfully.");
        }

        using (var command = new SQLiteCommand(createSettingsTableQuery, connection))
        {
            command.ExecuteNonQuery();
            Debug.WriteLine("Table 'settings' created successfully.");
        }

        using (var command = new SQLiteCommand(createLoginTableQuery, connection))
        {
            command.ExecuteNonQuery();
            Debug.WriteLine("Table 'login' created successfully.");
        }
    }


    public void SaveUser(string username, string color)
    {
        try
        {
            using (var connection = new SQLiteConnection($"Data Source={_databaseFilePath};Version=3;"))
            {
                connection.Open();

                string checkUserQuery = "SELECT COUNT(*) FROM User";
                using (var command = new SQLiteCommand(checkUserQuery, connection))
                {
                    long userCount = (long)command.ExecuteScalar();

                    if (userCount > 0)
                    {
                        string deleteQuery = "DELETE FROM User";
                        using (var deleteCommand = new SQLiteCommand(deleteQuery, connection))
                        {
                            deleteCommand.ExecuteNonQuery();
                            Debug.WriteLine("Existing user deleted.");
                        }
                    }

                    string insertQuery = "INSERT INTO User (Username, SelectedColour) VALUES (@Username, @Colour)";
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

    public void SaveSettings(string supabaseUrl, string supabaseApiKey)
    {
        try
        {
            using (var connection = new SQLiteConnection($"Data Source={_databaseFilePath};Version=3;"))
            {
                connection.Open();

                var deleteCommand = new SQLiteCommand("DELETE FROM Settings", connection);
                deleteCommand.ExecuteNonQuery();

                var insertCommand = new SQLiteCommand("INSERT INTO Settings (SupabaseUrl, SupabaseApiKey) VALUES (@SupabaseUrl, @SupabaseApiKey)", connection);
                insertCommand.Parameters.AddWithValue("@SupabaseUrl", supabaseUrl);
                insertCommand.Parameters.AddWithValue("@SupabaseApiKey", supabaseApiKey);

                insertCommand.ExecuteNonQuery();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error saving settings: {ex.Message}");
            throw;
        }
    }

    public User LoadUser()
    {
        try
        {
            using (var connection = new SQLiteConnection($"Data Source={_databaseFilePath};Version=3;"))
            {
                connection.Open();

                // Check if the User table exists
                if (!TableExists(connection, "User"))
                {
                    Debug.WriteLine("User table does not exist.");
                    return new User
                    {
                        Username = string.Empty,
                        SelectedColor = "#FFFFFF" // Default white color as a hex string
                    };
                }

                string query = "SELECT Username, SelectedColour FROM User LIMIT 1";
                using (var command = new SQLiteCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string username = reader["Username"].ToString();
                            string selectedColour = reader["SelectedColour"].ToString();

                            return new User
                            {
                                Username = username,
                                SelectedColor = selectedColour
                            };
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading user data: {ex.Message}");
        }

        // Return a default user if no data is found or an error occurs
        return new User
        {
            Username = string.Empty,
            SelectedColor = "#FFFFFF" // Default white color as a hex string
        };
    }


    public SupabaseSettings LoadSettings()
    {
        try
        {
            var supabaseSettings = new SupabaseSettings();

            using (var connection = new SQLiteConnection($"Data Source={_databaseFilePath};Version=3;"))
            {
                connection.Open();

                // Check if the Settings table exists
                if (!TableExists(connection, "Settings"))
                {
                    Debug.WriteLine("Settings table does not exist.");
                    return supabaseSettings; // Return default settings if the table doesn't exist
                }

                string query = "SELECT SupabaseUrl, SupabaseApiKey FROM Settings LIMIT 1";
                using (var command = new SQLiteCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            supabaseSettings.SupabaseUrl = reader["SupabaseUrl"].ToString();
                            supabaseSettings.SupabaseApiKey = reader["SupabaseApiKey"].ToString();
                        }
                    }
                }
            }

            return supabaseSettings;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading settings: {ex.Message}");
            return null;
        }
    }

    private bool TableExists(SQLiteConnection connection, string tableName)
    {
        string query = $"SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='{tableName}';";
        using (var command = new SQLiteCommand(query, connection))
        {
            long result = (long)command.ExecuteScalar();
            return result > 0;
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
            return false;
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

    public bool CheckUserExists(string username)
    {
        try
        {
            using (var connection = new SQLiteConnection($"Data Source={_databaseFilePath};Version=3;"))
            {
                connection.Open();

                string query = "SELECT EXISTS (SELECT 1 FROM User WHERE Username = @Username LIMIT 1)";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    return Convert.ToBoolean(command.ExecuteScalar());
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error checking if user '{username}' exists: {ex.Message}");
            return false;
        }
    }

    public bool IsUserLoggedIn()
    {
        try
        {
            using (var connection = new SQLiteConnection($"Data Source={_databaseFilePath};Version=3;"))
            {
                connection.Open();

                string query = "SELECT UserLogin FROM Login ORDER BY LoginID DESC LIMIT 1";
                using (var command = new SQLiteCommand(query, connection))
                {
                    object result = command.ExecuteScalar();
                    return result != null && Convert.ToBoolean(result);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error checking UserLogin status: {ex.Message}");
            return false; // Default to false in case of error
        }
    }

    public void UpdateUserLoginStatus(bool isLoggedIn)
    {
        try
        {
            using (var connection = new SQLiteConnection($"Data Source={_databaseFilePath};Version=3;"))
            {
                connection.Open();

                string query = "UPDATE Login SET UserLogin = @UserLogin";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserLogin", isLoggedIn ? 1 : 0);
                    command.ExecuteNonQuery();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error updating UserLogin status: {ex.Message}");
        }
    }

    public void InsertUserLoginStatus(string username, bool isLoggedIn)
    {
        try
        {
            using (var connection = new SQLiteConnection($"Data Source={_databaseFilePath};Version=3;"))
            {
                connection.Open();

                // Retrieve UserID based on the provided Username
                string getUserQuery = "SELECT UserID FROM User WHERE Username = @Username";
                int? userId = null;

                using (var getUserCommand = new SQLiteCommand(getUserQuery, connection))
                {
                    getUserCommand.Parameters.AddWithValue("@Username", username);
                    var result = getUserCommand.ExecuteScalar();
                    if (result != null)
                    {
                        userId = Convert.ToInt32(result);
                    }
                }

                if (userId == null)
                {
                    Debug.WriteLine($"Error: Username '{username}' not found in User table.");
                    return;
                }

                // Insert into Login table
                string query = "INSERT INTO Login (UserID, UserLogin) VALUES (@UserID, @UserLogin)";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserID", userId);
                    command.Parameters.AddWithValue("@UserLogin", isLoggedIn ? 1 : 0);
                    command.ExecuteNonQuery();
                }

                Debug.WriteLine($"Inserted UserLogin status for Username '{username}' (UserID {userId}): {isLoggedIn}");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error inserting UserLogin status: {ex.Message}");
        }
    }



}
