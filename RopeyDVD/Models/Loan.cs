using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RopeyDVD.Models
{
    public class Loan
    {
        [Key]
        public int LoanNumber { get; set; }
        [ForeignKey("LoanType")]
        public int LoanTypeNumber { get; set; }
        public LoanType LoanType { get; set; }
        [ForeignKey("DVDCopy")]
        public int CopyNumber { get; set; }
        public DVDCopy DVDCopy { get; set; }
        [ForeignKey("Member")]
        public int MemberNumber { get; set; }
        public Member Member { get; set; }
        public DateTime DateOut { get; set; }
        public DateTime DateDue { get; set; }
        public DateTime ?DateReturned { get; set; }

    }
}
