using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MauiMessenger.Models;
using MauiMessenger.ApiClient;


namespace MauiMessenger.Services
{
  public class AuthService
  {
    readonly DataRepository _data;
    readonly AppStateService _appState;
    readonly SignalRService _signalR;
    readonly Client _api;

    public AuthService(DataRepository data, Client api, AppStateService appState, SignalRService signalR)
    {
      _data = data;
      _api = api;
      _appState = appState;
      _signalR = signalR;
    }

    public async Task<UserDTO> TrySignIn(string  username, string password)
    {
      var response = await _api.LoginAsync(username, password);
      var user = response.User;
      await _appState.SetSession(response.AccessToken, response.RefreshToken, response.User);

      await _data.LoadChatsAsync();
      await _data.LoadContactsAsync();
      await _signalR.Connect();
      await _signalR.RegisterInHub(user.UserId);
      return user;

    }


  }
}
