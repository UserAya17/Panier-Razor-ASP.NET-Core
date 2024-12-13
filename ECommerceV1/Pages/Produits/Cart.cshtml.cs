using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using ECommerceV1.Models;

namespace ECommerceV1.Pages.Produits
{
    public class CartModel : PageModel
    {
        public List<CartItem> CartItems { get; set; } = new List<CartItem>();
        public decimal SubTotal { get; set; }

        // Load cart items and calculate subtotal
        public void OnGet()
        {
            CartItems = GetCartFromSession();
            CalculateSubTotal();
        }

        // Update the quantity of a product in the cart
        public IActionResult OnPostUpdateQuantity(int produitId, int quantity)
        {
            var cart = GetCartFromSession();
            var productToUpdate = cart.FirstOrDefault(p => p.ProduitId == produitId);
           
            if (productToUpdate.Produit.QuantiteStock < quantity)
            {
                ModelState.AddModelError("", "Not enough stock available.");
                return Page();
            }

           else if (productToUpdate != null && quantity > 0 && quantity <= productToUpdate.Produit.QuantiteStock)
            {
                productToUpdate.Quantity = quantity;
                SaveCartToSession(cart);
                CartItems = cart;
                CalculateSubTotal();
            }

            return RedirectToPage();
        }

        // Remove a product from the cart
        public IActionResult OnPostRemoveFromCart(int produitId)
        {
            var cart = GetCartFromSession();
            var productToRemove = cart.FirstOrDefault(p => p.ProduitId == produitId);

            if (productToRemove != null)
            {
                cart.Remove(productToRemove);
                SaveCartToSession(cart);
                CalculateSubTotal();
            }

            return RedirectToPage();
        }

        // Calculate the subtotal of the cart items
        private void CalculateSubTotal()
        {
            SubTotal = CartItems.Sum(item => item.Quantity * item.Produit.Prix);
        }

        // Retrieve the cart from the session
        private List<CartItem> GetCartFromSession()
        {
            var cartJson = HttpContext.Session.GetString("cart");
            return string.IsNullOrEmpty(cartJson) ? new List<CartItem>() : JsonSerializer.Deserialize<List<CartItem>>(cartJson);
        }

        // Save the cart back to the session
        private void SaveCartToSession(List<CartItem> cart)
        {
            var cartJson = JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString("cart", cartJson);
        }
    }
}
