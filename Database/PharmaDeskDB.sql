<<<<<<< HEAD
CREATE DATABASE IF NOT EXISTS PharmaDeskDB;
USE PharmaDeskDB;

-- 1. Roles table
CREATE TABLE Roles (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Name VARCHAR(50) NOT NULL UNIQUE,
    Description VARCHAR(255),
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 2. Users table
CREATE TABLE Users (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Username VARCHAR(100) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    Email VARCHAR(150) NOT NULL UNIQUE,
    FullName VARCHAR(200) NOT NULL,
    RoleId INT NOT NULL,
    IsActive BOOLEAN DEFAULT TRUE,
    LastLogin DATETIME,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (RoleId) REFERENCES Roles(Id) ON DELETE RESTRICT
);

-- 3. Suppliers table
CREATE TABLE Suppliers (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Name VARCHAR(200) NOT NULL,
    ContactPerson VARCHAR(150),
    Phone VARCHAR(50),
    Email VARCHAR(150),
    Address TEXT,
    TaxId VARCHAR(100),
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 4. Medicines table
CREATE TABLE Medicines (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Name VARCHAR(200) NOT NULL,
    GenericName VARCHAR(200),
    Brand VARCHAR(100),
    Barcode VARCHAR(100) UNIQUE,
    Category VARCHAR(100),
    DosageForm VARCHAR(50),     
    Strength VARCHAR(50),
    UnitPrice DECIMAL(10,2) NOT NULL,
    ReorderLevel INT DEFAULT 10,
    IsPrescriptionRequired BOOLEAN DEFAULT FALSE,
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 5. MedicineLots table
CREATE TABLE MedicineLots (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    MedicineId INT NOT NULL,
    LotNumber VARCHAR(100) NOT NULL,
    ManufacturerDate DATE,
    ExpiryDate DATE NOT NULL,
    QuantityReceived INT NOT NULL,
    QuantityRemaining INT NOT NULL,
    CostPrice DECIMAL(10,2) NOT NULL,
    SellingPrice DECIMAL(10,2) NOT NULL,
    SupplierId INT,
    ReceivedDate DATE,
    FOREIGN KEY (MedicineId) REFERENCES Medicines(Id) ON DELETE CASCADE,
    FOREIGN KEY (SupplierId) REFERENCES Suppliers(Id) ON DELETE SET NULL
);

-- 6. Inventory (real-time stock)
CREATE TABLE Inventory (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    MedicineId INT NOT NULL UNIQUE,
    CurrentStock INT NOT NULL DEFAULT 0,
    ReservedStock INT DEFAULT 0,
    LastUpdated TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (MedicineId) REFERENCES Medicines(Id) ON DELETE CASCADE
);

-- 7. Orders (Purchase Orders from suppliers)
CREATE TABLE Orders (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    OrderNumber VARCHAR(50) UNIQUE NOT NULL,
    SupplierId INT NOT NULL,
    OrderDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    ExpectedDeliveryDate DATE,
    Status ENUM('Pending','Approved','Shipped','Received','Cancelled') DEFAULT 'Pending',
    TotalAmount DECIMAL(12,2),
    Notes TEXT,
    CreatedBy INT,
    FOREIGN KEY (SupplierId) REFERENCES Suppliers(Id),
    FOREIGN KEY (CreatedBy) REFERENCES Users(Id)
);

-- 8. OrderItems
=======
CREATE DATABASE IF NOT EXISTS PharmaDeskDB CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE PharmaDeskDB;

CREATE TABLE Roles (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Name VARCHAR(40) NOT NULL UNIQUE
);

CREATE TABLE Users (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Username VARCHAR(80) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    Email VARCHAR(160) NOT NULL UNIQUE,
    FullName VARCHAR(180) NOT NULL,
    RoleId INT NOT NULL,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleId) REFERENCES Roles(Id)
);

CREATE TABLE Categories (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Name VARCHAR(120) NOT NULL,
    Description VARCHAR(500) NULL,
    ParentCategoryId INT NULL,
    CONSTRAINT FK_Categories_Parent FOREIGN KEY (ParentCategoryId) REFERENCES Categories(Id) ON DELETE RESTRICT
);

CREATE TABLE Medicines (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Name VARCHAR(180) NOT NULL,
    GenericName VARCHAR(180) NULL,
    Barcode VARCHAR(80) NOT NULL UNIQUE,
    CategoryId INT NOT NULL,
    DosageForm VARCHAR(80) NOT NULL,
    Strength VARCHAR(80) NOT NULL,
    UnitPrice DECIMAL(10,2) NOT NULL,
    StockQuantity INT NOT NULL,
    ReorderLevel INT NOT NULL DEFAULT 10,
    IsPrescriptionRequired BOOLEAN NOT NULL DEFAULT FALSE,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    ImageUrl VARCHAR(500) NULL,
    Description VARCHAR(1000) NULL,
    Rating DECIMAL(3,2) NOT NULL DEFAULT 4.60,
    IsNew BOOLEAN NOT NULL DEFAULT FALSE,
    IsPromotion BOOLEAN NOT NULL DEFAULT FALSE,
    DiscountPercent INT NOT NULL DEFAULT 0,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_Medicines_Categories FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
);

CREATE TABLE CartItems (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    UserId INT NOT NULL,
    MedicineId INT NOT NULL,
    Quantity INT NOT NULL,
    AddedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE KEY UX_CartItems_User_Medicine (UserId, MedicineId),
    CONSTRAINT FK_CartItems_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT FK_CartItems_Medicines FOREIGN KEY (MedicineId) REFERENCES Medicines(Id)
);

CREATE TABLE Orders (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    OrderNumber VARCHAR(40) NOT NULL UNIQUE,
    UserId INT NOT NULL,
    OrderDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    TotalAmount DECIMAL(12,2) NOT NULL,
    Discount DECIMAL(12,2) NOT NULL,
    Tax DECIMAL(12,2) NOT NULL,
    GrandTotal DECIMAL(12,2) NOT NULL,
    PaymentMethod VARCHAR(40) NOT NULL,
    ShippingAddress VARCHAR(500) NOT NULL,
    Status VARCHAR(40) NOT NULL,
    PrescriptionUploadUrl VARCHAR(500) NULL,
    InvoicePath VARCHAR(500) NULL,
    CONSTRAINT FK_Orders_Users FOREIGN KEY (UserId) REFERENCES Users(Id)
);

>>>>>>> 09cdda9 (Conectarea bazei de date la aplicatie)
CREATE TABLE OrderItems (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    OrderId INT NOT NULL,
    MedicineId INT NOT NULL,
    Quantity INT NOT NULL,
<<<<<<< HEAD
    UnitCost DECIMAL(10,2) NOT NULL,
    TotalCost DECIMAL(12,2),
    FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE CASCADE,
    FOREIGN KEY (MedicineId) REFERENCES Medicines(Id)
);

-- 9. Prescriptions
CREATE TABLE Prescriptions (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    PrescriptionNumber VARCHAR(50) UNIQUE NOT NULL,
    PatientName VARCHAR(200) NOT NULL,
    PatientAge INT,
    PatientGender ENUM('Male','Female','Other'),
    DoctorName VARCHAR(200),
    IssueDate DATE NOT NULL,
    ExpiryDate DATE,
    IsProcessed BOOLEAN DEFAULT FALSE,
    ProcessedBy INT,
    ProcessedDate DATETIME,
    Notes TEXT,
    FOREIGN KEY (ProcessedBy) REFERENCES Users(Id)
);

-- 10. PrescriptionItems
CREATE TABLE PrescriptionItems (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    PrescriptionId INT NOT NULL,
    MedicineId INT NOT NULL,
    Dosage VARCHAR(100),
    Duration VARCHAR(100),
    Instructions TEXT,
    FOREIGN KEY (PrescriptionId) REFERENCES Prescriptions(Id) ON DELETE CASCADE,
    FOREIGN KEY (MedicineId) REFERENCES Medicines(Id)
);

-- 11. Sales
CREATE TABLE Sales (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    InvoiceNumber VARCHAR(50) UNIQUE NOT NULL,
    SaleDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    CustomerName VARCHAR(200),
    CustomerPhone VARCHAR(50),
    TotalAmount DECIMAL(12,2) NOT NULL,
    Discount DECIMAL(10,2) DEFAULT 0,
    Tax DECIMAL(10,2) DEFAULT 0,
    GrandTotal DECIMAL(12,2) NOT NULL,
    PaymentMethod ENUM('Cash','Card','Insurance','Other') DEFAULT 'Cash',
    PrescriptionId INT NULL,
    SoldBy INT NOT NULL,
    FOREIGN KEY (PrescriptionId) REFERENCES Prescriptions(Id) ON DELETE SET NULL,
    FOREIGN KEY (SoldBy) REFERENCES Users(Id)
);

-- 12. SaleItems
CREATE TABLE SaleItems (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    SaleId INT NOT NULL,
    MedicineId INT NOT NULL,
    LotId INT NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(10,2) NOT NULL,
    DiscountPercent DECIMAL(5,2) DEFAULT 0,
    TotalPrice DECIMAL(12,2) NOT NULL,
    FOREIGN KEY (SaleId) REFERENCES Sales(Id) ON DELETE CASCADE,
    FOREIGN KEY (MedicineId) REFERENCES Medicines(Id),
    FOREIGN KEY (LotId) REFERENCES MedicineLots(Id)
);

-- 13. AuditLogs
CREATE TABLE AuditLogs (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    UserId INT NULL,
    Action VARCHAR(100) NOT NULL,
    TableName VARCHAR(100),
    RecordId INT,
    OldValue TEXT,
    NewValue TEXT,
    IpAddress VARCHAR(50),
    UserAgent VARCHAR(255),
    Timestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE SET NULL
);

-- 14. Settings
CREATE TABLE Settings (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    SettingKey VARCHAR(100) UNIQUE NOT NULL,
    SettingValue TEXT,
    Description VARCHAR(255),
    UpdatedBy INT,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (UpdatedBy) REFERENCES Users(Id)
);

-- Indexes for performance
CREATE INDEX idx_users_username ON Users(Username);
CREATE INDEX idx_medicines_name ON Medicines(Name);
CREATE INDEX idx_medicines_barcode ON Medicines(Barcode);
CREATE INDEX idx_lots_expiry ON MedicineLots(ExpiryDate);
CREATE INDEX idx_inventory_stock ON Inventory(CurrentStock);
CREATE INDEX idx_sales_date ON Sales(SaleDate);
CREATE INDEX idx_audit_user ON AuditLogs(UserId);
CREATE INDEX idx_audit_timestamp ON AuditLogs(Timestamp);

-- Insert default roles
INSERT INTO Roles (Name, Description) VALUES 
('Admin', 'Full system access'),
('Pharmacist', 'Can process sales, manage inventory, view reports'),
('Cashier', 'Can only process sales and view limited data');

-- Insert default admin user (password: Admin123! hashed with BCrypt)
-- BCrypt hash for "Admin123!" (cost factor 10)
INSERT INTO Users (Username, PasswordHash, Email, FullName, RoleId, IsActive) VALUES 
('admin', '$2a$10$N9qo8uLOickgx2ZMRZoMy.Mr/.6B7N5L2V/5XbL5XbL5XbL5XbL5', 'admin@pharmadesk.com', 'System Administrator', 1, TRUE);

-- Insert sample settings
INSERT INTO Settings (SettingKey, SettingValue, Description) VALUES
('CompanyName', 'PharmaDesk Pharmacy', 'Company display name'),
('TaxRate', '0.10', 'Default tax rate 10%'),
('ReceiptFooter', 'Thank you for shopping with us!', 'Footer on receipts');

-- Insert one sample supplier and medicine for testing
INSERT INTO Suppliers (Name, ContactPerson, Phone, Email, IsActive) VALUES 
('MediSource Ltd.', 'John Doe', '+123456789', 'orders@medisource.com', TRUE);

INSERT INTO Medicines (Name, GenericName, Brand, Barcode, Category, DosageForm, Strength, UnitPrice, ReorderLevel, IsPrescriptionRequired) VALUES
('Paracetamol 500mg', 'Paracetamol', 'Tylenol', '1234567890123', 'Analgesic', 'Tablet', '500mg', 5.99, 100, FALSE);

INSERT INTO MedicineLots (MedicineId, LotNumber, ExpiryDate, QuantityReceived, QuantityRemaining, CostPrice, SellingPrice, SupplierId, ReceivedDate) VALUES
(1, 'LOT001', DATE_ADD(CURDATE(), INTERVAL 2 YEAR), 500, 500, 3.50, 5.99, 1, CURDATE());

INSERT INTO Inventory (MedicineId, CurrentStock, ReservedStock) VALUES (1, 500, 0);
=======
    UnitPrice DECIMAL(10,2) NOT NULL,
    TotalPrice DECIMAL(12,2) NOT NULL,
    CONSTRAINT FK_OrderItems_Orders FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE CASCADE,
    CONSTRAINT FK_OrderItems_Medicines FOREIGN KEY (MedicineId) REFERENCES Medicines(Id)
);

CREATE TABLE AuditLogs (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    UserId INT NULL,
    Action VARCHAR(180) NOT NULL,
    TableName VARCHAR(120) NOT NULL,
    RecordId INT NULL,
    Timestamp DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_AuditLogs_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE SET NULL
);

CREATE TABLE Settings (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    SettingKey VARCHAR(120) NOT NULL UNIQUE,
    SettingValue VARCHAR(1000) NOT NULL
);

CREATE INDEX IX_Medicines_Name ON Medicines(Name);
CREATE INDEX IX_Medicines_CategoryId ON Medicines(CategoryId);
CREATE INDEX IX_Orders_OrderDate ON Orders(OrderDate);
CREATE INDEX IX_AuditLogs_Timestamp ON AuditLogs(Timestamp);

INSERT INTO Roles (Name) VALUES ('Admin'), ('Pharmacist'), ('User');

-- Parolele reale sunt generate automat de aplicatie cu BCrypt la prima rulare:
-- admin/Admin123!, farmacist/Farmacist123!, client/Client123!

INSERT INTO Categories (Name, Description) VALUES
('Sanatate', 'Medicamente si produse esentiale'),
('Frumusete', 'Dermatocosmetice si ingrijire'),
('Vitamine', 'Imunitate si suplimente'),
('Mama si copilul', 'Produse pentru familie'),
('Recomandate', 'Selectii facute de farmacisti');

INSERT INTO Settings (SettingKey, SettingValue) VALUES
('CompanyName', 'PharmaDesk'),
('TaxRate', '0.09'),
('Currency', 'RON');
>>>>>>> 09cdda9 (Conectarea bazei de date la aplicatie)
