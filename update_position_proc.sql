-- =============================================
-- CREATE UPDATE POSITION PROCEDURE
-- =============================================
CREATE OR REPLACE PROCEDURE update_position (
    p_id IN NUMBER,
    p_title IN VARCHAR2,
    p_description IN VARCHAR2,
    p_location IN VARCHAR2,
    p_status IN VARCHAR2,
    p_recruiter_id IN NUMBER,
    p_department_id IN NUMBER,
    p_budget IN NUMBER,
    p_closing_date IN DATE,
    p_success OUT NUMBER,
    p_message OUT VARCHAR2
)
IS
BEGIN
    p_success := 0;
    
    UPDATE "POSITION" 
    SET 
        Title = p_title,
        Description = p_description,
        Location = p_location,
        Status = p_status,
        RecruiterId = p_recruiter_id,
        DepartmentId = p_department_id,
        Budget = p_budget,
        ClosingDate = p_closing_date,
        UpdatedAt = SYSDATE
    WHERE Id = p_id;
    
    IF SQL%ROWCOUNT > 0 THEN
        p_success := 1; -- Success
        COMMIT;
    END IF;
    
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        p_success := 0; -- Failure
        p_message := 'Error in update_position: ' || SQLERRM;
        -- Optionally, you can log the error here
        DBMS_OUTPUT.PUT_LINE(p_message);
END update_position;
/

-- =============================================
-- TEST UPDATE PROCEDURE
-- =============================================
DECLARE
    v_success NUMBER;
BEGIN
    update_position(
        p_id => 129,  -- Existing position ID
        p_title => 'Senior Software Engineer (Updated)',
        p_description => 'Updated description',
        p_location => 'Updated Location',
        p_status => 'Active',
        p_recruiter_id => 1,  -- Existing recruiter ID
        p_department_id => 1,  -- Existing department ID
        p_budget => 100000,
        p_closing_date => TO_DATE('2023-12-31', 'YYYY-MM-DD'),
        p_success => v_success
    );
    
    IF v_success = 1 THEN
        DBMS_OUTPUT.PUT_LINE('Position updated successfully');
    ELSE
        DBMS_OUTPUT.PUT_LINE('Failed to update position');
    END IF;
END;
/
