using Microsoft.AspNetCore.Mvc;
using QueueUnderflow.Data;
using QueueUnderflow.Models;

namespace QueueUnderflow.Controllers
{
    public class AnswersController : Controller
    {
        private readonly ApplicationDbContext db;
        public AnswersController(ApplicationDbContext context)
        {
            db = context;
        }


        // Adaugarea unui raspuns asociat unei discutii in baza de date
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

        public IActionResult Edit(int id)
        {
            Answer answer = db.Answers.Find(id);
            ViewBag.Answer = answer;
            return View();
        }

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
