﻿namespace DnaFragment.DnaFragmentControllers
{    
    using DnaFragment.Infrastructure;   
    using DnaFragment.Models.LrProducts;   
    using Microsoft.AspNetCore.Mvc;    
    using DnaFragment.Services.LrProducts;
    using DnaFragment.Services.Administrators;
    using Microsoft.AspNetCore.Authorization;
    using DnaFragment.Data;
    using System.Linq;

    public class LrProductsController : Controller
    {     
        
        private readonly ILrProductsService lrProducts;
        private readonly IAdministratorService administrator;
        private readonly DnaFragmentDbContext data;

        public LrProductsController(         
               ILrProductsService lrProducts, IAdministratorService administrator, DnaFragmentDbContext data)
        {           
            this.lrProducts = lrProducts;
            this.administrator = administrator;
            this.data = data;
        }

        public IActionResult Add()
        {
            if (!administrator.UserIsRegister(User.GetId()))
            {
                return RedirectToAction(nameof(LrUsersController.Register), "LrUsers");
            }

            return View(new AddProductFormModel
            {
                Categories = lrProducts.AllCategories()
            });
        }
       

        [HttpPost]
        [Authorize]
        public IActionResult Add(AddProductFormModel lrProduct)
        {
            var lrUserId = administrator.AdministratorId(User.GetId());

            if (lrUserId == null)
            {
                return RedirectToAction(nameof(LrUsersController.Register), "LrUsers");
            }           

            if (!lrProducts.CategoryExsists(lrProduct.CategoryId))
            {
                this.ModelState.AddModelError(nameof(lrProduct.CategoryId), "Category does not exist.");
            }

            if (!ModelState.IsValid)
            {
                return View(lrProduct);
            }

            lrProducts.Create(lrProduct.Model,
                 lrProduct.PackagingVolume,
                 lrProduct.ChemicalIngredients,
                 lrProduct.Description,
                 lrProduct.Price,
                 lrProduct.Year,
                 lrProduct.Image,
                 lrProduct.PlateNumber,
                 lrProduct.CategoryId,
                 lrUserId);

            return  RedirectToAction(nameof(All));
        }

        [Authorize(Roles = "Administrator")]
        public IActionResult Update(string id)
        {
            var userId = this.User.GetId();

            if (!User.IsAdmin())
            {
                return RedirectToAction(nameof(LrUsersController.Register), "LrUsers");
            }

            var product = lrProducts.Details(id);

            return View(new AddProductUpdateFormModel {Description =product.Description,Price = product.Price ,CategoryId = product.CategoryId});
        } 

        public IActionResult Update(string id, AddProductUpdateFormModel product)
        {
           

            if (!User.IsAdmin())
            {
                return RedirectToAction(nameof(LrUsersController.Register), "LrUsers");
            }

            if (!lrProducts.CategoryExsists(product.CategoryId))
            {
                this.ModelState.AddModelError(nameof(product.CategoryId), "Category does not exist.");
            }

            if (!ModelState.IsValid)
            {
                product.Categories = lrProducts.AllCategories();

                return View(product);
            }


            lrProducts.Update(
                id,                
                product.Description,
                product.Price,
                product.CategoryId);

            return RedirectToAction(nameof(All));
        }

        [Authorize]
        public IActionResult Edit(string id)
        {
            string userId = User.GetId();
            if (administrator.UserIsRegister(userId))
            {
                return RedirectToAction(nameof(LrUsersController.Register), "LrUsers");
            }
            //var lrProduct = lrProducts.Details(id);

            if (!lrProducts.ExistUserProduct(id,userId))
            {
                lrProducts.CreateUserProduct(id, userId);
            }         
          

            return RedirectToAction(nameof(Favorits));
        }

        [Authorize]
        public IActionResult Favorits()
        {
            if(administrator.UserIsRegister(User.GetId()))
            {
                return Unauthorized();
            }
            var products = lrProducts.Favorits(User.GetId());

            return View(products);
        } 

        public IActionResult Details(string id)
        {
            var product = lrProducts.Details(id);
               

            return View(product);
        }

        public IActionResult AllProductsByCategory(int categoryId)
        {
            var products = lrProducts.AllProductsByCategory(categoryId);          

            return View(products);
        }

        public IActionResult All([FromQuery] AllProductsQueryModel query)
        {

            var queryResult = this.lrProducts.All(query.Brand, query.SearchTerm, query.Sorting, query.CurrentPage, AllProductsQueryModel.ProductsPerPage);
            var lrBrands = this.lrProducts.AllLrBrands();

            query.TotalProducts = queryResult.TotalProducts;
            query.Brands = lrBrands;
            query.LrProducts = queryResult.LrProducts;

            return View(query);
        }     

    }
}