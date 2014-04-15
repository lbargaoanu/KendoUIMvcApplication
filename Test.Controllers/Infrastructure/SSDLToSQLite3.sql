
-- --------------------------------------------------
-- Date Created: 04/15/2014 14:34:10
-- compatible SQLite
-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------
    
DROP TABLE IF EXISTS [Categories];
    
DROP TABLE IF EXISTS [CustomerDemographics];
    
DROP TABLE IF EXISTS [Customers];
    
DROP TABLE IF EXISTS [Orders];
    
DROP TABLE IF EXISTS [Employees];
    
DROP TABLE IF EXISTS [Territories];
    
DROP TABLE IF EXISTS [Regions];
    
DROP TABLE IF EXISTS [Order_Detail];
    
DROP TABLE IF EXISTS [Products];
    
DROP TABLE IF EXISTS [Suppliers];
    
DROP TABLE IF EXISTS [Shippers];
    
DROP TABLE IF EXISTS [CustomerCustomerDemographics];
    
DROP TABLE IF EXISTS [TerritoryEmployees];

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'Categories'
CREATE TABLE [Categories] (
    [CategoryID] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    [CategoryName] TEXT,
    [Description] TEXT,
    [Picture] BLOB,
    [RowVersion] BLOB NOT NULL
);

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
    [CustomerID1] TEXT,
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

-- Creating table 'Orders'
CREATE TABLE [Orders] (
    [OrderID] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    [OrderID1] INTEGER NOT NULL,
    [CustomerID] TEXT,
    [EmployeeID] INTEGER,
    [OrderDate] BLOB,
    [RequiredDate] BLOB,
    [ShippedDate] BLOB,
    [ShipVia] INTEGER,
    [Freight] REAL,
    [ShipName] TEXT,
    [ShipAddress] TEXT,
    [ShipCity] TEXT,
    [ShipRegion] TEXT,
    [ShipPostalCode] TEXT,
    [ShipCountry] TEXT,
    [RowVersion] BLOB NOT NULL,
    [Customer_Id] INTEGER,
    [Shipper_Id] INTEGER
			
		,CONSTRAINT [FK_Order_Customer]
    		FOREIGN KEY ([Customer_Id])
    		REFERENCES [Customers] ([CustomerID])					
    		
						
		,CONSTRAINT [FK_Employee_Orders]
    		FOREIGN KEY ([EmployeeID])
    		REFERENCES [Employees] ([EmployeeID])					
    		
						
		,CONSTRAINT [FK_Shipper_Orders]
    		FOREIGN KEY ([Shipper_Id])
    		REFERENCES [Shippers] ([ShipperID])					
    		
			);

-- Creating table 'Employees'
CREATE TABLE [Employees] (
    [EmployeeID] INTEGER NOT NULL,
    [EmployeeID1] INTEGER NOT NULL,
    [LastName] TEXT,
    [FirstName] TEXT,
    [Title] TEXT,
    [TitleOfCourtesy] TEXT,
    [BirthDate] BLOB,
    [HireDate] BLOB,
    [Address] TEXT,
    [City] TEXT,
    [Region] TEXT,
    [PostalCode] TEXT,
    [Country] TEXT,
    [HomePhone] TEXT,
    [Extension] TEXT,
    [Photo] BLOB,
    [Notes] TEXT,
    [ReportsTo] INTEGER,
    [PhotoPath] TEXT,
    [RowVersion] BLOB NOT NULL
			
		,CONSTRAINT [FK_Employee_Employee1]
    		FOREIGN KEY ([EmployeeID1])
    		REFERENCES [Employees] ([EmployeeID])					
    		
			);

-- Creating table 'Territories'
CREATE TABLE [Territories] (
    [TerritoryID] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    [TerritoryID1] TEXT,
    [TerritoryDescription] TEXT,
    [RegionID] INTEGER NOT NULL,
    [RowVersion] BLOB NOT NULL
			
		,CONSTRAINT [FK_Region_Territories]
    		FOREIGN KEY ([RegionID])
    		REFERENCES [Regions] ([RegionID])					
    		ON DELETE CASCADE
			);

-- Creating table 'Regions'
CREATE TABLE [Regions] (
    [RegionID] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    [RegionID1] INTEGER NOT NULL,
    [RegionDescription] TEXT,
    [RowVersion] BLOB NOT NULL
);

-- Creating table 'Order_Detail'
CREATE TABLE [Order_Detail] (
    [Order_DetailID] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    [OrderID] INTEGER NOT NULL,
    [ProductID] INTEGER NOT NULL,
    [UnitPrice] REAL NOT NULL,
    [Quantity] INTEGER NOT NULL,
    [Discount] REAL NOT NULL,
    [RowVersion] BLOB NOT NULL
			
		,CONSTRAINT [FK_Order_Detail_Order]
    		FOREIGN KEY ([OrderID])
    		REFERENCES [Orders] ([OrderID])					
    		ON DELETE CASCADE
						
		,CONSTRAINT [FK_Order_Detail_Product]
    		FOREIGN KEY ([ProductID])
    		REFERENCES [Products] ([ProductID])					
    		ON DELETE CASCADE
			);

-- Creating table 'Products'
CREATE TABLE [Products] (
    [ProductID] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    [ProductName] TEXT,
    [SupplierID] INTEGER,
    [CategoryID] INTEGER,
    [QuantityPerUnit] TEXT,
    [UnitPrice] REAL,
    [UnitsInStock] INTEGER,
    [UnitsOnOrder] INTEGER,
    [ReorderLevel] INTEGER,
    [Discontinued] INTEGER NOT NULL,
    [RowVersion] BLOB NOT NULL
			
		,CONSTRAINT [FK_Product_Category]
    		FOREIGN KEY ([CategoryID])
    		REFERENCES [Categories] ([CategoryID])					
    		
						
		,CONSTRAINT [FK_Product_Supplier]
    		FOREIGN KEY ([SupplierID])
    		REFERENCES [Suppliers] ([SupplierID])					
    		
			);

-- Creating table 'Suppliers'
CREATE TABLE [Suppliers] (
    [SupplierID] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
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
    [HomePage] TEXT,
    [RowVersion] BLOB NOT NULL
);

-- Creating table 'Shippers'
CREATE TABLE [Shippers] (
    [ShipperID] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    [ShipperID1] INTEGER NOT NULL,
    [CompanyName] TEXT,
    [Phone] TEXT,
    [RowVersion] BLOB NOT NULL
);

-- Creating table 'CustomerCustomerDemographics'
CREATE TABLE [CustomerCustomerDemographics] (
    [Customer_Id] INTEGER NOT NULL,
    [CustomerDemographic_Id] INTEGER NOT NULL
 , PRIMARY KEY ([Customer_Id], [CustomerDemographic_Id])	
					
		,CONSTRAINT [FK_Customer_CustomerDemographics_Source]
    		FOREIGN KEY ([Customer_Id])
    		REFERENCES [Customers] ([CustomerID])					
    		ON DELETE CASCADE
						
		,CONSTRAINT [FK_Customer_CustomerDemographics_Target]
    		FOREIGN KEY ([CustomerDemographic_Id])
    		REFERENCES [CustomerDemographics] ([CustomerDemographicID])					
    		ON DELETE CASCADE
			);

-- Creating table 'TerritoryEmployees'
CREATE TABLE [TerritoryEmployees] (
    [Territory_Id] INTEGER NOT NULL,
    [Employee_Id] INTEGER NOT NULL
 , PRIMARY KEY ([Territory_Id], [Employee_Id])	
					
		,CONSTRAINT [FK_Territory_Employees_Source]
    		FOREIGN KEY ([Territory_Id])
    		REFERENCES [Territories] ([TerritoryID])					
    		ON DELETE CASCADE
						
		,CONSTRAINT [FK_Territory_Employees_Target]
    		FOREIGN KEY ([Employee_Id])
    		REFERENCES [Employees] ([EmployeeID])					
    		ON DELETE CASCADE
			);

-- --------------------------------------------------
-- --------------------------------------------------