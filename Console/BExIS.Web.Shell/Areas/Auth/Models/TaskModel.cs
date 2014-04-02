using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BExIS.Security.Entities.Objects;

namespace BExIS.Web.Shell.Areas.Auth.Models
{
    public class TaskModel
    {
        public long Id { get; set; }

        public string TaskName { get; set; }

        public string Description { get; set; }

        public static TaskModel Convert(Task task)
        {
            return new TaskModel()
            {
                Id = task.Id,
                TaskName = task.Name,
                Description = task.Description
            };
        }
    }
}