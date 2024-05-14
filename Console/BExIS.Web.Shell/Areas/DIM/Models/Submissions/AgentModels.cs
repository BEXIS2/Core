using BExIS.Dim.Entities.Submissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Modules.Dim.UI.Models.Submissions
{
    public class CreateAgentModel
    {
        public string Name { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public long RepositoryId { get; set; }
    }

    public class ReadAgentModel
    {
        public string Name { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public long RepositoryId { get; set; }
    }

    public class ReadGridRowAgentModel
    {
        public string Name { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public long RepositoryId { get; set; }

        public static ReadGridRowAgentModel Convert(Agent agent)
        {
            return new ReadGridRowAgentModel()
            {
                Name = agent.Name,
                Username = agent.Username,
                Password = agent.Password,
                RepositoryId = agent.Repository.Id
            };
        }
    }

    public class UpdateAgentModel
    {

    }
}