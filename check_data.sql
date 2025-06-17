-- Check data in Position table
SELECT p.*, r.Name as RecruiterName, d.Name as DepartmentName
FROM Position p
JOIN Recruiter r ON p.RecruiterId = r.Id
JOIN Department d ON p.DepartmentId = d.Id;

-- Check Recruiter table
SELECT * FROM Recruiter;

-- Check Department table
SELECT * FROM Department;
