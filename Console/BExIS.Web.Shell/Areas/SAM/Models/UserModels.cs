using BExIS.Security.Entities.Subjects;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace BExIS.Modules.Sam.UI.Models
{
    public static class UserExtensions
    {
        public static IQueryable<UserGridRowModel> ToUserGridRowModel(this IQueryable<User> source)
        {
            Expression<Func<User, UserGridRowModel>> conversion = u =>
                new UserGridRowModel()
                {
                    Email = u.Email,
                    Id = u.Id,
                    IsAdministrator = u.IsAdministrator,
                    UserName = u.Name
                };

            return source.Select(conversion);
        }

        public static IQueryable<UserMembershipGridRowModel> ToUserMembershipGridRowModel(this IQueryable<User> source)
        {
            Expression<Func<User, UserMembershipGridRowModel>> conversion = u =>
                new UserMembershipGridRowModel()
                {
                    Email = u.Email,
                    Id = u.Id,
                    Username = u.Name,
                };

            return source.Select(conversion);
        }
    }

    public class CreateUserModel
    {
        public string Email { get; set; }
        public bool IsAdministrator { get; set; }
        public string UserName { get; set; }
    }

    public class DeleteUserModel
    {
        public string Email { get; set; }
        public bool IsAdministrator { get; set; }
        public string UserName { get; set; }
    }

    public class UserGridRowModel
    {
        public string Email { get; set; }
        public long Id { get; set; }
        public bool IsAdministrator { get; set; }
        public string UserName { get; set; }

        public static UserGridRowModel Convert(User user)
        {
            return new UserGridRowModel()
            {
                Email = user.Email,
                Id = user.Id,
                IsAdministrator = user.IsAdministrator,
                UserName = user.Name
            };
        }
    }

    public class UserMembershipGridRowModel
    {
        public string Email { get; set; }
        public string FullName { get; set; }
        public long Id { get; set; }
        public string Username { get; set; }

        public static UserMembershipGridRowModel Convert(User user)
        {
            return new UserMembershipGridRowModel()
            {
                Id = user.Id,
                Username = user.Name,
                Email = user.Email
            };
        }
    }

    public class UserFeaturePermissionGridRowModel
    {
        public string Email { get; set; }
        public string FullName { get; set; }
        public long Id { get; set; }
        public string Username { get; set; }
        public bool EffectiveRight { get; set; }
        public int FeaturePermissionType { get; set; }

        public static UserMembershipGridRowModel Convert(User user, int featurePermissionType, bool effectiveRight)
        {
            return new UserMembershipGridRowModel()
            {
                Id = user.Id,
                Username = user.Name,
                Email = user.Email
            };
        }
    }
}