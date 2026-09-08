using System.ComponentModel.DataAnnotations;

namespace AnimeWave.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Введите имя")]
        [Display(Name = "Имя")]
        public string DisplayName { get; set; } = string.Empty;


        [Required(ErrorMessage = "Введите Email")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;


        [Required(ErrorMessage = "Введите пароль")]
        [DataType(DataType.Password)]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;


        [Required]
        [DataType(DataType.Password)]
        [Compare(
            "Password",
            ErrorMessage = "Пароли не совпадают")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}