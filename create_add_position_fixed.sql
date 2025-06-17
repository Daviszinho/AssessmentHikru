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
    p_success OUT NUMBER,
    p_error_code OUT NUMBER,
    p_error_message OUT VARCHAR2
)
IS
    v_new_id NUMBER;
    v_recruiter_exists NUMBER := 0;
    v_department_exists NUMBER := 0;
    v_budget_valid NUMBER := 1;
BEGIN
    -- Initialize outputs
    p_success := 0;
    p_new_id := NULL;
    p_error_code := 0;
    p_error_message := NULL;
    
    -- Input validation
    IF p_title IS NULL OR p_recruiter_id IS NULL OR p_department_id IS NULL THEN
        p_error_code := 400;
        p_error_message := 'Missing required fields: Title, Recruiter ID, and Department ID are required';
        RETURN;
    END IF;
    
    -- Validate budget is not negative
    IF p_budget < 0 THEN
        p_error_code := 400;
        p_error_message := 'Invalid budget: Budget cannot be negative';
        v_budget_valid := 0;
    END IF;
    
    -- Check if recruiter exists
    BEGIN
        SELECT 1 INTO v_recruiter_exists 
        FROM "RECRUITER" 
        WHERE "ID" = p_recruiter_id
        AND ROWNUM = 1;
    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            v_recruiter_exists := 0;
    END;
    
    -- Check if department exists
    BEGIN
        SELECT 1 INTO v_department_exists 
        FROM "DEPARTMENT" 
        WHERE "ID" = p_department_id
        AND ROWNUM = 1;
    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            v_department_exists := 0;
    END;
    
    -- Validate references
    IF v_recruiter_exists = 0 OR v_department_exists = 0 OR v_budget_valid = 0 THEN
        p_error_code := 404;
        p_error_message := 'Validation failed: ' || 
                          CASE 
                              WHEN v_recruiter_exists = 0 THEN 'Recruiter not found. '
                              ELSE '' 
                          END ||
                          CASE 
                              WHEN v_department_exists = 0 THEN 'Department not found. '
                              ELSE '' 
                          END ||
                          CASE 
                              WHEN v_budget_valid = 0 THEN 'Invalid budget.'
                              ELSE '' 
                          END;
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
