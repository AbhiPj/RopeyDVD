namespace RopeyDVD.Models;
using System.ComponentModel.DataAnnotations;


public class MembershipCategory
{
    [Key]
    public int MembershipCategoryNumber { get; set; }
    public string MembershipCategoryDescription { get; set; }
    public int MembershipCategoryTotalLoans { get; set; }
    public ICollection<Member> Member { get; set; }


}
