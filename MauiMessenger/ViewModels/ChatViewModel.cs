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
  //[QueryProperty(nameof(Chat), "Chat")]
  //[QueryProperty(nameof(Messages), "Messages")]
  public partial class ChatViewModel : BaseViewModel
  //, IQueryAttributable
  {

    private readonly SignalRService _signalR;
    private readonly DataRepository _data;
    private readonly AppStateService _appState;
    private readonly ChatService _chatService;

    [ObservableProperty]
    private ChatDTO chat;

    public ObservableCollection<ChatItem> ChatItems { get; set; } = new();
    public List<int> Indexes { get; set; } = new();

    [ObservableProperty]
    private string message;

    public Command ChatHeaderClickedCommand { get; }
    public Command SendMessageCommand { get; }
    public Command BackButtonClickedCommand { get; }

    public UserDTO User { get => _appState.CurrentUser; }

    public Guid? LastReadMessageId;


    public event Action MessageSent;

    public event Action<Guid, Guid?> ChatClosed;


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
      SendMessageCommand = new Command(async () =>
      {
        await _chatService.SendMessageAsync(chat.Id, this.Message);
        Message = "";
        MessageSent?.Invoke();
        //await _data.UpdateChatAsync(chat.Id);
      });

      BackButtonClickedCommand = new Command(async () =>
      {
        ChatClosed?.Invoke(Chat.Id, LastReadMessageId);
        await Shell.Current.Navigation.PopModalAsync();
      });

      ChatClosed += async (chatId, lastReadMessageId) => { await _chatService.OnChatClosed(chatId, lastReadMessageId); };
    }



    //public void ApplyQueryAttributes(IDictionary<string, object> query)
    //{
    //  if (query.TryGetValue("Chat", out var value) && value is ChatDTO chat)
    //  {
    //    Chat = chat;
    //  }
    //}

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
      var ind = ChatItems.OfType<MessageItem>().ToList().IndexOf(ChatItems.OfType<MessageItem>().FirstOrDefault(m => m.Message.Id == chat.LastReadMessageId));
      return ind == -1 ? ind : Indexes[ind] + 1;
    }

    private CancellationTokenSource? _readPositionCts;
    private int _lastReadMessageIndex = -1; // индекс последнего прочитанного в Indexes (только сообщения)

    public async Task OnVisibleRangeChanged(int firstItem, int lastItem, DateTime readAt)
    {
      if (_readPositionCts != null)
      {
        _readPositionCts.Cancel();
        _readPositionCts.Dispose();
      }
      _readPositionCts = new CancellationTokenSource();
      var token = _readPositionCts.Token;

      try
      {
        await Task.Delay(300, token); // Debounce
      }
      catch (TaskCanceledException) { return; }

      // Ограничиваем lastItem индексом Indexes
      if (lastItem >= Indexes.Count) lastItem = Indexes.Count - 1;
      if (lastItem < 0) return;

      // Находим последнее сообщение среди видимых
      var indTuple = Indexes
          .Select((msgIndex, msgListIndex) => (msgIndex, msgListIndex));
      var lastMessageTuple = indTuple
          .LastOrDefault(t => t.msgListIndex <= lastItem);

      if (lastMessageTuple.msgListIndex == 0 && Indexes.Count > 0 && _lastReadMessageIndex >= lastMessageTuple.msgListIndex)
        return; // ничего нового

      if (lastMessageTuple.msgListIndex <= _lastReadMessageIndex)
        return; // пользователь прокрутил уже прочитанные

      // Обновляем позицию
      _lastReadMessageIndex = lastMessageTuple.msgListIndex;
      var lastMessage = ChatItems[Indexes[_lastReadMessageIndex]] as MessageItem;
      if (lastMessage == null) return;

      LastReadMessageId = lastMessage.Message.Id;
      await _chatService.UpdateChatReadPosition(Chat.Id, lastMessage.Message.Id, readAt);
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
