//using Microsoft.AspNetCore.SignalR.Client;
//using System.Collections.ObjectModel;
//using System.ComponentModel;
//using CommunityToolkit.Maui.Alerts;
//using CommunityToolkit.Maui.Extensions;
//using MauiMessenger.Services;
//using MauiMessenger.Models;
//using MauiMessenger.Views;
//using CommunityToolkit.Maui;
//using CommunityToolkit.Maui.Services;
//using CommunityToolkit.Maui.Views;
//using CommunityToolkit.Mvvm; // for ObservableObject

//namespace MauiMessenger.ViewModels
//{
//  public class ChattViewModel : INotifyPropertyChanged
//  {


//    Client apiClient { get; set; }

//    public UserDto User { get; set; }
//    public ChatDTO Chat { get; set; }

//    public Guid PrivateChatUserId(ChatDTO chat)
//    {
//      if (chat.Type.Id == PrivateTypeId)
//      {
//        return Chat.Members.First(m => m.UserId != User.UserId).UserId;
//      }
//      else
//      {
//        throw new Exception("Not private chat");
//      }
//    }

//    // to refactor -> add rest api request
//    public ChatDTO? TryGetChatWithUser(Guid userId)
//    {
//      return Chats.FirstOrDefault(c => c.Type.Id == PrivateTypeId && PrivateChatUserId(c) == userId);
//    }

//    public Guid CurrentPrivateChatUserId
//    {
//      get
//      {
//        return PrivateChatUserId(Chat);

//      }
//    }

//    public ContactDTO? TryGetContactOfUser(Guid userId)
//    {
//      return Contacts.FirstOrDefault(c => c.UserId == userId);
//    }

//    private ChatPage? _chatPage { get; set; }

//    private IPopupService _popupService { get; set; }

//    public string NewGroupName { get; set; }

//    public string Message { get; set; }

//    // список всех полученных сообщений
//    public ObservableCollection<MessageDto> Messages { get; } = new();
//    public ObservableCollection<ChatDTO> Chats { get; } = new();

//    public ObservableCollection<ContactDTO> Contacts { get; } = new();

//    public ObservableCollection<UserDto> SearchResults { get; } = new();
//    public ObservableCollection<object> SelectedGroupMembers { get; set; } = new();



//    // команда отправки сообщений
//    public Command SendMessageCommand { get; }

//    public Command AddContactCommand { get; }
//    public Command ToSearchPageCommand { get; }
//    public Command ToGroupCreationPopupCommand { get; }
//    public Command BackPageCommand { get; }

//    public Command ChatClickedCommand { get; }


//    public Command ContactClickedCommand { get; }
//    public Command CreatePrivateChatCommand { get; }
//    public Command CreateGroupChatCommand { get; }
//    public delegate void Notify();
//    public event Notify ClosePopupRequest;

//    public ChattViewModel(Client ApiClient)
//    {
//      apiClient = ApiClient;

//      _popupService = new PopupService();
//      IsConnected = false;    // по умолчанию не подключены
//      IsBusy = true;         // отправка сообщения не идет
//      Message = "";
//      //UserName = "";

//      SendMessageCommand = new Command(async () => await SendMessage(), () => true);
//      AddContactCommand = new Command<Guid>(async (userId) => await AddContact(userId), (userId) => true);
//      ToSearchPageCommand = new Command(ToSearchPage, () => true);
//      ToGroupCreationPopupCommand = new Command(ToGroupCreationPopup, () => true);
//      BackPageCommand = new Command(async () => await BackPage(), () => true);
//      ChatClickedCommand = new Command<Guid>(async (chatId) => await OnChatClicked(chatId), (chatId) => true);

//      ContactClickedCommand = new Command<Guid>(async (contactId) => await OnChatWithUserClicked(contactId));
//      CreatePrivateChatCommand = new Command<Guid>(async (userId) => await CreatePrivateChat(userId), (userId) => true);
//      CreateGroupChatCommand = new Command(async () => await CreateGroupChat(), () => true);


//    }

//    public async Task ToUserPage(Guid userId)
//    {
//      var contact = TryGetContactOfUser(userId);
//      await Shell.Current.Navigation.PushModalAsync(new UserPage(this, await apiClient.UsersAsync(userId, null, null), contact != null));

//    }

//    public async Task ToChatPage(ChatDTO chat)
//    {
//      await Shell.Current.Navigation.PushModalAsync(new ChatInfoPage(this, chat));
//    }
//    // go to chat with user or create if not exist
//    public async Task OnChatWithUserClicked(Guid userId)
//    {
//      try
//      {
//        var chat = TryGetChatWithUser(userId);
//        if (chat == null)
//        {
//          await CreatePrivateChat(userId);
//        }
//        else
//        {
//          await OnChatClicked(chat.Id);
//        }
//      }
//      catch (Exception ex)
//      {
//        Console.WriteLine(ex.Message);
//      }

//    }


//    public bool IsCurrentUserId(Guid userId)
//    { return userId == User.UserId; }

//    public bool IsIncomingMessage(object message)
//    {
//      if (message is MessageDto msg)
//      {
//        return msg.SenderId != User.UserId;
//      }
//      return false;
//    }
//    private string _searchQuery;
//    public string SearchQuery
//    {
//      get => _searchQuery;
//      set
//      {
//        if (_searchQuery != value)
//        {
//          _searchQuery = value;
//          OnPropertyChanged();
//          SearchUsersAsync(_searchQuery); // вызываем метод поиска
//        }
//      }
//    }

//    // идет ли отправка сообщений
//    bool isBusy;
//    public bool IsBusy
//    {
//      get => isBusy;
//      set
//      {
//        if (isBusy != value)
//        {
//          isBusy = value;
//          OnPropertyChanged("IsBusy");
//        }
//      }
//    }
//    // осуществлено ли подключение
//    bool isConnected;
//    public bool IsConnected
//    {
//      get => isConnected;
//      set
//      {
//        if (isConnected != value)
//        {
//          isConnected = value;
//          OnPropertyChanged("IsConnected");
//        }
//      }
//    }



//    public void ToSearchPage()
//    {
//      Shell.Current.Navigation.PushModalAsync(new SearchPage(this));
//    }

//    public void ToGroupCreationPopup()
//    {
//      _popupService.ShowPopupAsync<GroupCreationPopup>(Shell.Current);
//    }

//    public async Task ToChatInfoPage(ChatDTO chat)
//    {
//      if (chat.Type.Name == "group")
//      {
//        await ToChatPage(chat);
//      }
//      {
//        await ToUserPage(PrivateChatUserId(chat));
//      }
//    }

//    public async Task BackPage()
//    {
//      await Shell.Current.Navigation.PopModalAsync();
//    }


//    // подключение к чату
//    public async Task Connect()
//    {
//      if (IsConnected)
//        return;
//      try
//      {
//        await hubConnection.StartAsync();

//        IsConnected = true;
//        IsBusy = false;
//      }
//      catch (Exception ex)
//      {
//        SendLocalMessage(new MessageDto { Content = $"Ошибка подключения: {ex.Message}", CreatedAt = DateTime.UtcNow });
//      }
//    }

//    public async Task RegisterInHub()
//    {
//      if (!IsConnected) return;
//      try
//      {
//        await hubConnection.InvokeAsync("Register", User.UserId);
//      }
//      catch (Exception ex)
//      {

//      }
//    }
//    // Отключение от чата
//    public async Task Disconnect()
//    {
//      if (!IsConnected) return;

//      await hubConnection.StopAsync();
//      IsConnected = false;
//      SendLocalMessage(new MessageDto { Content = "Вы покинули...", CreatedAt = DateTime.UtcNow });

//    }

//    // Отправка сообщения
//    async Task SendMessage()
//    {
//      try
//      {
//        IsBusy = true;
//        var msg = new CreateMessageRequest
//        {
//          Message = this.Message,
//          ChatId = Chat.Id,
//          UserId = User.UserId
//        };


//        var response = await apiClient.MessagesPOSTAsync(msg);
//        await hubConnection.InvokeAsync("Send", response);
//      }
//      catch (Exception ex)
//      {
//        SendLocalMessage(new MessageDto { Content = $"Ошибка отправки: {ex.Message}", CreatedAt = DateTime.UtcNow });
//      }

//      IsBusy = false;
//    }

//    // Добавление сообщения
//    private void SendLocalMessage(MessageDto message)
//    {
//      try
//      {
//        MainThread.BeginInvokeOnMainThread(async () =>
//        {
//          Messages.Add(message);
//          if (_chatPage != null) { await _chatPage.ScrollMessagesToBottom(); }
//        });
//      }
//      catch (Exception ex)
//      {
//        Console.WriteLine("Ошибка " + ex.Message);
//      }
//    }

//    public async Task AddContact(Guid contactUserId)
//    {
//      try
//      {
//        IsBusy = true;
//        var contact = new CreateContactRequest
//        {
//          UserId = User.UserId,
//          ContactUserId = contactUserId,

//        };
//        var response = await apiClient.ContactsPOSTAsync(contact);

//        Contacts.Add(response);
//      }
//      catch (Exception ex)
//      {
//        Console.WriteLine("Ошибка " + ex.Message);
//      }
//      IsBusy = false;
//    }

//    private void AddLocalChat(ChatDTO chat)
//    {
//      try
//      {
//        MainThread.BeginInvokeOnMainThread(async () =>
//        {
//          if (Chats.FirstOrDefault(c => c.Id == chat.Id) == null)
//          {
//            Chats.Add(chat);
//          }
//        });
//      }
//      catch (Exception ex)
//      {
//        Console.WriteLine("Ошибка " + ex.Message);
//      }
//    }
//    public event PropertyChangedEventHandler PropertyChanged;
//    public void OnPropertyChanged(string prop = "")
//    {
//      if (PropertyChanged != null)
//        PropertyChanged(this, new PropertyChangedEventArgs(prop));
//    }

//    public void SetChat(Guid chatId)
//    {
//      this.Chat = this.Chats.FirstOrDefault(c => c.Id == chatId);
//    }

//    private async Task OnChatClicked(Guid chatId)
//    {
//      IsBusy = true;
//      SetChat(chatId);
//      var msgs = await apiClient.MessagesGETAsync(chatId);
//      Messages.Clear();
//      foreach (var msg in msgs.Messages)
//      {
//        Messages.Add(msg);
//      }
//      _chatPage = new ChatPage(this, Chat);
//      await Shell.Current.Navigation.PushModalAsync(_chatPage);
//      IsBusy = false;
//    }
//    public async Task<UserDto> Login(string username)
//    {
//      User = await apiClient.LoginAsync(username, "");
//      var userChats = (await apiClient.ChatsAsync(User.UserId)).Chats.ToList();
//      var userContacts = (await apiClient.ContactsAsync(User.UserId)).Contacts.ToList();

//      Chats.Clear();
//      return User;
//    }

//    public async Task<List<MessageDto>> GetChatMessages(Guid chatId)
//    {
//      List<MessageDto> messages = (await apiClient.MessagesGETAsync(chatId)).Messages.ToList() ?? [];
//      return messages;
//    }

//    public async Task PostMessage(string message)
//    {
//      await apiClient.MessagesPOSTAsync(new CreateMessageRequest { ChatId = Chat.Id, Message = message, UserId = User.UserId });
//    }


//    public async Task<UserDto> Login(string username)
//    {
//      User = await apiClient.LoginAsync(username, "");
//      var userChats = (await apiClient.ChatsAsync(User.UserId)).Chats.ToList();
//      var userContacts = (await apiClient.ContactsAsync(User.UserId)).Contacts.ToList();

//      Chats.Clear();
//      foreach (var chat in userChats)
//      {
//        if (chat.Type.Name == "private")
//        {
//          chat.Name = chat.Members.FirstOrDefault(m => m.UserId != User.UserId)?.Username ?? "Чат";
//        }
//        Chats.Add(chat);
//      }
//      return User;
//    }
//    private async Task SearchUsersAsync(string query)
//    {
//      if (string.IsNullOrEmpty(query))
//      {
//        SearchResults.Clear();
//      }
//      else
//      {
//        var results = (await apiClient.SearchAsync(query)).ToList();
//        SearchResults.Clear();
//        foreach (var user in results)
//          SearchResults.Add(user);
//      }
//    }

//    public async Task<ChatDTO> CreateGroupChat()
//    {

//      var members = SelectedGroupMembers.OfType<UserDto>().Select(u => new CreateMemberRequest { UserId = u.UserId, Role = MemberTypeId }).ToList();
//      members.Add(new CreateMemberRequest { UserId = User.UserId, Role = OwnerTypeId });
//      var chat = await apiClient.ChatAsync(

//        new CreateChatRequest
//        {
//          Name = NewGroupName,
//          Type = GroupTypeId,
//          CreatedBy = User.UserId,
//          Members = members
//        }
//      );



//      AddLocalChat(chat);
//      SetChat(chat.Id);
//      await hubConnection.InvokeAsync("NotifyChatCreated", chat.Id);
//      ClosePopupRequest.Invoke();
//      await Shell.Current.Navigation.PushModalAsync(new ChatPage(this, Chat));
//      return chat;
//    }
//    public async Task<ChatDTO> CreatePrivateChat(Guid userId)
//    {
//      var chat = await apiClient.ChatAsync(
//      new CreateChatRequest
//      {
//        Name = "",
//        Type = PrivateTypeId,
//        CreatedBy = User.UserId,
//        Members = new List<CreateMemberRequest>
//        {
//            new CreateMemberRequest { UserId = User.UserId, Role = MemberTypeId},
//            new CreateMemberRequest { UserId = userId, Role = MemberTypeId }
//        }
//      }
//    );
//      chat.Name = chat.Members.FirstOrDefault(m => m.UserId != User.UserId)?.Username ?? "Чат";
//      AddLocalChat(chat);
//      SetChat(chat.Id);
//      await hubConnection.InvokeAsync("NotifyChatCreated", chat.Id);
//      await Shell.Current.Navigation.PushModalAsync(new ChatPage(this, Chat));
//      return chat;
//    }
//  }

//}