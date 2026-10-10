USE MessengerDB;

IF OBJECT_ID('Users', 'U') IS NULL
BEGIN
    CREATE TABLE Users
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Login NVARCHAR(50) NOT NULL UNIQUE,
        PasswordHash NVARCHAR(255) NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
    );
END;

IF OBJECT_ID('Dialogs', 'U') IS NULL
BEGIN
    CREATE TABLE Dialogs
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(100) NULL,
        Type INT NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
    );
END;

IF OBJECT_ID('DialogMembers', 'U') IS NULL
BEGIN
    CREATE TABLE DialogMembers
    (
        DialogId INT NOT NULL,
        UserId INT NOT NULL,
        JoinedAt DATETIME2 NOT NULL DEFAULT GETDATE(),

        PRIMARY KEY(DialogId, UserId),
        FOREIGN KEY(DialogId) REFERENCES Dialogs(Id),
        FOREIGN KEY(UserId) REFERENCES Users(Id)
    );
END;

IF OBJECT_ID('Messages', 'U') IS NULL
BEGIN
    CREATE TABLE Messages
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        DialogId INT NOT NULL,
        SenderId INT NOT NULL,
        Content NVARCHAR(MAX) NOT NULL,
        SentAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        IsEdited BIT NOT NULL DEFAULT 0,

        FOREIGN KEY(DialogId) REFERENCES Dialogs(Id),
        FOREIGN KEY(SenderId) REFERENCES Users(Id)
    );
END;

IF OBJECT_ID('FriendRequests', 'U') IS NULL
BEGIN
    CREATE TABLE FriendRequests
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        SenderId INT NOT NULL,
        ReceiverId INT NOT NULL,
        Status INT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),

        FOREIGN KEY(SenderId) REFERENCES Users(Id),
        FOREIGN KEY(ReceiverId) REFERENCES Users(Id)
    );
END;