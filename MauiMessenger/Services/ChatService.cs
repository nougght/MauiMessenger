
using MauiMessenger.ApiClient;
using MauiMessenger.Models;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
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


    public async Task SendMessageAsync(Guid chatId, string text, List<FileResult> files, string? audioPath = null)
    {
      try
      {
        var tmp = new MessageDTO
        {
          Id = Guid.NewGuid(),
          ConversationId = chatId,
          SenderId = _appState.CurrentUser.UserId,
          Content = text,
          CreatedAt = DateTime.Now,
          Username = _appState.CurrentUser.Username,
          Files = audioPath != null ?
            new List<MessageFileDTO>
            {
              new MessageFileDTO
              {
                FileType = "audio/m4a",
                FileKey = "",
                FileName = "voice.m4a",
                Id = Guid.NewGuid()
              }
            } :
          files.Select(f => new MessageFileDTO { FileType = f.ContentType, FileKey = "", FileName = "file", Id = Guid.NewGuid() }).ToList()
        };


        // add temporary object for ui 

        MainThread.BeginInvokeOnMainThread(async () =>
          {

            await _data.AddMessage(tmp);
            await _data.UpdateChatAsync(tmp.ConversationId);
          }
        );

        var msg = new CreateMessageRequest
        {
          Message = text,
          ConversationId = chatId,
          UserId = _appState.CurrentUser.UserId
        };

        // api request
        var response = await _api.MessagesPOSTAsync(msg);
        response.Files = new List<MessageFileDTO>();
        if (audioPath != null)
        {
          using var fileStream = File.OpenRead(audioPath);

          using var content = new StreamContent(fileStream);
          content.Headers.ContentType =
              new System.Net.Http.Headers.MediaTypeHeaderValue("audio/m4a");
          long size = fileStream.Length;

          var audioResp = await _api.PresignedUrlPOSTAsync(new PostFileRequest { MessageId = response.Id, FileName = "audio.m4a", FileType = "audio/m4a", Size = size });

          using var request = new HttpRequestMessage(HttpMethod.Put, audioResp.PutUrl)
          {
            Content = content
          };

          using (var httpClient = new HttpClient())
          {
            var putResponse = await httpClient.SendAsync(request);
            putResponse.EnsureSuccessStatusCode();
          }
          audioResp.File.URL = await _api.PresignedUrlGETAsync(audioResp.File.FileKey);
          response.Files.Add(audioResp.File);
          //files.Add(new FileResult(audioPath));
        }
        else
        {
          var streams = new List<Stream>();
          for (var i = 0; i < files.Count; i++)
          {
            if (files[i] != null)
            {
              streams.Add(await files[i].OpenReadAsync());
              long size = streams[i].Length;
              var fileResp = await _api.PresignedUrlPOSTAsync(new PostFileRequest { MessageId = response.Id, FileName = files[i].FileName, FileType = files[i].ContentType, Size = size });


              // uploading file to s3
              using (var httpClient = new HttpClient())
              {
                var putRequest = new HttpRequestMessage(HttpMethod.Put, fileResp.PutUrl)
                {
                  Content = new StreamContent(streams[i])
                };
                putRequest.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(files[i].ContentType);
                var putResponse = await httpClient.SendAsync(putRequest);
                putResponse.EnsureSuccessStatusCode();
              }
              fileResp.File.URL = await _api.PresignedUrlGETAsync(fileResp.File.FileKey);
              response.Files.Add(fileResp.File);
            }
          }
        }
        // replacing temporary object

        MainThread.BeginInvokeOnMainThread(async () =>
        {
          await _data.UpdateMessage(response, tmp.Id);
          await _data.UpdateChatAsync(tmp.ConversationId);

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

    public ConversationDTO? GetConversation(Guid chatId) => _data.GetConversation(chatId);
    public ObservableCollection<ConversationDTO> GetConversations() => _data.Conversations;
    public async Task<ObservableCollection<ChatItem>> GetChatItems(Guid chatId) => await _data.GetChatItems(chatId);
    public async Task<List<int>> GetMessageIndexes(Guid chatId) => await _data.GetMessageIndexes(chatId);

    public ObservableCollection<ContactDTO> GetContacts() => _data.Contacts;
    public async Task LoadMessagesAsync(Guid chatId) => await _data.LoadMessagesAsync(chatId);

    public Guid MemberTypeId { get => _data.MemberTypeId; }
    public Guid AdminTypeId { get => _data.AdminTypeId; }
    public Guid OwnerTypeId { get => _data.OwnerTypeId; }


    public Guid PrivateTypeId { get => _data.PrivateTypeId; }
    public Guid GroupTypeId { get => _data.GroupTypeId; }

    public async Task<ConversationDTO> GetOrCreateChatAsync(Guid userId)
    {
      var chat = _data.GetChatWithUser(userId);
      if (chat == null)
      {
        chat = await CreatePrivateChat(userId);
      }
      return chat;
    }


    public async Task<ConversationDTO> CreatePrivateChat(Guid userId)
    {
      var existing = _data.GetChatWithUser(userId);
      if (existing == null)
      {
        var chat = await _api.PrivatePOSTAsync(
          new CreatePrivateChatRequest
          {
            CreatedBy = _appState.CurrentUser.UserId,
            OtherUserId = userId
          }
        );

        //chat.Name = chat.Members.FirstOrDefault(m => m.UserId != _appState.CurrentUser.UserId)?.Username ?? "Чат";
        await _data.AddChat(chat);


        await _signalR.NotifyChatCreated(chat.Id);
        return chat;
      }
      else
      {
        throw new NotImplementedException();
      }
    }

    public async Task<ConversationDTO> CreateGroupChat(IEnumerable<UserDTO> users, string name, string? description)
    {

      var members = users.Select(u => new CreateMemberRequest { UserId = u.UserId, Role = _data.MemberTypeId }).ToList();
      members.Add(new CreateMemberRequest { UserId = _appState.CurrentUser.UserId, Role = _data.OwnerTypeId });

      var chat = await _api.GroupPOSTAsync(

        new CreateGroupChatRequest
        {
          Title = name,
          Description = description,
          CreatedBy = _appState.CurrentUser.UserId,
          Members = members
        }

      );

      await _data.AddChat(chat);
      await _signalR.NotifyChatCreated(chat.Id);
      return chat;
    }


    public async Task<ConversationDTO> CreateChannel(IEnumerable<UserDTO> users, string name, string? description)
    {

      var chat = await _api.ChannelPOSTAsync(

        new CreateChannelRequest
        {
          Title = name,
          Description = description,
          CreatedBy = _appState.CurrentUser.UserId,
          OwnerId = _appState.CurrentUser.UserId
        }

      );

      await _data.AddChat(chat);
      await _signalR.NotifyChatCreated(chat.Id);
      return chat;
    }

    public async Task MarkMessagesRead(Guid chatId, Guid userId, Guid readPositionId, DateTime readAt)
    {
      await _data.MarkMessagesRead(chatId, userId, readPositionId, readAt);
    }


    // send new chat position by api and signalr(for online users)
    public async Task UpdateChatReadPosition(Guid chatId, Guid newReadPositionId, DateTime lastReadAt)
    {
      try
      {
        await _api.UserReadPositionAsync(chatId, _appState.CurrentUser.UserId,
          new PatchReadPositionRequest
          {
            LastReadMessageId = newReadPositionId,
            LastReadAt = lastReadAt,
          });
        await _signalR.UpdateChatReadPosition(chatId, _appState.CurrentUser.UserId, newReadPositionId, lastReadAt);
      }
      catch (Exception ex)
      {
        throw;
      }
      //await MainThread.InvokeOnMainThreadAsync(async () =>
      //{

      //  await  _data.UpdateMessageReadStatuses(chatId, newReadPositionId);
      //}
      //);
    }

    public async Task OnChatClosed(Guid chatId, Guid? lastReadMessageId)
    {
      if (lastReadMessageId == null)
      {
        return;
      }
      await MainThread.InvokeOnMainThreadAsync(async () =>
      {
        // updating local chat read position
        var chat = GetConversation(chatId);
        var oldReadPositionId = chat.Id;

        var updatedCount = await _data.UpdateMessageReadStatuses(chatId, oldReadPositionId, lastReadMessageId!.Value);
        await _data.UpdateChatReadPosition(chatId, lastReadMessageId!.Value, updatedCount);
      });
    }

    public async Task<List<string>> GetAiSuggestions(Guid chatId, string draft)
    {
      var response = await _api.AiSuggestsAsync(userId: _appState.CurrentUser.UserId, chatId: chatId, responseText: draft);
      return response.Messages.ToList();

    }


    public async Task<PrivateChatDetailsDTO?> GetPrivateChatDetailsAsync(Guid chatId)
    {
      return await _data.GetPrivateChatDetailsAsync(chatId);
    }

    public async Task<GroupChatDetailsDTO?> GetGroupChatDetailsAsync(Guid chatId)
    {
      return await _data.GetGroupChatDetailsAsync(chatId);
    }

    public async Task<ChannelDetailsDTO?> GetChannelDetailsAsync(Guid chatId)
    {
      return await _data.GetChannelDetailsAsync(chatId);
    }
  }
}
