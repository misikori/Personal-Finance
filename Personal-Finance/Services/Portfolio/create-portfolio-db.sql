-- ============================================================================
-- Portfolio Database Setup Script
-- 
-- ⚠️  MAC ONLY - FOR LOCAL TESTING PURPOSES
-- 
-- This script is a WORKAROUND for Mac's Docker networking issues with MSSQL.
-- On Mac, EF Core migrations can't connect to MSSQL in Docker, so we run this
-- script manually inside the container to create the database.
-- 
-- On Windows/Linux, use normal EF Core migrations instead:
--   dotnet ef database update --project ../Portfolio.Data/Portfolio.Data.csproj
-- 
-- HOW TO USE (Mac only):
--   docker cp create-portfolio-db.sql mssql:/tmp/
--   docker exec mssql /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P MATF12345678rs2 -i /tmp/create-portfolio-db.sql -C
-- 
-- ============================================================================

-- Create database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'PortfolioDb')
BEGIN
    CREATE DATABASE PortfolioDb;
    PRINT 'Database PortfolioDb created successfully';
END
ELSE
BEGIN
    PRINT 'Database PortfolioDb already exists';
END
GO

-- Use the database
USE PortfolioDb;
GO

-- Create Positions table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Positions')
BEGIN
    CREATE TABLE Positions (
        Id NVARCHAR(50) NOT NULL PRIMARY KEY,
        Username NVARCHAR(100) NOT NULL,
        Symbol NVARCHAR(10) NOT NULL,
        Quantity DECIMAL(18,8) NOT NULL,
        AveragePurchasePrice DECIMAL(18,4) NOT NULL,
        FirstPurchaseDate DATETIME2 NOT NULL,
        LastUpdated DATETIME2 NOT NULL
    );
    
    -- Create unique index on Username + Symbol
    CREATE UNIQUE INDEX IX_Positions_Username_Symbol 
        ON Positions (Username, Symbol);
    
    PRINT 'Table Positions created successfully';
END
ELSE
BEGIN
    PRINT 'Table Positions already exists';
END
GO

-- Create Transactions table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Transactions')
BEGIN
    CREATE TABLE Transactions (
        Id NVARCHAR(50) NOT NULL PRIMARY KEY,
        Username NVARCHAR(100) NOT NULL,
        Symbol NVARCHAR(10) NOT NULL,
        Type NVARCHAR(10) NOT NULL,
        Quantity DECIMAL(18,8) NOT NULL,
        PricePerShare DECIMAL(18,4) NOT NULL,
        TransactionDate DATETIME2 NOT NULL
    );
    
    -- Create index on Username for faster queries
    CREATE INDEX IX_Transactions_Username 
        ON Transactions (Username);
    
    -- Create index on TransactionDate for faster queries
    CREATE INDEX IX_Transactions_TransactionDate 
        ON Transactions (TransactionDate);
    
    PRINT 'Table Transactions created successfully';
END
ELSE
BEGIN
    PRINT 'Table Transactions already exists';
END
GO

-- Verify tables were created
SELECT 'Database setup complete!' AS Status;
SELECT name AS TableName FROM sys.tables ORDER BY name;
GO
