using System.ComponentModel.DataAnnotations;

namespace RopeyDVD.Models
{
    public class Actor
    {

        [Key]
        public int ActorNumber { get; set; } 
        public string ActorSurname { get; set; }
        public int ActorFirstName { get; set; }
        public ICollection<CastMember> CastMember { get; set; }

    }
}
