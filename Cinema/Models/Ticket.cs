namespace Cinema.Models
{
    [PrimaryKey(nameof(ApplicationUserId), nameof(MovieId))]
    public class Ticket
    {
        public string ApplicationUserId { get; set; }= string.Empty;
        public ApplicationUser ApplicationUser { get; set; }= null!;
        public int MovieId { get; set; }
        public Movie Movie { get; set; }= null!;
        public int Count { get; set; }
        public DateTime Date { get; set; }
        public decimal TotalPrice { get; set; }
        public string SelectedSeats { get; set; } 


    }
}
