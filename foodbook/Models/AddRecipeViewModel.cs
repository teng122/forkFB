using System.ComponentModel.DataAnnotations;

namespace foodbook.Models
{
    public class AddRecipeViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên công thức")]
        [Display(Name = "Tên công thức")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Ảnh hoặc Video chính")]
        public IFormFile? MainMedia { get; set; }

        [Display(Name = "Ảnh thumbnail")]
        public IFormFile? ThumbnailImage { get; set; }

        [Display(Name = "Mô tả nguyên liệu")]
        public string? Description { get; set; }

        [Display(Name = "Nguyên liệu")]
        public List<string> Ingredients { get; set; } = new List<string>();

        [Display(Name = "Phân loại")]
        public List<string> RecipeTypes { get; set; } = new List<string>();

        [Required(ErrorMessage = "Vui lòng nhập thời gian nấu")]
        [Display(Name = "Thời gian nấu (phút)")]
        public int CookTime { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn độ khó")]
        [Display(Name = "Độ khó")]
        public string Level { get; set; } = "dễ";

        [Display(Name = "Các bước thực hiện")]
        public List<RecipeStepViewModel> Steps { get; set; } = new List<RecipeStepViewModel>();
    }

    public class RecipeStepViewModel
    {
        public int StepNumber { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập mô tả bước")]
        public string Instruction { get; set; } = string.Empty;
        
        public IFormFile? StepImage { get; set; }
    }
}

