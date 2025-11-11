using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using MauiMessenger.Models;
using MauiMessenger.Services;


namespace MauiMessenger.ViewModels
{
  public partial class ChatInfoViewModel : BaseViewModel
  {
    private readonly ChatService _chatService;
    private readonly Client _api;
    private readonly DataRepository _data;
    private readonly AppStateService _appState;

    [ObservableProperty]
    private ChatDTO chat;


    public Command ChatMemberClickedCommand { get; }


    public ChatInfoViewModel(ChatService chatService, DataRepository data, AppStateService appState, Client api, ChatDTO chat)
    {
      _chatService = chatService;
      _data = data;
      _appState = appState;
      _api = api;
      this.Chat = chat;

      ChatMemberClickedCommand = new Command<Guid>(async (userId) => await
      // go to user page
      , (userId) => true);


    }

  }
