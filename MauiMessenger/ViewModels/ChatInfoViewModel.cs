using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using MauiMessenger.Models;
using MauiMessenger.Services;

using MauiMessenger.ApiClient;

namespace MauiMessenger.ViewModels
{

  [QueryProperty(nameof(Conversation), "Conversation")]
  [QueryProperty(nameof(GroupDetails), "GroupDetails")]
  public partial class ChatInfoViewModel : BaseViewModel
  {
    private readonly ChatService _chatService;
    private readonly Client _api;
    private readonly DataRepository _data;
    private readonly AppStateService _appState;

    [ObservableProperty]
    private ConversationDTO conversation;

    [ObservableProperty]
    private GroupChatDetailsDTO groupDetails;


    public Command ChatMemberClickedCommand { get; }
    public Command BackButtonClickedCommand { get; }


    public ChatInfoViewModel(ChatService chatService, DataRepository data, AppStateService appState, Client api)
    {
      _chatService = chatService;
      _data = data;
      _appState = appState;
      _api = api;
      //this.Chat = chat;

      ChatMemberClickedCommand = new Command<Guid>(async (userId) =>
        await NavigationService.GoToUserPageAsync(await _data.GetUser(userId)), (userId) => true);
      BackButtonClickedCommand = new Command(async () => await NavigationService.GoBackAsync());

    }

  }
}