using MauiMessenger.Models;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace MauiMessenger.Services
{
  public class ChatService
  {

    private readonly SignalRService _signalR;
    private readonly Client _api;
    private readonly DataRepository _data;
    private readonly AppStateService _appState;



    public ChatService(DataRepository data, Client api, SignalRService signalR, AppStateService appState)
    {
      _data = data;
      _api = api;
      _signalR = signalR;
      _appState = appState;
    }

    
    public async Task SendMessageAsync(Guid chatId, string text)
    {
      try
      {
        var tmp = new MessageDTO
        {
          Id = Guid.NewGuid(),
          ChatId = chatId,
          SenderId = _appState.CurrentUser.UserId,
          Content = text,
          CreatedAt = DateTime.Now,
          Username = _appState.CurrentUser.Username,
        };

        // add temporary object for ui 

        MainThread.BeginInvokeOnMainThread(async () =>
          {

            await _data.AddMessage(tmp);
            await _data.UpdateChatAsync(tmp.ChatId);
          }
        );

        var msg = new CreateMessageRequest
        {
          Message = text,
          ChatId = chatId,
          UserId = _appState.CurrentUser.UserId
        };

        // api request
        var response = await _api.MessagesPOSTAsync(msg);
        // replacing temporary object

        MainThread.BeginInvokeOnMainThread(async () =>
        {
          await _data.UpdateMessage(response, tmp.Id);
          await _data.UpdateChatAsync(tmp.ChatId);

        }
        );
        // notifying other clients if online
        await _signalR.SendMessage(response);


      }
      catch (Exception ex)
      {
        await Application.Current.MainPage.DisplayAlert("Ошибка отправки сообщения", ex.ToString(), "OK");
      }

      //IsBusy = false;
    }

    public ChatDTO? GetChat(Guid chatId) => _data.GetChat(chatId);
    public ObservableCollection<ChatDTO> GetChats() => _data.Chats;
    public async Task<ObservableCollection<MessageDTO>> GetMessages(Guid chatId) => await _data.GetMessages(chatId);

    public ObservableCollection<ContactDTO> GetContacts() => _data.Contacts;
    public async Task LoadMessagesAsync(Guid chatId) => await _data.LoadMessagesAsync(chatId);

    public Guid MemberTypeId { get => _data.MemberTypeId;}
    public Guid AdminTypeId { get => _data.AdminTypeId; }
    public Guid OwnerTypeId { get => _data.OwnerTypeId; }


    public Guid PrivateTypeId { get => _data.PrivateTypeId; }
    public Guid GroupTypeId { get => _data.GroupTypeId; }

    public async Task<ChatDTO> GetOrCreateChatAsync(Guid userId)
    {
        var chat = _data.GetChatWithUser(userId);
        if (chat == null)
        {
          chat = await CreatePrivateChat(userId);
        }
        return chat;
    }


    public async Task<ChatDTO> CreatePrivateChat(Guid userId)
    {
      var existing = _data.GetChatWithUser(userId);
      if (existing == null)
      {
        var chat = await _api.ChatPOSTAsync(
          new CreateChatRequest
          {
            Name = "",
            Type = _data.PrivateTypeId,
            CreatedBy = _appState.CurrentUser.UserId,
            Members = new List<CreateMemberRequest>
            {
              new CreateMemberRequest { UserId = _appState.CurrentUser.UserId, Role = _data.MemberTypeId},
              new CreateMemberRequest { UserId = userId, Role = _data.MemberTypeId }
            }
          }
        );

        chat.Name = chat.Members.FirstOrDefault(m => m.UserId != _appState.CurrentUser.UserId)?.Username ?? "Чат";
        await _data.AddChat(chat);


        await _signalR.NotifyChatCreated(chat.Id);
        return chat;
      }
      else
      {
        throw new NotImplementedException();
      }
    }

    public async Task<ChatDTO> CreateGroupChat(IEnumerable<UserDTO> users, string name)
    {
      
      var members = users.Select(u => new CreateMemberRequest { UserId = u.UserId, Role = _data.MemberTypeId }).ToList();
      members.Add(new CreateMemberRequest { UserId = _appState.CurrentUser.UserId, Role = _data.OwnerTypeId });

      var chat = await _api.ChatPOSTAsync(

        new CreateChatRequest
        {
          Name = name,
          Type = _data.GroupTypeId,
          CreatedBy = _appState.CurrentUser.UserId,
          Members = members
        }

      );

      await _data.AddChat(chat);
      await _signalR.NotifyChatCreated(chat.Id);
      return chat;
    }


    public async Task UpdateChatReadPosition(Guid chatId, Guid newReadPositionId)
    {

      await MainThread.InvokeOnMainThreadAsync(async () =>
      {

        await  _data.UpdateMessageReadStatuses(chatId, newReadPositionId);
      }
      );
    }
  }
}
