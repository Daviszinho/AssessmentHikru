-- Oracle Database Initialization Script
-- Script Date: 6/18/2025 6:23 PM
-- Database: Oracle Cloud Database
-- ServerVersion: Oracle Database 19c/21c

SET SERVEROUTPUT ON;

-- Drop existing objects if they exist
DROP VIEW PositionDetails;
/
DROP TABLE Position;
/
DROP TABLE Department;
/
DROP TABLE Recruiter;
/
DROP SEQUENCE RECRUITER_SEQ;
/
DROP SEQUENCE DEPARTMENT_SEQ;
/
DROP SEQUENCE POSITION_SEQ;
/

-- Create sequences
CREATE SEQUENCE RECRUITER_SEQ START WITH 1 INCREMENT BY 1;
/
CREATE SEQUENCE DEPARTMENT_SEQ START WITH 1 INCREMENT BY 1;
/
CREATE SEQUENCE POSITION_SEQ START WITH 1 INCREMENT BY 1;
/

-- Create tables
CREATE TABLE Recruiter (
    ID NUMBER PRIMARY KEY,
    NAME VARCHAR2(100) NOT NULL,
    IS_ACTIVE NUMBER(1) DEFAULT 1,
    CREATED_AT TIMESTAMP DEFAULT SYSTIMESTAMP,
    UPDATED_AT TIMESTAMP DEFAULT SYSTIMESTAMP
);
/

CREATE TABLE Department (
    ID NUMBER PRIMARY KEY,
    NAME VARCHAR2(100) NOT NULL,
    IS_ACTIVE NUMBER(1) DEFAULT 1,
    CREATED_AT TIMESTAMP DEFAULT SYSTIMESTAMP,
    UPDATED_AT TIMESTAMP DEFAULT SYSTIMESTAMP
);
/

CREATE TABLE Position (
    ID NUMBER PRIMARY KEY,
    TITLE VARCHAR2(200) NOT NULL,
    DESCRIPTION CLOB,
    LOCATION VARCHAR2(200),
    STATUS VARCHAR2(50),
    RECRUITER_ID NUMBER,
    DEPARTMENT_ID NUMBER NOT NULL,
    BUDGET NUMBER(10,2),
    CLOSING_DATE DATE,
    IS_ACTIVE NUMBER(1) DEFAULT 1,
    CREATED_AT TIMESTAMP DEFAULT SYSTIMESTAMP,
    UPDATED_AT TIMESTAMP DEFAULT SYSTIMESTAMP,
    CONSTRAINT FK_POSITION_DEPARTMENT FOREIGN KEY (DEPARTMENT_ID) REFERENCES Department(ID) ON DELETE CASCADE,
    CONSTRAINT FK_POSITION_RECRUITER FOREIGN KEY (RECRUITER_ID) REFERENCES Recruiter(ID) ON DELETE SET NULL
);
/

-- Create indexes for better performance
CREATE INDEX IDX_POSITION_DEPARTMENT_ID ON Position(DEPARTMENT_ID);
/
CREATE INDEX IDX_POSITION_RECRUITER_ID ON Position(RECRUITER_ID);
/
CREATE INDEX IDX_POSITION_STATUS ON Position(STATUS);
/

-- Insert sample data
INSERT INTO Recruiter (ID, NAME) VALUES (RECRUITER_SEQ.NEXTVAL, 'Ana');
/
INSERT INTO Recruiter (ID, NAME) VALUES (RECRUITER_SEQ.NEXTVAL, 'Bernardo');
/
INSERT INTO Recruiter (ID, NAME) VALUES (RECRUITER_SEQ.NEXTVAL, 'Carlos');
/

INSERT INTO Department (ID, NAME) VALUES (DEPARTMENT_SEQ.NEXTVAL, 'IT');
/
INSERT INTO Department (ID, NAME) VALUES (DEPARTMENT_SEQ.NEXTVAL, 'Product');
/
INSERT INTO Department (ID, NAME) VALUES (DEPARTMENT_SEQ.NEXTVAL, 'ProfessionalServices');
/

INSERT INTO Position (ID, TITLE, DESCRIPTION, LOCATION, STATUS, RECRUITER_ID, DEPARTMENT_ID, BUDGET, CLOSING_DATE) VALUES 
(POSITION_SEQ.NEXTVAL, 'Senior Software Developer', 'Full-stack development role focusing on modern web technologies and cloud solutions', 'San Jose, Costa Rica', 'Draft', 1, 1, 10000, TO_DATE('2025-06-16', 'YYYY-MM-DD'));
/

INSERT INTO Position (ID, TITLE, DESCRIPTION, LOCATION, STATUS, RECRUITER_ID, DEPARTMENT_ID, BUDGET, CLOSING_DATE) VALUES 
(POSITION_SEQ.NEXTVAL, 'Technical Recruiter', 'Recruiting technical talent for our growing engineering team', 'Montevideo, Uruguay', 'Open', 1, 1, 1000, TO_DATE('2025-07-03', 'YYYY-MM-DD'));
/

INSERT INTO Position (ID, TITLE, DESCRIPTION, LOCATION, STATUS, RECRUITER_ID, DEPARTMENT_ID, BUDGET, CLOSING_DATE) VALUES 
(POSITION_SEQ.NEXTVAL, 'Product Manager', 'Lead product strategy and development for our core platform', 'Montevideo, Uruguay', 'Draft', 1, 1, 1000, TO_DATE('2025-09-11', 'YYYY-MM-DD'));
/

INSERT INTO Position (ID, TITLE, DESCRIPTION, LOCATION, STATUS, RECRUITER_ID, DEPARTMENT_ID, BUDGET, CLOSING_DATE) VALUES 
(POSITION_SEQ.NEXTVAL, 'DevOps Engineer', 'Build and maintain our cloud infrastructure and CI/CD pipelines', 'Remote', 'Draft', 1, 1, 10000, TO_DATE('2025-06-20', 'YYYY-MM-DD'));
/

-- Create view for position details
CREATE VIEW PositionDetails AS
SELECT 
    p.ID as PositionID,
    p.TITLE as PositionTitle,
    p.DESCRIPTION as PositionDescription,
    p.LOCATION as PositionLocation,
    p.STATUS as PositionStatus,
    p.BUDGET as PositionBudget,
    p.CLOSING_DATE as PositionClosingDate,
    p.CREATED_AT as PositionCreatedAt,
    p.UPDATED_AT as PositionUpdatedAt,
    r.ID as RecruiterID,
    r.NAME as RecruiterName,
    d.ID as DepartmentID,
    d.NAME as DepartmentName
FROM Position p
LEFT JOIN Recruiter r ON p.RECRUITER_ID = r.ID
JOIN Department d ON p.DEPARTMENT_ID = d.ID
WHERE p.IS_ACTIVE = 1;
/

-- Commit the transaction
COMMIT;
/

-- Grant permissions (adjust as needed for your Oracle user)
-- GRANT SELECT, INSERT, UPDATE, DELETE ON Recruiter TO your_oracle_user;
-- GRANT SELECT, INSERT, UPDATE, DELETE ON Department TO your_oracle_user;
-- GRANT SELECT, INSERT, UPDATE, DELETE ON Position TO your_oracle_user;
-- GRANT SELECT ON PositionDetails TO your_oracle_user;
-- GRANT USAGE ON RECRUITER_SEQ TO your_oracle_user;
-- GRANT USAGE ON DEPARTMENT_SEQ TO your_oracle_user;
-- GRANT USAGE ON POSITION_SEQ TO your_oracle_user; 