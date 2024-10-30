using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
            SetAccessRights();

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
            SetAccessRights();

            var discussions = Search(searchValue);
            if (discussions == null)
            {
                ViewBag.IsNull = "null";
                return View();
            }

            if (TempData["notification"] != null)
            {
                ViewBag.Notification = TempData["notification"];
                ViewBag.Icon = TempData["icon"];
                ViewBag.Type = TempData["type"];
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
            SetAccessRights();

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
            SetAccessRights();

            Discussion discussion = db.Discussions
                .Include("Answers")
                .Include("User")
                .Include("Answers.User")
                .Where(disc => disc.Id == id).First();

            return View( discussion );
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Show([FromForm] Answer answer)
        {
            SetAccessRights();

            answer.Date = DateTime.Now;

            answer.UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                db.Answers.Add(answer);
                db.SaveChanges();
                return Redirect("/Discussions/Show/" + answer.DiscussionId);
            }

            else
            {
                Discussion disc = db.Discussions.Include("Category")
                                         .Include("User")
                                         .Include("Answers")
                                         .Include("Answers.User")
                                         .Where(disc => disc.Id == answer.DiscussionId)
                                         .First();

                return View(disc);
            }
        }

        [Authorize(Roles = "User, Admin")]
        public IActionResult New()
        {
            SetAccessRights();

            Discussion discussion = new Discussion();

            discussion.Categ = GetAllCategories();

            return View(discussion);
        }

        [Authorize(Roles = "User, Admin")]
        [HttpPost]
        public IActionResult New(Discussion discussion)
        {
            SetAccessRights();

            discussion.Date = DateTime.Now;

            discussion.UserId = _userManager.GetUserId(User);


            if (ModelState.IsValid)
            {
                db.Discussions.Add(discussion);
                db.SaveChanges();
                TempData["notification"] = "The discussion has been successfully added";
                TempData["icon"] = "bi-plus-circle";
                TempData["type"] = "bg-success";
                return Redirect("/Home/Index");
            }
            else
            {

                discussion.Categ = GetAllCategories();
                return View(discussion);
            }

        }

        [Authorize(Roles = "User, Admin")]
        public IActionResult Edit(int id)
        {
            SetAccessRights();

            Discussion discussion = db.Discussions.Include("Category").Where(disc => disc.Id == id).First();

            discussion.Categ = GetAllCategories();

            if(discussion.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                return View(discussion);
            }
            else
            {
                TempData["notification"] = "You're not allowed to modify a discussion that you didn't create.";
                TempData["type"] = "bg-danger";
                
                return Redirect("/Home/Index");
            }
        }

        [Authorize(Roles = "User, Admin")]
        [HttpPost]
        public ActionResult Edit(int id, Discussion requestDiscussion)
        {
            SetAccessRights();

            Discussion discussion = db.Discussions.Find(id);

            if (ModelState.IsValid) 
            {


                if (discussion.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
                {
                    discussion.Title = requestDiscussion.Title;
                    discussion.Content = requestDiscussion.Content;
                    discussion.CategoryId = requestDiscussion.CategoryId;
          
                    TempData["notification"] = "The discussion has been modified.";
                    TempData["icon"] = "bi-plus-circle";
                    TempData["type"] = "bg-success";
                    db.SaveChanges();

                    return Redirect("/Home/Index");
                }
                else
                {
                    
                    TempData["notification"] = "You're not allowed to modify a discussion that you didn't create.";
                    TempData["type"] = "bg-danger";
                    return Redirect("/Home/Index");
                }
                
            }
            else
            {
                requestDiscussion.Categ = GetAllCategories();
                return View(requestDiscussion);
            }
        }

        [Authorize(Roles = "Admin")]
        public IActionResult EditAdmin(int id)
        {
            SetAccessRights();

            Discussion discussion = db.Discussions.Include("Answers").Include("Category").Where(disc => disc.Id == id).First();

            discussion.Categ = GetAllCategories();

            if (User.IsInRole("Admin"))
            {
                return View(discussion);
            }
            else
            {
                TempData["notification"] = "You're not allowed to modify a discussion that you didn't create.";
                TempData["type"] = "bg-danger";
                return Redirect("/Home/Index");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult EditAdmin(int id, Discussion requestDiscussion)
        {
            SetAccessRights();

            Discussion discussion = db.Discussions.Find(id);


            if (ModelState.IsValid)
            {

                if (User.IsInRole("Admin"))
                {
                    discussion.Title = requestDiscussion.Title;
                    discussion.Content = requestDiscussion.Content;
                    discussion.CategoryId = requestDiscussion.CategoryId;

                    TempData["notification"] = "The discussion has been modified.";
                    TempData["icon"] = "bi-plus-circle";
                    TempData["type"] = "bg-success";
                    db.SaveChanges();

                    return Redirect("/Home/Index");
                }
                else
                {
                    TempData["notification"] = "You're not allowed to modify a discussion that you didn't create.";
                    TempData["type"] = "bg-danger";
                    return Redirect("/Home/Index");
                }

            }
            else
            {
                requestDiscussion.Categ = GetAllCategories();
                return View(requestDiscussion);
            }
        }

        [Authorize(Roles = "User, Admin")]
        [HttpPost]
        public ActionResult Delete(int id)
        {
            SetAccessRights();

            Discussion discussion = db.Discussions.Include("Answers").Where(disc => disc.Id == id).First();

            if (discussion.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                db.Discussions.Remove(discussion);
                db.SaveChanges();
              
                TempData["notification"] = "The discussion has been deleted.";
                TempData["icon"] = "bi-plus-circle";
                TempData["type"] = "bg-success";
                db.SaveChanges();

                return Redirect("/Home/Index");
            }
            else
            {
                TempData["notification"] = "You're not allowed to modify a discussion that you didn't create.";
                TempData["type"] = "bg-danger";
                return Redirect("/Home/Index");
            }
        }

        private void SetAccessRights()
        {
            ViewBag.AfisareButoane = false;

            if (User.IsInRole("User"))
            {
                ViewBag.AfisareButoane = true;
            }

            ViewBag.EsteAdmin = User.IsInRole("Admin");

            ViewBag.UserCurent = _userManager.GetUserId(User);
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
