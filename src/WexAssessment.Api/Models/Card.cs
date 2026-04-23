namespace WexAssessment.Api.Models;

public class Card
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public decimal CreditLimit { get; set; }
    public ICollection<Transaction> Transactions { get; set; } = [];
}