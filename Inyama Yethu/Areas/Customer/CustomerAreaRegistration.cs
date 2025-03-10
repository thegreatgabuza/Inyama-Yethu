using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Inyama_Yethu.Conventions;

namespace Inyama_Yethu.Areas.Customer
{
    public class CustomerAreaRegistration : IAreaConvention
    {
        public void Apply(ControllerModel controller)
        {
            if (controller.Attributes.Any(attr => attr.GetType().Name == "CustomerAreaAttribute") ||
                controller.ControllerName.StartsWith("Customer"))
            {
                controller.RouteValues["area"] = "Customer";
            }
        }
    }
} 