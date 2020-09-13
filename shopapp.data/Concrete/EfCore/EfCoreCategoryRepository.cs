using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using shopapp.data.Abstract;
using shopapp.entity;

namespace shopapp.data.Concrete.EfCore
{
    public class EfCoreCategoryRepository : EfCoreGenericRepository<Category, ShopContext>, ICategoryRepository
    {
        public void DeleteFromCategory(int ProductId, int CategoryId)
        {
             using (var context = new ShopContext())
            {
               // var cmd="delete from productcategory where ProductId=@p0 and CategoryId=@p1";
               // context.Database.ExecuteSqlRaw(cmd,ProductId,CategoryId);

                context.Database.ExecuteSqlRaw("delete from productcategory where ProductId=@p0 and CategoryId=@p1",ProductId,CategoryId);//bunu denedim gayette oldu
                 //sqltext yazıcam sorgum bu karşı veride şu diyorum 
            }
        }

        public Category GetByIdWithProducts(int categoryId)
        {
            using (var context = new ShopContext())
            {
                return context.Categories
                .Where(i=>i.CategoryId==categoryId)
                .Include(i=>i.ProductCategories)
                .ThenInclude(i=>i.Product)
                .FirstOrDefault();
            }
        }

        
    }
}