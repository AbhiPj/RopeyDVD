using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RopeyDVD.Data;
using RopeyDVD.Models;
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

        //Returns the list of members in the database
        public async Task<IActionResult> index()
        {
            return View(await _context.Member.ToListAsync());
        }

        public async Task<IActionResult> MemberDVD(int? id)
        {
            //Checking if id is null
            if (id == null)
            {
                return NotFound();
            }

            //Getting date time of a month ago
            DateTime localDate = DateTime.Now.AddDays(-31);

            //Finding Member using id
            var member = await _context.Member.FindAsync(id);

            //Joining Member wiht DVDCopy and DVDtitle
            var @memberDetails = _context.Member.Where(a => a.MembershipNumber == id)
            .Join(
                _context.Loan,
                Member => Member.MembershipNumber,
                Loan => Loan.MemberNumber,
                (Member, Loan) => new
                {
                    Member,Loan, DateOut = Loan.DateOut,
                }
            )
            .Join(
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
            .Where(a => a.DateOut >= localDate).ToList();       // Join table where date out of loan is greater than a month ago

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


        // Returns member list along witht their total number of loans and their respective category
        public async Task<IActionResult> MemberList()
        {
            //Joining member with MembershipCategories table
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

            var MemberQuery =
                from m in _context.Member
                join L in _context.Loan on m.MembershipNumber equals L.MemberNumber into ps
                from p in ps.DefaultIfEmpty()
                select new { Member = m, Count = p == null ? "False" : "True"};
            List<MemberListViewModel> memberDVD = new List<MemberListViewModel>();
            var DVDTotal = memberList.GroupBy(a => a.Member.MembershipFirstName).Select(g => new { name = g.Key, count = g.Count() }).ToList();

            //Getting the total count of the DVD loan for member
            foreach (var member in DVDTotal)
            {
                var counter = 0;
                foreach (var item in MemberQuery)
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

        //Displays DVD loaned by user
        public async Task<IActionResult> MemberDVDLoan()
        {
            var lastLoan = DateTime.Now.AddDays(-31);   //Date a month ago

            //Joining Member to multiple tables
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
                .OrderByDescending(c => c.DVDCopy.DVDCopy.DateOut);     //Order by Date out of loan

            //Getting member where dvd is loaned before one monhth
            var group = MemberDVD.GroupBy(a => a.DVDCopy.DVDCopy.Member.MembershipNumber)
                .Select(g => new { name = g.Key, count = g.Count() ,date = g.Max(a => a.DVDCopy.DVDCopy.DateOut), })
                .Where(a => a.date < lastLoan)
                .ToList();



            List<Member> member = new List<Member>();
            foreach(var g in group)
            {
                var a = _context.Member.Find(g.name);
                member.Add(a);
            }

            //Joining members that have loaned dvd before one month
            List<MemberDVDLoan> memberDVDList = new List<MemberDVDLoan>();
            foreach (var m in member)
            {
                //Joining Member table
                var memberJoin = _context.Member.Where(a => a.MembershipNumber == m.MembershipNumber)
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
                    )
                    .Join(
                 _context.DVDCopy,
                 DVDCopy => DVDCopy.Loan.CopyNumber,
                 Loan => Loan.CopyNumber,
                 (DVDCopy, Loan) => new
                 {
                     DVDCopy,
                     Loan,
                 }
                 )
                    .Join(
                 _context.DVDTitle,
                 DVDCopy => DVDCopy.Loan.DVDNumber,
                 DVDTitle => DVDTitle.DVDNumber,
                 (DVDCopy, DVDTitle) => new
                 {
                     DVDCopy,
                     DVDTitle,
                 }
                 ).FirstOrDefault();

                var date = DateTime.Now;        //Current date
                var Days = date - memberJoin.DVDCopy.DVDCopy.DateOut;       //Getting total number of loan days
                var NoOfDays = Days.Days;
                memberDVDList.Add(new MemberDVDLoan()
                {
                    FirstName = memberJoin.DVDCopy.DVDCopy.Member.MembershipFirstName,
                    LastName = memberJoin.DVDCopy.DVDCopy.Member.MembershipLastName,
                    Address = memberJoin.DVDCopy.DVDCopy.Member.MembershipAddress,
                    DateOut = memberJoin.DVDCopy.DVDCopy.DateOut.ToString("dd/MM/yyyy"),
                    DVDTitle = memberJoin.DVDTitle.DVDName,
                    NoOfDays = NoOfDays,
                });
            }
            

            return View(memberDVDList);
        }





    }
}
