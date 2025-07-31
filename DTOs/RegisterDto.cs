using System.ComponentModel.DataAnnotations;

namespace CRMApp.DTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "نام کاربری الزامی است")]
        public string Username { get; set; }

        [Required(ErrorMessage = "رمز عبور الزامی است")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "ایمیل الزامی است")]
        [EmailAddress(ErrorMessage = "ایمیل وارد شده معتبر نیست")]
        public string Email { get; set; }

        [Required(ErrorMessage = "نقش الزامی است")]
        public string RoleName { get; set; }
    }
}
