using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MauiMessenger.Models;


namespace MauiMessenger.Services
{
  public class AuthService
  {
    readonly DataRepository _data;
    readonly Client _api;

    public AuthService(DataRepository data, Client api)
    {
      _data = data;
      _api = api;
    }

    public async Task<UserDTO> TrySignIn(string  username)
    {
      var user = await _api.LoginAsync(username, "");
      return user;

    }
  }
}
