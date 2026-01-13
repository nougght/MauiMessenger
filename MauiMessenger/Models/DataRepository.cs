using CommunityToolkit.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;
using MauiMessenger.ApiClient;
using MauiMessenger.Models;
using MauiMessenger.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MauiMessenger.Models
{
  public class DataRepository : ObservableObject
  {
    private readonly Client _api;
    private readonly SignalRService _signalR;
    private readonly AppStateService _appState;

    public ObservableCollection<ConversationDTO> Conversations { get; } = new();
    public ObservableCollection<UserDTO> Users { get; } = new();


    public ObservableCollection<ContactDTO> Contacts { get; } = new();

    private readonly Dictionary<Guid, ObservableCollection<ChatItem>> _chatItemsByChat = new();
    private readonly Dictionary<Guid, List<int>> _messageIndexesByChat = new();
    private readonly Dictionary<string, string> _avatarURLs = new();

    public Dictionary<Guid, GroupChatDetailsDTO> GroupChatsDetails { get; } = new();
    public Dictionary<Guid, PrivateChatDetailsDTO> PrivateChatsDetails { get; } = new();
    public Dictionary<Guid, ChannelDetailsDTO> ChannelsDetails { get; } = new();


    public async Task<UserDTO?> GetUser(Guid? userId)
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

    public async Task<string> UpdateAvatarAsync(Guid ownerId, string ownerType, string contentType, Stream stream)
    {
      var fileResp = await _api.AvatarUrlPOSTAsync(
        new PostAvatarRequest
        {
          OwnerId = ownerId,
          OwnerType = ownerType,
          FileType = contentType
        });


      // uploading file to s3
      using (var httpClient = new HttpClient())
      {
        var putRequest = new HttpRequestMessage(HttpMethod.Put, fileResp.PutUrl)
        {
          Content = new StreamContent(stream)
        };
        putRequest.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        var putResponse = await httpClient.SendAsync(putRequest);
        putResponse.EnsureSuccessStatusCode();
      }
      var getUrl = await _api.PresignedUrlGETAsync(fileResp.OwnerId.ToString());
      _avatarURLs[ownerId.ToString()] = getUrl;
      return getUrl;
    }

    public async Task<string> GetAvatarURLAsync(string fileKey)
    {
      if (_avatarURLs.TryGetValue(fileKey, out var url))
      {
        return url;
      }
      else
      {
        var response = await _api.AvatarUrlGETAsync(fileKey);
        if (!string.IsNullOrEmpty(response))
        {
          _avatarURLs[fileKey] = response;
        }
        return response;
      }
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

    //public Guid PrivateChatUserId(ChatDTO chat)
    //{
    //  if (chat.Type.Id == PrivateTypeId)
    //  {
    //    return chat.Members.First(m => m.UserId != _appState.CurrentUser.UserId).UserId;
    //  }
    //  else
    //  {
    //    throw new Exception("Not private chat");
    //  }
    //}

    public ConversationDTO? GetChatWithUser(Guid userId)
    {
      var conversatonId = PrivateChatsDetails.Values.FirstOrDefault(d => d.OtherUser.UserId == userId)?.ConversationId;
      return conversatonId != null ? GetConversation(conversatonId.Value) : null;
    }




    public ConversationDTO? GetConversation(Guid id) => Conversations.FirstOrDefault(c => c.Id == id);

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

    public async Task<GroupChatDetailsDTO?> GetGroupChatDetailsAsync(Guid conversationId)
    {
      if (GroupChatsDetails.TryGetValue(conversationId, out var details))
      {
        return details;
      }
      else
      {
        if (GetConversation(conversationId)?.Type.Id != GroupTypeId)
        {
          return null;
        }
        var response = await _api.GroupGETAsync(conversationId, _appState.CurrentUser.UserId);
        GroupChatsDetails[conversationId] = response;
        return response;
      }
    }

    public async Task<PrivateChatDetailsDTO?> GetPrivateChatDetailsAsync(Guid conversationId)
    {
      if (PrivateChatsDetails.TryGetValue(conversationId, out var details))
      {
        return details;
      }
      else
      {
        if (GetConversation(conversationId)?.Type.Id != PrivateTypeId)
        {
          return null;
        }
        var response = await _api.PrivateGETAsync(conversationId, _appState.CurrentUser.UserId);
        PrivateChatsDetails[conversationId] = response;
        return response;
      }
    }

    public async Task<ChannelDetailsDTO?> GetChannelDetailsAsync(Guid conversationId)
    {
      if (ChannelsDetails.TryGetValue(conversationId, out var details))
      {
        return details;
      }
      else
      {
        if (GetConversation(conversationId)?.Type.Id != ChannelTypeId)
        {
          return null;
        }
        var response = await _api.ChannelGETAsync(conversationId, _appState.CurrentUser.UserId);
        ChannelsDetails[conversationId] = response;
        return response;
      }
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

    public Guid ChannelTypeId { get; private set; }
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

    public DataRepository(Client api, AppStateService state, SignalRService signalR)
    {
      _api = api;
      _appState = state;
      _signalR = signalR;

      _signalR.OnStatusesReceived += (statuses) =>
      {
        var param = statuses.Select(s => UserStatus.FromUserStatusDto(s)).ToHashSet();
        _appState.UpdateUserStatuses(param);
      };
      Conversations.CollectionChanged += (a, e) =>
      {
        Debug.WriteLine($"[CollectionChanged] Action = {e.Action}");

        if (e.OldItems != null)
          Debug.WriteLine($"OldItems: {e.OldItems.Count}");
        if (e.NewItems != null)
          Debug.WriteLine($"NewItems: {e.NewItems.Count}");

        foreach (var chat in Conversations)
          Debug.WriteLine($"Chat: {chat.Id}, unread={chat.UnreadMessagesCount}");

        Debug.WriteLine("-----");
      };
      _signalR = signalR;
    }


    public async Task LoadEnumsAsync()
    {
      ChatTypes = (await _api.ConversationTypesAsync()).ToDictionary().Select(p => new KeyValuePair<Guid, string>(new Guid(p.Key), p.Value)).ToDictionary();
      MemberRoles = (await _api.MemberRolesAsync()).ToDictionary().Select(p => new KeyValuePair<Guid, string>(new Guid(p.Key), p.Value)).ToDictionary();

      PrivateTypeId = ChatTypes.FirstOrDefault(r => r.Value == "private").Key;

      if (PrivateTypeId == Guid.Empty)
        throw new InvalidOperationException("ChatType 'private' not found");


      GroupTypeId = ChatTypes.FirstOrDefault(r => r.Value == "group").Key;

      if (GroupTypeId == Guid.Empty)
        throw new InvalidOperationException("ChatType 'group' not found");

      ChannelTypeId = ChatTypes.FirstOrDefault(r => r.Value == "channel").Key;

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
        var userChats = (await _api.ChatsAsync(_appState.CurrentUser.UserId)).Conversations;
        Conversations.Clear();

        HashSet<string> users = new();
        foreach (var chat in userChats)
        {
          //if (chat.Type.Id == PrivateTypeId)
          //{
          //  chat.Title = chat.Members.FirstOrDefault(m => m.UserId != _appState.CurrentUser.UserId)?.Username ?? "Чат";
          //}
          Conversations.Add(chat);
          //foreach (var member in chat.Members)
          //{
          //  if (member.UserId !=  _appState.CurrentUser.UserId)
          //  {
          //    users.Add(member.UserId.ToString());
          //  }
          //}
        }

        //await _signalR.SendUsersStatusesRequest(users);

      }
      else
      {
        await Application.Current.MainPage.DisplayAlert("Внимание", "LoadChatsAsync - CurrentUser == null", "OK");

      }
    }

    public async Task LoadUserStatuseseAsync(List<Guid> userIds)
    {
      await _signalR.SendUsersStatusesRequest(userIds.Select(u => u.ToString()).ToHashSet());
    }
    public async Task<ConversationDTO> UpdateChatAsync(Guid conversationId)
    {
      var existing = GetConversation(conversationId);
      var ind = Conversations.IndexOf(existing);

      var updated = await _api.ConversationGETAsync(conversationId, _appState.CurrentUser.UserId);
      //updated.Name = existing.Name;
      await MainThread.InvokeOnMainThreadAsync(() => Conversations[ind] = updated);

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


    public async Task AddChat(ConversationDTO chat)
    {
      if (GetConversation(chat.Id) == null)
      {
        await MainThread.InvokeOnMainThreadAsync(() => Conversations.Add(chat));
      }

    }


    public async Task<List<ChatItem>> ProcessChatItems(ConversationDTO chat, ICollection<MessageDTO> messages)
    {
      var chatItems = new List<ChatItem>();
      var messageItems = messages.Select(m => new MessageItem
      {
        Message = m,
        CreatedAt = m.CreatedAt!.Value.DateTime
      });
      //var serviceMessages = chat.Members.Where(m => m.AddedAt != chat.CreatedAt).OrderBy(m => m.AddedAt)
      //    .Select(m => new ServiceMessageItem
      //    {
      //      Type = ChatItemType.ServiceMessage,
      //      Text = $"{chat.CreatorUsername} добавил участника {m.Username}",
      //      CreatedAt = m.AddedAt.DateTime
      //    }).ToList();
      var serviceMessages = new List<ServiceMessageItem>();
      if (chat.Type.Id != PrivateTypeId)
      {
        serviceMessages.Add(new ServiceMessageItem
        {
          CreatedAt = chat.CreatedAt.DateTime,
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
      var response = (await _api.MessagesGETAsync(chatId, _appState.CurrentUser.UserId)).Messages;
      //response.Files = new List<MessageFileDTO>();
      foreach (var msg in response)
      {
        foreach (var file in msg.Files)
        {
          var url = await _api.PresignedUrlGETAsync(file.FileKey);
          file.URL = url;

        }

      }
      var chatItems = await ProcessChatItems(GetConversation(chatId)!, response);



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
      var chat = GetConversation(message.ConversationId);
      if (chat != null)
      {

        await MainThread.InvokeOnMainThreadAsync(async () =>
          {
            var chatItems = await GetChatItems(message.ConversationId);
            var indexes = await GetMessageIndexes(message.ConversationId);
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
                var index = Conversations.IndexOf(chat);
                var updated = new ConversationDTO
                {
                  Id = chat.Id,
                  //Members = chat.Members,
                  Title = chat.Title,
                  Type = chat.Type,
                  LastMessage = message,
                  //UpdatedAt = message.CreatedAt,
                  UnreadMessagesCount = chat.UnreadMessagesCount + 1,
                  LastReadMessageId = chat.LastReadMessageId
                };

                Conversations.RemoveAt(index);
                Conversations.Insert(index, updated);
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
      var chat = GetConversation(updated.ConversationId);
      if (chat != null)
      {
        var chatItems = await GetChatItems(updated.ConversationId);
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
              Type = updated.Files.Count > 1 && updated.Files.First().FileType.StartsWith("audio/") ? ChatItemType.Audio : ChatItemType.Message
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
                ConversationId = old.ConversationId,
                SenderId = old.SenderId,
                Content = old.Content,
                CreatedAt = old.CreatedAt,
                Username = old.Username,
                UpdatedAt = old.UpdatedAt,
                IsRead = true,
                ReadByCount = old.ReadByCount + 1,
                Files = old.Files
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
                ConversationId = old.ConversationId,
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
      var chat = GetConversation(chatId);
      var index = Conversations.IndexOf(chat);

      Conversations[index] = new ConversationDTO
      {
        Id = chat.Id,
        //Members = chat.Members,
        Title = chat.Title,
        Type = chat.Type,
        LastMessage = chat.LastMessage,
        //UpdatedAt = chat.UpdatedAt,
        UnreadMessagesCount = chat.UnreadMessagesCount < updatedMessagesCount ? 0 : chat.UnreadMessagesCount - updatedMessagesCount,
        LastReadMessageId = lastReadMessageId
      };
    }
  }
}
