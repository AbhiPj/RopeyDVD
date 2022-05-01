using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RopeyDVD.Data;
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

        public async Task<IActionResult> ViewActors()
        {
            return View(await _context.Actors.ToListAsync());
        }

        public async Task<IActionResult> details(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }

            //var @module =  _context.CastMember
            //    .Where(m => m.ActorNumber == id);

            var actor= await _context.Actors.FindAsync(id);

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
            
            if (@actorDetails == null)
            {
                return NotFound();
            }

            var DVDName = actorDetails.Select(x => x.DVDName);
            var ActorNumber = actorDetails.Select(x => x.ActorNumber);

            ActorDetailsViewModel actorDetailsModel = new ActorDetailsViewModel()
            {
                DVDName = DVDName,
                ActorNumber = ActorNumber,
                ActorName= actor.ActorFirstName + " " + actor.ActorSurname,
            };

            return View("ActorDetails", actorDetailsModel);
            //return View(await _context.Actors.ToListAsync());
        }

        public async Task<IActionResult> DVDCopy(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @dvdDetails = _context.CastMember.Where(a => a.ActorNumber == id)
                .Join(
            _context.DVDTitle,
            CastMember => CastMember.DVDNumber,
            DVDTitle => DVDTitle.DVDNumber,
            (CastMember, DVDTitle) => new
            {
                CastMember,
                DVDTitle
            }
            ).Join(
            _context.DVDCopy,
            DVDTitle => DVDTitle.DVDTitle.DVDNumber,
            DVDCopy => DVDCopy.DVDNumber,
            (DVDTitle, DVDCopy) => new
            {
                DVDTitle,
                DVDCopy
            }
            )
            .Join(
            _context.Loan,
            DVDCopy => DVDCopy.DVDCopy.CopyNumber,
            Loan => Loan.CopyNumber,
            (DVDCopy, Loan) => new
            {
                DVDDate = Loan.DateReturned,
                DVDName = DVDCopy.DVDTitle.DVDTitle.DVDName,
                DVDCopy = DVDCopy.DVDCopy.CopyNumber,
            }
            )
            .Where(a => a.DVDDate != null).ToList();

            var DVDTotal = dvdDetails.GroupBy(a => a.DVDName).Select(g => new { name = g.Key, count = g.Count() }).ToList();
            List<DVDCopiesTotalViewModel> copy = new List<DVDCopiesTotalViewModel>();


            foreach (var DVD in DVDTotal)
            {
                copy.Add(new DVDCopiesTotalViewModel()
                {
                    DVDName = DVD.name,
                    Total = DVD.count,
                });
            }
       


            return View("DVDCopy", copy);

        }

    }
}
