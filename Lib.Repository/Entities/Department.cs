/*CREATE TABLE Department (
    DepartmentId NUMBER PRIMARY KEY,
    Name VARCHAR2(255) NOT NULL,

);
*/

namespace Lib.Repository.Entities;

public class Department
{
    public int Id { get; set; }
    public string Name {    get; set; }
}
