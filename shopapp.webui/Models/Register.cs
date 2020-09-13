using System.ComponentModel.DataAnnotations;

namespace shopapp.webui.Models
{
    public class Register
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        [DataType(DataType.Password)]//dedimki şifre gizli olsun
        public string Password { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Compare("Password")] //oasswordle repasworde girdiğim değer aynı olmak zorunda
        public string RePassword { get; set; }
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

    }
}