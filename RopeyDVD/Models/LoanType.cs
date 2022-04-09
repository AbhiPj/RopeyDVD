namespace RopeyDVD.Models;
using System.ComponentModel.DataAnnotations;


public class LoanType
{
    [Key]
    public int LoanTypeNumber { get; set; }
    public string loanType { get; set; }
    public string LoanDuration { get; set; }
    public ICollection<Loan> Loan { get; set; }



}
