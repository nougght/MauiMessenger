using CommunityToolkit.Mvvm.ComponentModel;
using MauiMessenger.Models;
using MauiMessenger.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiMessenger.ViewModels
{
  [QueryProperty(nameof(Chat), "Chat")]
  //[QueryProperty(nameof(Messages), "Messages")]
  public partial class ChatViewModel : BaseViewModel
  {

    private readonly SignalRService _signalR;
    private readonly Client _api;
    private readonly AppStateService _appState;
    private readonly ChatService _chatService;

    [ObservableProperty]
    private ChatDTO chat;

    public ObservableCollection<MessageDTO> Messages { get; private set; }

    [ObservableProperty]
    private string message;

    public Command ChatHeaderClickedCommand { get; }
    public Command SendMessageCommand { get; }

    public UserDTO User { get => _appState.CurrentUser;}

    public ChatViewModel(SignalRService signalR, Client apiClient, ChatService chatService,
      AppStateService state)
    {
      _signalR = signalR;
      _api = apiClient;
      _appState = state;
      _chatService = chatService;

      ChatHeaderClickedCommand = new Command(async () => await OnChatHeaderClicked(chat), () => true);
      SendMessageCommand = new Command(async() => { message = ""; await _chatService.SendMessageAsync(chat.Id, this.Message); });
    }
    
    partial void OnChatChanged(ChatDTO value)
    {
      Messages = _chatService.GetMessages(chat.Id);

    }

    private async Task OnChatHeaderClicked(ChatDTO chat)
    {
      if (chat.Type.Id == _chatService.GroupTypeId)
      {
        // go to chat page
      }
      else
      {
        // go to user page
      }

    }


  }
}
