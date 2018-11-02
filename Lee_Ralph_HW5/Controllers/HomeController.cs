using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Lee_Ralph_HW5.DAL;
using Lee_Ralph_HW5.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Lee_Ralph_HW5.Controllers
{
    public enum Range{GreaterThan, LessThan}

    public class HomeController : Controller
    {
        private AppDbContext _db;

        public HomeController(AppDbContext context)
        {
            _db = context;
        }

        public ActionResult Index(String SearchString)
        {
            ViewBag.TotalResults = _db.Repositories.Count();

            List<Repository> SelectedRepositories = new List<Repository>();

            var query = from r in _db.Repositories
                        select r;

            //checks to see if its not null
            if (SearchString != null && SearchString != "")
            {
                query = query.Where(r => r.RepositoryName.Contains(SearchString) ||
                                    r.UserName.Contains(SearchString));
            }

            //executes query
            SelectedRepositories = query.Include(r => r.Language).ToList();

            //view bags
            ViewBag.TotalRepositories = _db.Repositories.Count();
            ViewBag.SelectedRepositories = SelectedRepositories.Count();

            //returns to view
            return View(SelectedRepositories.OrderByDescending(r => r.StarCount));
        }

        public IActionResult Details(int? id)
        {
            if (id == null) //Repo id not specified
            {
                return View("Error", new String[] { "Repository ID not specified - which repo do you want to view?" });
            }

            Repository repo = _db.Repositories.Include(r => r.Language).FirstOrDefault(r => r.RepositoryID == id);

            if (repo == null) //Repo does not exist in database
            {
                return View("Error", new String[] { "Repository not found in database" });
            }

            //if code gets this far, all is well
            return View(repo);
        }

        public ActionResult DetailedSearch()
        {
            ViewBag.AllLanguages = GetAllLanguages();
            return View();
        }

        public ActionResult DisplaySearchResults(String SearchString, String SearchDescription, int SelectedLanguage,
                                                Range StarRange, String StarNumbers, DateTime? datelastmodified)
        {
            //creates list
            List<Repository> SelectedRepos;

            //starts teh query
            var query = from r in _db.Repositories
                        select r;

            //checks to see if its not null
            if (!string.IsNullOrEmpty(SearchString))
            {
                query = query.Where(r => r.RepositoryName.Contains(SearchString) ||
                                    r.UserName.Contains(SearchString));
            }

            //checks to see if description is not null
            if (SearchDescription != null && SearchDescription != "")
            {
                query = query.Where(r => r.RepositoryName.Contains(SearchDescription));
            }

            //checks languages
            if (SelectedLanguage == 0)
            {
                ViewBag.Nothing = 0;
            }
            else
            {
                query = query.Where(r => r.Language.LanguageID == SelectedLanguage);
            }

            //checks number of stars
            switch (StarRange)
            {
                case Range.GreaterThan:
                    query = query.Where(r => r.StarCount >= Convert.ToDecimal(StarNumbers));
                    break;
                case Range.LessThan:
                    query = query.Where(r => r.StarCount <= Convert.ToDecimal(StarNumbers));
                    break;
            }

            //checks date
            if (datelastmodified != null)
            {
                DateTime dateselected = datelastmodified ?? new DateTime(1900, 1, 1);
                query = query.Where(r => r.LastUpdate >= dateselected);
            }
            else
            {
                ViewBag.Nothing = 0;
            }


            //executes query
            SelectedRepos = query.Include(r => r.Language).ToList();

            //view bags
            ViewBag.TotalRepositories = _db.Repositories.Count();
            ViewBag.SelectedRepositories = SelectedRepos.Count;

            //returns to view
            return View("Index", SelectedRepos.OrderByDescending(r => r.StarCount));
        }

        public SelectList GetAllLanguages()
        {
            List<Language> Languages = _db.Languages.ToList();

            //add a record for all months
            Language SelectNone = new Language() { LanguageID = 0, Name = "All Languages" };
            Languages.Add(SelectNone);

            //convert list to select list
            SelectList AllLanguages = new SelectList(Languages.OrderBy(l => l.LanguageID), "LanguageID", "Name");

            //return the select list
            return AllLanguages;


        }
    }
}
