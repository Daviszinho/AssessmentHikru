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

SELECT 1;
PRAGMA foreign_keys=OFF;
BEGIN TRANSACTION;
CREATE TABLE [Recruiter] (
  [ID] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL
, [NAME] text NOT NULL
);
CREATE TABLE [Department] (
  [ID] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL
, [NAME] text NOT NULL
);
CREATE TABLE [Position] (
  [ID] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL
, [TITLE] text NOT NULL
, [DESCRIPTION] text NOT NULL
, [LOCATION] text NOT NULL
, [STATUS] text NOT NULL
, [RECRUITERID] bigint NOT NULL
, [DEPARTMENTID] bigint NOT NULL
, [BUDGET] real NULL
, [CLOSINGDATE] text NULL
, [CREATEDAT] text DEFAULT (CURRENT_TIMESTAMP) NULL
, [UPDATEDAT] text DEFAULT (CURRENT_TIMESTAMP) NULL
, CONSTRAINT [FK_Position_0_0] FOREIGN KEY ([DEPARTMENTID]) REFERENCES [Department] ([ID]) ON DELETE CASCADE ON UPDATE NO ACTION
, CONSTRAINT [FK_Position_1_0] FOREIGN KEY ([RECRUITERID]) REFERENCES [Recruiter] ([ID]) ON DELETE CASCADE ON UPDATE NO ACTION
);
INSERT INTO [Recruiter] ([ID],[NAME]) VALUES (
1,'Ana');
INSERT INTO [Recruiter] ([ID],[NAME]) VALUES (
2,'Bernardo');
INSERT INTO [Recruiter] ([ID],[NAME]) VALUES (
3,'Carlos');
INSERT INTO [Department] ([ID],[NAME]) VALUES (
1,'IT');
INSERT INTO [Department] ([ID],[NAME]) VALUES (
2,'Product');
INSERT INTO [Department] ([ID],[NAME]) VALUES (
3,'ProfessionalServices');
INSERT INTO [Position] ([ID],[TITLE],[DESCRIPTION],[LOCATION],[STATUS],[RECRUITERID],[DEPARTMENTID],[BUDGET],[CLOSINGDATE],[CREATEDAT],[UPDATEDAT]) VALUES (
1,'SoftwareDeveloper','Desc2','SJO','Draft',1,1,10000,'2025-06-16',NULL,NULL);
INSERT INTO [Position] ([ID],[TITLE],[DESCRIPTION],[LOCATION],[STATUS],[RECRUITERID],[DEPARTMENTID],[BUDGET],[CLOSINGDATE],[CREATEDAT],[UPDATEDAT]) VALUES (
2,'recruiter','desc','urugua','open',1,1,1000,'2025-07-03','2025-06-18 23:32:29','2025-06-18 23:32:29');
INSERT INTO [Position] ([ID],[TITLE],[DESCRIPTION],[LOCATION],[STATUS],[RECRUITERID],[DEPARTMENTID],[BUDGET],[CLOSINGDATE],[CREATEDAT],[UPDATEDAT]) VALUES (
3,'3','321','uru','draft',1,1,1000,'2025-09-11','2025-06-18 23:33:56','2025-06-18 23:33:56');
INSERT INTO [Position] ([ID],[TITLE],[DESCRIPTION],[LOCATION],[STATUS],[RECRUITERID],[DEPARTMENTID],[BUDGET],[CLOSINGDATE],[CREATEDAT],[UPDATEDAT]) VALUES (
5,'4','jdskla','sadjfals','draft',1,1,0,'2025-06-20','2025-06-18 23:39:07','2025-06-18 23:39:07');
CREATE TRIGGER [fki_Position_DEPARTMENTID_Department_ID] BEFORE Insert ON [Position] FOR EACH ROW BEGIN SELECT RAISE(ROLLBACK, 'Insert on table Position violates foreign key constraint FK_Position_0_0') WHERE NOT EXISTS (SELECT * FROM Department WHERE  ID = NEW.DEPARTMENTID); END;
CREATE TRIGGER [fku_Position_DEPARTMENTID_Department_ID] BEFORE Update ON [Position] FOR EACH ROW BEGIN SELECT RAISE(ROLLBACK, 'Update on table Position violates foreign key constraint FK_Position_0_0') WHERE NOT EXISTS (SELECT * FROM Department WHERE  ID = NEW.DEPARTMENTID); END;
CREATE TRIGGER [fki_Position_RECRUITERID_Recruiter_ID] BEFORE Insert ON [Position] FOR EACH ROW BEGIN SELECT RAISE(ROLLBACK, 'Insert on table Position violates foreign key constraint FK_Position_1_0') WHERE NOT EXISTS (SELECT * FROM Recruiter WHERE  ID = NEW.RECRUITERID); END;
CREATE TRIGGER [fku_Position_RECRUITERID_Recruiter_ID] BEFORE Update ON [Position] FOR EACH ROW BEGIN SELECT RAISE(ROLLBACK, 'Update on table Position violates foreign key constraint FK_Position_1_0') WHERE NOT EXISTS (SELECT * FROM Recruiter WHERE  ID = NEW.RECRUITERID); END;
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

