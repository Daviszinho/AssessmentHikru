-- Check package status
SELECT object_name, object_type, status, last_ddl_time, timestamp
FROM user_objects
WHERE object_name = 'POSITION_PKG'
ORDER BY object_type, object_name;

-- Test get_all_positions procedure
SET SERVEROUTPUT ON;
DECLARE
  v_cursor SYS_REFCURSOR;
  v_id NUMBER;
  v_title VARCHAR2(100);
  v_description VARCHAR2(1000);
  v_location VARCHAR2(255);
  v_status VARCHAR2(20);
  v_recruiter_id NUMBER;
  v_department_id NUMBER;
  v_budget NUMBER(18,2);
  v_closing_date DATE;
  v_recruiter_name VARCHAR2(255);
  v_department_name VARCHAR2(255);
  v_count NUMBER := 0;
BEGIN
  DBMS_OUTPUT.PUT_LINE('Testing position_pkg.get_all_positions...');
  
  position_pkg.get_all_positions(v_cursor);
  
  LOOP
    FETCH v_cursor INTO 
      v_id, v_title, v_description, v_location, v_status, 
      v_recruiter_id, v_recruiter_name, v_department_id, v_department_name,
      v_budget, v_closing_date;
    EXIT WHEN v_cursor%NOTFOUND;
    
    v_count := v_count + 1;
    DBMS_OUTPUT.PUT_LINE('Position ' || v_count || ':');
    DBMS_OUTPUT.PUT_LINE('  ID: ' || v_id);
    DBMS_OUTPUT.PUT_LINE('  Title: ' || v_title);
    DBMS_OUTPUT.PUT_LINE('  Recruiter: ' || v_recruiter_name);
    DBMS_OUTPUT.PUT_LINE('  Department: ' || v_department_name);
    DBMS_OUTPUT.PUT_LINE('  Status: ' || v_status);
    DBMS_OUTPUT.PUT_LINE('-------------------');
  END LOOP;
  
  CLOSE v_cursor;
  
  IF v_count = 0 THEN
    DBMS_OUTPUT.PUT_LINE('No positions found in the database.');
    
    -- Check if Position table has any data
    SELECT COUNT(*) INTO v_count FROM Position;
    DBMS_OUTPUT.PUT_LINE('Total rows in Position table: ' || v_count);
    
    -- If no data, suggest inserting test data
    IF v_count = 0 THEN
      DBMS_OUTPUT.PUT_LINE('No data found in Position table. Consider running the INSERT statements in scripts.sql');
    END IF;
  END IF;
  
EXCEPTION
  WHEN OTHERS THEN
    DBMS_OUTPUT.PUT_LINE('Error: ' || SQLERRM);
    IF v_cursor%ISOPEN THEN
      CLOSE v_cursor;
    END IF;
END;
/
