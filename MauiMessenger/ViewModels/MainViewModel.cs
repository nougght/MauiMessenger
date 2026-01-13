using CommunityToolkit.Mvvm.DependencyInjection;
using MauiMessenger.ApiClient;
using MauiMessenger.Models;
using MauiMessenger.Services;
using MauiMessenger.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MauiMessenger.ViewModels
{
  public partial class MainViewModel : BaseViewModel
  {
    private readonly SignalRService _signalR;
    private readonly Client _api;
    private readonly DataRepository _data;
    private readonly AppStateService _appState;
    private readonly ChatService _chatService;
    private readonly AuthService _authService;


    public MainViewModel(SignalRService signalR, Client apiClient,
      DataRepository data, AppStateService state, ChatService chatService,
      AuthService authService)
    {
      _signalR = signalR;
      _api = apiClient;
      _data = data;
      _appState = state;
      _chatService = chatService;
      _authService = authService;


      // trying without mainthread invoke
      _signalR.OnChatCreated += async chat =>
      {
        //if (chat.Type.Id == _data.PrivateTypeId)
        //{
        //  chat.Name = chat.Members.FirstOrDefault(m => m.UserId != _appState.CurrentUser.UserId)?.Username ?? "Чат";
        //}
        await _data.AddChat(chat);

      };
      _signalR.OnMessageReceived += async message =>
      {
        if (message.SenderId != _appState.CurrentUser.UserId)
        {

          foreach (var file in message.Files)
          {
            var url = await _api.PresignedUrlGETAsync(file.FileKey);
            file.URL = url;

          }
          await _data.AddMessage(message);

          //await _data.UpdateChatAsync(message.ChatId);
        }
      };

      // mark our sent messages as read
      _signalR.OnUpdateReadStatuses += async (chatId, userId, readPositionId, readAt) =>
      {
        if (userId != _appState.CurrentUser.UserId)
        {
          await _data.MarkMessagesRead(chatId, userId, readPositionId, readAt);
        }
      };
    }



    public async Task TryEnter()
    {
      try
      {

        await _appState.LoadTokenAndUserId();

        if (_appState.RefreshToken != null && _appState.SavedUserId != null)
        {
          var accessToken = await _api.RefreshAsync(_appState.RefreshToken, _appState.SavedUserId);
          if (!string.IsNullOrEmpty(accessToken))
          {
            _appState.SetAccessToken(accessToken);
            var user = await _api.UserAsync(_appState.SavedUserId);
            await _authService.InitSession(accessToken!, _appState.RefreshToken, user!);
            Application.Current.MainPage = new AppShell();
            return;
          }
        }

      }
      catch (Exception ex)
      {
        throw;
      }
    }
  }
}
