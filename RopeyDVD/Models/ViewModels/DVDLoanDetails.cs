using Microsoft.EntityFrameworkCore;

namespace RopeyDVD.Models.ViewModels

{
    [Keyless]
    public class DVDLoanDetails
    {
        public string MemberName { get; set; }  
        public DateTime DateOut { get; set; }   
        public DateTime DateDue { get; set; }
        public DateTime ?DateReturned { get; set; }
        public string DVDTitle { get; set; }

    }
}
