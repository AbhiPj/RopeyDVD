using System.ComponentModel.DataAnnotations;

namespace RopeyDVD.Models
{
    public class DVDCategory
    {
        [Key]
        public int CategoryNumber { get; set; }
        public string CategotyDescription { get; set; }
        public string AgeRestricted { get; set; }
        public ICollection<DVDTitle> DVDTitle { get; set; }

    }
}
