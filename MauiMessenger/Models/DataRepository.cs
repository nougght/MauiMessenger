using CommunityToolkit.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;
using MauiMessenger.Models;
using MauiMessenger.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;

namespace MauiMessenger.Models
{
  public class DataRepository : ObservableObject
  {
    private readonly Client _api;
    private readonly AppStateService _appState;

    public ObservableCollection<ChatDTO> Chats { get; } = new();
    public ObservableCollection<UserDTO> Users { get; } = new();


    public ObservableCollection<ContactDTO> Contacts { get; } = new();


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

    public ObservableCollection<MessageDTO> GetMessages(Guid chatId)
    {
      if (!_messagesByChat.TryGetValue(chatId, out var messages))
      {
        messages = new ObservableCollection<MessageDTO>();
        _messagesByChat[chatId] = messages;
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
          if (chat.Type.Name == "private")
          {
            chat.Name = chat.Members.FirstOrDefault(m => m.UserId != User.UserId)?.Username ?? "Чат";
          }
          Chats.Add(chat);
        }
      }
      else
      {
        await Application.Current.MainPage.DisplayAlert("Внимание", "LoadChatsAsync - CurrentUser == null", "OK");

      }
    }

    public async Task AddChat(ChatDTO chat)
    {
      if (GetChat(chat.Id) == null)
      {
        Chats.Add(chat);
      }

    }

    public async Task LoadMessagesAsync(Guid chatId)
    {
      var messages = (await _api.MessagesGETAsync(chatId)).Messages;

      var collection = GetMessages(chatId);

      UpdateCollection(collection, messages, (a, b) => a.Id == b.Id && a.UpdatedAt == b.UpdatedAt);


    }

    public async Task AddMessage(MessageDTO message)
    {
      var chat = GetChat(message.ChatId);
      if (chat != null)
      {
        GetMessages(message.ChatId).Add(message);
      }
      else
      {
        await Application.Current.MainPage.DisplayAlert("Внимание", "AddMessage - чат не найден", "OK");
      }
      }

    public async Task UpdateMessage(MessageDTO updated)
    {
      var chat = GetChat(updated.ChatId);
      if (chat != null)
      {
        var messages = GetMessages(updated.ChatId);
        var existing = messages.FirstOrDefault(m => m.Id == chat.Id);
        if (existing != null)
        {
          var index = messages.IndexOf(existing);
          messages[index] = updated;
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
      if (GetContactByUser(contact.UserId) != null)
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

  }
}
