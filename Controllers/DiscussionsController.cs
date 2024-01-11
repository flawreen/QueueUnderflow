using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QueueUnderflow.Data;
using QueueUnderflow.Models;
using System;
using System.Text.RegularExpressions;

namespace QueueUnderflow.Controllers
{
    public class DiscussionsController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DiscussionsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // motorul de cautare
        [NonAction]
        public IQueryable<Discussion> Search(string searchValue)
        {
            if (searchValue == null || searchValue.Length == 0)
            {
                return null;
            }
            // regex care ia doar litere si cifre
            var regex = new Regex(@"\W+");
            // lista de keywords
            var keywords = regex.Split(searchValue.ToLower());
            // iau obiectele din baza de date
            var discussions = db.Discussions.Include("Answers").Include("Category");
            // max-heap: cele mai relevante discutii gasite
            var rez = new PriorityQueue<Discussion, int>(Comparer<int>.Create((a, b) => b - a));

            foreach (var discussion in discussions)
            {
                // fac split sa iau doar cuvintele sub forma de lista din titlu, description + raspunsuri
                var allWords = regex.Split(discussion.Title.ToLower()).Union(regex.Split(discussion.Content.ToLower())).ToList();
                foreach (var comm in discussion.Answers)
                {
                    allWords.Union(regex.Split(comm.Content.ToLower()));
                }

                // relevanta = cate keyword-uri se regasesc in titlu, descriere si raspunsuri
                int relevance = allWords.Intersect(keywords).Count();
                if (relevance > 0)
                {
                    rez.Enqueue(discussion, relevance);  // adaug in max-heap
                }
            }

            // scot in ordine obiectele din max-heap si le incarc intr-o lista
            List<Discussion> res = new List<Discussion>();
            while (rez.TryDequeue(out var obj, out var priority))
            {
                res.Add(obj);
            }

            // nu s-au gasit rezultate
            if (!res.Any())
            {
                return null;
            }

            return res.AsQueryable();
        }

        [HttpPost]
        public IActionResult Index(string searchValue) // actiunea care afiseaza rezultatele searchului
        {
            var discussions = Search(searchValue);
            if (discussions == null)
            {
                ViewBag.IsNull = "null";
                return View();
            }

            var sortType = Convert.ToString(HttpContext.Request.Query["sort"]);
            if (sortType == "answers")
            {
                discussions = discussions.OrderByDescending(a => a.Answers.Count());
            }
            else
            {
                discussions = discussions.OrderBy(a => a.Date);
            }

            ViewBag.Discussions = discussions;
            return View();
        }

        public IActionResult Index()
        {
            var discussions = db.Discussions.Include("Category");
            var sortType = Convert.ToString(HttpContext.Request.Query["sort"]);
            if (sortType == "answers")
            {
                discussions = discussions.OrderByDescending(a => a.Answers.Count());
            }
            else
            {
                discussions = discussions.OrderBy(a => a.Date);
            }
            ViewBag.Discussions = discussions;

            return View();
        }

        
        public ActionResult Show(int id)
        {
            Discussion discussion = db.Discussions
                .Include("Answers")
                .Include("User")
                .Include("Answers.User")
                .Where(disc => disc.Id == id).First();

            return View( discussion );
        }

        [Authorize(Roles = "User, Admin")]
        public IActionResult New()
        {
            return View();
        }

        [Authorize(Roles = "User, Admin")]
        [HttpPost]
        public IActionResult New(Discussion discussion)
        {
            discussion.Date = DateTime.Now;

            // preluam id-ul utilizatorului care posteaza articolul
            discussion.UserId = _userManager.GetUserId(User);


            if (ModelState.IsValid)
            {
                db.Discussions.Add(discussion);
                db.SaveChanges();
                TempData["message"] = "The discussion has been successfully added";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }
            else
            {

                discussion.Category = (Category?)GetAllCategories();
                return View(discussion);
            }

        }

        [Authorize(Roles = "User, Admin")]
        public IActionResult Edit(int id)
        {
            Discussion discussion = db.Discussions.Find(id);

            ViewBag.Discussion = discussion;

            return View();
        }

        [Authorize(Roles = "User, Admin")]
        [HttpPost]
        public ActionResult Edit(int id, Discussion requestDiscussion)
        {
            Discussion discussion = db.Discussions.Find(id);

            try
            {
                discussion.Title = requestDiscussion.Title;
                discussion.Content = requestDiscussion.Content;
                discussion.Date = requestDiscussion.Date;

                db.SaveChanges();

                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                return RedirectToAction("Edit", discussion.Id);
            }
        }

        [Authorize(Roles = "User, Admin")]
        [HttpPost]
        public ActionResult Delete(int id)
        {
            Discussion discussion = db.Discussions.Find(id);

            db.Discussions.Remove(discussion);

            db.SaveChanges();

            return RedirectToAction("Index");
        }


        [NonAction]
        public IEnumerable<SelectListItem> GetAllCategories()
        {

            var selectList = new List<SelectListItem>();


            var categories = from cat in db.Categories
                             select cat;

            foreach (var category in categories)
            {
                selectList.Add(new SelectListItem
                {
                    Value = category.Id.ToString(),
                    Text = category.CategoryName.ToString()
                });
            }

            return selectList;
        }
    }
}
