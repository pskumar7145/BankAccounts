namespace API.Models.DTO
{
    public class DepositsDTO
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
    }

}
