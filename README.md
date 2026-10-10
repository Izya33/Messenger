# TCP Messenger — C# Client-Server Application

A desktop messaging application developed in **C# and .NET**, featuring a centralized client-server architecture, TCP/IP communication, a WPF user interface, and Microsoft SQL Server database integration.

The project was developed as part of my Software Development studies at Hochschule Mittweida.

## Features

- **User Authentication** — User registration and login
- **Private Messaging** — Send and receive messages between users
- **Group Chats** — Create group conversations with multiple participants
- **Friend System** — Search for users, send, accept, and decline friend requests
- **Real-Time Updates** — Receive new messages, friend requests, and group notifications while connected
- **Message History** — Store and retrieve conversations from the database
- **Multiple Clients** — Support for multiple simultaneously connected users

## Technologies

- **Language:** C#
- **Framework:** .NET, WPF, XAML
- **Networking:** TCP/IP, System.Net.Sockets
- **Database:** Microsoft SQL Server
- **Development Environment:** Visual Studio
- **Architecture:** Client-Server, layered architecture, event-driven communication

## Project Architecture

The solution consists of four separate projects:

| Project | Description |
|---|---|
| MessengerClient | WPF user interface and client-side TCP communication |
| MessengerServer | Central server handling client connections, requests, and message distribution |
| MessengerDatabase | Database access layer responsible for SQL operations |
| MessengerShared | Shared message types, models, and communication structures |

**Communication flow:**

`Client ↔ TCP Server ↔ Database Access Layer ↔ SQL Server`

Clients do not access the database directly. All requests are processed centrally by the server.

The application uses shared message models and message types to distinguish between operations such as login, registration, messaging, friend requests, and group creation.

On the client side, C# events are used to notify the user interface about incoming messages and changes.

## Database Setup

The application requires a running Microsoft SQL Server instance.

### 1. Install SQL Server

Install Microsoft SQL Server Express:

https://www.microsoft.com/en-us/sql-server/sql-server-downloads

SQL Server Management Studio (SSMS) is optional and can be used to restore the provided database backup or manage the database manually.

### 2. Configure the Database

Two setup options are available:

**Option A — Restore the provided database**

Restore `Database/MessengerDB.bak` using SQL Server Management Studio.

**Option B — Create a new database (recommended)**

Create an empty database named `MessengerDB`.

The application automatically creates the required tables when the database connection is configured correctly.

### 3. Configure the Connection String

Open:

`MessengerDatabase/DatabaseConnection.cs`

Adjust the SQL Server instance name and connection settings to match your local installation.

The project was developed using Microsoft SQL Server 2025 Express.

## Running the Application

1. Open `Messenger.slnx` in Visual Studio.
2. Make sure the SQL Server instance is running.
3. Configure the database connection.
4. Build the solution.
5. Start `MessengerServer`.
6. Start one or more instances of `MessengerClient`.

For testing communication between users, start at least two client instances.

Register two users, log in, and test private messaging, friend requests, and group functionality.

## Implementation Highlights

**Centralized Request Processing**

All client requests are sent to the server, which handles the application logic and database operations.

**Event-Driven Communication**

Incoming messages and notifications trigger C# events, allowing the WPF interface to update without directly coupling UI components to the networking layer.

**Persistent Data Storage**

User accounts, conversations, messages, group memberships, and friend requests are stored in a relational database.

**Multi-Client Synchronization**

The server notifies connected users about relevant changes, such as new messages, accepted friend requests, and newly created groups.

## Limitations and Future Improvements

This is an educational project and is not intended for production use.

Potential improvements include:

- Stronger password hashing and authentication security
- Encrypted client-server communication
- Improved user interface and user experience
- Additional group management functionality
- File and image sharing
- Online status and message timestamps
