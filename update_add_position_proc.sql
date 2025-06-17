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
    -- Initialize success to 0 (failure)
    p_success := 0;
    
    -- Log input parameters
    DBMS_OUTPUT.PUT_LINE('--- Input Parameters ---');
    DBMS_OUTPUT.PUT_LINE('p_title: ' || p_title);
    DBMS_OUTPUT.PUT_LINE('p_recruiter_id: ' || p_recruiter_id);
    DBMS_OUTPUT.PUT_LINE('p_department_id: ' || p_department_id);
    
    -- Check if recruiter exists
    SELECT COUNT(*) INTO v_count FROM "RECRUITER" WHERE "ID" = p_recruiter_id;
    DBMS_OUTPUT.PUT_LINE('Recruiter exists: ' || CASE WHEN v_count > 0 THEN 'YES' ELSE 'NO' END);
    
    -- Check if department exists
    SELECT COUNT(*) INTO v_count FROM "DEPARTMENT" WHERE "ID" = p_department_id;
    DBMS_OUTPUT.PUT_LINE('Department exists: ' || CASE WHEN v_count > 0 THEN 'YES' ELSE 'NO' END);
    
    -- If both exist, insert the position
    IF v_count > 0 THEN
        -- Get next ID from sequence
        SELECT "ADMIN"."SEQ_POSITION_ID".NEXTVAL INTO p_new_id FROM DUAL;
        DBMS_OUTPUT.PUT_LINE('New ID: ' || p_new_id);
        
        -- Insert the position
        INSERT INTO "POSITION" (
            "ID",
            "TITLE",
            "DESCRIPTION",
            "LOCATION",
            "STATUS",
            "RECRUITERID",
            "DEPARTMENTID",
            "BUDGET",
            "CLOSINGDATE",
            "CREATEDAT",
            "UPDATEDAT"
        ) VALUES (
            p_new_id,
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
        
        p_success := 1; -- Success
        DBMS_OUTPUT.PUT_LINE('Position inserted successfully');
    ELSE
        DBMS_OUTPUT.PUT_LINE('Validation failed: Recruiter or Department does not exist');
    END IF;
    
    COMMIT;
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        DBMS_OUTPUT.PUT_LINE('Error: ' || SQLERRM);
        p_success := 0;
END add_position;
/
