using Pixtract.Domain.Enums;

namespace Pixtract.Domain.Entities;

// pentru viitoarele implementari
public class BatchJob
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
    public string Category { get; set; }
    public int TotalImages { get; set; }

    // cate din totalImages au fost procesate
    public int ProcessedImages { get; set; } = 0;

    // Pending, Processing, Done, Failed
    public BatchStatus Status { get; set; }        
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
