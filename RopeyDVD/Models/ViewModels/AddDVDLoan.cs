namespace RopeyDVD.Models.ViewModels
{
    public class AddDVDLoan
    {
        public int Age { get; set; }
        public string Price { get; set; }

        public int CopyNumber { get; set; }
        public int LoanType { get; set; }
        public int MemberNumber { get; set; }
        public DateTime DateOut { get; set; }   

    }
}
