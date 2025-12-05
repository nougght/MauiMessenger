using CommunityToolkit.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;
using MauiMessenger.Models;
using MauiMessenger.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MauiMessenger.Models
{
  public class DataRepository : ObservableObject
  {
    private readonly Client _api;
    private readonly AppStateService _appState;

    public ObservableCollection<ChatDTO> Chats { get; } = new();
    public ObservableCollection<UserDTO> Users { get; } = new();


    public ObservableCollection<ContactDTO> Contacts { get; } = new();


    public async Task<UserDTO> GetUserById(Guid? userId)
    {

      var existing = Users.FirstOrDefault(u => u.UserId == userId);
      if (existing == null)
      {
        UserDTO response = await _api.UserAsync(userId);
        UpdateCollection(Users, Enumerable.Repeat(response, 1), (a, b) => a.UserId == b.UserId);
        return response;
      }
      return existing;
    }

    //public async Task<UserDTO?> GetUserByUsername(string username)
    //{
    //  var existing = Users.FirstOrDefault(u => u.Username == username);
    //  if (existing != null)
    //  {
    //    return existing;
    //  }
    //  else {
    //    var response = await _api.UserAsync()
    //}
    public ContactDTO? GetContactByUser(Guid userId)
    {
      return Contacts.FirstOrDefault(c => c.UserId == userId);
    }

    public Guid PrivateChatUserId(ChatDTO chat)
    {
      if (chat.Type.Id == PrivateTypeId)
      {
        return chat.Members.First(m => m.UserId != _appState.CurrentUser.UserId).UserId;
      }
      else
      {
        throw new Exception("Not private chat");
      }
    }

    public ChatDTO? GetChatWithUser(Guid userId)
    {
      return Chats.FirstOrDefault(c => c.Type.Id == PrivateTypeId && PrivateChatUserId(c) == userId);
    }



    public readonly Dictionary<Guid, ObservableCollection<MessageDTO>> _messagesByChat = new();


    public ChatDTO? GetChat(Guid id) => Chats.FirstOrDefault(c => c.Id == id);

    public async Task<ObservableCollection<MessageDTO>> GetMessages(Guid chatId)
    {
      ObservableCollection<MessageDTO>? messages;

      _messagesByChat.TryGetValue(chatId, out messages);
      if (messages == null)
      {
        await MainThread.InvokeOnMainThreadAsync(async () =>
          {
            messages = new ObservableCollection<MessageDTO>();
            _messagesByChat[chatId] = messages;
          }
        );
      }
      return messages;
    }

    public Dictionary<Guid, string> ChatTypes { get; private set; } = new();
    public Dictionary<Guid, string> MemberRoles { get; private set; } = new();



    public Guid MemberTypeId { get; private set; }
    public Guid AdminTypeId { get; private set; }
    public Guid OwnerTypeId { get; private set; }


    public Guid PrivateTypeId { get; private set; }
    public Guid GroupTypeId { get; private set; }


    public static void UpdateCollection<T>(
      ObservableCollection<T> target,
      IEnumerable<T> source,
      Func<T, T, bool> comparer)
    {
      // Удаляем отсутствующие
      var toRemove = target.Where(t => !source.Any(s => comparer(t, s))).ToList();
      foreach (var item in toRemove)
        target.Remove(item);

      // Добавляем новые
      var toAdd = source.Where(s => !target.Any(t => comparer(t, s))).ToList();
      foreach (var item in toAdd)
        target.Add(item);
    }

    public DataRepository(Client api, AppStateService state)
    {
      _api = api;
      _appState = state;
    }






    public async Task LoadEnumsAsync()
    {
      ChatTypes = (await _api.ChatTypesAsync()).ToDictionary().Select(p => new KeyValuePair<Guid, string>(new Guid(p.Key), p.Value)).ToDictionary();
      MemberRoles = (await _api.MemberRolesAsync()).ToDictionary().Select(p => new KeyValuePair<Guid, string>(new Guid(p.Key), p.Value)).ToDictionary();

      PrivateTypeId = ChatTypes.FirstOrDefault(r => r.Value == "private").Key;

      if (PrivateTypeId == Guid.Empty)
        throw new InvalidOperationException("ChatType 'private' not found");


      GroupTypeId = ChatTypes.FirstOrDefault(r => r.Value == "group").Key;

      if (GroupTypeId == Guid.Empty)
        throw new InvalidOperationException("ChatType 'group' not found");



      OwnerTypeId = MemberRoles.FirstOrDefault(r => r.Value == "owner").Key;

      if (OwnerTypeId == Guid.Empty)
        throw new InvalidOperationException("MemberRole 'owner' not found");


      AdminTypeId = MemberRoles.FirstOrDefault(r => r.Value == "admin").Key;

      if (AdminTypeId == Guid.Empty)
        throw new InvalidOperationException("MemberRole 'admin' not found");


      MemberTypeId = MemberRoles.FirstOrDefault(r => r.Value == "member").Key;

      if (MemberTypeId == Guid.Empty)
        throw new InvalidOperationException("MemberRole 'member' not found");

    }

    public async Task LoadChatsAsync()
    {
      if (_appState.CurrentUser != null)
      {
        var userChats = (await _api.ChatsAsync(_appState.CurrentUser.UserId)).Chats;
        Chats.Clear();

        foreach (var chat in userChats)
        {
          if (chat.Type.Id == PrivateTypeId)
          {
            chat.Name = chat.Members.FirstOrDefault(m => m.UserId != _appState.CurrentUser.UserId)?.Username ?? "Чат";
          }
          Chats.Add(chat);
        }
      }
      else
      {
        await Application.Current.MainPage.DisplayAlert("Внимание", "LoadChatsAsync - CurrentUser == null", "OK");

      }
    }

    public async Task<ChatDTO> UpdateChatAsync(Guid chatId)
    {
      var existing = GetChat(chatId);
      var ind = Chats.IndexOf(existing);

      var updated = await _api.ChatGETAsync(chatId, _appState.CurrentUser.UserId);
      updated.Name = existing.Name;
      await MainThread.InvokeOnMainThreadAsync(() => Chats[ind] = updated);

      //existing.Name = updated.Name;
      //existing.LastMessage = updated.LastMessage;
      //existing.Members = updated.Members;
      //existing.UpdatedAt = updated.UpdatedAt;
      //existing.UnreadMessagesCount = updated.UnreadMessagesCount;

      return updated;
    }

    public async Task LoadContactsAsync()
    {
      if (_appState.CurrentUser != null)
      {
        var userContacts = (await _api.ContactsAsync(_appState.CurrentUser.UserId)).Contacts;
        Contacts.Clear();

        foreach (var contact in userContacts)
        {
          Contacts.Add(contact);
        }
      }
      else
      {
        await Application.Current.MainPage.DisplayAlert("Внимание", "LoadContactsAsync - CurrentUser == null", "OK");

      }
    }


    public async Task AddChat(ChatDTO chat)
    {
      if (GetChat(chat.Id) == null)
      {
        await MainThread.InvokeOnMainThreadAsync(() => Chats.Add(chat));
      }

    }

    public async Task LoadMessagesAsync(Guid chatId)
    {
      var messages = (await _api.MessagesGETAsync(chatId)).Messages;

      var collection = await GetMessages(chatId);

      UpdateCollection(collection, messages, (a, b) => a.Id == b.Id && a.UpdatedAt == b.UpdatedAt);


    }

    public async Task AddMessage(MessageDTO message)
    {
      var chat = GetChat(message.ChatId);
      if (chat != null)
      {

        await MainThread.InvokeOnMainThreadAsync(async () =>
          {
            var msgs = (await GetMessages(message.ChatId));
            if (msgs != null)
            {
              msgs.Add(message);
            }
            else
            {
              await LoadMessagesAsync(chat.Id);
            }
          }
        );
      }
      else
      {
        await Application.Current.MainPage.DisplayAlert("Внимание", "AddMessage - чат не найден", "OK");
      }
    }

    public async Task UpdateMessage(MessageDTO updated, Guid oldId)
    {
      var chat = GetChat(updated.ChatId);
      if (chat != null)
      {
        var messages = await GetMessages(updated.ChatId);
        var existing = messages.FirstOrDefault(m => m.Id == oldId);
        if (existing != null)
        {
          var index = messages.IndexOf(existing);
          MainThread.BeginInvokeOnMainThread(async () =>
          {
            messages[index] = updated;
          });
          // event messages changed
        }
        else
        {
          await Application.Current.MainPage.DisplayAlert("Внимание", "UpdateMessage - не найден элемент в коллекции messages", "OK");

        }

      }
    }


    public async Task AddContact(ContactDTO contact)
    {
      if (GetContactByUser(contact.UserId) == null)
      {
        Contacts.Add(contact);
      }
    }
    //private async Task SearchUsersAsync(string query)
    //{
    //  if (string.IsNullOrEmpty(query))
    //  {
    //    SearchResults.Clear();
    //  }
    //  else
    //  {
    //    var results = (await apiClient.SearchAsync(query)).ToList();
    //    SearchResults.Clear();
    //    foreach (var user in results)
    //      SearchResults.Add(user);
    //  }
    //}



    public async Task MarkMessagesRead(Guid chatId, Guid userId, Guid readPositionId, DateTime readAt)
    {
      _messagesByChat.TryGetValue(chatId, out var messages);

      for (var i = 0; i < messages.Count; i++)
      {

        var old = messages[i];

        messages[i] = new MessageDTO
        {
          Id = old.Id,
          ChatId = old.ChatId,
          SenderId = old.SenderId,
          Content = old.Content,
          CreatedAt = old.CreatedAt,
          Username = old.Username,
          UpdatedAt = old.UpdatedAt,
          IsRead = true,
          ReadByCount = old.ReadByCount + 1
        };
      }
    }
    // try to use when chat closed or scroll ended
    public async Task UpdateMessageReadStatuses(Guid chatId, Guid oldReadPositionMessageId, Guid newReadPositionMessageId)
    {
        // Attention! work only when messages update before chat

        if (!_messagesByChat.TryGetValue(chatId, out var messages) || messages == null) return;

        var prevPosition = messages.Select((item, index) => (item, index))
          .FirstOrDefault(m => m.item.Id == oldReadPositionMessageId);
        int prevPositionIndex = prevPosition != default ? prevPosition.index : 0;

        var newPosition = messages.Select((item, index) => (item, index))
          .FirstOrDefault(m => m.item.Id == newReadPositionMessageId);
        int newPositionIndex = newPosition != default ? newPosition.index : prevPositionIndex;

        if (newPositionIndex < prevPositionIndex) return;

        for (var i = prevPositionIndex; i <= newPositionIndex; ++i)
        {
          var old = messages[i];
          // Копируем все поля кроме IsRead и ReadByCount — им присваиваем новые значения
          await MainThread.InvokeOnMainThreadAsync(() =>
          {
            messages[i] = new MessageDTO
            {
              Id = old.Id,
              ChatId = old.ChatId,
              SenderId = old.SenderId,
              Content = old.Content,
              CreatedAt = old.CreatedAt,
              Username = old.Username,
              UpdatedAt = old.UpdatedAt,
              IsRead = true,
              ReadByCount = old.ReadByCount + 1
            };
          });
        }
    }
    //public async Task UpdateChatReadPosition(Guid chatId, Guid lastReadMessageId)
    //{
    //  Chats.FirstOrDefault(c => c.Id == chatId).LastReadMessageId;
    //}
  }
}
