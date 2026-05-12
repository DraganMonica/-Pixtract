namespace Pixtract.Domain.Entities;

public class Plan
{
    public int Id { get; set; }

    // Free, Pro, Ultra
    public string Name { get; set; } = null!;

    // 5, , 100
    public int DailyImageLimit { get; set; }  

    //nr imagini per request
    public int ImagesPerRequest { get; set; } // 1, 5, 10
    public decimal Price { get; set; }
    public bool CanExportHistory { get; set; }
}
