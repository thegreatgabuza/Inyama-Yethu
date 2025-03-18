using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Inyama_Yethu.Conventions;
using System.Linq;

namespace Inyama_Yethu.Areas.Admin
{
    public class AdminAreaRegistration : IAreaConvention
    {
        public void Apply(ControllerModel controller)
        {
            // Check if the controller is in the Admin area namespace
            if (controller.ControllerType.Namespace?.Contains(".Areas.Admin.Controllers") == true ||
                controller.Attributes.Any(attr => attr.GetType().Name == "AreaAttribute" && 
                                                 (attr as Microsoft.AspNetCore.Mvc.AreaAttribute)?.RouteValue == "Admin"))
            {
                controller.RouteValues["area"] = "Admin";
            }
        }
    }
} 