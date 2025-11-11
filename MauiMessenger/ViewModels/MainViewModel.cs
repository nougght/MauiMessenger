using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using MauiMessenger.Services;
using MauiMessenger.Models;

namespace MauiMessenger.ViewModels
{
  public partial class MainViewModel : BaseViewModel
  {
    private readonly SignalRService _signalR;
    private readonly Client _api;
    private readonly DataRepository _data;
    private readonly AppStateService _appState;


    public MainViewModel(SignalRService signalR, Client apiClient,
      DataRepository data, AppStateService state)
    {
      _signalR = signalR;
      _api = apiClient;
      _data = data;
      _appState = state;


      // trying without mainthread invoke
      _signalR.OnChatCreated += async chat =>  await _data.AddChat(chat);
      _signalR.OnMessageReceived += async message => await _data.AddMessage(message);

    }



  }
}
