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


        // Adaugarea unui raspuns asociat unei discutii in baza de date
        [Authorize(Roles = "User, Admin")]
        [HttpPost]
        public IActionResult New(Answer answer)
        {
            answer.Date = DateTime.Now;

            try
            {
                db.Answers.Add(answer);
                db.SaveChanges();
                return Redirect("/Discussions/Show/" + answer.DiscussionId); // de modificat DiscussionId
            }

            catch (Exception)
            {
                return Redirect("/Discussions/Show/" + answer.DiscussionId);
            }

        }

        // Stergerea unui raspuns asociat unei discutii din baza de date
        [Authorize(Roles = "User, Admin")]
        [HttpPost]
        public IActionResult Delete(int id)
        {
            Answer answer = db.Answers.Find(id);
            db.Answers.Remove(answer);
            db.SaveChanges();
            return Redirect("/Discussions/Show/" + answer.DiscussionId);
        }

        // In acest moment vom implementa editarea intr-o pagina View separata
        // Se editeaza un raspuns existent
        [Authorize(Roles = "User, Admin")]
        public IActionResult Edit(int id)
        {
            Answer answer = db.Answers.Find(id);
            ViewBag.Answer = answer;
            return View();
        }

        [Authorize(Roles = "User, Admin")]
        [HttpPost]
        public IActionResult Edit(int id, Answer requestAnswer)
        {
            Answer answer = db.Answers.Find(id);
            try
            {

                answer.Content = requestAnswer.Content;

                db.SaveChanges();

                return Redirect("/Discussions/Show/" + answer.DiscussionId);
            }
            catch (Exception e)
            {
                return Redirect("/Discussions/Show/" + answer.DiscussionId);
            }

        }
    }
}
