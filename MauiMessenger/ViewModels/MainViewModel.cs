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
    private readonly ChatService _chatService;


    public MainViewModel(SignalRService signalR, Client apiClient,
      DataRepository data, AppStateService state, ChatService chatService)
    {
      _signalR = signalR;
      _api = apiClient;
      _data = data;
      _appState = state;
      _chatService = chatService;


      // trying without mainthread invoke
      _signalR.OnChatCreated += async chat =>
      {
        if (chat.Type.Id == _data.PrivateTypeId)
        {
          chat.Name = chat.Members.FirstOrDefault(m => m.UserId != _appState.CurrentUser.UserId)?.Username ?? "Чат";
        }
        await _data.AddChat(chat);

      };
      _signalR.OnMessageReceived += async message =>
      {
        if (message.SenderId != _appState.CurrentUser.UserId)
        {
          
          await _data.AddMessage(message);
          await _data.UpdateChatAsync(message.ChatId);
        }
      };

      _signalR.OnUpdateReadStatuses += _chatService.
    }
  }
}
