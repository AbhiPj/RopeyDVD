using Microsoft.EntityFrameworkCore;

namespace RopeyDVD.Models.ViewModels
{
    [Keyless]

    public class DVDCopiesTotalViewModel
    {
        public int Total { get; set; }
        public string DVDName { get; set; } 
    }
}
