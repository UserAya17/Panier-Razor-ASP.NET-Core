using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ECommerceV1.Data;
using ECommerceV1.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ECommerceV1.Pages.Produits
{
    public class UserModel : PageModel
    {
        private readonly ECommerceV1Context _context;

        public UserModel(ECommerceV1Context context)
        {
            _context = context;
        }

        public IList<Produit> Produit { get; set; } = default!;

        [BindProperty(SupportsGet = true)]
        public string? SearchString { get; set; }

        public SelectList? Descriptions { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? ProduitDescription { get; set; }

        public SelectList? Categories { get; set; }
        [BindProperty(SupportsGet = true)]
        public int? CategorieId { get; set; }

        public async Task OnGetAsync()
        {
            IQueryable<string> genreQuery = from m in _context.Produit
                                            orderby m.Nom
                                            select m.Nom;

            var categoryQuery = from c in _context.Categorie
                                orderby c.Nom
                                select c;

            var Produits = _context.Produit.AsQueryable();

            if (!string.IsNullOrEmpty(SearchString))
            {
                Produits = Produits.Where(s => s.Nom.Contains(SearchString));
            }

            if (!string.IsNullOrEmpty(ProduitDescription))
            {
                Produits = Produits.Where(x => x.Nom == ProduitDescription);
            }

            if (CategorieId.HasValue)
            {
                Produits = Produits.Where(p => p.CategorieId == CategorieId);
            }

            Descriptions = new SelectList(await genreQuery.Distinct().ToListAsync());
            Categories = new SelectList(await categoryQuery.ToListAsync(), "Id", "Nom");
            Produit = await Produits.ToListAsync();
        }

        public void AddProduit(Produit p, int qt)
        {
            List<CartItem> cart = GetCartFromSession();
            CartItem cartItem = cart.FirstOrDefault(ci => ci.ProduitId == p.Id);

            if (cartItem != null)
            {
                cartItem.Quantity += qt;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProduitId = p.Id,
                    sousPrix = p.Prix * qt,
                    Quantity = qt,
                    Produit = p
                });
            }

            SaveCartToSession(cart);
        }

        public async Task<IActionResult> OnPostAddToCartAsync(int id, int quantity)
        {
            var product = await _context.Produit.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            if (product.QuantiteStock < quantity)
            {
                ModelState.AddModelError("", "Not enough stock available.");
                return Page();
            }

            AddProduit(product, quantity);
            return RedirectToPage();
        }

        private List<CartItem> GetCartFromSession()
        {
            var cartJson = HttpContext.Session.GetString("cart");

            if (!string.IsNullOrEmpty(cartJson))
            {
                return JsonSerializer.Deserialize<List<CartItem>>(cartJson);
            }

            return new List<CartItem>();
        }

        private void SaveCartToSession(List<CartItem> cart)
        {
            var cartJson = JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString("cart", cartJson);
        }
    }
}
