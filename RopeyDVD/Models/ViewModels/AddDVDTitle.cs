namespace RopeyDVD.Models.ViewModels
{
    public class AddDVDTitle
    {
        public string DVDName { get; set; }
        public int StandardCharge { get; set; }
        public int PenaltyCharge { get; set; }
        public DateTime DateReleased { get; set; }

        public int ActorNumber { get; set; }
        public int StudioNumber { get; set; }
        public int ProducerNumber { get; set; }

        public int CategoryNumber { get; set; }
        public int DVDCategoryCategoryNumber { get; set; }

        
    }
}
