using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RopeyDVD.Data;
using RopeyDVD.Models;
using RopeyDVD.Models.ViewModels;

namespace RopeyDVD.Controllers
{
    public class DVDController : Controller
    {
        private readonly ApplicationDBContext _context;
        public DVDController(ApplicationDBContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

       /* public async Task<IActionResult> DVDDetails()
        {
            var @DVDDetails = _context.Member.Where(a => a.MembershipNumber == id)
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
                    DateOut = DVDCopy.DVDCopy.DateOut,
                    DVDTitle,
                }
                )
            .Where(a => a.DateOut >= localDate).ToList();
        }*/

        public async Task<IActionResult> DVDCopies()
        {
            return View(await _context.DVDCopy.ToListAsync());
        }
        public async Task<IActionResult> DVDCopyLoan(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @DVDLoanDetails = _context.DVDCopy.Where(a => a.CopyNumber == id)
                .Join(
            _context.Loan,
            DVDCopy => DVDCopy.CopyNumber,
            Loan => Loan.CopyNumber,
            (DVDCopy, Loan) => new
            {
                DVDCopy,Loan,
            }
            ).Join(
                _context.Member,
                DVDCopy => DVDCopy.Loan.MemberNumber,
                Member => Member.MembershipNumber,
                (DVDCopy, Member) => new
                {
                    DVDCopy,
                    Member,
                }
                ).Join(
                _context.DVDTitle,
                DVDCopy => DVDCopy.DVDCopy.DVDCopy.DVDNumber,
                DVDTitle => DVDTitle.DVDNumber,
                (DVDCopy, DVDTitle) => new
                {
                    DVDCopy,
                    DVDTitle,
                    DateOut = DVDCopy.DVDCopy.Loan.DateOut,
                }
                ).OrderByDescending(a => a.DateOut)
                .FirstOrDefault();

            if(DVDLoanDetails == null)
            {
                return NotFound();
            }

            DVDLoanDetails dvdLoanDetails = new DVDLoanDetails()
            {
                DVDTitle = DVDLoanDetails.DVDTitle.DVDName,
                DateOut = DVDLoanDetails.DateOut,
                DateDue = DVDLoanDetails.DVDCopy.DVDCopy.Loan.DateDue,
                DateReturned = DVDLoanDetails.DVDCopy.DVDCopy.Loan.DateReturned,
            };

            return View("DVDLoanDetails",dvdLoanDetails);
        }

        public async Task<IActionResult> AddDVDLoan()
        {

            var DVDCopy = await _context.DVDCopy.ToListAsync();
            var LoanType = await _context.LoanTypes.ToListAsync();
            var Member = await _context.Member.ToListAsync();

            var DVDCopyList = new List<SelectListItem>();
            DVDCopyList = DVDCopy.Select(a => new SelectListItem()
            {
                Value = a.CopyNumber.ToString(),
                Text=a.CopyNumber.ToString()
            }).ToList();

            var defItem = new SelectListItem()
            {
                Value = "",
                Text = "select"
            };
            DVDCopyList.Insert(0, defItem);


            var LoanTypeList = new List<SelectListItem>();
            LoanTypeList = LoanType.Select(a => new SelectListItem()
            {
                Value = a.LoanTypeNumber.ToString(),
                Text = a.LoanDuration.ToString()
            }).ToList();
            LoanTypeList.Insert(0, defItem);
    
            var MemberList = new List<SelectListItem>();
            MemberList = Member.Select(a => new SelectListItem()
            {
                Value = a.MembershipNumber.ToString(),
                Text = a.MembershipLastName.ToString()
            }).ToList();
            MemberList.Insert(0, defItem);

            ViewData["DVDCopy"] = DVDCopyList;
            ViewData["LoanTypeList"] = LoanTypeList;
            ViewData["MemberList"] = MemberList;


            return View("AddDVDLoan");
        }

        public async Task<IActionResult> Create(AddDVDLoan addDVDLoan)
        {

            //-------------------------------------

            var DVDCopy = await _context.DVDCopy.ToListAsync();
            var LoanType = await _context.LoanTypes.ToListAsync();
            var Member = await _context.Member.ToListAsync();

            var DVDCopyList = new List<SelectListItem>();
            DVDCopyList = DVDCopy.Select(a => new SelectListItem()
            {
                Value = a.CopyNumber.ToString(),
                Text = a.CopyNumber.ToString()
            }).ToList();

            var defItem = new SelectListItem()
            {
                Value = "",
                Text = "select"
            };
            DVDCopyList.Insert(0, defItem);


            var LoanTypeList = new List<SelectListItem>();
            LoanTypeList = LoanType.Select(a => new SelectListItem()
            {
                Value = a.LoanTypeNumber.ToString(),
                Text = a.LoanDuration.ToString()
            }).ToList();
            LoanTypeList.Insert(0, defItem);

            var MemberList = new List<SelectListItem>();
            MemberList = Member.Select(a => new SelectListItem()
            {
                Value = a.MembershipNumber.ToString(),
                Text = a.MembershipLastName.ToString()
            }).ToList();
            MemberList.Insert(0, defItem);

            ViewData["DVDCopy"] = DVDCopyList;
            ViewData["LoanTypeList"] = LoanTypeList;
            ViewData["MemberList"] = MemberList;



            //--------------------------------------

            var memberLoanType = _context.Loan.Where(a => a.MemberNumber == addDVDLoan.MemberNumber);
            var loanCount = memberLoanType.Count();

            var DVDDetails = _context.DVDCopy.Where(a => a.CopyNumber == addDVDLoan.CopyNumber)
                            .Join(
                        _context.DVDTitle,
                        DVDCopy => DVDCopy.DVDNumber,
                        DVDTitle => DVDTitle.DVDNumber,
                        (DVDCopy, DVDTitle) => new
                        {
                            DVDTitle,
                            DVDCopy,
                        }
                        ).FirstOrDefault();

            var price = DVDDetails.DVDTitle.StandardCharge;
            var memberDetails = _context.Member.Where(a => a.MembershipNumber == addDVDLoan.MemberNumber)
                .Join(
            _context.MembershipCategories,
            Member => Member.MembershipCategoryNumber,
            MembershipCategories => MembershipCategories.MembershipCategoryNumber,
            (Member, MembershipCategories) => new
            {
                Member,
                MembershipCategories,
            }
            )
                .FirstOrDefault();

            DateTime currentDate = DateTime.Now;
            var loanType = await _context.LoanTypes
                .FirstOrDefaultAsync(m => m.LoanTypeNumber == addDVDLoan.LoanType);

            var loanDuration = loanType.LoanDuration;
            var totalPrice = loanDuration * price;
            DateTime DateDue = DateTime.Now.AddDays(loanType.LoanDuration);

            ViewData["TotalPrice"] = totalPrice;


            if (addDVDLoan.Age > 18)
            {
                if (memberDetails.MembershipCategories.MembershipCategoryTotalLoans > loanCount)
                {
                    Loan ad = new Loan()
                    {
                        MemberNumber = addDVDLoan.MemberNumber,
                        LoanTypeNumber = addDVDLoan.LoanType,
                        CopyNumber = addDVDLoan.CopyNumber,
                        DateOut = currentDate,
                        DateDue = DateDue,
                    };

                        _context.Add(ad);
                        await _context.SaveChangesAsync();
                    //return RedirectToAction("AddDVDLoan");
                    return View("AddDVDLoan");

                }



            }
            else if(addDVDLoan.Age < 18)
            {
                var DVD = _context.DVDCopy.Where(a => a.CopyNumber == addDVDLoan.CopyNumber)
                .Join(
            _context.DVDTitle,
            DVDCopy => DVDCopy.DVDNumber,
            DVDTitle => DVDTitle.DVDNumber,
            (DVDCopy, DVDTitle) => new
            {
                DVDTitle,
                DVDCopy,
            }
            ).Join(
                _context.DVDCategory,
                DVDTitle => DVDTitle.DVDTitle.CategoryNumber,
                DVDCategory => DVDCategory.CategoryNumber,
                (DVDCopy, DVDCategory) => new
                {
                    DVDCopy,
                    DVDCategory,
                }
                )
            .FirstOrDefault();

                if (DVD.DVDCategory.AgeRestricted == "False")
                {
                    if(memberDetails.MembershipCategories.MembershipCategoryTotalLoans > loanCount )
                    {
                        Loan ad = new Loan()
                        {
                            MemberNumber = addDVDLoan.MemberNumber,
                            LoanTypeNumber = addDVDLoan.LoanType,
                            CopyNumber = addDVDLoan.CopyNumber,
                            DateOut = currentDate,
                            DateDue = DateDue,
                        };

                        _context.Add(ad);
                        await _context.SaveChangesAsync();
                        //return RedirectToAction("AddDVDLoan");
                        return View("AddDVDLoan");
                    }
                }

            }   
            return NotFound();
        }

        public async Task<IActionResult> ReturnDVD()
        {
            var DVD= GetLoanDVDList();
            ViewData["DVDList"] = DVD;
            return View("ReturnDVD");
        }

        public async Task<IActionResult> ReturnAdd(AddDVDLoan addLoan)
        {

            var copyNumber = addLoan.CopyNumber;
            var Loan = await _context.Loan.FirstOrDefaultAsync(x => x.CopyNumber == copyNumber && x.DateReturned == null);
            DateTime DateReturned = DateTime.Now;
            Loan.DateReturned = DateReturned;
            _context.Update(Loan);
            await _context.SaveChangesAsync();

            if(DateReturned > Loan.DateDue )
            {

                var DVD = await _context.DVDCopy.Where(a => a.CopyNumber == Loan.CopyNumber)
                        .Join(
                _context.DVDTitle,
                DVDCopy => DVDCopy.DVDNumber,
                DVDTitle => DVDTitle.DVDNumber,
                (DVDCopy, DVDTitle) => new
                {
                    Penalty= DVDTitle.PenaltyCharge,
                }).FirstOrDefaultAsync();

                var penaltyCharge = DVD.Penalty;
                var PenaltyDuration = DateReturned.Subtract(Loan.DateDue).TotalDays;
                var TotalPenalty = penaltyCharge * PenaltyDuration;

                var DVDLoanList = GetLoanDVDList();
                ViewData["DVDList"] = DVDLoanList;
                ViewData["TotalPenalty"] = TotalPenalty;

                return View("ReturnDVD");
            }

            return RedirectToAction("ReturnDVD");
        }

        public List<SelectListItem> GetLoanDVDList()
        {
            var DVD = _context.Loan.Where(x => x.DateReturned == null);
            var DVDList = new List<SelectListItem>();
            DVDList = DVD.Select(a => new SelectListItem()
            {
                Value = a.CopyNumber.ToString(),
                Text = a.CopyNumber.ToString()
            }).ToList();

            var defItem = new SelectListItem()
            {
                Value = "",
                Text = "select"
            };
            DVDList.Insert(0, defItem);

            return DVDList;
        }

    }
}
