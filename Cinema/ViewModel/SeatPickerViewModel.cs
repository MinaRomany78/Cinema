namespace Cinema.ViewModel
{
    public class SeatPickerViewModel
    {
        public int MovieId { get; set; }
        public Movie Movie { get; set; }

        public int Count { get; set; }
        public DateTime Time { get; set; }

        public string SelectedSeats { get; set; }
    }

}

