using CommunityToolkit.Mvvm.ComponentModel;
using MauiMessenger.Models;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MauiMessenger.Services
{
  public class SignalRService
  {

    HubConnection _hubConnection;

    public bool IsConnected;

    //readonly DataRepository _data;


    public event Action<MessageDTO>? OnMessageReceived;

    public event Action<ChatDTO>? OnChatCreated;

    public event Action<Guid, Guid, Guid, DateTime>? OnUpdateReadStatuses;

    public event Action? HubConnectionClosed;

    public SignalRService()
    {
      _hubConnection = new HubConnectionBuilder()
          //.WithUrl("http://10.0.2.2:8080/chat")
          .WithUrl("http://127.0.0.1:8080/chat")
          .Build();

      _hubConnection.Closed += async (error) =>
      {
        
        await Application.Current.MainPage.DisplayAlert("Внимание", "SignalR подключение прервано - Closed\nПереподключение через 5 сек.", "OK");
        //SendLocalMessage(new MessageDto { Content = "Подключение закрыто...", CreatedAt = DateTime.UtcNow });
        IsConnected = false;
        HubConnectionClosed?.Invoke();
        await Task.Delay(5000);
        await Connect();
      };

      Debug.WriteLine("SignalRService CREATED");
      _hubConnection.Remove("Receive");
      _hubConnection.On<MessageDTO>("Receive", (message) =>
      {
        //SendLocalMessage(message);
        OnMessageReceived?.Invoke(message);
#if DEBUG
        Debug.WriteLine($"[ChatPage] [SignalRService] Message Received");
#endif
      });

      _hubConnection.Remove("ChatCreated");
      _hubConnection.On<ChatDTO>("ChatCreated", (chat) =>
      {
        //AddLocalChat(chat);
        OnChatCreated?.Invoke(chat);
      });

      _hubConnection.On<Guid, Guid, Guid, DateTime>("UpdateReadStatuses", (chatId, userId, positionId, readAt) =>
      {
        //AddLocalChat(chat);
        OnUpdateReadStatuses?.Invoke(chatId, userId, positionId, readAt);
      });

      IsConnected = false;
    }

    public async Task Connect()
    {
      try
      {
        await _hubConnection.StartAsync();
        IsConnected = true;
      }
      catch (Exception ex)
      {
        //SendLocalMessage(new MessageDto { Content = $"Ошибка подключения: {ex.Message}", CreatedAt = DateTime.UtcNow });
        await Application.Current.MainPage.DisplayAlert("Ошибка подключения SignalR", ex.ToString(), "OK");
      }
    }


    public async Task Disconnect()
    {

      await _hubConnection.StopAsync();
      IsConnected = false;
      await Application.Current.MainPage.DisplayAlert("Внимание", "SignalR подключение завершено - Disconnect()", "OK");
      //SendLocalMessage(new MessageDto { Content = "Вы покинули...", CreatedAt = DateTime.UtcNow });

    }

    public async Task NotifyChatCreated(Guid chatId)
    {
      await _hubConnection.InvokeAsync("NotifyChatCreated", chatId);
    }

    public async Task RegisterInHub(Guid userId)
    {
      await _hubConnection.InvokeAsync("Register", userId);
    }

//    public async Task UpdateReadPosition(ChatDTO ) {
//    await _hubConnection.InvokeAsync("UpdateReadPosition", new ReadPositionDto
//{
//    ChatId = chatId,
//    UserId = currentUserId,
//    LastReadMessageId = messageId
//  });

    public async Task SendMessage(MessageDTO message)
    {
      await _hubConnection.InvokeAsync("Send", message);
    }


    public async Task UpdateChatReadPosition(Guid chatId, Guid userId, Guid newReadPositionId)
    {
      await _hubConnection.InvokeAsync("UpdateChatReadPosition", chatId, userId, newReadPositionId);
    }
  }
}
