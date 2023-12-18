using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
            try
            {
                db.Discussions.Add(discussion);

                db.SaveChanges();

                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                return View();
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
    }
}
