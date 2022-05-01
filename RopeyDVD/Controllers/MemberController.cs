using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RopeyDVD.Data;
using RopeyDVD.Models.ViewModels;

namespace RopeyDVD.Controllers
{
    public class MemberController : Controller
    {
        private readonly ApplicationDBContext _context;
        public MemberController(ApplicationDBContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> index()
        {
            return View(await _context.Member.ToListAsync());
        }

        public async Task<IActionResult> MemberDVD(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            DateTime localDate = DateTime.Now.AddDays(-31);

            var member = await _context.Member.FindAsync(id);

            var @memberDetails = _context.Member.Where(a => a.MembershipNumber == id)
                .Join(
            _context.Loan,
            Member => Member.MembershipNumber,
            Loan => Loan.MemberNumber,
            (Member, Loan) => new
            {
                Member,Loan, DateOut = Loan.DateOut,
            }
            ).Join(
                _context.DVDCopy,
                DVDCopy => DVDCopy.Loan.CopyNumber,
                Loan => Loan.CopyNumber,
                (DVDCopy,Loan) => new
                {
                    DVDCopy, Loan,
                }
                ).Join(
                _context.DVDTitle,
                DVDCopy => DVDCopy.Loan.DVDNumber,
                DVDTitle => DVDTitle.DVDNumber,
                (DVDCopy, DVDTitle) => new
                {
                    DVDCopy,
                    DateOut = DVDCopy.DVDCopy.DateOut,DVDTitle,
                }
                )
            .Where(a => a.DateOut >= localDate).ToList();

            List<MemberDVD> memberDVD = new List<MemberDVD>();


            foreach (var DVD in memberDetails)
            {
                memberDVD.Add(new MemberDVD()
                {
                    Title = DVD.DVDTitle.DVDName,
                    CopyNumber = DVD.DVDCopy.Loan.CopyNumber,
                });
            }

            return View(memberDVD);

        }

    }
}
