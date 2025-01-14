using System;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;

public class DatabaseService
{
    private readonly string _databaseFolderPath;
    private readonly string _databaseFilePath;

    public DatabaseService()
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
                SupabaseURL TEXT NOT NULL,
                SupabaseAPIKey TEXT NOT NULL,
                ServerStatus TEXT NOT NULL,
                ServerIPAddress TEXT NOT NULL,
                ServerPortNumber TEXT NOT NULL
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

                // Check if the user already exists in the database
                string checkUserQuery = "SELECT COUNT(*) FROM User WHERE Username = @Username";
                using (var command = new SQLiteCommand(checkUserQuery, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    long userCount = (long)command.ExecuteScalar();

                    // If the user exists, update their color
                    if (userCount > 0)
                    {
                        string updateQuery = "UPDATE User SET Colour = @Colour WHERE Username = @Username";
                        using (var updateCommand = new SQLiteCommand(updateQuery, connection))
                        {
                            updateCommand.Parameters.AddWithValue("@Colour", color);
                            updateCommand.Parameters.AddWithValue("@Username", username);
                            updateCommand.ExecuteNonQuery();
                            Debug.WriteLine("User color updated successfully.");
                        }
                    }
                    // If the user doesn't exist, insert a new user with the color
                    else
                    {
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
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error saving user: {ex.Message}");
            throw;
        }
    }



    public string GetDatabaseFilePath()
    {
        return _databaseFilePath;
    }
}
