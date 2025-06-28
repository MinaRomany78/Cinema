using Cinema.Models;

namespace Cinema.ViewModel
{
    public class CinemawithCategoryVm
    {
        public List<Models.Cinema> Cinemas { get; set; } = null!;
        public List<Models.Category> Categories { get; set; } = null!;
        public Models.Movie Movie { get; set; } = null!;
        public List<Actor> ActorList { get; set; }= null!;
        public List<int> SelectedActorIds { get; set; } = null!; // 👈 اللي هيتم اختياره

    }
}
