using EntityFramework_Slider.Data;
using EntityFramework_Slider.Models;
using EntityFramework_Slider.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Diagnostics;

namespace EntityFramework_Slider.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            //SessionStorage'e datani bu yolla qoyuruq !
            HttpContext.Session.SetString("name", "Pervin");
            /////////////////////////////////////////////

            //Cooki 'e datani bu yolla qoyuruq !

        //    Response.Cookies.Append("surname", "Rehimli");

            /////////////////////////////////////
            ///

            //Cookie' ye vaxt qoya bilirik ve bu muddetde cookie-de datani saxlayir.

            Response.Cookies.Append("surname", "Rehimli",new CookieOptions { MaxAge = TimeSpan.FromMinutes(30)});

            ////////////////////////////////////////////////////////////////////////////

            //Tutaqki elimizde book tipinden object'imiz var ve bunu cookie 'e qoymaq isteyirik.Bildiyimiz kimi string ve
            // int - den ferqli olaraq objectin qoyulmasi ferqlidi.


            Book book = new()
            {
                Id = 1,
                Name = "Xosrov ve Shirin"
            };


            Response.Cookies.Append("book", JsonConvert.SerializeObject(book));

            //////////////////////////////////////////////////////////////

            List<Slider> sliders = await _context.Sliders.ToListAsync();

            SliderInfo sliderInfo = await _context.SliderInfos.FirstOrDefaultAsync();

            IEnumerable<Blog> blogs = await _context.Blogs.Where(m=>!m.SoftDelete).ToListAsync();

            IEnumerable<Category> categories = await _context.Categories.Where(m => !m.SoftDelete).ToListAsync();

            IEnumerable<Product> products = await _context.Products.Include(m=>m.Images).Where(m => !m.SoftDelete).ToListAsync();


            HomeVM model = new()
            {
                Sliders = sliders,
                SliderInfo = sliderInfo,
                Blogs = blogs,
                Categories = categories,
                Products = products
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddBasket(int? id)
        {

            if (id == null) return BadRequest();

            Product? dbProduct = await _context.Products.Include(m=>m.Images).FirstOrDefaultAsync(m=>m.Id == id);

            if(dbProduct == null) return NotFound();

            List<BasketVM> basket;

            if (Request.Cookies["basket"] != null)
            {
                basket = JsonConvert.DeserializeObject<List<BasketVM>>(Request.Cookies["basket"]);
            }
            else
            {
                basket = new List<BasketVM>();
            }

            BasketVM? existProduct = basket.FirstOrDefault(m => m.Id == id);

            if (existProduct == null) 
            {

                basket?.Add(new BasketVM
                {
                    Id = dbProduct.Id,
                    Count = 1,
                    Price = dbProduct.Price,
                    Image = dbProduct.Images.FirstOrDefault().Image,
                    TotalPrice = (int)dbProduct.Price


            });

            }
            else
            {
                existProduct.Count++;
                existProduct.TotalPrice = (int)(existProduct.Price * existProduct.Count);

            }



            Response.Cookies.Append("basket", JsonConvert.SerializeObject(basket));

            return RedirectToAction(nameof(Index));


        }













        //Bu Action vasitesile SessionStorage'deki Token'i goturub Json'la formatini deyisib return edeceyik. /home/Test/

        public IActionResult Test()
        {
            var sessionData = HttpContext.Session.GetString("name");

            //Cookie'ni goturmek 
            var cookieData = Request.Cookies["surname"];
            ////////////////////

            //Object datani Cookie'den goturmek ucun yaziriq :

            var objectData = JsonConvert.DeserializeObject<Book>(Request.Cookies["book"]);

            //   return Json(sessionData + " " + cookieData);

            return Json(objectData);
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        


    }

    //Object gondermek ucun cookie'ye bir book classi yaradiram! 
    class Book
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    ////////////////////////////////////////////////////////////
}