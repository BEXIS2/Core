using Microsoft.AspNet.Identity;
using System;
using System.Threading.Tasks;

namespace BExIS.Security.Services.Utilities
{
    public class SmsService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            throw new NotImplementedException();
        }
    }
}