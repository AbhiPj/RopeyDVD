using Microsoft.EntityFrameworkCore;

namespace RopeyDVD.Models.ViewModels
{
    [Keyless]
    public class MemberDVD
    {
        public string Title { get; set; }
        public int CopyNumber { get; set; }
    }
}
