using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RopeyDVD.Models
{
    public class CastMember
    {
        [ForeignKey("DVDTitle")]
        public int DVDNumber { get; set; }
        public DVDTitle DVDTitle { get; set; }

        [ForeignKey("Actor")]
        public int ActorNumber { get; set; }
        public Actor Actor { get; set; }

    }
}
