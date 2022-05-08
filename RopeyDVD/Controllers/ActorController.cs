using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RopeyDVD.Data;
using RopeyDVD.Models;
using RopeyDVD.Models.ViewModels;
using System.Linq;

namespace RopeyDVD.Controllers
{
    public class ActorController : Controller
    {
        private readonly ApplicationDBContext _context;

        public ActorController(ApplicationDBContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }


        //Method to view all actors present in the database
        public async Task<IActionResult> ViewActors()
        {
            return View(await _context.Actors.ToListAsync());   //returns actor list to View
        }


        // Method to display actor details to the user
        public async Task<IActionResult> Details(int? id)
        {
            //checking if id is null
            if (id == null)
            {
                return NotFound();
            }

            var actor= await _context.Actors.FindAsync(id); //finding the actor using the id passed into the controller


            //Joining Castmember table with DVDTitle 
            var @actorDetails =  _context.CastMember.Where(a =>a.ActorNumber == id)
            .Join(
            _context.DVDTitle,
            CastMember => CastMember.DVDNumber,
            DVDTitle => DVDTitle.DVDNumber,
            (CastMember, DVDTitle) => new
            {
                DVDName = DVDTitle.DVDName,
                ActorNumber = CastMember.ActorNumber,
            }
            ).ToList();
            
            //if actor is not found it redirects to not found
            if (@actorDetails == null)
            {
                return NotFound();
            }

            var DVDName = actorDetails.Select(x => x.DVDName);          //Inserting all DVDName to variable
            var ActorNumber = actorDetails.Select(x => x.ActorNumber);  //Inserting all ActorNumber to variable

            //Storing data into ActorDetailsViewModel object
            ActorDetailsViewModel actorDetailsModel = new ActorDetailsViewModel()   
            {
                DVDName = DVDName,
                ActorNumber = ActorNumber,
                ActorName= actor.ActorFirstName + " " + actor.ActorSurname,
            };
            
            //returning actor details to view
            return View("ActorDetails", actorDetailsModel);
        }

        //Method to display DVDCopies of an actor
        public async Task<IActionResult> DVDCopy(int? id)
        {
            //Checking if id is null
            if (id == null)
            {
                return NotFound();
            }

            //Joining CastMember Table to DVDTitle, DVDCopy and Loan where Date returned is null
            var dvdDetails = _context.CastMember.Where(a => a.ActorNumber == id)
                .Join(
            _context.DVDTitle,                          //Joining DVDTitle
            CastMember => CastMember.DVDNumber,
            DVDTitle => DVDTitle.DVDNumber,
            (CastMember, DVDTitle) => new
            {
                CastMember,
                DVDTitle
            }
            ).Join(
            _context.DVDCopy,                           //Joining DVDCopy
            DVDTitle => DVDTitle.DVDTitle.DVDNumber,
            DVDCopy => DVDCopy.DVDNumber,
            (DVDTitle, DVDCopy) => new
            {
                DVDTitle,
                DVDCopy
            }
            )
            .Join(
            _context.Loan,                              //Joining Loan
            DVDCopy => DVDCopy.DVDCopy.CopyNumber,
            Loan => Loan.CopyNumber,
            (DVDCopy, Loan) => new
            {
                DVDDate = Loan.DateReturned,
                DVDName = DVDCopy.DVDTitle.DVDTitle.DVDName,
                DVDCopy = DVDCopy.DVDCopy.CopyNumber,
            }
            )
            .Where(a => a.DVDDate != null).ToList();        //Checking if the DVDCopy is in loan or not

            var DVDTotal = dvdDetails.GroupBy(a => a.DVDName).Select(g => new { name = g.Key, count = g.Count() }).ToList();

            //Creating DVDCopiesTotalViewModel object to pass data into view
            List<DVDCopiesTotalViewModel> copy = new List<DVDCopiesTotalViewModel>();
            foreach (var DVD in DVDTotal)
            {
                copy.Add(new DVDCopiesTotalViewModel()
                {
                    DVDName = DVD.name,
                    Total = DVD.count,
                });
            }


            //Returning copy into DVDCopy
            return View("DVDCopy", copy);

        }

    }
}
