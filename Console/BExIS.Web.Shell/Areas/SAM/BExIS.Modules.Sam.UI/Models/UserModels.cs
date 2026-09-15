using BExIS.Security.Entities.Subjects;
using BExIS.UI.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace BExIS.Modules.Sam.UI.Models
{
    public class UserModel
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("userName")]
        public string UserName { get; set; }

        [JsonProperty("registrationDate")]
        public DateTime RegistrationDate { get; set; }

        [JsonProperty("modificationDate")]
        public DateTime ModificationDate { get; set; }

        public static UserModel Convert(User user)
        {
            return new UserModel()
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                RegistrationDate = user.RegistrationDate,
                ModificationDate = user.ModificationDate
            };
        }
    }

    public class ReadUserModel
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("userName")]
        public string UserName { get; set; }

        [JsonProperty("registrationDate")]
        public DateTime RegistrationDate { get; set; }

        [JsonProperty("modificationDate")]
        public DateTime ModificationDate { get; set; }

        [JsonProperty("groupIds")]
        public List<long> GroupIds { get; set; }

        public static ReadUserModel Convert(User user)
        {
            return new ReadUserModel()
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                RegistrationDate = user.RegistrationDate,
                ModificationDate = user.ModificationDate,
                GroupIds = user.Groups.Select(g => g.Id).ToList()
            };
        }
    }

    public class CreateUserModel
    {
        [Remote("ValidateEmail", "Users")]
        [Required]
        [JsonProperty("email")]
        public string Email { get; set; }

        [Remote("ValidateUsername", "Users")]
        [Required]
        [JsonProperty("userName")]
        public string UserName { get; set; }

        [JsonProperty("groupIds")]
        public List<long> GroupIds { get; set; }

        public CreateUserModel()
        {
            Email = "";
            UserName = "";
            GroupIds = new List<long>();
        }

    }

    public class DeleteUserModel
    {
        public string Email { get; set; }
        public string UserName { get; set; }
    }

    public class UpdateUserModel
    {
        [Remote("ValidateEmail", "Users", AdditionalFields = "Id")]
        [Required]
        public string Email { get; set; }

        public long Id { get; set; }

        [Remote("ValidateUsername", "Users", AdditionalFields = "Id")]
        public string UserName { get; set; }

        [JsonProperty("groupIds")]
        public List<long> GroupIds { get; set; }

        [JsonProperty("registrationDate")]
        public DateTimeOffset RegistrationDate { get; set; }

        [JsonProperty("modificationDate")]
        public DateTimeOffset ModificationDate { get; set; }

        public static UpdateUserModel Convert(User user)
        {
            return new UpdateUserModel()
            {
                Email = user.Email,
                Id = user.Id,
                UserName = user.Name,
                RegistrationDate = user.RegistrationDate,
                ModificationDate = user.ModificationDate,
                GroupIds = user.Groups.Select(g => g.Id).ToList()
            };
        }
    }

    public class UserGridRowModel
    {
        public string Email { get; set; }
        public long Id { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string IsEmailConfirmed { get; set; }
        public DateTime RegistrationDate { get; set; }

        public static UserGridRowModel Convert(User user)
        {
            return new UserGridRowModel()
            {
                Email = user.Email,
                Id = user.Id,
                Name = user.Name,
                DisplayName = user.DisplayName,
                IsEmailConfirmed = user.IsEmailConfirmed.ToString(),
                RegistrationDate = user.RegistrationDate
            };
        }
    }

    public class UserMembershipGridRowModel
    {
        public string Email { get; set; }
        public long Id { get; set; }
        public bool IsUserInGroup { get; set; }
        public string Name { get; set; }

        public static UserMembershipGridRowModel Convert(User user, string groupName)
        {
            return new UserMembershipGridRowModel()
            {
                Email = user.Email,
                Id = user.Id,
                IsUserInGroup = user.Groups.Any(g => g.Name.ToUpperInvariant() == groupName.ToUpperInvariant()),
                Name = user.Name
            };
        }
    }
}