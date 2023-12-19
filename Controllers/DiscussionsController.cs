using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QueueUnderflow.Data;
using QueueUnderflow.Models;
using System;

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



        public IActionResult Index()
        {
            var discussions = from discussion in db.Discussions
                           select discussion;

            ViewBag.Discussions = discussions;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

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
    }
}
