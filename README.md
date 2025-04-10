# 💬 Chat Application Using Supabase

## 📄 Description

This is a group chat application that allows users to create and join group chats, where they can exchange messages with fellow members in real time. The goal of this project is to strengthen our collaboration as developers, enhance our skills with WPF, and understand how a typical chat application works under the hood.

### ✨ Key Features

-	🧑‍💼 User profile creation and modification
-	👥 Group creation and deletion
-	🔔 Desktop notifications for new messages
-	🔍 Filter groups by group name
-	🎨 Customizable user profile themes

### 🛠️ Technologies Used
-	WPF (MVVM architecture)
-	Supabase REST API
-	SQLite
-	.NET 8.0

## 🧰 Installation

### Requirements
-	Windows 10 (version 10.0.17763.0 or later)
-	.NET 8.0 SDK
-	Internet connection (required to interact with Supabase)
-	Visual Studio (for building and running the project)

## 🚀 Usage
- Setting Up Supabase
- Create an account at Supabase and start a new project.
- In your project dashboard, get your Project URL and API Key from Settings > API.
- Go to the SQL Editor and execute the following stored function to allow the app to initialize the required tables:

```sql
create or replace function execute_sql(sql_statement text)
returns void
language plpgsql
security definer
as $$
begin
  execute sql_statement;
end;
$$;
```

## Running the App
1.	Open the project in Visual Studio.
2.	Run the application.
3.	On startup, enter your Supabase Project URL and API Key.
4.	Create a profile, log in, and start creating or joining group chats using the "Edit" button on the main screen.


## 🤝Contributing and Contacts
We welcome contributions, suggestions, or questions!
Feel free to reach out via email: princesithole49@gmail.com


## 🙌 Acknowledges
Special thanks to:
- [@JimmyNos](https://github.com/JimmyNos) (Quality Assurance & Co-Programmer)
- [@Kioe-Saiba-Archfey-Warlock](https://github.com/Kioe-Saiba-Archfey-Warlock) (Product Feature Owner)
