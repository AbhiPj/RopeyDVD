namespace RopeyDVD.Models.ViewModels
{
    public class DVDLoanViewModel
    {
        public string DVDTitle { get; set; }
        public int CopyNumber { get; set; }
        public string MemberName { get; set; }
        public DateTime DateOut { get; set; }
        public string TotalLoans { get; set; }

    }
}
