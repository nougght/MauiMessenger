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
  [QueryProperty(nameof(User), "User")]
  public partial class UserPageViewModel : BaseViewModel
  {
    private readonly ChatService _chatService;
    private readonly Client _api;
    private readonly DataRepository _data;
    private readonly AppStateService _appState;



    [ObservableProperty]
    private UserDTO user;

    [ObservableProperty]
    private bool isInContacts;


    public Command AddToContactCommand { get; }

    public Command ChatWithUserCommand { get; }

    public Command BackButtonClickedCommand { get; }
    public UserPageViewModel(ChatService chatService, DataRepository data, AppStateService appState, Client api, UserDTO user)
    {
      _chatService = chatService;
      _data = data;
      _appState = appState;
      _api = api;

      AddToContactCommand = new Command(async () => await AddToContact(), () => true);
      ChatWithUserCommand = new Command(async () => await ChatWithUser(), () => true);

      BackButtonClickedCommand = new Command(async () => await NavigationService.GoBackAsync());
    }

    partial void OnUserChanged(UserDTO value)
    {
      IsInContacts = _data.GetContactByUser(value.UserId) == null ? false : true;
    }

    public async Task AddToContact()
    {
      IsBusy = true;
      CreateContactRequest contact = new()
      {
        UserId = _appState.CurrentUser.UserId,
        ContactUserId = this.User.UserId
      };
      var response = await _api.ContactsPOSTAsync(contact);
      await _data.AddContact(response);
      IsBusy = false;
    }


    public async Task ChatWithUser()
    {
      var chat = await _chatService.GetOrCreateChatAsync(User.UserId);
      // go to chat page
      await NavigationService.GoToChatAsync(chat);
    }

  }
}
