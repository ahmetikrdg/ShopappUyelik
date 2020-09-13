using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using shopapp.entity;

namespace shopapp.webui.Models
{
    public class ProductModel
    {
        public int ProductId { get; set; }  
        [Display(Name="Name",Prompt="Enter product name")]
        [Required(ErrorMessage="Ürün İsmi Zorunlu Bir Alandır :)")]
        [StringLength(60,MinimumLength=5,ErrorMessage="Ürün ismi 5-10 karakter arası falan")]
        public string Name { get; set; }       
                [Required(ErrorMessage="Url İsmi Zorunlu Bir Alandır :)")]
        public string Url { get; set; }       
                [Required(ErrorMessage="Price İsmi Zorunlu Bir Alandır :)")]
                [Range(1,10000,ErrorMessage="1 ile 10000 arası değer girmelisin")]
        public double? Price { get; set; } //? yapmazsan 0 değer olarak algılar ve anlamsız olur requered
                [Required(ErrorMessage="Description İsmi Zorunlu Bir Alandır :)")]
        public string Description { get; set; }         
        public string ImageUrl { get; set; }
        public bool IsApproved { get; set; }
        public bool IsHome { get; set; }
        public List<Category> SelectedCategories { get; set; }
    }//burada ve viewde istediğin gibi oynarsın en sonda gönderirsin zaten controllerda bakarsan tüm değişiklik yapılmış modee gitmiş ve product.csy atılmış şekilde görürsün.
}