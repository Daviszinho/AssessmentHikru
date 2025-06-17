        -- =============================================
-- DROP EXISTING PROCEDURE
-- =============================================
BEGIN
   EXECUTE IMMEDIATE 'DROP PROCEDURE get_all_positions';
EXCEPTION
   WHEN OTHERS THEN
      IF SQLCODE != -4043 THEN  -- ORA-04043: object does not exist
         RAISE;
      END IF;
END;
/

CREATE OR REPLACE PROCEDURE get_all_positions (
    p_cursor OUT SYS_REFCURSOR
)
IS
BEGIN
    OPEN p_cursor FOR 
        SELECT 
            p.Id, 
            p.Title, 
            p.Description, 
            p.Location, 
            p.Status, 
            p.RecruiterId, 
            r.Name AS RecruiterName,
            p.DepartmentId, 
            d.Name AS DepartmentName,
            p.Budget, 
            p.ClosingDate
        FROM "POSITION" p
        LEFT JOIN "RECRUITER" r ON p.RecruiterId = r.Id
        LEFT JOIN "DEPARTMENT" d ON p.DepartmentId = d.Id
        ORDER BY p.Id;
END get_all_positions;
/
SELECT table_name FROM all_tables WHERE lower(table_name) IN ('position', 'recruiter', 'department');


-- =============================================
-- TEST THE PROCEDURE
-- =============================================
SET SERVEROUTPUT ON;
DECLARE
    v_cursor SYS_REFCURSOR;
    v_id NUMBER;
    v_title VARCHAR2(100);
    v_description VARCHAR2(1000);
    v_location VARCHAR2(255);
    v_status VARCHAR2(20);
    v_recruiter_id NUMBER;
    v_recruiter_name VARCHAR2(255);
    v_department_id NUMBER;
    v_department_name VARCHAR2(255);
    v_budget NUMBER(18,2);
    v_closing_date DATE;
    v_count NUMBER := 0;
BEGIN
    DBMS_OUTPUT.PUT_LINE('Testing get_all_positions procedure...');
    
    -- Call the procedure
    get_all_positions(v_cursor);
    
    -- Simple test to show we got data
    LOOP
        FETCH v_cursor INTO 
            v_id, 
            v_title, 
            v_description, 
            v_location, 
            v_status, 
            v_recruiter_id, 
            v_recruiter_name,
            v_department_id, 
            v_department_name,
            v_budget, 
            v_closing_date;
        EXIT WHEN v_cursor%NOTFOUND;
        
        v_count := v_count + 1;
        DBMS_OUTPUT.PUT_LINE('Position ' || v_count || ': ID=' || v_id || ', Title=' || v_title);
    END LOOP;
    
    CLOSE v_cursor;
    
    IF v_count = 0 THEN
        DBMS_OUTPUT.PUT_LINE('No positions found in the database.');
    ELSE
        DBMS_OUTPUT.PUT_LINE('Successfully retrieved ' || v_count || ' positions.');
    END IF;
EXCEPTION
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('Error: ' || SQLERRM);
        IF v_cursor%ISOPEN THEN
            CLOSE v_cursor;
        END IF;
        DBMS_OUTPUT.PUT_LINE('Error backtrace: ' || DBMS_UTILITY.FORMAT_ERROR_BACKTRACE);
END;
/
