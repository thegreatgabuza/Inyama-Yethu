using System.ComponentModel.DataAnnotations;

namespace Inyama_Yethu.ViewModels
{
    public class PromoteToSeniorViewModel
    {
        public string UserId { get; set; }
        
        [Display(Name = "Email")]
        public string Email { get; set; }
        
        [Display(Name = "Promote to Senior Employee")]
        public bool IsCurrentlySenior { get; set; }
    }
} 