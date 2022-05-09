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

        //Method to view the details of DVD
        public async Task<IActionResult> DVDDetails()
        {
            //Joining Multiple tables to Show to user
            var DVDDetails = _context.DVDTitle
                .Join(                            //Joining Producer table
            _context.Producers,
            Producers => Producers.ProducerNumber,
            DVDTitle => DVDTitle.ProducerNumber,
            (DVDTitle, Producers) => new
            {
                Producers,
                DVDTitle,
            }
            ).Join(                            //Joining Studio table
            _context.Studios,       
            DVDTitle => DVDTitle.DVDTitle.StudioNumber,
            Studios => Studios.StudioNumber,
            (DVDTitle, Studio) => new
            {
                Studio,
                DVDTitle,
            }
            ).OrderBy(a => a.DVDTitle.DVDTitle.DateReleased);   // Showing data ordered by the date released of the dvd

            //Creating DVDDetailsViewModel to pass data into view
            List<DVDDetailsViewModel> DVDDetailsList = new List<DVDDetailsViewModel>();

            //Inserting data into DVDDetailsViewModel object
            foreach (var dvd in DVDDetails)
            {
                DVDDetailsList.Add(new DVDDetailsViewModel()
                {
                    DVDName = dvd.DVDTitle.DVDTitle.DVDName,
                    DVDNumber = dvd.DVDTitle.DVDTitle.DVDNumber,
                    studio = dvd.DVDTitle.DVDTitle.Studio.StudioName,
                    producer = dvd.DVDTitle.DVDTitle.Producer.ProducerName,
                    DateReleased= dvd.DVDTitle.DVDTitle.DateReleased.ToString("dd/MM/yyyy"),
                }); 
            }

            //returning DVD details to DVDDetails view
            return View("DVDDetails", DVDDetailsList);

        }

        //Method to view Cast members of a DVD
        public async Task<IActionResult> DVDCastMembers(int id)
        {
            //Joining CastMember table with Actor table
            var DVDCastMember = _context.CastMember.Where(a => a.DVDNumber == id)
            .Join(          
            _context.Actors,        //Joining actor table
            Actor => Actor.ActorNumber,
            CastMember => CastMember.ActorNumber,
            (Actor, CastMember) => new
            {
                CastMember,
                Actor,
            }
            ).OrderBy(a => a.Actor.Actor.ActorSurname);     // Ordering data by Actor surname

            //Creating Actor object 
            List<Actor> dvdActor = new List<Actor>();
            foreach (var dvd in DVDCastMember)
            {
                dvdActor.Add(new Actor()                    //Adding data to Actor object
                {
                    ActorFirstName = dvd.Actor.Actor.ActorFirstName,
                    ActorSurname = dvd.Actor.Actor.ActorSurname,
                    ActorNumber = dvd.Actor.ActorNumber,
                });
            }
            //Returning Actor details to DVDDetails view
            return View(dvdActor);
        }

        //Method to view all DVD copies present in the database
        public async Task<IActionResult> DVDCopies()
        {
            return View(await _context.DVDCopy.ToListAsync());  //returning DVDCopy to DVDCopies view
        }

        //Method to Show DVD Copy loan details
        public async Task<IActionResult> DVDCopyLoan(int? id)
        {
            //Checking if id is null
            if (id == null)
            {
                return NotFound();
            }

            //Joining multiple tables to DVDCopy table
            var @DVDLoanDetails = _context.DVDCopy.Where(a => a.CopyNumber == id)
            .Join(                          //Joining loan
            _context.Loan,
            DVDCopy => DVDCopy.CopyNumber,
            Loan => Loan.CopyNumber,
            (DVDCopy, Loan) => new
            {
                DVDCopy,
                Loan,
            }
            )
            .Join(                          //Joining Member
            _context.Member,
            DVDCopy => DVDCopy.Loan.MemberNumber,
            Member => Member.MembershipNumber,
            (DVDCopy, Member) => new
            {
                DVDCopy,
                Member,
            }
            ).Join(                         //Joining DVDTitle
            _context.DVDTitle,
            DVDCopy => DVDCopy.DVDCopy.DVDCopy.DVDNumber,
            DVDTitle => DVDTitle.DVDNumber,
            (DVDCopy, DVDTitle) => new
            {
                DVDCopy,
                DVDTitle,
                DateOut = DVDCopy.DVDCopy.Loan.DateOut,
            }
            ).OrderByDescending(a => a.DateOut)     //Order by Date out of the DVD copy in descending order
            .FirstOrDefault();
            
            //Checking if DVD details is null
            if (DVDLoanDetails == null)     
            {
                return NotFound();
            }

            //Creating DVDLoanDetails object
            DVDLoanDetails dvdLoanDetails = new DVDLoanDetails()
            {
                DVDTitle = DVDLoanDetails.DVDTitle.DVDName,
                DateOut = DVDLoanDetails.DateOut,
                DateDue = DVDLoanDetails.DVDCopy.DVDCopy.Loan.DateDue,
                DateReturned = DVDLoanDetails.DVDCopy.DVDCopy.Loan.DateReturned,
            };

            //Returning DVD loan details to view
            return View("DVDLoanDetails", dvdLoanDetails);
        }

        //Method to show Add dvd loan view to user along with the dropdown lists
        public async Task<IActionResult> AddDVDLoan()
        {

            var DVDCopyList = GetDVDCopyNotLoaned();    //Getting DVD copy list from GetDVDCopyNotLoaned() method
            var LoanTypeList = GetLoanType();           //Getting Loan type list from GetLoanType() method
            var MemberList = GetMember();               //Getting Member list from GetMember() method

            //Viewdata to pass data to View
            ViewData["DVDCopy"] = DVDCopyList;
            ViewData["LoanTypeList"] = LoanTypeList;
            ViewData["MemberList"] = MemberList;

            return View("AddDVDLoan");
        }

        //Method to add new loan to the database
        public async Task<IActionResult> Create(AddDVDLoan addDVDLoan)
        {

            var DVDCopyList = GetDVDCopyNotLoaned();    //Getting DVD copy list from GetDVDCopyNotLoaned() method
            var LoanTypeList = GetLoanType();           //Getting Loan type list from GetLoanType() method
            var MemberList = GetMember();               //Getting Member list from GetMember() method

            //Using ViewData to pass data
            ViewData["DVDCopy"] = DVDCopyList;
            ViewData["LoanTypeList"] = LoanTypeList;
            ViewData["MemberList"] = MemberList;

            var memberLoanType = _context.Loan.Where(a => a.MemberNumber == addDVDLoan.MemberNumber);   //Getting loan where member number is equal to user input
            var loanCount = memberLoanType.Count();         

            //Joinig DVDCopy with DVDTitle where copy number is equal to user selected copy number
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

            //Getting price from DVDDetails
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

            DateTime currentDate = DateTime.Now;    //Current Date and time
            var loanType = await _context.LoanTypes
                .FirstOrDefaultAsync(m => m.LoanTypeNumber == addDVDLoan.LoanType);
            var loanDuration = loanType.LoanDuration;
            var totalPrice = loanDuration * price;
            DateTime DateDue = DateTime.Now.AddDays(loanType.LoanDuration);

            ViewData["TotalPrice"] = totalPrice;

            //Checking if user age is greater than 18
            if (addDVDLoan.Age > 18)
            {
                //Checking the number of loans for a user according to their membership category
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
                    await _context.SaveChangesAsync();                      //Adding changes to database
                    //return RedirectToAction("AddDVDLoan");
                    return View("AddDVDLoan");

                }
                else
                {
                    ViewData["ErrorMessage"] = "Loan Capacity reached";     //Passing error message to View
                    return View("AddDVDLoan");
                }
            }
            else if (addDVDLoan.Age < 18)                                   //Condition for User age less than 18
            {
                //Joining DVDCopy to multiple tables
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
                        )
                    .Join(
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

                //Cheking if the dvd is age restrected
                if (DVD.DVDCategory.AgeRestricted == "False")
                {
                    //Checking the number of loans for a user according to their membership category
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
                        ViewData["ErrorMessage"] = "Loan Capacity reached";     //Passing error message to view
                        return View("AddDVDLoan");
                    }
                }
                else
                {
                    ViewData["ErrorMessage"] = "DVD is adult rated";            //Passing error message to view
                    return View("AddDVDLoan");
                }
            }
            return NotFound();
        }

        //Method to Display ReturnDVD view with LoanDVDList
        public async Task<IActionResult> ReturnDVD()
        {
            //Gets DVD list
            var DVD = GetLoanDVDList();     //Getting DVD that are currently on loan
            ViewData["DVDList"] = DVD;      //Passing data to View
            return View("ReturnDVD");
        }

        //Update loan database after user returns DVD
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

        //Returns list of DVD that have not been loaned
        public List<SelectListItem> GetDVDCopyNotLoaned()
        {
            var DVDCopy = _context.DVDCopy.ToList();        //Getting DVDCopy from database
            var loan = _context.Loan.Where(a => a.DateReturned == null);    //Getting loan where date returned is null
            var DVDCopyList = new List<SelectListItem>();       
            var a = _context.DVDCopy.ToList();

            //Checking and removing DVDCopy that are currently on loan
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
            //Grouping the DVDCopy 
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

        //Returns List of Loan types 
        public List<SelectListItem> GetLoanType()
        {
            //SelectListItem for default select option
            var defItem = new SelectListItem()
            {
                Value = "",
                Text = "select"
            };
            
            //Getting Loan Type
            var LoanType = _context.LoanTypes.ToList();

            //Converting into SelectListItem
            var LoanTypeList = new List<SelectListItem>();
            LoanTypeList = LoanType.Select(a => new SelectListItem()
            {
                Value = a.LoanTypeNumber.ToString(),
                Text = a.LoanDuration.ToString()
            }).ToList();
            LoanTypeList.Insert(0, defItem);
            return LoanTypeList;
        }

        //Returns Member list
        public List<SelectListItem> GetMember()
        {
            var defItem = new SelectListItem()
            {
                Value = "",
                Text = "select"
            };

            //Getting member list from database
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

        //Returns required lists and add dvd view page
        public async Task<IActionResult> AddDVDTitle()
        {
            //Getting all the data from different methods
            var actor = GetActorList();
            var studio = GetStudioList();
            var producer = GetProducerList();
            var dvdCategory = GetCategoryList();

            //Passing data to View
            ViewData["Actor"] = actor;
            ViewData["Studio"] = studio;
            ViewData["Producer"] = producer;
            ViewData["Category"] = dvdCategory;

            //Returnig view
            return View("AddDVDTitle");
        }

        //Returns Loan DVD list
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

        //Returns Producer list
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

        //Returns Studio list
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

        //Returns Actor list
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

        //Returns Category list
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

        //Method for adding new dvd title to database
        public async Task<IActionResult> ADDDVD(AddDVDTitle AddDVDTitle)
        {
            var cate = _context.DVDCategory.ToList();
            //Creating new DVDTitle object
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
            await _context.SaveChangesAsync();  //Saving to database

            var DVDNumber = _context.DVDTitle
            .OrderByDescending(a => a.DVDNumber)
            .First();
            var actorNumber = AddDVDTitle.ActorNumber;

            //New CastMember object
            CastMember castMember = new CastMember()
            {
                ActorNumber = actorNumber,
                DVDNumber = DVDNumber.DVDNumber,
            };
            _context.Add(castMember);
            await _context.SaveChangesAsync();  //Saving to database

            return View("ADDDVDTitle");
        }

        //Displays DVD copy lists purchased Before 365 days
        public async Task<IActionResult> DVDCopyList()
        {
            DateTime dateYearAgo = DateTime.Now.AddDays(-365);      //Getting date time from 365 days ago

            //Joinig DVDCopy with DvdTitle where date purchased is a year ago
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

            //New DVDCopyViewModel object
            List<DVDCopyViewModel> dvdCopyViewModel = new List<DVDCopyViewModel>();
            foreach (var dvd in dvdCopyList)
            {
                dvdCopyViewModel.Add(new DVDCopyViewModel()
                {
                    DVDNumber = dvd.DVDTitle.DVDNumber,
                    DVDTitle = dvd.DVDTitle.DVDName,
                    CopyNumber = dvd.DVDCopy.CopyNumber,
                    DatePurchased = dvd.DVDCopy.DatePurchased.ToString("dd/MM/yyyy"),
                });
            }
            //Returning dvdCopyViewModel to OldDVDList
            return View("OldDVDList", dvdCopyViewModel);
        }

        //Finding and deleting DVDCopy 
        public async Task<IActionResult> DVDCopyDelete(int? id)
        {
            //Checking if id is null
            if (id == null)
            {
                return NotFound();
            }

            //Finding the dvdCopy
            var dvd = await _context.DVDCopy
                .FirstOrDefaultAsync(m => m.CopyNumber == id);
            if (dvd == null)
            {
                return NotFound();
            }   
            _context.DVDCopy.Remove(dvd);           //Removing DVDCopy from database
            await _context.SaveChangesAsync();      //Saving changes

            return RedirectToAction("DVDCopyList");

        }

        //Method to display loan lists
        public async Task<IActionResult> DVDLoanList()
        {

            //Joining Loan table with DVDCopy and DVDTitle
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
            ).OrderBy(a => a.DVDCopy.DVDCopy.Member.DateOut);       //Order by Date out of DVD loan

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

        //Method to display Total DVD loaned by date
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

        //Redirects user to DVDLoan page
        public async Task<IActionResult> DVDLoan()
        {
            return View("DVDLoan");
        }

        //Showing previous DVD copy loan
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
                }); 
            }


            return View("DVDCopyPreviousLoan", DVDCopyLoanList);
        }
    }
}
