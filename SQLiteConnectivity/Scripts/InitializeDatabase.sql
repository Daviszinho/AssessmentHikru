-- Create Department table
CREATE TABLE IF NOT EXISTS Department (
    DepartmentID INTEGER PRIMARY KEY AUTOINCREMENT,
    DepartmentName TEXT NOT NULL,
    Location TEXT,
    Budget DECIMAL(18, 2)
);

-- Create Recruiter table
CREATE TABLE IF NOT EXISTS Recruiter (
    RecruiterID INTEGER PRIMARY KEY AUTOINCREMENT,
    RecruiterName TEXT NOT NULL,
    Email TEXT,
    Phone TEXT
);

-- Create Position table
CREATE TABLE IF NOT EXISTS Position (
    PositionID INTEGER PRIMARY KEY AUTOINCREMENT,
    PositionTitle TEXT NOT NULL,
    PositionDescription TEXT,
    PositionLocation TEXT,
    DepartmentID INTEGER,
    RecruiterID INTEGER,
    PositionBudget DECIMAL(18, 2),
    PositionClosingDate TEXT,
    PositionStatus TEXT,
    FOREIGN KEY (DepartmentID) REFERENCES Department(DepartmentID),
    FOREIGN KEY (RecruiterID) REFERENCES Recruiter(RecruiterID)
);

-- Create PositionDetails view
CREATE VIEW IF NOT EXISTS PositionDetails AS
SELECT 
    p.PositionID,
    p.PositionTitle,
    p.PositionDescription,
    p.PositionLocation,
    p.DepartmentID,
    d.DepartmentName,
    p.RecruiterID,
    r.RecruiterName,
    p.PositionBudget,
    p.PositionClosingDate,
    p.PositionStatus
FROM 
    Position p
LEFT JOIN 
    Department d ON p.DepartmentID = d.DepartmentID
LEFT JOIN 
    Recruiter r ON p.RecruiterID = r.RecruiterID;

-- Insert sample data
INSERT OR IGNORE INTO Department (DepartmentName, Location, Budget) VALUES 
    ('Engineering', 'New York', 1000000.00),
    ('Marketing', 'Chicago', 750000.00),
    ('Sales', 'Los Angeles', 1200000.00);

INSERT OR IGNORE INTO Recruiter (RecruiterName, Email, Phone) VALUES 
    ('John Doe', 'john.doe@example.com', '555-0101'),
    ('Jane Smith', 'jane.smith@example.com', '555-0102'),
    ('Mike Johnson', 'mike.johnson@example.com', '555-0103');

INSERT OR IGNORE INTO Position (PositionTitle, PositionDescription, PositionLocation, DepartmentID, RecruiterID, PositionBudget, PositionClosingDate, PositionStatus) VALUES 
    ('Senior Developer', 'Senior .NET Developer', 'New York', 1, 1, 120000.00, '2023-12-31', 'Open'),
    ('Marketing Manager', 'Digital Marketing Manager', 'Chicago', 2, 2, 95000.00, '2023-11-30', 'Open'),
    ('Sales Executive', 'Enterprise Sales Executive', 'Los Angeles', 3, 3, 110000.00, '2023-12-15', 'Open');
