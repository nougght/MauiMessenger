using CommunityToolkit.Mvvm.ComponentModel;
using MauiMessenger.ApiClient;
using MauiMessenger.Models;
using MauiMessenger.Services;
using Microsoft.Maui.Dispatching;
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


    [NotifyPropertyChangedFor(nameof(IsPrivate))]
    [ObservableProperty]
    private ConversationDTO conversation;

    [ObservableProperty]
    private GroupChatDetailsDTO? groupChatDetails;

    [ObservableProperty]
    private PrivateChatDetailsDTO? privateChatDetails;

    [ObservableProperty]
    private ChannelDetailsDTO? channelDetails;

    public ObservableCollection<ChatItem> ChatItems { get; set; } = new();

  
    public ObservableCollection<string> Suggestions { get; private set; } = new();



    public List<int> Indexes { get; set; } = new();


    [NotifyPropertyChangedFor(nameof(IsAudioMode))]
    [NotifyPropertyChangedFor(nameof(IsMessageMode))]
    [ObservableProperty]
    private string message;

    public ObservableCollection<FileResult> Files { get; set; } = new();


    [ObservableProperty]
    private TimeSpan recordDuration = TimeSpan.Zero;

    private IDispatcher _recordTimer;


    [NotifyPropertyChangedFor(nameof(AudioIsNotRecording))]
    [ObservableProperty]
    private bool isAudioRecording;

    public bool AudioIsNotRecording => !IsAudioRecording;

    public bool IsAudioMode => IsAudioRecording || string.IsNullOrEmpty(message);

    public bool IsMessageMode => !IsAudioMode;

    public string? AudioFilePath { get; set; }

    public int? CurrentSuggestionIndex { get; set; }
    public bool IsSuggestionsVisible => Suggestions.Count > 0;
    public bool IsSuggestionsButtonVisible => Suggestions.Count == 0;

    public Command AiSuggestionsClickedCommand { get; }
    public Command UseSuggestionClickedCommand { get; }
    public Command ChatHeaderClickedCommand { get; }
    public Command PickFileCommand { get; }
    public Command RecordClickedCommand { get; }
    public Command SendMessageCommand { get; }
    public Command BackButtonClickedCommand { get; }

    public UserDTO User { get => _appState.CurrentUser; }

    public Guid? LastReadMessageId;

    [ObservableProperty]
    private UserStatus? status;

    public string StatusText { get; set; } = "";

    public event Action MessageSent;

    public event Action<Guid, Guid?> ChatClosed;



    public event Action<string> Alert;



    public int LastReadMessageIndex; // индекс последнего прочитанного в Indexes (только сообщения)

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

      AiSuggestionsClickedCommand = new Command(async () => await OnAiSuggestionsClicked(), () => true);
      UseSuggestionClickedCommand = new Command<string>(async (choise) => await OnUseSuggestionClicked(choise), (choise) => true);
      ChatHeaderClickedCommand = new Command(async () => await OnChatHeaderClicked(conversation, groupChatDetails), () => true);
      PickFileCommand = new Command(async () =>
      {
        await OnPickFileAsync();
      });
      //RecordClickedCommand = new Command(() => {
      //  if (IsAudioRecording == false)
      //  {

      //  }
      //}, () => true);
      SendMessageCommand = new Command(async () =>
      {
        await _chatService.SendMessageAsync(conversation.Id, this.Message, this.Files.ToList(), audioPath: AudioFilePath);
        Message = "";
        MessageSent?.Invoke();
        //await _data.UpdateChatAsync(chat.Id);
      });

      BackButtonClickedCommand = new Command(async () =>
      {
        ChatClosed?.Invoke(Conversation.Id, LastReadMessageId);
        await Shell.Current.Navigation.PopModalAsync();
      });

      ChatClosed += async (chatId, lastReadMessageId) => { await _chatService.OnChatClosed(chatId, lastReadMessageId); };
      MessageSent += () => Suggestions.Clear();
      MessageSent += () => Files.Clear();
      Suggestions.CollectionChanged += (_, __) =>
      {
        OnPropertyChanged(nameof(IsSuggestionsVisible));
        OnPropertyChanged(nameof(IsSuggestionsButtonVisible));

      };
    }

    public bool IsPrivate => Conversation == default(ConversationDTO) ? false : Conversation.Type.Name == "private";

    
    private void StartRecordingTimer()
    {
      RecordDuration = TimeSpan.Zero;

      _recordTimer = Dispatcher.GetForCurrentThread();

      _recordTimer.StartTimer(TimeSpan.FromSeconds(1), () =>
      {
        if (!IsAudioRecording)
          return false;

        RecordDuration = RecordDuration.Add(TimeSpan.FromSeconds(1));

        return IsAudioRecording; // продолжать таймер, пока идет запись
      });

    }
    private void StopRecordingTimer()
    {
      IsAudioRecording = false;
    }


    public async Task OnPressed()
    {
      var IsOk = await FileService.StartRecorder();
      if (IsOk)
      {
        IsAudioRecording = true;
        StartRecordingTimer();
      }
      else
      {
        Alert?.Invoke("Запись голосовых сообщений доступна только на мобильных устройствах");
      }
    }

    public async Task OnReleased()
    {
      if (IsAudioRecording)
      {
        await FileService.StopRecorder();
        StopRecordingTimer();
        AudioFilePath = FileService.GetRecordedFilePath();
        await _chatService.SendMessageAsync(Conversation.Id, this.Message, this.Files.ToList(), audioPath: AudioFilePath);
      }
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

    partial void OnPrivateChatDetailsChanged(PrivateChatDetailsDTO? value)
    {
      MainThread.BeginInvokeOnMainThread(async () =>
      {
        if (IsPrivate && privateChatDetails != null)
        {
          Status = _appState.GetUserStatusById(value.OtherUser.UserId);
          if (Status == null)
          {
            await _data.LoadUserStatuseseAsync(new List<Guid> { value.OtherUser.UserId });
            Status = _appState.GetUserStatusById(value.OtherUser.UserId);

          }
        }
      });
    }


    //}

    public int GetReadPosition()
    {
      var ind = ChatItems.OfType<MessageItem>().ToList().IndexOf(ChatItems.OfType<MessageItem>().FirstOrDefault(m => m.Message.Id == Conversation.LastReadMessageId));
      return ind == -1 ? ind : Indexes[ind] + 1;
    }

    private CancellationTokenSource? _readPositionCts;

    public async Task OnVisibleRangeChanged(int firstItem, int lastItem, DateTime readAt)
    {
      try
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
        if (lastItem >= ChatItems.Count) lastItem = ChatItems.Count - 1;
        if (lastItem < 0) return;

        var i = LastReadMessageIndex == -1 ? 0 : LastReadMessageIndex;
        while (i <= lastItem && (!(ChatItems[i] is MessageItem msg) || msg.Message.SenderId == _appState.CurrentUser.UserId))
        {
          ++i;
        }
        // если не оказалось входящих сообщений в области  видимости
        if (i > lastItem)
        {
          return;
        }
        // Находим последнее сообщение среди видимых
        var indTuple = Indexes
            .Select((msgIndex, msgListIndex) => (msgIndex, msgListIndex));
        var lastMessageTuple = indTuple
            .LastOrDefault(t => t.msgListIndex <= lastItem);

        if (lastMessageTuple.msgListIndex == 0 && Indexes.Count > 0 && LastReadMessageIndex >= lastMessageTuple.msgListIndex)
          return; // ничего нового

        if (lastMessageTuple.msgListIndex <= LastReadMessageIndex)
          return; // пользователь прокрутил уже прочитанные

        // Обновляем позицию
        LastReadMessageIndex = lastMessageTuple.msgListIndex;
        var lastMessage = ChatItems[Indexes[LastReadMessageIndex]] as MessageItem;
        if (lastMessage == null) return;

        LastReadMessageId = lastMessage.Message.Id;
        await _chatService.UpdateChatReadPosition(Conversation.Id, lastMessage.Message.Id, readAt);
      }
      catch (Exception ex)
      {
        throw;
      }
    }




    private async Task OnPickFileAsync()
    {
      var res = await FileService.PickFileAsync();
      await MainThread.InvokeOnMainThreadAsync(() =>
      {
        foreach (var file in res)
        {
          Files.Add(file);
        }
      });
    }

    private async Task OnUseSuggestionClicked(string choice)
    {
      await MainThread.InvokeOnMainThreadAsync(async () =>
      {
        Message = choice;
        Suggestions.Clear();
      });
    }
    private async Task OnAiSuggestionsClicked()
    {
      var suggestions = await _chatService.GetAiSuggestions(this.Conversation.Id, this.Message);
      await MainThread.InvokeOnMainThreadAsync(async () =>
      {
        Suggestions.Clear();
        foreach (var suggestion in suggestions)
        {
          Suggestions.Add(suggestion);
        }
        CurrentSuggestionIndex = 0;
      });

    }
    private async Task OnChatHeaderClicked(ConversationDTO chat, GroupChatDetailsDTO details)
    {
      if (chat.Type.Id == _chatService.GroupTypeId)
      {
        // go to chat page
        await NavigationService.GoToChatInfoPageAsync(chat, details);
      }
      else
      {
        // go to user page
        var user = await _data.GetUser(privateChatDetails.OtherUser.UserId);
        await NavigationService.GoToUserPageAsync(user);
      }

    }


  }
}
