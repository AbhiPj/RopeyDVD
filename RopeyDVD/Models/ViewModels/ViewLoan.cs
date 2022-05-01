using Microsoft.AspNetCore.Mvc.Rendering;

namespace RopeyDVD.Models.ViewModels
{
    public class ViewLoan
    {
        public List<SelectListItem> DVDCopyNumber { get; set; }
        public List<SelectListItem> LoanType { get; set; }
        public List<SelectListItem> Member { get; set; }

    }
}
