using Microsoft.EntityFrameworkCore;

namespace RopeyDVD.Models.ViewModels
{
    [Keyless]
    public class ActorDetailsViewModel
    {
        public IEnumerable<int> ActorNumber { get; set; }
        public IEnumerable<string> DVDName { get; set; }
        public string ActorName { get; set; }
    }
}
