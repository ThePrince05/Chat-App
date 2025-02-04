using Chat_App.MVVM.Model;
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
                Colour TEXT NOT NULL
            );";

        string createSettingsTableQuery = @"
            CREATE TABLE IF NOT EXISTS Settings (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                SupabaseUrl TEXT,
                SupabaseApiKey TEXT
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

    public UserModel LoadUser()
    {
        try
        {
            using (var connection = new SQLiteConnection($"Data Source={_databaseFilePath};Version=3;"))
            {
                connection.Open();

                string query = "SELECT Username, Colour FROM User LIMIT 1";
                using (var command = new SQLiteCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string username = reader["Username"].ToString();
                            string colorHex = reader["Colour"].ToString();

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

        return new UserModel
        {
            Username = string.Empty,
            SelectedColor = Brushes.White
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
}
