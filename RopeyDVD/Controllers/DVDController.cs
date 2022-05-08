using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RopeyDVD.Data;
using RopeyDVD.Models;
using RopeyDVD.Models.ViewModels;
using System.Linq;

namespace RopeyDVD.Controllers
{
    [Authorize(Roles = "Admin, User")]
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

        public async Task<IActionResult> DVDDetails()
        {
            var DVDDetails = _context.DVDTitle
                .Join(
            _context.Producers,
            Producers => Producers.ProducerNumber,
            DVDTitle => DVDTitle.ProducerNumber,
            (DVDTitle, Producers) => new
            {
                Producers,
                DVDTitle,
            }
            ).Join(
            _context.Studios,
            DVDTitle => DVDTitle.DVDTitle.StudioNumber,
            Studios => Studios.StudioNumber,
            (DVDTitle, Studio) => new
            {
                Studio,
                DVDTitle,
            }
            ).OrderBy(a => a.DVDTitle.DVDTitle.DateReleased);
            //.Join(
            //_context.CastMember,
            //DVDTitle => DVDTitle.DVDTitle.DVDTitle.DVDNumber,
            //CastMember => CastMember.DVDNumber,
            //(DVDTitle, CastMember) => new
            //{
            //    CastMember,
            //    DVDTitle,
            //}
            //);

            List<DVDDetailsViewModel> DVDDetailsList = new List<DVDDetailsViewModel>();

            foreach (var dvd in DVDDetails)
            {
                DVDDetailsList.Add(new DVDDetailsViewModel()
                {
                    DVDName = dvd.DVDTitle.DVDTitle.DVDName,
                    DVDNumber = dvd.DVDTitle.DVDTitle.DVDNumber,
                    studio = dvd.DVDTitle.DVDTitle.Studio.StudioName,
                    producer = dvd.DVDTitle.DVDTitle.Producer.ProducerName,
                }); ;
            }

            return View("DVDDetails", DVDDetailsList);

        }

        public async Task<IActionResult> DVDCastMembers(int id)
        {
            //var DVDCastMember = _context.DVDTitle.Where(a => a.DVDNumber == id)
            //    .Join(
            //_context.CastMember,
            //DVDTitle => DVDTitle.DVDNumber,
            //CastMember => CastMember.DVDNumber,
            //(DVDTitle, CastMember) => new
            //{
            //    CastMember,
            //    DVDTitle,
            //}
            //);

            var DVDCastMember = _context.CastMember.Where(a => a.DVDNumber == id)
                .Join(
            _context.Actors,
            Actor => Actor.ActorNumber,
            CastMember => CastMember.ActorNumber,
            (Actor, CastMember) => new
            {
                CastMember,
                Actor,
            }
            ).OrderBy(a => a.Actor.Actor.ActorSurname);

            List<Actor> dvdActor = new List<Actor>();

            foreach (var dvd in DVDCastMember)
            {
                dvdActor.Add(new Actor()
                {
                    ActorFirstName = dvd.Actor.Actor.ActorFirstName,
                    ActorSurname = dvd.Actor.Actor.ActorSurname,
                    ActorNumber = dvd.Actor.ActorNumber,
                });
            }



            return View(dvdActor);
        }

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
                DVDCopy,
                Loan,
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

            if (DVDLoanDetails == null)
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

            return View("DVDLoanDetails", dvdLoanDetails);
        }

        public async Task<IActionResult> AddDVDLoan()
        {
            var DVDCopyList = GetDVDCopyNotLoaned();
            var LoanTypeList = GetLoanType();
            var MemberList = GetMember();

            ViewData["DVDCopy"] = DVDCopyList;
            ViewData["LoanTypeList"] = LoanTypeList;
            ViewData["MemberList"] = MemberList;


            return View("AddDVDLoan");
        }

        public async Task<IActionResult> Create(AddDVDLoan addDVDLoan)
        {

            var DVDCopyList = GetDVDCopyNotLoaned();
            var LoanTypeList = GetLoanType();
            var MemberList = GetMember();


            ViewData["DVDCopy"] = DVDCopyList;
            ViewData["LoanTypeList"] = LoanTypeList;
            ViewData["MemberList"] = MemberList;

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
                else
                {
                    ViewData["ErrorMessage"] = "Loan Capacity reached";
                    return View("AddDVDLoan");
                }
            }
            else if (addDVDLoan.Age < 18)
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
                    else
                    {
                        ViewData["ErrorMessage"] = "Loan Capacity reached";
                        return View("AddDVDLoan");
                    }
                }
                else
                {
                    ViewData["ErrorMessage"] = "DVD is adult rated";
                    return View("AddDVDLoan");
                }
            }
            return NotFound();
        }

        public async Task<IActionResult> ReturnDVD()
        {
            var DVD = GetLoanDVDList();
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

            if (DateReturned > Loan.DateDue)
            {

                var DVD = await _context.DVDCopy.Where(a => a.CopyNumber == Loan.CopyNumber)
                        .Join(
                _context.DVDTitle,
                DVDCopy => DVDCopy.DVDNumber,
                DVDTitle => DVDTitle.DVDNumber,
                (DVDCopy, DVDTitle) => new
                {
                    Penalty = DVDTitle.PenaltyCharge,
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

        public List<SelectListItem> GetDVDCopyNotLoaned()
        {
            var DVDCopy = _context.DVDCopy.ToList();
            var loan = _context.Loan.Where(a => a.DateReturned == null);
            var DVDCopyList = new List<SelectListItem>();
            var a = _context.DVDCopy.ToList();
            foreach (var dvd in a.ToList())
            {
                foreach (var l in loan.ToList())
                {
                    if (l.CopyNumber == dvd.CopyNumber)
                    {
                        var item = DVDCopy.Find(x => x.CopyNumber == dvd.CopyNumber);
                        DVDCopy.Remove(item);
                        break;
                    }
                }
            }
            var group = DVDCopy.GroupBy(a => a.CopyNumber).Select(g => new { name = g.Key, count = g.Count() }).ToList();

            DVDCopyList = group.Select(a => new SelectListItem()
            {
                Value = a.name.ToString(),
                Text = a.name.ToString()
            }).ToList();

            var defItem = new SelectListItem()
            {
                Value = "",
                Text = "select"
            };
            DVDCopyList.Insert(0, defItem);

            return DVDCopyList;
        }
        public List<SelectListItem> GetLoanType()
        {
            var defItem = new SelectListItem()
            {
                Value = "",
                Text = "select"
            };
            var LoanType = _context.LoanTypes.ToList();
            var LoanTypeList = new List<SelectListItem>();
            LoanTypeList = LoanType.Select(a => new SelectListItem()
            {
                Value = a.LoanTypeNumber.ToString(),
                Text = a.LoanDuration.ToString()
            }).ToList();
            LoanTypeList.Insert(0, defItem);
            return LoanTypeList;
        }
        public List<SelectListItem> GetMember()
        {
            var defItem = new SelectListItem()
            {
                Value = "",
                Text = "select"
            };

            var Member = _context.Member.ToList();
            var MemberList = new List<SelectListItem>();
            MemberList = Member.Select(a => new SelectListItem()
            {
                Value = a.MembershipNumber.ToString(),
                Text = a.MembershipLastName.ToString()
            }).ToList();
            MemberList.Insert(0, defItem);
            return MemberList;
        }



        public async Task<IActionResult> AddDVDTitle()
        {
            var actor = GetActorList();
            var studio = GetStudioList();
            var producer = GetProducerList();
            var dvdCategory = GetCategoryList();


            ViewData["Actor"] = actor;
            ViewData["Studio"] = studio;
            ViewData["Producer"] = producer;
            ViewData["Category"] = dvdCategory;

            return View("AddDVDTitle");
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

        public List<SelectListItem> GetProducerList()
        {
            var producer = _context.Producers.ToList();
            var ProducerList = new List<SelectListItem>();
            ProducerList = producer.Select(a => new SelectListItem()
            {
                Value = a.ProducerNumber.ToString(),
                Text = a.ProducerName.ToString()
            }).ToList();

            var defItem = new SelectListItem()
            {
                Value = "",
                Text = "select"
            };
            ProducerList.Insert(0, defItem);

            return ProducerList;
        }

        public List<SelectListItem> GetStudioList()
        {
            var studio = _context.Studios.ToList();
            var studioList = new List<SelectListItem>();
            studioList = studio.Select(a => new SelectListItem()
            {
                Value = a.StudioNumber.ToString(),
                Text = a.StudioName.ToString()
            }).ToList();

            var defItem = new SelectListItem()
            {
                Value = "",
                Text = "select"
            };
            studioList.Insert(0, defItem);

            return studioList;
        }

        public List<SelectListItem> GetActorList()
        {
            var actor = _context.Actors.ToList();
            var actorList = new List<SelectListItem>();
            actorList = actor.Select(a => new SelectListItem()
            {
                Value = a.ActorNumber.ToString(),
                Text = a.ActorFirstName.ToString()
            }).ToList();

            var defItem = new SelectListItem()
            {
                Value = "",
                Text = "select"
            };
            actorList.Insert(0, defItem);

            return actorList;
        }

        public List<SelectListItem> GetCategoryList()
        {
            var DVDCategory = _context.DVDCategory.ToList();
            var categoryList = new List<SelectListItem>();
            categoryList = DVDCategory.Select(a => new SelectListItem()
            {
                Value = a.CategoryNumber.ToString(),
                Text = a.CategotyDescription.ToString()
            }).ToList();

            var defItem = new SelectListItem()
            {
                Value = "",
                Text = "select"
            };
            categoryList.Insert(0, defItem);

            return categoryList;
        }


        public async Task<IActionResult> ADDDVD(AddDVDTitle AddDVDTitle)
        {
            var cate = _context.DVDCategory.ToList();
            DVDTitle dvd = new DVDTitle()
            {
                DVDName = AddDVDTitle.DVDName,
                ProducerNumber = AddDVDTitle.ProducerNumber,
                CategoryNumber = AddDVDTitle.CategoryNumber,
                DVDCategoryCategoryNumber = AddDVDTitle.CategoryNumber,
                StudioNumber = AddDVDTitle.StudioNumber,
                DateReleased = AddDVDTitle.DateReleased,
                StandardCharge = AddDVDTitle.StandardCharge,
                PenaltyCharge = AddDVDTitle.PenaltyCharge,
            };

            _context.Add(dvd);
            await _context.SaveChangesAsync();

            var DVDNumber = _context.DVDTitle
            .OrderByDescending(a => a.DVDNumber)
            .First();
            var actorNumber = AddDVDTitle.ActorNumber;

            CastMember castMember = new CastMember()
            {
                ActorNumber = actorNumber,
                DVDNumber = DVDNumber.DVDNumber,
            };
            _context.Add(castMember);
            await _context.SaveChangesAsync();

            return View("ADDDVDTitle");
        }

        public async Task<IActionResult> DVDCopyList()
        {
            DateTime dateYearAgo = DateTime.Now.AddDays(-365);
            var dvdCopyList = _context.DVDCopy
                .Join(
             _context.DVDTitle,
             DVDCopy => DVDCopy.DVDNumber,
             DVDTitle => DVDTitle.DVDNumber,
             (DVDCopy, DVDTitle) => new
             {
                 DVDCopy,
                 DVDTitle,
                 DatePurchased = DVDCopy.DatePurchased,
             }
             ).Where(a => a.DatePurchased < dateYearAgo)
             .ToList();

            List<DVDCopyViewModel> dvdCopyViewModel = new List<DVDCopyViewModel>();

            foreach (var dvd in dvdCopyList)
            {
                dvdCopyViewModel.Add(new DVDCopyViewModel()
                {
                    DVDNumber = dvd.DVDTitle.DVDNumber,
                    DVDTitle = dvd.DVDTitle.DVDName,
                    CopyNumber = dvd.DVDCopy.CopyNumber,
                    DatePurchased = dvd.DVDCopy.DatePurchased,
                });
            }
            return View("OldDVDList", dvdCopyViewModel);
        }

        public async Task<IActionResult> DVDCopyDelete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var dvd = await _context.DVDCopy
                .FirstOrDefaultAsync(m => m.CopyNumber == id);
            if (dvd == null)
            {
                return NotFound();
            }
            _context.DVDCopy.Remove(dvd);
            await _context.SaveChangesAsync();

            return RedirectToAction("DVDCopyList");

        }

        public async Task<IActionResult> DVDLoanList()
        {
            var LoanList = _context.Loan.Where(x => x.DateReturned == null)
                .Join(
             _context.Member,
             Member => Member.MemberNumber,
             Loan => Loan.MembershipNumber,
             (Member, Loan) => new
             {
                 Member,
                 Loan,
             }
             ).Join(
             _context.DVDCopy,
             DVDCopy => DVDCopy.Member.CopyNumber,
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
             (DVDCopy, Loan) => new
             {
                 DVDCopy,
                 Loan,
             }
        ).OrderBy(a => a.DVDCopy.DVDCopy.Member.DateOut);

            List<DVDLoanViewModel> dvdLoan = new List<DVDLoanViewModel>();

            foreach (var dvd in LoanList)
            {
                dvdLoan.Add(new DVDLoanViewModel()
                {
                    DVDTitle = dvd.Loan.DVDName,
                    CopyNumber = dvd.DVDCopy.DVDCopy.Member.CopyNumber,
                    MemberName = dvd.DVDCopy.DVDCopy.Member.Member.MembershipFirstName,
                });
            }

            var DateOutLoan = _context.Loan.GroupBy(a => a.DateOut.Date).Select(g => new { name = g.Key, count = g.Count() }).ToList();

            List<DVDLoanViewModel> dvdLoanDate = new List<DVDLoanViewModel>();

            foreach (var loan in DateOutLoan)
            {
                dvdLoanDate.Add(new DVDLoanViewModel()
                {
                    DateOut = loan.name,
                    TotalLoans = loan.count.ToString(),
                });
            }

            ViewData["LoanDateCount"] = dvdLoanDate;


            return View("DVDLoanList", dvdLoan);
        }

        public async Task<IActionResult> DVDDateTotal()
        {
            var DateOutLoan = _context.Loan.GroupBy(a => a.DateOut.Date).Select(g => new { name = g.Key, count = g.Count() }).ToList();

            List<DVDLoanViewModel> dvdLoanDate = new List<DVDLoanViewModel>();

            foreach (var loan in DateOutLoan)
            {
                dvdLoanDate.Add(new DVDLoanViewModel()
                {
                    DateOut = loan.name,
                    TotalLoans = loan.count.ToString(),
                });
            }
            return View("LoanDate", dvdLoanDate);

        }

        public async Task<IActionResult> DVDLoan()
        {
            return View("DVDLoan");
        }

        public async Task<IActionResult> DVDCopyPreviousLoan()
        {
            var lastLoanDays = DateTime.Now.AddDays(-31);
            var DVDCopyLoan = _context.DVDCopy
                .Join(
                 _context.Loan,
                 DVDCopy => DVDCopy.CopyNumber,
                 Loan => Loan.CopyNumber,
                 (DVDCopy, Loan) => new
                 {
                     DVDCopy,
                     Loan,
                     DateOut = Loan.DateOut,
                 }
                 ).Join(
                 _context.DVDTitle,
                 DVDCopy => DVDCopy.DVDCopy.DVDNumber,
                 DVDTitle => DVDTitle.DVDNumber,
                 (DVDCopy, DVDTitle) => new
                 {
                     DVDCopy,
                     DVDTitle,
                 }
                 )
                .Where(a => a.DVDCopy.DateOut < lastLoanDays).OrderByDescending(c => c.DVDCopy.DateOut)
                 .GroupBy(e => e.DVDCopy.DVDCopy.CopyNumber)
            .Select(e => e.First());

            List<PreviousDVDCopyViewModel> DVDCopyLoanList = new List<PreviousDVDCopyViewModel>();

            foreach (var dvd in DVDCopyLoan)
            {
                var date = DateTime.Now.Day;
                var NoOfDays = dvd.DVDCopy.DateOut.Day - date;
                DVDCopyLoanList.Add(new PreviousDVDCopyViewModel()
                {
                    DVDName = dvd.DVDTitle.DVDName,
                    CopyNumber = dvd.DVDCopy.DVDCopy.CopyNumber,
                }); ;
            }


            return View("DVDCopyPreviousLoan", DVDCopyLoanList);
        }
    }
}
