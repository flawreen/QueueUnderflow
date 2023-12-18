using Microsoft.AspNetCore.Mvc;
using QueueUnderflow.Data;
using QueueUnderflow.Models;
using System;

namespace QueueUnderflow.Controllers
{
    public class DiscussionsController : Controller
    {
        private readonly ApplicationDbContext db;

        public DiscussionsController(ApplicationDbContext context)
        {
            db = context;
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
            Discussion discussion = db.Discussions.Find(id);

            ViewBag.Discussion = discussion;

            return View();
        }

        public IActionResult New()
        {
            return View();
        }

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

        public IActionResult Edit(int id)
        {
            Discussion discussion = db.Discussions.Find(id);

            ViewBag.Discussion = discussion;

            return View();
        }

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
