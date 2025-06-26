-- Script Date: 6/18/2025 6:23 PM  - ErikEJ.SqlCeScripting version 3.5.2.102
-- Database information:
-- Database: C:\Users\dpena\Documents\Projects\Hikru\Assessment\RestWebServices\db_hikru_test.db
-- ServerVersion: 3.46.1
-- DatabaseSize: 20 KB
-- Created: 6/18/2025 5:14 PM

-- User Table information:
-- Number of tables: 3
-- Department: -1 row(s)
-- Position: -1 row(s)
-- Recruiter: -1 row(s)

-- Enable foreign key constraints
PRAGMA foreign_keys=ON;



-- Drop existing tables if they exist
DROP VIEW IF EXISTS PositionDetails;
DROP TABLE IF EXISTS Position;
DROP TABLE IF EXISTS Department;
DROP TABLE IF EXISTS Recruiter;

BEGIN TRANSACTION;

CREATE TABLE [Recruiter] (
  [ID] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
  [NAME] TEXT NOT NULL
);

CREATE TABLE [Department] (
  [ID] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
  [NAME] TEXT NOT NULL
);

CREATE TABLE [Position] (
  [ID] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
  [TITLE] TEXT NOT NULL,
  [DESCRIPTION] TEXT NOT NULL,
  [LOCATION] TEXT NOT NULL,
  [STATUS] TEXT NOT NULL,
  [RECRUITERID] INTEGER NOT NULL,
  [DEPARTMENTID] INTEGER NOT NULL,
  [BUDGET] REAL NULL,
  [CLOSINGDATE] TEXT NULL,
  [CREATEDAT] TEXT DEFAULT (datetime('now')) NULL,
  [UPDATEDAT] TEXT DEFAULT (datetime('now')) NULL,
  FOREIGN KEY ([DEPARTMENTID]) REFERENCES [Department] ([ID]) ON DELETE CASCADE,
  FOREIGN KEY ([RECRUITERID]) REFERENCES [Recruiter] ([ID]) ON DELETE CASCADE
);

-- Insert sample data
INSERT INTO [Recruiter] ([NAME]) VALUES ('Ana');
INSERT INTO [Recruiter] ([NAME]) VALUES ('Bernardo');
INSERT INTO [Recruiter] ([NAME]) VALUES ('Carlos');

INSERT INTO [Department] ([NAME]) VALUES ('IT');
INSERT INTO [Department] ([NAME]) VALUES ('Product');
INSERT INTO [Department] ([NAME]) VALUES ('ProfessionalServices');

INSERT INTO [Position] ([TITLE],[DESCRIPTION],[LOCATION],[STATUS],[RECRUITERID],[DEPARTMENTID],[BUDGET],[CLOSINGDATE],[CREATEDAT],[UPDATEDAT]) VALUES 
('Senior Software Developer','Full-stack development role focusing on modern web technologies and cloud solutions','San Jose, Costa Rica','Draft',1,1,10000,'2025-06-16',datetime('now'),datetime('now'));

INSERT INTO [Position] ([TITLE],[DESCRIPTION],[LOCATION],[STATUS],[RECRUITERID],[DEPARTMENTID],[BUDGET],[CLOSINGDATE],[CREATEDAT],[UPDATEDAT]) VALUES 
('Technical Recruiter','Recruiting technical talent for our growing engineering team','Montevideo, Uruguay','Open',1,1,1000,'2025-07-03',datetime('now'),datetime('now'));

INSERT INTO [Position] ([TITLE],[DESCRIPTION],[LOCATION],[STATUS],[RECRUITERID],[DEPARTMENTID],[BUDGET],[CLOSINGDATE],[CREATEDAT],[UPDATEDAT]) VALUES 
('Product Manager','Lead product strategy and development for our core platform','Montevideo, Uruguay','Draft',1,1,1000,'2025-09-11',datetime('now'),datetime('now'));

INSERT INTO [Position] ([TITLE],[DESCRIPTION],[LOCATION],[STATUS],[RECRUITERID],[DEPARTMENTID],[BUDGET],[CLOSINGDATE],[CREATEDAT],[UPDATEDAT]) VALUES 
('DevOps Engineer','Build and maintain our cloud infrastructure and CI/CD pipelines','Remote','Draft',1,1,10000,'2025-06-20',datetime('now'),datetime('now'));

-- Create view for position details
CREATE VIEW PositionDetails AS
SELECT 
    p.ID as PositionID,
    p.TITLE as PositionTitle,
    p.DESCRIPTION as PositionDescription,
    p.LOCATION as PositionLocation,
    p.STATUS as PositionStatus,
    p.BUDGET as PositionBudget,
    p.CLOSINGDATE as PositionClosingDate,
    p.CREATEDAT as PositionCreatedAt,
    p.UPDATEDAT as PositionUpdatedAt,
    r.ID as RecruiterID,
    r.NAME as RecruiterName,
    d.ID as DepartmentID,
    d.NAME as DepartmentName
FROM Position p
JOIN Recruiter r ON p.RECRUITERID = r.ID
JOIN Department d ON p.DEPARTMENTID = d.ID;

COMMIT;

