using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QueueUnderflow.Data;
using QueueUnderflow.Models;
using System;

namespace QueueUnderflow.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public CategoriesController(
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
            var categories = db.Categories.Include("Discussions");
            ViewBag.Categories = categories;

            return View();
        }

        public ActionResult Show(int id)
        {
            Category categ = db.Categories.Include("Discussions").Where(cat => cat.Id == id).First();

            // Alegem sa afisam 3 discutii pe pagina
            int _perPage = 3;
            var discussions = db.Discussions.Include("Category").Include("User").OrderBy(a => a.Date);
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
                ViewBag.Alert = TempData["messageType"];
            }

            // Fiind un numar variabil de discutii, verificam de fiecare data utilizand
            // metoda Count()
            int totalItems = discussions.Count();

            // Se preia pagina curenta din View-ul asociat
            // Numarul paginii este valoarea parametrului page din ruta
            // /Discussions/Show/Model.Id?page=valoare
            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);
            
            var offset = 0;
            
            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * _perPage;
            }
            var paginatedDiscussions = discussions.Skip(offset).Take(_perPage);

            // Preluam numarul ultimei pagini
            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);
            ViewBag.Discussions = paginatedDiscussions;
            return View(categ);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult New()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult New(Category categ)
        {
            try
            {
                db.Categories.Add(categ);

                db.SaveChanges();

                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                return View();
            }

        }

        [Authorize(Roles = "Admin")]
        public IActionResult Edit(int id)
        {
            Category categ = db.Categories.Find(id);

            ViewBag.Categories = categ;

            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult Edit(int id, Category requestCateg)
        {
            Category categ = db.Categories.Find(id);

            try
            {
                categ.CategoryName = requestCateg.CategoryName;

                db.SaveChanges();

                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                return RedirectToAction("Edit", categ.Id);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult Delete(int id)
        {
            Category categ = db.Categories.Find(id);

            db.Categories.Remove(categ);

            db.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
