using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Inyama_Yethu.Conventions;

namespace Inyama_Yethu.Areas.Employee
{
    public class EmployeeAreaRegistration : IAreaConvention
    {
        public void Apply(ControllerModel controller)
        {
            if (controller.Attributes.Any(attr => attr.GetType().Name == "EmployeeAreaAttribute") ||
                controller.ControllerName.StartsWith("Employee"))
            {
                controller.RouteValues["area"] = "Employee";
            }
        }
    }
} 