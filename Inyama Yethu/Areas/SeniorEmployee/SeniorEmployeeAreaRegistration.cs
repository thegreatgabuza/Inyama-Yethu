using Microsoft.AspNetCore.Mvc;

namespace Inyama_Yethu.Areas.SeniorEmployee
{
    [Area("SeniorEmployee")]
    public class SeniorEmployeeAreaAttribute : AreaAttribute
    {
        public SeniorEmployeeAreaAttribute() : base("SeniorEmployee")
        {
        }
    }
} 