using Inyama_Yethu.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Inyama_Yethu.Data
{
    public static class TaskCategorySeeder
    {
        public static async Task SeedDefaultCategoriesAsync(ApplicationDbContext context)
        {
            if (!await context.TaskCategories.AnyAsync())
            {
                var defaultCategories = new List<TaskCategory>
                {
                    new TaskCategory 
                    { 
                        Name = "Mating", 
                        Description = "Tasks related to animal breeding and mating procedures",
                        IsSystem = true
                    },
                    new TaskCategory 
                    { 
                        Name = "PregnancyCheck", 
                        Description = "Tasks for monitoring and checking pregnant animals",
                        IsSystem = true
                    },
                    new TaskCategory 
                    { 
                        Name = "Vaccination", 
                        Description = "Tasks related to animal vaccinations and immunizations",
                        IsSystem = true
                    },
                    new TaskCategory 
                    { 
                        Name = "PigletProcessing", 
                        Description = "Tasks for handling and processing newborn piglets",
                        IsSystem = true
                    },
                    new TaskCategory 
                    { 
                        Name = "Feeding", 
                        Description = "Tasks related to animal feeding and nutrition",
                        IsSystem = true,
                        IsDefault = true
                    },
                    new TaskCategory 
                    { 
                        Name = "Weighing", 
                        Description = "Tasks for monitoring animal weight and growth",
                        IsSystem = true
                    },
                    new TaskCategory 
                    { 
                        Name = "Weaning", 
                        Description = "Tasks related to weaning processes",
                        IsSystem = true
                    },
                    new TaskCategory 
                    { 
                        Name = "AbattoirShipment", 
                        Description = "Tasks related to preparing and shipping animals to abattoir",
                        IsSystem = true
                    },
                    new TaskCategory 
                    { 
                        Name = "Farrowing", 
                        Description = "Tasks related to sow farrowing and birth management",
                        IsSystem = true
                    },
                    new TaskCategory 
                    { 
                        Name = "Piglet Care", 
                        Description = "General tasks for piglet care and maintenance",
                        IsSystem = true
                    },
                    new TaskCategory 
                    { 
                        Name = "Livestock", 
                        Description = "General livestock management tasks",
                        IsSystem = true
                    },
                    new TaskCategory 
                    { 
                        Name = "Health", 
                        Description = "Tasks related to animal health and wellness",
                        IsSystem = true
                    },
                    new TaskCategory 
                    { 
                        Name = "Maintenance", 
                        Description = "Facility and equipment maintenance tasks",
                        IsSystem = false
                    },
                    new TaskCategory 
                    { 
                        Name = "Cleaning", 
                        Description = "Cleaning and sanitation tasks",
                        IsSystem = false
                    },
                    new TaskCategory 
                    { 
                        Name = "Administration", 
                        Description = "Administrative and record-keeping tasks",
                        IsSystem = false
                    }
                };

                await context.TaskCategories.AddRangeAsync(defaultCategories);
                await context.SaveChangesAsync();
            }
        }
    }
} 