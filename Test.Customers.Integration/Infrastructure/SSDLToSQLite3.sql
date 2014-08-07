
-- --------------------------------------------------
-- Date Created: 08/07/2014 17:29:40
-- compatible SQLite
-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------
    
DROP TABLE IF EXISTS [CustomerDemographics];
    
DROP TABLE IF EXISTS [Customers];

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'CustomerDemographics'
CREATE TABLE [CustomerDemographics] (
    [CustomerDemographicID] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    [CustomerTypeID] TEXT,
    [CustomerDesc] TEXT,
    [RowVersion] BLOB NOT NULL
);

-- Creating table 'Customers'
CREATE TABLE [Customers] (
    [CustomerID] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    [CompanyName] TEXT,
    [ContactName] TEXT,
    [ContactTitle] TEXT,
    [Address] TEXT,
    [City] TEXT,
    [Region] TEXT,
    [PostalCode] TEXT,
    [Country] TEXT,
    [Phone] TEXT,
    [Fax] TEXT,
    [Bool] INTEGER,
    [RowVersion] BLOB NOT NULL
);

-- --------------------------------------------------
-- --------------------------------------------------