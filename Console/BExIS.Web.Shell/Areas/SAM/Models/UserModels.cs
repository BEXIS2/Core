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
                    UserName = u.UserName
                };

            return source.Select(conversion);
        }
    }

    public class UserGridRowModel
    {
        public string Email { get; set; }
        public long Id { get; set; }
        public string UserName { get; set; }

        public static UserGridRowModel Convert(User user)
        {
            return new UserGridRowModel()
            {
                Email = user.Email,
                Id = user.Id,
                UserName = user.UserName
            };
        }
    }
}