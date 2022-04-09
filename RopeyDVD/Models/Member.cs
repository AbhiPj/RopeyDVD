using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RopeyDVD.Models
{
    public class Member
    {
        [Key]
        public int MembershipNumber { get; set; }

        [ForeignKey("MembershipCategory")]
        public int MembershipCategoryNumber { get; set; }
        public MembershipCategory MembershipCategory { get; set; }

        public string MembershipLastName { get; set; }
        public string MembershipFirstName { get; set; }
        public string MembershipAddress { get; set; }
        public DateTime MemberDOB { get; set; }
        public ICollection<Loan> Loan { get; set; }







    }
}
