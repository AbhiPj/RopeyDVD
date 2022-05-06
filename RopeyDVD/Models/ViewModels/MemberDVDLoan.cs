namespace RopeyDVD.Models.ViewModels
{
    public class MemberDVDLoan
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public DateTime DateOut { get; set; }
        public string DVDTitle { get; set; }
        public int NoOfDays { get; set; }

    }
}
