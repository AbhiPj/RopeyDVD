using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RopeyDVD.Models
{
    public class DVDTitle
    {
        [Key]
        public int DVDNumber { get; set; }

        [ForeignKey("Producer")]
        public int ProducerNumber { get; set; }
        public Producer Producer { get; set; }
        [ForeignKey("Category")]
        public int CategoryNumber { get; set; }
        public DVDCategory DVDCategory { get; set; }
        [ForeignKey("Studio")]
        public int StudioNumber { get; set; }
        public Studio Studio { get; set; }
        public DateTime DateReleased { get; set; }
        public int StandardCharge { get; set; }
        public int PenaltyCharge { get; set; }
        public ICollection<CastMember> CastMember { get; set; }
        public ICollection<DVDCopy> DVDCopy { get; set; }

    }
}
