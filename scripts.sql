-- =============================================
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

-- Create Recruiter table
CREATE TABLE Recruiter (
    RecruiterId NUMBER PRIMARY KEY,
    Name VARCHAR2(255) NOT NULL
);

-- Create Department table
CREATE TABLE Department (
    DepartmentId NUMBER PRIMARY KEY,
    Name VARCHAR2(255) NOT NULL
);

-- Create Position table
CREATE TABLE Position (
    PositionId NUMBER PRIMARY KEY,
    Title VARCHAR2(100) NOT NULL,
    Description VARCHAR2(1000) NOT NULL,
    Location VARCHAR2(255) NOT NULL,
    Status VARCHAR2(20) CHECK (Status IN ('draft', 'open', 'closed', 'archived')) NOT NULL,
    RecruiterId NUMBER NOT NULL,
    DepartmentId NUMBER NOT NULL,
    Budget NUMBER(12,2) NOT NULL,
    ClosingDate DATE,
    CONSTRAINT fk_recruiter FOREIGN KEY (RecruiterId) REFERENCES Recruiter(RecruiterId),
    CONSTRAINT fk_department FOREIGN KEY (DepartmentId) REFERENCES Department(DepartmentId)
);

-- =============================================
-- INSERT SAMPLE DATA
-- =============================================

-- Insert Recruiters
INSERT INTO Recruiter (RecruiterId, Name) 
VALUES (1, 'Ana Rodríguez');

INSERT INTO Recruiter (RecruiterId, Name) 
VALUES (2, 'Carlos Gómez');

-- Insert Departments
INSERT INTO Department (DepartmentId, Name) 
VALUES (1, 'Recursos Humanos');

INSERT INTO Department (DepartmentId, Name) 
VALUES (2, 'Tecnología');

-- Insert Positions
INSERT INTO Position (PositionId, Title, Description, Location, Status, RecruiterId, DepartmentId, Budget, ClosingDate)
VALUES (1, 'Desarrollador Backend', 'Desarrollo de APIs con PostgreSQL y Oracle.', 'San José', 'open', 2, 2, 75000.00, TO_DATE('2025-06-30', 'YYYY-MM-DD'));

INSERT INTO Position (PositionId, Title, Description, Location, Status, RecruiterId, DepartmentId, Budget, ClosingDate)
VALUES (2, 'Analista de Recursos Humanos', 'Gestión de contrataciones y selección de personal.', 'Heredia', 'draft', 1, 1, 50000.00, NULL);

INSERT INTO Position (PositionId, Title, Description, Location, Status, RecruiterId, DepartmentId, Budget, ClosingDate)
VALUES (3, 'Administrador de Base de Datos', 'Manejo de bases de datos Oracle y optimización de consultas.', 'San José', 'closed', 2, 2, 90000.00, TO_DATE('2025-05-15', 'YYYY-MM-DD'));

COMMIT;

-- =============================================
-- PACKAGE SPECIFICATION
-- =============================================
CREATE OR REPLACE PACKAGE position_pkg AS
    -- Cursor type for position data
    TYPE position_cursor IS REF CURSOR;
    
    -- Get all positions
    PROCEDURE get_all_positions(p_cursor OUT position_cursor);
    
    -- Get position by ID
    PROCEDURE get_position_by_id(
        p_position_id IN NUMBER,
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
        p_closing_date IN DATE DEFAULT NULL,
        p_position_id OUT NUMBER
    );
    
    -- Update position by ID
    PROCEDURE update_position(
        p_position_id IN NUMBER,
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
    
    -- Remove position by ID
    PROCEDURE remove_position(
        p_position_id IN NUMBER,
        p_success OUT NUMBER
    );
END position_pkg;
/

-- Package body
CREATE OR REPLACE PACKAGE BODY position_pkg AS
    -- Get all positions
    PROCEDURE get_all_positions(p_cursor OUT position_cursor) IS
    BEGIN
        OPEN p_cursor FOR
            SELECT p.PositionId, p.Title, p.Description, p.Location, 
                   p.Status, p.RecruiterId, r.Name as RecruiterName,
                   p.DepartmentId, d.Name as DepartmentName,
                   p.Budget, p.ClosingDate
            FROM Position p
            JOIN Recruiter r ON p.RecruiterId = r.RecruiterId
            JOIN Department d ON p.DepartmentId = d.DepartmentId
            ORDER BY p.PositionId;
    END get_all_positions;
    
    -- Get position by ID
    PROCEDURE get_position_by_id(
        p_position_id IN NUMBER,
        p_cursor OUT position_cursor
    ) IS
    BEGIN
        OPEN p_cursor FOR
            SELECT p.PositionId, p.Title, p.Description, p.Location, 
                   p.Status, p.RecruiterId, r.Name as RecruiterName,
                   p.DepartmentId, d.Name as DepartmentName,
                   p.Budget, p.ClosingDate
            FROM Position p
            JOIN Recruiter r ON p.RecruiterId = r.RecruiterId
            JOIN Department d ON p.DepartmentId = d.DepartmentId
            WHERE p.PositionId = p_position_id;
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
        p_closing_date IN DATE DEFAULT NULL,
        p_position_id OUT NUMBER
    ) IS
        v_new_id NUMBER;
    BEGIN
        -- Get next position ID
        SELECT NVL(MAX(PositionId), 0) + 1 INTO v_new_id FROM Position;
        
        -- Insert new position
        INSERT INTO Position (
            PositionId, Title, Description, Location, Status,
            RecruiterId, DepartmentId, Budget, ClosingDate
        ) VALUES (
            v_new_id, p_title, p_description, p_location, p_status,
            p_recruiter_id, p_department_id, p_budget, p_closing_date
        );
        
        p_position_id := v_new_id;
        COMMIT;
    EXCEPTION
        WHEN OTHERS THEN
            ROLLBACK;
            RAISE;
    END add_position;
    
    -- Update position by ID
    PROCEDURE update_position(
        p_position_id IN NUMBER,
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
            ClosingDate = p_closing_date
        WHERE PositionId = p_position_id;
        
        IF SQL%ROWCOUNT > 0 THEN
            p_success := 1;
            COMMIT;
        END IF;
    EXCEPTION
        WHEN OTHERS THEN
            ROLLBACK;
            RAISE;
    END update_position;
    
    -- Remove position by ID
    PROCEDURE remove_position(
        p_position_id IN NUMBER,
        p_success OUT NUMBER
    ) IS
    BEGIN
        p_success := 0;
        
        DELETE FROM Position
        WHERE PositionId = p_position_id;
        
        IF SQL%ROWCOUNT > 0 THEN
            p_success := 1;
            COMMIT;
        END IF;
    EXCEPTION
        WHEN OTHERS THEN
            ROLLBACK;
            RAISE;
    END remove_position;
END position_pkg;
/
