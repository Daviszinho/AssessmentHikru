-- ======================================
-- DROP EXISTING OBJECTS (in reverse dependency order)
-- =============================================

-- Drop package body if exists
BEGIN
   EXECUTE IMMEDIATE 'DROP PACKAGE BODY position_pkg';
EXCEPTION
   WHEN OTHERS THEN
      IF SQLCODE != -4043 THEN  -- ORA-04043: object does not exist
         RAISE;
      END IF;
END;
/

-- Drop package spec if exists
BEGIN
   EXECUTE IMMEDIATE 'DROP PACKAGE position_pkg';
EXCEPTION
   WHEN OTHERS THEN
      IF SQLCODE != -4043 THEN  -- ORA-04043: object does not exist
         RAISE;
      END IF;
END;
/

-- Drop tables with CASCADE CONSTRAINTS to handle foreign keys
-- Drop Position table (using quoted identifier for reserved word)
BEGIN
   EXECUTE IMMEDIATE 'DROP TABLE "POSITION" CASCADE CONSTRAINTS';
EXCEPTION
   WHEN OTHERS THEN
      IF SQLCODE != -942 THEN  -- ORA-00942: table or view does not exist
         DBMS_OUTPUT.PUT_LINE('Error dropping POSITION: ' || SQLERRM);
         RAISE;
      END IF;
END;
/

-- Try alternative case if needed
BEGIN
   EXECUTE IMMEDIATE 'DROP TABLE "Position" CASCADE CONSTRAINTS';
EXCEPTION
   WHEN OTHERS THEN
      IF SQLCODE != -942 THEN  -- ORA-00942: table or view does not exist
         DBMS_OUTPUT.PUT_LINE('Error dropping Position: ' || SQLERRM);
         RAISE;
      END IF;
END;
/

BEGIN
   EXECUTE IMMEDIATE 'DROP TABLE Recruiter CASCADE CONSTRAINTS';
EXCEPTION
   WHEN OTHERS THEN
      IF SQLCODE != -942 THEN
         RAISE;
      END IF;
END;
/

BEGIN
   EXECUTE IMMEDIATE 'DROP TABLE Department CASCADE CONSTRAINTS';
EXCEPTION
   WHEN OTHERS THEN
      IF SQLCODE != -942 THEN
         RAISE;
      END IF;
END;
/

-- =============================================
-- CREATE TABLES
-- =============================================

-- Create sequence for auto-incrementing IDs
CREATE SEQUENCE seq_recruiter_id START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE seq_department_id START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE seq_position_id START WITH 1 INCREMENT BY 1;

-- Create Recruiter table
CREATE TABLE Recruiter (
    Id NUMBER DEFAULT seq_recruiter_id.NEXTVAL PRIMARY KEY,
    Name VARCHAR2(255) NOT NULL
);

-- Create Department table
CREATE TABLE Department (
    Id NUMBER DEFAULT seq_department_id.NEXTVAL PRIMARY KEY,
    Name VARCHAR2(255) NOT NULL
);

-- Create Position table
CREATE TABLE Position (
    Id NUMBER DEFAULT seq_position_id.NEXTVAL PRIMARY KEY,
    Title VARCHAR2(100) NOT NULL,
    Description VARCHAR2(1000) NOT NULL,
    Location VARCHAR2(255) NOT NULL,
    Status VARCHAR2(20) CHECK (Status IN ('draft', 'open', 'closed', 'archived')) NOT NULL,
    RecruiterId NUMBER NOT NULL,
    DepartmentId NUMBER NOT NULL,
    Budget NUMBER(18,2),
    ClosingDate DATE,
    CreatedAt TIMESTAMP DEFAULT SYSTIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT SYSTIMESTAMP,
    CONSTRAINT fk_recruiter FOREIGN KEY (RecruiterId) REFERENCES Recruiter(Id),
    CONSTRAINT fk_department FOREIGN KEY (DepartmentId) REFERENCES Department(Id)
);

-- =============================================
-- INSERT SAMPLE DATA
-- =============================================

-- Insert sample recruiters
INSERT INTO Recruiter (Name) VALUES ('John Doe');
INSERT INTO Recruiter (Name) VALUES ('Jane Smith');

-- Insert sample departments
INSERT INTO Department (Name) VALUES ('Engineering');
INSERT INTO Department (Name) VALUES ('Marketing');
INSERT INTO Department (Name) VALUES ('HR');

-- Insert sample positions
INSERT INTO Position (
    Title, 
    Description, 
    Location, 
    Status, 
    RecruiterId, 
    DepartmentId, 
    Budget, 
    ClosingDate
) VALUES (
    'Senior Software Engineer', 
    'Looking for an experienced software engineer', 
    'New York', 
    'open', 
    1, 
    1, 
    120000, 
    TO_DATE('2023-12-31', 'YYYY-MM-DD')
);

INSERT INTO Position (
    Title, 
    Description, 
    Location, 
    Status, 
    RecruiterId, 
    DepartmentId, 
    Budget, 
    ClosingDate
) VALUES (
    'Marketing Specialist', 
    'Marketing professional needed', 
    'Remote', 
    'open', 
    2, 
    2, 
    80000, 
    TO_DATE('2023-11-30', 'YYYY-MM-DD')
);

-- =============================================
-- CREATE PACKAGE SPECIFICATION
-- =============================================
CREATE OR REPLACE PACKAGE position_pkg AS
    -- Cursor type for position data
    TYPE position_cursor IS REF CURSOR;
    
    -- Get all positions
    PROCEDURE get_all_positions(p_cursor OUT position_cursor);
    
    -- Get position by ID
    PROCEDURE get_position_by_id(
        p_id IN NUMBER,
        p_cursor OUT position_cursor
    );
    
    -- Add new position
    PROCEDURE add_position(
        p_title IN VARCHAR2,
        p_description IN VARCHAR2,
        p_location IN VARCHAR2,
        p_status IN VARCHAR2,
        p_recruiter_id IN NUMBER,
        p_department_id IN NUMBER,
        p_budget IN NUMBER,
        p_closing_date IN DATE,
        p_success OUT NUMBER
    );
    
    -- Update position
    PROCEDURE update_position(
        p_id IN NUMBER,
        p_title IN VARCHAR2 DEFAULT NULL,
        p_description IN VARCHAR2 DEFAULT NULL,
        p_location IN VARCHAR2 DEFAULT NULL,
        p_status IN VARCHAR2 DEFAULT NULL,
        p_recruiter_id IN NUMBER DEFAULT NULL,
        p_department_id IN NUMBER DEFAULT NULL,
        p_budget IN NUMBER DEFAULT NULL,
        p_closing_date IN DATE DEFAULT NULL,
        p_success OUT NUMBER
    );
    
    -- Remove position
    PROCEDURE remove_position(
        p_id IN NUMBER,
        p_success OUT NUMBER
    );
END position_pkg;
/

-- =============================================
-- CREATE PACKAGE BODY
-- =============================================
CREATE OR REPLACE PACKAGE BODY position_pkg AS
    -- Get all positions
    PROCEDURE get_all_positions(p_cursor OUT position_cursor) IS
    BEGIN
        -- Log the start of the procedure
        DBMS_OUTPUT.PUT_LINE('Starting get_all_positions procedure');
        
        -- Get counts for debugging
        DECLARE
            v_position_count NUMBER;
            v_recruiter_count NUMBER;
            v_department_count NUMBER;
        BEGIN
            -- Get table counts with proper case
            SELECT COUNT(*) INTO v_position_count FROM "Position";
            SELECT COUNT(*) INTO v_recruiter_count FROM "Recruiter";
            SELECT COUNT(*) INTO v_department_count FROM "Department";
            
            -- Log the counts
            DBMS_OUTPUT.PUT_LINE('Table counts - Position: ' || v_position_count || 
                               ', Recruiter: ' || v_recruiter_count || 
                               ', Department: ' || v_department_count);
        EXCEPTION
            WHEN OTHERS THEN
                DBMS_OUTPUT.PUT_LINE('Error getting table counts: ' || SQLERRM);
        END;
        
        -- Use LEFT JOINs to return positions even if related records are missing
        OPEN p_cursor FOR
            SELECT 
                p.Id AS ID, 
                p.Title AS TITLE, 
                p.Description AS DESCRIPTION, 
                p.Location AS LOCATION, 
                p.Status AS STATUS, 
                p.RecruiterId AS RECRUITERID, 
                r.Name AS RECRUITERNAME,
                p.DepartmentId AS DEPARTMENTID, 
                d.Name AS DEPARTMENTNAME,
                p.Budget AS BUDGET, 
                p.ClosingDate AS CLOSINGDATE
            FROM "Position" p
            LEFT JOIN "Recruiter" r ON p.RecruiterId = r.Id
            LEFT JOIN "Department" d ON p.DepartmentId = d.Id
            ORDER BY p.Id;
            
        DBMS_OUTPUT.PUT_LINE('Query executed successfully');
    END get_all_positions;
    
    -- Get position by ID
    PROCEDURE get_position_by_id(
        p_id IN NUMBER,
        p_cursor OUT position_cursor
    ) IS
    BEGIN
        OPEN p_cursor FOR
            SELECT 
                p.Id AS ID, 
                p.Title AS TITLE, 
                p.Description AS DESCRIPTION, 
                p.Location AS LOCATION, 
                p.Status AS STATUS, 
                p.RecruiterId AS RECRUITERID, 
                r.Name AS RECRUITERNAME,
                p.DepartmentId AS DEPARTMENTID, 
                d.Name AS DEPARTMENTNAME,
                p.Budget AS BUDGET, 
                p.ClosingDate AS CLOSINGDATE
            FROM Position p
            JOIN Recruiter r ON p.RecruiterId = r.Id
            JOIN Department d ON p.DepartmentId = d.Id
            WHERE p.Id = p_id;
    END get_position_by_id;
    
    -- Add new position
    PROCEDURE add_position(
        p_title IN VARCHAR2,
        p_description IN VARCHAR2,
        p_location IN VARCHAR2,
        p_status IN VARCHAR2,
        p_recruiter_id IN NUMBER,
        p_department_id IN NUMBER,
        p_budget IN NUMBER,
        p_closing_date IN DATE,
        p_success OUT NUMBER
    ) IS
    BEGIN
        p_success := 0;
        
        INSERT INTO Position (
            Id, Title, Description, Location, Status, 
            RecruiterId, DepartmentId, Budget, ClosingDate
        ) VALUES (
            seq_position_id.NEXTVAL,
            p_title,
            p_description,
            p_location,
            p_status,
            p_recruiter_id,
            p_department_id,
            p_budget,
            p_closing_date
        );
        
        p_success := 1;
    EXCEPTION
        WHEN OTHERS THEN
            p_success := 0;
            RAISE;
    END add_position;
    
    -- Update position by ID
    PROCEDURE update_position(
        p_id IN NUMBER,
        p_title IN VARCHAR2 DEFAULT NULL,
        p_description IN VARCHAR2 DEFAULT NULL,
        p_location IN VARCHAR2 DEFAULT NULL,
        p_status IN VARCHAR2 DEFAULT NULL,
        p_recruiter_id IN NUMBER DEFAULT NULL,
        p_department_id IN NUMBER DEFAULT NULL,
        p_budget IN NUMBER DEFAULT NULL,
        p_closing_date IN DATE DEFAULT NULL,
        p_success OUT NUMBER
    ) IS
    BEGIN
        p_success := 0;
        
        UPDATE Position
        SET 
            Title = NVL(p_title, Title),
            Description = NVL(p_description, Description),
            Location = NVL(p_location, Location),
            Status = NVL(p_status, Status),
            RecruiterId = NVL(p_recruiter_id, RecruiterId),
            DepartmentId = NVL(p_department_id, DepartmentId),
            Budget = NVL(p_budget, Budget),
            ClosingDate = NVL(p_closing_date, ClosingDate),
            UpdatedAt = SYSTIMESTAMP
        WHERE Id = p_id;
        
        p_success := 1;
    EXCEPTION
        WHEN OTHERS THEN
            p_success := 0;
            RAISE;
    END update_position;
    
    -- Remove position by ID
    PROCEDURE remove_position(
        p_id IN NUMBER,
        p_success OUT NUMBER
    ) IS
    BEGIN
        p_success := 0;
        
        DELETE FROM Position
        WHERE Id = p_id;
        
        p_success := 1;
    EXCEPTION
        WHEN OTHERS THEN
            p_success := 0;
            RAISE;
    END remove_position;
END position_pkg;
/


