using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QueueUnderflow.Data;
using QueueUnderflow.Models;

namespace QueueUnderflow.Controllers
{
    public class AnswersController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AnswersController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }


        [Authorize(Roles = "User, Admin")]
        [HttpPost]
        public IActionResult New(Answer answer)
        {
            answer.Date = DateTime.Now;

            try
            {
                db.Answers.Add(answer);
                db.SaveChanges();
                return Redirect("/Discussions/Show/" + answer.DiscussionId);
            }

            catch (Exception)
            {
                return Redirect("/Discussions/Show/" + answer.DiscussionId);
            }

        }

        [Authorize(Roles = "User, Admin")]
        [HttpPost]
        public IActionResult Delete(int id)
        {
            Answer answer = db.Answers.Find(id);

            if (answer.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                db.Answers.Remove(answer);
                db.SaveChanges();
                return Redirect("/Discussions/Show/" + answer.DiscussionId);
            }
            else
            {
                TempData["notification"] = "You're not allowed to delete that answer.";
                TempData["type"] = "bg-danger";
                return RedirectToAction("Index", "Discussions");
            }
        }

        [Authorize(Roles = "User, Admin")]
        public IActionResult Edit(int id)
        {
            Answer answer = db.Answers.Find(id);

            if (answer.UserId == _userManager.GetUserId(User))
            {
                return View(answer);
            }
            else
            {
                TempData["notification"] = "You're not allowed to edit that answer.";
                TempData["type"] = "bg-danger";
                return RedirectToAction("Index", "Discussions");
            }
        }

        [Authorize(Roles = "User, Admin")]
        [HttpPost]
        public IActionResult Edit(int id, Answer requestAnswer)
        {
            Answer answer = db.Answers.Find(id);
         
            if (answer.UserId == _userManager.GetUserId(User))
            {
                if (ModelState.IsValid)
                {
                    answer.Content = requestAnswer.Content;

                    db.SaveChanges();

                    return Redirect("/Discussions/Show/" + answer.DiscussionId);
                }
                else
                {
                    return View(requestAnswer);
                }
            }
            else
            {
                TempData["notification"] = "You're not allowed to make changes";
                TempData["type"] = "bg-danger";
                return RedirectToAction("Index", "Discussions");

            }
        }
    }
}
