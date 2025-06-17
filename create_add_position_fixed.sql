CREATE OR REPLACE PROCEDURE add_position (
    p_title IN VARCHAR2,
    p_description IN VARCHAR2,
    p_location IN VARCHAR2,
    p_status IN VARCHAR2,
    p_recruiter_id IN NUMBER,
    p_department_id IN NUMBER,
    p_budget IN NUMBER,
    p_closing_date IN DATE,
    p_new_id OUT NUMBER,
    p_success OUT NUMBER
)
IS
    v_new_id NUMBER;
BEGIN
    -- Initialize outputs
    p_success := 0;
    p_new_id := NULL;
    
    -- Input validation
    IF p_title IS NULL OR p_recruiter_id IS NULL OR p_department_id IS NULL THEN
        RETURN;
    END IF;
    
    -- Get the next ID from the sequence
    SELECT "ADMIN"."SEQ_POSITION_ID".NEXTVAL INTO v_new_id FROM DUAL;
    
    -- Insert the new position
    INSERT INTO "POSITION" (
        ID,
        TITLE,
        DESCRIPTION,
        LOCATION,
        STATUS,
        RECRUITERID,
        DEPARTMENTID,
        BUDGET,
        CLOSINGDATE,
        CREATEDAT,
        UPDATEDAT
    ) VALUES (
        v_new_id,
        p_title,
        p_description,
        p_location,
        p_status,
        p_recruiter_id,
        p_department_id,
        p_budget,
        p_closing_date,
        SYSDATE,
        SYSDATE
    );
    
    -- Set output parameters
    p_new_id := v_new_id;
    p_success := 1;
    
    -- Commit the transaction
    COMMIT;
    
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        p_success := 0;
        p_new_id := NULL;
        DBMS_OUTPUT.PUT_LINE('Error adding position: ' || SQLERRM);
END add_position;
/

-- Verify the procedure was created
SELECT object_name, object_type, status 
FROM user_objects 
WHERE object_name = 'ADD_POSITION';

-- Show any compilation errors
SELECT line, position, text 
FROM user_errors 
WHERE name = 'ADD_POSITION' 
ORDER BY line, position;
