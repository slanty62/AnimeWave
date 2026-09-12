using System.ComponentModel.DataAnnotations;

namespace AnimeWave.ViewModels
{
    public class ProfileEditViewModel
    {
        [Required(ErrorMessage = "Введите имя.")]
        [MaxLength(
            100,
            ErrorMessage = "Имя не должно превышать 100 символов."
        )]
        [Display(Name = "Имя")]
        public string DisplayName { get; set; }
            = string.Empty;


        [Required(ErrorMessage = "Введите email.")]
        [EmailAddress(ErrorMessage = "Некорректный email.")]
        [Display(Name = "Email")]
        public string Email { get; set; }
            = string.Empty;


        // =========================================================
        // AVATAR
        // =========================================================

        [Required]
        public string AvatarStyle { get; set; }
            = "violet";


        public List<ProfileAvatarOptionViewModel> AvatarOptions { get; set; }
            = new();


        // =========================================================
        // INTERFACE THEME
        // =========================================================

        [Required]
        public string ThemeStyle { get; set; }
            = "violet";


        public List<ProfileThemeOptionViewModel> ThemeOptions { get; set; }
            = new();


        // =========================================================
        // PASSWORD
        // =========================================================

        [DataType(DataType.Password)]
        [Display(Name = "Текущий пароль")]
        public string? CurrentPassword { get; set; }


        [DataType(DataType.Password)]
        [MinLength(
            6,
            ErrorMessage = "Новый пароль должен содержать минимум 6 символов."
        )]
        [Display(Name = "Новый пароль")]
        public string? NewPassword { get; set; }


        [DataType(DataType.Password)]
        [Compare(
            nameof(NewPassword),
            ErrorMessage = "Пароли не совпадают."
        )]
        [Display(Name = "Повторите новый пароль")]
        public string? ConfirmNewPassword { get; set; }
    }



    public class ProfileAvatarOptionViewModel
    {
        public string Id { get; set; }
            = string.Empty;

        public string Name { get; set; }
            = string.Empty;

        public string Symbol { get; set; }
            = string.Empty;
    }



    public class ProfileThemeOptionViewModel
    {
        public string Id { get; set; }
            = string.Empty;

        public string Name { get; set; }
            = string.Empty;

        public string Description { get; set; }
            = string.Empty;
    }
}