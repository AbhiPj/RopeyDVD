using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RopeyDVD.Data;
using RopeyDVD.Models.ViewModels;

namespace RopeyDVD.Controllers
{
    [Authorize(Roles = "Admin, User")]
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

        public async Task<IActionResult> MemberList()
        {
            var memberList = _context.Member
                .Join(
            _context.MembershipCategories,
            Member => Member.MembershipCategoryNumber,
            MembershipCategory => MembershipCategory.MembershipCategoryNumber,
            (Member, MembershipCategory) => new
            {
                Member,
                MembershipCategory,
            }
            );

            var q =
                from m in _context.Member
                join L in _context.Loan on m.MembershipNumber equals L.MemberNumber into ps
                from p in ps.DefaultIfEmpty()
                select new { Member = m, Count = p == null ? "False" : "True"};
            List<MemberListViewModel> memberDVD = new List<MemberListViewModel>();
            var DVDTotal = memberList.GroupBy(a => a.Member.MembershipFirstName).Select(g => new { name = g.Key, count = g.Count() }).ToList();

            foreach (var member in DVDTotal)
            {
                var counter = 0;
                foreach (var item in q)
                {
                    
                    if(member.name == item.Member.MembershipFirstName)
                    {
                        if (item.Count == "True")
                        {
                            counter = counter + 1;
                        }
                        if (item.Count == "False")
                        {
                            
                        }
                    }
                }

                foreach(var mlist in memberList)
                {
                    if (member.name == mlist.Member.MembershipFirstName)
                    {
                        if (counter > mlist.MembershipCategory.MembershipCategoryTotalLoans)
                        {
                            memberDVD.Add(new MemberListViewModel()
                            {
                                MemberName = member.name,
                                TotalDVD = counter,
                                Category = mlist.MembershipCategory.MembershipCategoryDescription,
                                Exceeds = "True",
                            });
                        }
                        if (counter <= mlist.MembershipCategory.MembershipCategoryTotalLoans)
                        {
                            memberDVD.Add(new MemberListViewModel()
                            {
                                MemberName = member.name,
                                Category = mlist.MembershipCategory.MembershipCategoryDescription,
                                TotalDVD = counter,
                                Exceeds = "False",
                            });
                        }
                    }
                }
            }
            return View("MemberList", memberDVD);

        }
        public async Task<IActionResult> MemberDVDLoan()
        {
            var lastLoan = DateTime.Now.AddDays(-31);
            var MemberDVD = _context.Member
                .Join(
                 _context.Loan,
                 Member => Member.MembershipNumber,
                 Loan => Loan.MemberNumber,
                 (Member, Loan) => new
                 {
                     Member,
                     Loan,
                     DateOut = Loan.DateOut,
                 }
                 ).Join(
                 _context.DVDCopy,
                 DVDCopy => DVDCopy.Loan.CopyNumber,
                 Loan => Loan.CopyNumber,
                 (DVDCopy, Loan) => new
                 {
                     DVDCopy,
                     Loan,
                 }
                 ).Join(
                 _context.DVDTitle,
                 DVDCopy => DVDCopy.Loan.DVDNumber,
                 DVDTitle => DVDTitle.DVDNumber,
                 (DVDCopy, DVDTitle) => new
                 {
                     DVDCopy,
                     DVDTitle,
                 }
                 )
                .Where(a => a.DVDCopy.DVDCopy.DateOut < lastLoan).OrderBy(c => c.DVDCopy.DVDCopy.DateOut)
                 .GroupBy(e => e.DVDCopy.DVDCopy.Member.MembershipNumber)
            .Select(e => e.First());

            List<MemberDVDLoan> memberDVDList = new List<MemberDVDLoan>();

            foreach (var dvd in MemberDVD)
            {
                var date = DateTime.Now.Day;
                var NoOfDays =  dvd.DVDCopy.DVDCopy.DateOut.Day - date;
                memberDVDList.Add(new MemberDVDLoan()
                {
                    FirstName = dvd.DVDCopy.DVDCopy.Member.MembershipFirstName,
                    LastName = dvd.DVDCopy.DVDCopy.Member.MembershipLastName,
                    Address = dvd.DVDCopy.DVDCopy.Member.MembershipAddress,
                    DateOut = dvd.DVDCopy.DVDCopy.DateOut,
                    DVDTitle = dvd.DVDTitle.DVDName,
                    NoOfDays = NoOfDays,
                }); ;
            }

            return View(memberDVDList);
        }
    }
}
