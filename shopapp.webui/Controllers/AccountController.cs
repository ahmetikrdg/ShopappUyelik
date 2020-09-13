using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using shopapp.webui.EMailServices;
using shopapp.webui.Extensions;
using shopapp.webui.Identity;
using shopapp.webui.Models;

namespace shopapp.webui.Controllers
{
    public class AccountController:Controller
    {
        private UserManager<User> _userManager;
        private SignInManager<User> _signInManager;
        private IEmailSender _IEmailSender;
        public AccountController(UserManager<User> userManager,SignInManager<User> signInManager,IEmailSender IEmailSender)
        {
            _userManager=userManager;
            _signInManager=signInManager;
            _IEmailSender=IEmailSender;
        }
        public IActionResult Login(string ReturnUrl=null)//ilgili login sayfasını gösterecek
        {
            return View(new LoginModel{
                ReturnUrl=ReturnUrl
            });
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if(!ModelState.IsValid)
            {
                return View(model);
            }
           // var user= await _userManager.FindByNameAsync(model.UserName);
           var user= await _userManager.FindByEmailAsync(model.EMail);
            if(user==null)
            {
                ModelState.AddModelError("","Bu kullanıcı adı ile daha önce hesap oluşturulmadı");
                return View(model);
            }

            var result= await _signInManager.PasswordSignInAsync(user,model.Password,true,false);

            if(result.Succeeded)
            {
                return Redirect(model.ReturnUrl??"~/");//değer nulsa anasayfaya gider ?? ".." ile 
            }
            ModelState.AddModelError("","Kullanıcı Adı veya Parola Yanlış");
            return View(model);
        }
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public  async Task<IActionResult> Register(Register model)
        {
            if(!ModelState.IsValid)//ısvalid değilse girdiği bilgileri gönder
            {
              return View(model);
            }
            var user=new User()
            {
                FirstName=model.FirstName,
                LastName=model.LastName,
                UserName=model.UserName,
                Email=model.Email
            };

            var result=await _userManager.CreateAsync(user,model.Password);
            if(result.Succeeded)
            {//generate token
               var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var url = Url.Action("ConfirmEmail","Account",new {
                    userId = user.Id,
                    token = code
                });
                //email
                await _IEmailSender.SendEmailAsyc(model.Email,"Hesabınızı Onaylayınız",$"Lütfen EMail Hesabınızı Onaylamak İçin Linke <a href='https://localhost:5001{url}'> Tıklayınız </a>");

                return RedirectToAction("Login","Account");
            }
            ModelState.AddModelError("","Bilinmeyen bir hata oluştu lütfen tekrar deneyiniz.");
            Console.WriteLine(Url);
            return View(model);
        }

         public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
                 TempData.Put("message",new AlertMesaage()
                    {
                        Title="Oturum Kapatılı",
                        Message="En Kısa Zamanda Görüşmek Üzere...)",
                        AlertType="primary"
                    });
            return Redirect("~/");

        }

        public async Task<IActionResult> ConfirmEmail(string UserId,string Token)
        {
             if(UserId==null || Token ==null)
            {
                TempData.Put("message",new AlertMesaage()
                {
                    Title="Geçersiz Token",
                    Message="Geçersin Bir Token",
                    AlertType="danger"
                });
                return View();
            }
            var user = await _userManager.FindByIdAsync(UserId);
            if(user!=null)
            {
                var result = await _userManager.ConfirmEmailAsync(user,Token);
                 if(result.Succeeded)
                {
                    TempData.Put("message", new AlertMesaage()
                    {
                        Title="Hesabınız onaylandı.",
                        Message="Hesabınız onaylandı.",
                        AlertType="success"
                    });
                    return View();
                }
            }
                TempData.Put("message",new AlertMesaage()
                    {
                        Title="Hesabınız Onaylanmadı",
                        Message="Oleyy Hesabınız Onaylanmadı :(",
                        AlertType="warning"
                    });
            return View();
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string Email)
        {
            if(string.IsNullOrEmpty(Email))
            {
                return View();
            }
            var user=await _userManager.FindByEmailAsync(Email);

            if(user==null)
            {
                return View();
            }
            var token=await _userManager.GeneratePasswordResetTokenAsync(user);

            //generate token
               var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var url = Url.Action("ResetPassword","Account",new {
                    userId = user.Id,
                    token = code
                });
                //email
                await _IEmailSender.SendEmailAsyc(Email,"Reset Password",$"parolanızı yenilemek için EMail Linke <a href='https://localhost:5001{url}'> Tıklayınız </a>");

            return View();
        }

        public IActionResult ResetPassword(string UserId,string token)
        {
            if(UserId==null||token==null)
            {
                return RedirectToAction("Home","Index");
            }
            var model=new ResetPasswordModel{Token=token};
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            if(!ModelState.IsValid)
            {
                return View(model);
            }
            var user= await _userManager.FindByEmailAsync(model.Email);
            if(user==null)
            {
                return RedirectToAction("Home","Index");
            }
            var result=await _userManager.ResetPasswordAsync(user,model.Token,model.Password);
            if(result.Succeeded)
            {
                return RedirectToAction("Login","Account");
            }
            return View(model);
        }

    }
}