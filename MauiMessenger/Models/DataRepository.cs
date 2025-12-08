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
using System.Diagnostics;

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



    public readonly Dictionary<Guid, ObservableCollection<ChatItem>> _chatItemsByChat = new();
    public readonly Dictionary<Guid, List<int>> _messageIndexesByChat = new();

    public ChatDTO? GetChat(Guid id) => Chats.FirstOrDefault(c => c.Id == id);

    public async Task<ObservableCollection<ChatItem>> GetChatItems(Guid chatId)
    {
      _chatItemsByChat.TryGetValue(chatId, out var items);
      if (items == null)
      {
        await MainThread.InvokeOnMainThreadAsync(async () =>
          {
            items = new ObservableCollection<ChatItem>();
            _chatItemsByChat[chatId] = items;
          }
        );
      }
      return items;
    }

    public async Task<List<int>> GetMessageIndexes(Guid chatId)
    {
      _messageIndexesByChat.TryGetValue(chatId, out var indexes);
      if (indexes == null)
      {
        indexes = new List<int>();
        _messageIndexesByChat[chatId] = indexes;
      }
      return indexes;
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
      Chats.CollectionChanged += (a, e) =>
      {
        Debug.WriteLine($"[CollectionChanged] Action = {e.Action}");

        if (e.OldItems != null)
          Debug.WriteLine($"OldItems: {e.OldItems.Count}");
        if (e.NewItems != null)
          Debug.WriteLine($"NewItems: {e.NewItems.Count}");

        foreach (var chat in Chats)
          Debug.WriteLine($"Chat: {chat.Id}, unread={chat.UnreadMessagesCount}");

        Debug.WriteLine("-----");
      };
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


    public async Task<List<ChatItem>> ProcessChatItems(ChatDTO chat, ICollection<MessageDTO> messages)
    {
      var chatItems = new List<ChatItem>();
      var messageItems = messages.Select(m => new MessageItem
      {
        Message = m,
        CreatedAt = m.CreatedAt!.Value.DateTime
      });
      var serviceMessages = chat.Members.Where(m => m.AddedAt != chat.CreatedAt).OrderBy(m => m.AddedAt)
          .Select(m => new ServiceMessageItem
          {
            Type = ChatItemType.ServiceMessage,
            Text = $"{chat.CreatorUsername} добавил участника {m.Username}",
            CreatedAt = m.AddedAt!.Value.DateTime
          }).ToList();
      if (chat.Type.Id != PrivateTypeId)
      {
        serviceMessages.Add(new ServiceMessageItem
        {
          CreatedAt = chat.CreatedAt!.Value.DateTime,
          Text = $"{chat.CreatorUsername} создал чат",
          Type = ChatItemType.ServiceMessage
        });
      }

      chatItems.AddRange(messageItems);
      chatItems.AddRange(serviceMessages);
      var sorted = chatItems
        .OfType<IHasCreatedAt>()
        .OrderBy(x => x.CreatedAt)
        .Cast<ChatItem>()
        .ToList();

      var unreadPosition = chatItems.IndexOf(chatItems.FirstOrDefault(i => i is MessageItem msg && msg.Message.SenderId != _appState.CurrentUser.UserId && !msg.Message.IsRead));

      if (unreadPosition != -1)
      {
        chatItems.Insert(unreadPosition, new UnreadMarkerItem { Type = ChatItemType.UnreadMarker });
      }

      DateTime lastDaySeparator = DateTime.MinValue;

      for (var i = 0; i < chatItems.Count; ++i)
      {
        var item = chatItems[i];

        if (item is IHasCreatedAt hasDate)
        {
          if (lastDaySeparator.Date != hasDate.CreatedAt.Date)


          {
            lastDaySeparator = hasDate.CreatedAt.Date;
            // Вставка DividerItem
            chatItems.Insert(i, new DaySeparatorItem { Date = lastDaySeparator, Type = ChatItemType.DaySeparator });
            i++; // чтобы пропустить вставленный элемент
          }
        }
      }
      return chatItems;
    }

    public List<int> SearchMessageIndexes(ICollection<ChatItem> items)
    {
      List<int> indexes = new List<int>();
      foreach (var item in items.Index())
      {
        if (item.Item is MessageItem)
        {
          indexes.Add(item.Index);
        }
      }
      return indexes;
    }

    public async Task LoadMessagesAsync(Guid chatId)
    {
      var chatItems = await ProcessChatItems(GetChat(chatId)!, (await _api.MessagesGETAsync(chatId, _appState.CurrentUser.UserId)).Messages);
      var inds = SearchMessageIndexes(chatItems);
      ObservableCollection<ChatItem> collection = await GetChatItems(chatId);
      var indexes = await GetMessageIndexes(chatId);

      collection.Clear();
      indexes.Clear();

      foreach (var item in chatItems)
      {
        collection.Add(item);
      }
      foreach (var item in inds)
      {
        indexes.Add(item);
      }
      //UpdateCollection(collection, chatItems, (a, b) => a.Id == b.Id && a.UpdatedAt == b.UpdatedAt);


    }

    public async Task AddMessage(MessageDTO message)
    {
      var chat = GetChat(message.ChatId);
      if (chat != null)
      {

        await MainThread.InvokeOnMainThreadAsync(async () =>
          {
            var chatItems = await GetChatItems(message.ChatId);
            var indexes = await GetMessageIndexes(message.ChatId);
            if (chatItems != null)
            {
              var ind = chatItems.Count;
              if (indexes.Count == 0 || chatItems[indexes.Last()] is IHasCreatedAt item && item.CreatedAt.ToLocalTime().Date != message.CreatedAt!.Value.ToLocalTime().Date)
              {
                chatItems.Insert(ind, new DaySeparatorItem { Date = message.CreatedAt!.Value.DateTime, Type = ChatItemType.DaySeparator });
                ++ind;
              }
              chatItems.Insert(ind, new MessageItem
              {
                Message = message,
                CreatedAt = message.CreatedAt!.Value.DateTime,
                Type = ChatItemType.Message
              });
              indexes.Add(ind);
              if (message.SenderId != _appState.CurrentUser.UserId)
              {
                var index = Chats.IndexOf(chat);
                var updated = new ChatDTO
                {
                  Id = chat.Id,
                  Members = chat.Members,
                  Name = chat.Name,
                  Type = chat.Type,
                  LastMessage = message,
                  UpdatedAt = message.CreatedAt,
                  UnreadMessagesCount = chat.UnreadMessagesCount + 1,
                  LastReadMessageId = chat.LastReadMessageId
                };

                Chats.RemoveAt(index);
                Chats.Insert(index, updated);
              }
              {
                // event messages changed
              }
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
        var chatItems = await GetChatItems(updated.ChatId);
        var existing = chatItems.FirstOrDefault(m => m is MessageItem msg && msg.Message.Id == oldId);
        if (existing != null)
        {
          var index = chatItems.IndexOf(existing);
          MainThread.BeginInvokeOnMainThread(async () =>
          {
            chatItems.RemoveAt(index);
            chatItems.Insert(index, new MessageItem
            {
              Message = updated,
              CreatedAt = updated.CreatedAt!.Value.DateTime,
              Type = ChatItemType.Message
            });
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


    // use to  mark sent messages as read when signal received
    public async Task MarkMessagesRead(Guid chatId, Guid userId, Guid readPositionId, DateTime readAt)
    {
      _chatItemsByChat.TryGetValue(chatId, out var chatItems);

      await MainThread.InvokeOnMainThreadAsync(() =>
      {
        var readPosition = chatItems.Select((item, index) => (item, index))
        .FirstOrDefault(m => m.item is MessageItem msg && msg.Message.Id == readPositionId)!.index;
        for (var i = 0; i <= readPosition; i++)
        {

          if (chatItems[i] is MessageItem msg)
          {
            var old = msg.Message;
            chatItems.RemoveAt(i);
            chatItems.Insert(i, new MessageItem
            {
              Message = new MessageDTO
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
              },
              CreatedAt = old.CreatedAt!.Value.DateTime,
              Type = ChatItemType.Message
            });
          }
        }
      });
    }

    // use to update received message statuses when chat closed
    public async Task<int> UpdateMessageReadStatuses(Guid chatId, Guid oldReadPositionMessageId, Guid newReadPositionMessageId)
    {
      // Attention! work only when messages update before chat

      if (!_chatItemsByChat.TryGetValue(chatId, out var chatItems) || chatItems == null) return 0;

      var prevPosition = chatItems.Select((item, index) => (item, index))
        .FirstOrDefault(m => m.item is MessageItem msg && msg.Message.Id == oldReadPositionMessageId);
      int prevPositionIndex = prevPosition != default ? prevPosition.index : 0;

      var newPosition = chatItems.Select((item, index) => (item, index))
        .FirstOrDefault(m => m.item is MessageItem msg && msg.Message.Id == newReadPositionMessageId);
      int newPositionIndex = newPosition != default ? newPosition.index : prevPositionIndex;

      if (newPositionIndex < prevPositionIndex) return 0;
      await MainThread.InvokeOnMainThreadAsync(() =>
      {
        for (var i = prevPositionIndex; i <= newPositionIndex; ++i)
        {
          if (chatItems[i] is MessageItem msg)
          {
            var old = msg.Message;
            // Копируем все поля кроме IsRead и ReadByCount — им присваиваем новые значения

            chatItems[i] = new MessageItem
            {
              Message = new MessageDTO
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
              },
              CreatedAt = old.CreatedAt!.Value.DateTime,
              Type = ChatItemType.Message
            };
          }
        }
      });
      return newPositionIndex - prevPositionIndex + 1;
    }


    // use to update current users chat read position when chat closed
    public async Task UpdateChatReadPosition(Guid chatId, Guid lastReadMessageId, int updatedMessagesCount)
    {
      var chat = GetChat(chatId);
      var index = Chats.IndexOf(chat);

      Chats[index] = new ChatDTO
      {
        Id = chat.Id,
        Members = chat.Members,
        Name = chat.Name,
        Type = chat.Type,
        LastMessage = chat.LastMessage,
        UpdatedAt = chat.UpdatedAt,
        UnreadMessagesCount = chat.UnreadMessagesCount < updatedMessagesCount ? 0 : chat.UnreadMessagesCount - updatedMessagesCount,
        LastReadMessageId = lastReadMessageId
      };
    }
  }
}
