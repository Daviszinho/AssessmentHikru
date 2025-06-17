/*
CREATE TABLE Recruiter (
    RecruiterId NUMBER PRIMARY KEY,
    Name VARCHAR2(255) NOT NULL,
    Email VARCHAR2(255) UNIQUE NOT NULL
);
*/

namespace Lib.Repository.Entities;

public class Recruiter
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}
