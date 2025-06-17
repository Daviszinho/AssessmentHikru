
namespace Lib.Repository.Entities;

public class Position
{
     public int PositionId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Location { get; set; }
        public int? DepartmentId { get; set; }
        public int? RecruiterId { get; set; }
        public decimal? Budget { get; set; }
        public DateTime? ClosingDate { get; set; }
        public string? Status { get; set; }
}