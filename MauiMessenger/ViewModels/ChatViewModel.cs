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
  public partial class ChatViewModel : BaseViewModel, IQueryAttributable
  {

    private readonly SignalRService _signalR;
    private readonly DataRepository _data;
    private readonly AppStateService _appState;
    private readonly ChatService _chatService;

    [ObservableProperty]
    private ChatDTO chat;

    public ObservableCollection<MessageDTO> Messages { get; set; } = new();

    [ObservableProperty]
    private string message;

    public Command ChatHeaderClickedCommand { get; }
    public Command SendMessageCommand { get; }

    public UserDTO User { get => _appState.CurrentUser;}


    public event Action MessageSent; 


    public ChatViewModel(SignalRService signalR, Client apiClient, ChatService chatService,
      AppStateService state, DataRepository data)
    {
      _signalR = signalR;
      _data = data;
      _appState = state;
      _chatService = chatService;
      //foreach (var message in Messages)
      //{
      //  ;

      //}

      ChatHeaderClickedCommand = new Command(async () => await OnChatHeaderClicked(chat), () => true);
      SendMessageCommand = new Command(async() => 
      {
        await _chatService.SendMessageAsync(chat.Id, this.Message);
        Message = "";
        MessageSent?.Invoke();
        //await _data.UpdateChatAsync(chat.Id);
      });
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
      if (query.TryGetValue("Chat", out var value) && value is ChatDTO chat)
      {
        Chat = chat;
      }
    }

    //public void OnAppearing()
    //{
    //  if (Chat != null)
    //  {

    //    MainThread.BeginInvokeOnMainThread(async () =>
    //    {
    //      Messages = _chatService.GetMessages(chat.Id);
    //    });

    //  }
    //}
    //partial void OnChatChanged(ChatDTO value)
    //{
    //  MainThread.BeginInvokeOnMainThread(async () =>
    //  {
    //    Messages = _chatService.GetMessages(chat.Id);
    //  });

    //}

    public int GetReadPosition()
    {
      return Messages.IndexOf(Messages.FirstOrDefault(m => m.Id == chat.LastReadMessageId)) + 1;
    }

    public async Task OnVisibleRangeChanged(int firstItem, int lastItem)
    {
      var lastMessage = Messages[lastItem];
      if (!lastMessage.IsRead)
      {
        //_data.UpdateChatReadPosition(chat.Id, lastMessage.Id);
        //await Task.Delay(300);
        await _chatService.UpdateChatReadPosition(chat.Id, lastMessage.Id);

      }
    }


    private async Task OnChatHeaderClicked(ChatDTO chat)
    {
      if (chat.Type.Id == _chatService.GroupTypeId)
      {
        // go to chat page
        await NavigationService.GoToChatInfoPageAsync(chat);
      }
      else
      {
        // go to user page
        var user = await _data.GetUserById(_data.PrivateChatUserId(chat));
        await NavigationService.GoToUserPageAsync(user);
      }

    }


  }
}
