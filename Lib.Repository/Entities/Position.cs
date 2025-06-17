/*
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
*/


namespace Lib.Repository.Entities;

public class Position
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Location { get; set; }
    public string Status { get; set; }
    public int RecruiterId { get; set; }
    public int DepartmentId { get; set; }
    public decimal Budget { get; set; }
    public DateTime ClosingDate { get; set; }
}