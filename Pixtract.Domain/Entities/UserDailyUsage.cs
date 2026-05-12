namespace Pixtract.Domain.Entities;

public class UserDailyUsage
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
    public DateTime Date { get; set; }

    //setting ul e default momentan, creste cand se adauga imaginile
    public int ImagesUsed { get; set; } = 0;
}
