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

    public async Task<UserDTO> TrySignIn(string  username)
    {
      var user = await _api.LoginAsync(username, "");
      _appState.CurrentUser = user;
      await _data.LoadChatsAsync();
      await _data.LoadContactsAsync();
      await _signalR.Connect();
      await _signalR.RegisterInHub(user.UserId);
      return user;

    }
  }
}
