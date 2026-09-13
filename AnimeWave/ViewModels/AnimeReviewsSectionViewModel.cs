using System.ComponentModel.DataAnnotations;

namespace AnimeWave.ViewModels
{
    public class AnimeReviewsSectionViewModel
    {
        public int AnimeId { get; set; }

        public bool IsAuthenticated { get; set; }

        public double AverageRating { get; set; }

        public int ReviewsCount { get; set; }


        public AnimeReviewFormViewModel Form { get; set; }
            = new();


        public List<AnimeReviewItemViewModel> Reviews { get; set; }
            = new();
    }



    public class AnimeReviewFormViewModel
    {
        public int AnimeId { get; set; }


        [Range(
            1,
            10,
            ErrorMessage = "Выберите оценку от 1 до 10."
        )]
        public int Rating { get; set; } = 10;


        [Required(ErrorMessage = "Напишите отзыв.")]
        [MaxLength(
            2000,
            ErrorMessage = "Максимум 2000 символов."
        )]
        public string Text { get; set; }
            = string.Empty;
    }



    public class AnimeReviewItemViewModel
    {
        public int Id { get; set; }

        public string AuthorName { get; set; }
            = string.Empty;

        public string AvatarStyle { get; set; }
            = "violet";

        public string AvatarSymbol { get; set; }
            = "月";

        public int Rating { get; set; }

        public string Text { get; set; }
            = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public bool IsMine { get; set; }
    }
}