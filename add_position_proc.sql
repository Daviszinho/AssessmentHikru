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
) AS
    v_count NUMBER;
BEGIN
    -- Initialize outputs
    p_success := 0;
    p_new_id := NULL;
    
    -- Input validation
    IF p_title IS NULL OR p_recruiter_id IS NULL OR p_department_id IS NULL THEN
        RETURN;
    END IF;
    
    -- Check if recruiter exists
    SELECT COUNT(*) INTO v_count 
    FROM recruiters 
    WHERE id = p_recruiter_id;
    
    IF v_count = 0 THEN
        RETURN;
    END IF;
    
    -- Check if department exists
    SELECT COUNT(*) INTO v_count 
    FROM departments 
    WHERE id = p_department_id;
    
    IF v_count = 0 THEN
        RETURN;
    END IF;
    
    -- Insert the new position
    INSERT INTO positions (
        title,
        description,
        location,
        status,
        recruiter_id,
        department_id,
        budget,
        closing_date,
        created_at,
        updated_at
    ) VALUES (
        p_title,
        p_description,
        p_location,
        NVL(p_status, 'draft'),
        p_recruiter_id,
        p_department_id,
        p_budget,
        p_closing_date,
        SYSDATE,
        SYSDATE
    )
    RETURNING id INTO p_new_id;
    
    -- Set success flag
    p_success := 1;
    
    COMMIT;
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        p_success := 0;
        p_new_id := NULL;
        -- Log the error (consider adding a logging table in production)
        DBMS_OUTPUT.PUT_LINE('Error in add_position: ' || SQLERRM);
END add_position;
/

-- Grant execute permission to the appropriate user/role
-- GRANTE EXECUTE ON add_position TO your_role;
