using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Inyama_Yethu.Conventions;

namespace Inyama_Yethu.Areas.Admin
{
    public class AdminAreaRegistration : IAreaConvention
    {
        public void Apply(ControllerModel controller)
        {
            if (controller.Attributes.Any(attr => attr.GetType().Name == "AdminAreaAttribute") ||
                controller.ControllerName.StartsWith("Admin"))
            {
                controller.RouteValues["area"] = "Admin";
            }
        }
    }
} 