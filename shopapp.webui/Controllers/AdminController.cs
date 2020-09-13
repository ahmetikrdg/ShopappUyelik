using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using shopapp.business.Abstract;
using shopapp.entity;
using shopapp.webui.Models;

namespace shopapp.webui.Controllers
{
    [Authorize]
    public class AdminController:Controller
    {
        private IProductService _productService;
        private ICategoryService _categoryService;

        public AdminController(IProductService productService,ICategoryService categoryService)
        {
            this._productService=productService;
            _categoryService=categoryService;
        }

        public IActionResult ProductList()
        {
            return View(new ProductListViewModel()
            {
                Products= _productService.GetAll(),//ana yerden efcoregenericten geliyor
            });
        }

        [HttpGet]
        public IActionResult CreateProduct()
        {//formu çağırdığım an bura get gelir. Gönderdiğim an postla gider oda aşağıda

            return View();      
      
        }
        [HttpPost]
         public async Task<IActionResult>  CreateProduct(ProductModel model,IFormFile file)
        {//formu çağırdığım an bura get gelir. Gönderdiğim an postla gider
              
              if(ModelState.IsValid)//girilen değerler benim kriterime uyuyorsa
              {
                    var entity = new Product
                    {
                        Name=model.Name,
                        Url=model.Url,
                        Price=model.Price,
                        Description=model.Description,
                        ImageUrl=model.ImageUrl,
                        IsApproved=model.IsApproved,
                        IsHome=model.IsHome,
                    };
                                ViewBag.Categories=_categoryService.GetAll();

                    if(file!=null)
                    {
                        var extention = Path.GetExtension(file.FileName);
                        var randomName = string.Format($"{Guid.NewGuid()}{extention}");
                        entity.ImageUrl = randomName;
                        var path = Path.Combine(Directory.GetCurrentDirectory(),"wwwroot\\img",randomName);

                        using(var stream = new FileStream(path,FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                    }
                    _productService.Create(entity);
                    //TempData["message"]=$"{entity.Name} isimli ürün eklendi."; //bu mesajı view admin productlistte gösteririz.çünkü o sayfaya gidiyoruz.ViewData yapsaydım olmazdı çünkü o sayfa yeni bir sayfa.Direk layoulta yazdımki belki diğer sayfalardada kullanırız diye
                    var msg= new AlertMesaage()
                    {
                        Message=$"{entity.Name} isimli ürün eklendi.",
                        AlertType="success"
                    };
                    TempData["message"]=JsonConvert.SerializeObject(msg);
                    return RedirectToAction("ProductList");//postlayınca productlist sayfası gelsin
              }
              return View(model);//eğer girdiği bilgiler hatalıysa girdikleriyle birlikte geri dönderirim sayfayı
        }

        [HttpGet]
        public IActionResult ProductEdit(int? id)
        {
            if(id==null)
            {
                return NotFound();
            }
            var entity=_productService.GetByWidthCategoriesId((int)id);
             if(entity==null)
            {
                return NotFound();
            }
            var model=new ProductModel()
            {
                ProductId=entity.ProductId,
                Name=entity.Name,
                Url=entity.Url,
                Price=entity.Price,
                ImageUrl=entity.ImageUrl,
                Description=entity.Description,
                IsHome=entity.IsHome,
                IsApproved=entity.IsApproved,
                SelectedCategories=entity.ProductCategories.Select(i=>i.Category).ToList()
            };
            ViewBag.Categories=_categoryService.GetAll();
            return View(model);            
        }

        [HttpPost]
        public async Task<IActionResult> ProductEdit(ProductModel model,int [] CategoryIds,IFormFile file)//inputta gönderilen name neyse Ifrmdada o
        {
            if(ModelState.IsValid)//girilen değerler benim kriterime uyuyorsa
              {
                 var entity=_productService.GetById(model.ProductId);//modelin içindekini alıyoki kullanıcı girmesin
                    if(entity==null)
                    {
                        return NotFound();
                    }
                 entity.Name=model.Name;
                 entity.Price=model.Price;
                 entity.Url=model.Url;
                 entity.ImageUrl=model.ImageUrl;
                 entity.Description=model.Description;
                 entity.IsHome=model.IsHome;
                 entity.IsApproved=model.IsHome;

                    if(file!=null)
                    {
                        var extention = Path.GetExtension(file.FileName);
                        var randomName = string.Format($"{Guid.NewGuid()}{extention}");
                        entity.ImageUrl = randomName;
                        var path = Path.Combine(Directory.GetCurrentDirectory(),"wwwroot\\img",randomName);

                        using(var stream = new FileStream(path,FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                    }




                 _productService.Update(entity,CategoryIds);
                // TempData["message"]=$"{entity.Name} isimli ürün güncellendi.";
                var msg= new AlertMesaage()
                {
                    Message=$"{entity.Name} isimli ürün güncelle.",
                    AlertType="primary"
                };
                TempData["message"]=JsonConvert.SerializeObject(msg);
                return RedirectToAction("ProductList");   
              }
              ViewBag.Categories=_categoryService.GetAll();
              return View(model);      
        }
        
        [HttpPost]
        public IActionResult DeleteProduct(int productId)
        {
            var entity=_productService.GetById(productId);
            if(entity!=null)
            {
                _productService.Delete(entity);
            }
            //TempData["message"]=$"{entity.Name} isimli ürün silindi.";
            var msg= new AlertMesaage()
           {
               Message=$"{entity.Name} isimli ürün silindi.",
               AlertType="danger"
           };
           TempData["message"]=JsonConvert.SerializeObject(msg);
            return RedirectToAction("ProductList");         
        }
  //--------------------------------------------CATEGORY-----------------------------------------------------------------------------
        public IActionResult CategoryList()
        {
            return View(new CategoryListViewModel()
            {
                Categories= _categoryService.GetAll(),//ana yerden efcoregenericten geliyor
            });
        }

         [HttpGet]
        public IActionResult CategoryCreate()
        {//formu çağırdığım an bura get gelir. Gönderdiğim an postla gider oda aşağıda
            return View();            
        }
        [HttpPost]
        public IActionResult CategoryCreate(CategoryModel model)
        {//formu çağırdığım an bura get gelir. Gönderdiğim an postla gider
           if(ModelState.IsValid)//girilen değerler benim kriterime uyuyorsa
              {
                var entity = new Category
                {
                    Name=model.Name,
                    Url=model.Url
                };
                _categoryService.Create(entity);
                var msg= new AlertMesaage()
                {
                    Message=$"{entity.Name} isimli kategori eklendi.",
                    AlertType="success"
                };
                TempData["message"]=JsonConvert.SerializeObject(msg);
                return RedirectToAction("CategoryList");//postlayınca productlist sayfası gelsin
             }
             return View(model);   
        }

         [HttpGet]
        public IActionResult CategoryEdit(int? id)
        {
            if(id==null)
            {
                return NotFound();
            }
            var entity=_categoryService.GetByIdWithProducts((int)id);
             if(entity==null)
            {
                return NotFound();
            }
            var model=new CategoryModel()
            {
                CategoryId=entity.CategoryId,
                Name=entity.Name,
                Url=entity.Url,
                Products=entity.ProductCategories.Select(p=>p.Product).ToList(),
            };
        
            return View(model);            
        }

        [HttpPost]
        public IActionResult CategoryEdit(CategoryModel model)
        {
            if(ModelState.IsValid)//girilen değerler benim kriterime uyuyorsa
              {
                  var entity=_categoryService.GetById(model.CategoryId);//modelin içindekini alıyoki kullanıcı girmesin
                  if(entity==null)
                  {
                      return NotFound();
                  }
                 entity.Name=model.Name;
                 entity.Url=model.Url;
                 _categoryService.Update(entity);
                // TempData["message"]=$"{entity.Name} isimli ürün güncellendi.";
                var msg= new AlertMesaage()
                {
                    Message=$"{entity.Name} isimli kategori güncelle.",
                    AlertType="primary"
                };
                TempData["message"]=JsonConvert.SerializeObject(msg);
                    return RedirectToAction("CategoryList");  
              }
              return View(model);       
        }

        public IActionResult CategoryDelete(int CategoryId)
        {
            var entity=_categoryService.GetById(CategoryId);
            if(entity!=null)
            {
                _categoryService.Delete(entity);
            }
            //TempData["message"]=$"{entity.Name} isimli ürün silindi.";
            var msg= new AlertMesaage()
           {
               Message=$"{entity.Name} isimli kategori silindi.",
               AlertType="danger"
           };
           TempData["message"]=JsonConvert.SerializeObject(msg);
            return RedirectToAction("CategoryList");         
        }
        [HttpPost]
        public IActionResult deleteFromCategory(int productId,int CategoryId)
        {
            _categoryService.DeleteFromCategory(productId,CategoryId);
                        return RedirectToAction("CategoryList");         
        }
        
    }
}