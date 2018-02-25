using System.Collections.Generic;
using PQT.Domain.Entities;

namespace PQT.Domain.Abstract
{
    public interface IAuthenticationService
    {
        User ValidateLogin(string username, string password);
    }
}