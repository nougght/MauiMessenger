using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MauiMessenger.Models;
using MauiMessenger.Services;


namespace MauiMessenger.ViewModels
{
  public partial class GroupCreationViewModel : BaseViewModel
  {
    private readonly Client _api;
    private readonly ChatService _chatService;

    [ObservableProperty]
    private string groupName;


    [ObservableProperty]
    private string searchQuery;


    public delegate void Notify();

    public event Notify ClosePopupRequest;



    public ObservableCollection<UserDTO> SearchResults { get; } = new();
    public ObservableCollection<object> SelectedGroupMembers { get; set; } = new();


    public Command CreateGroupChatCommand { get; }


    public GroupCreationViewModel(Client api, ChatService chatService)
    {
      _api = api;
      _chatService = chatService;

      CreateGroupChatCommand = new Command(async () => await OnCreateGroupChat(), () => true);

    }


    partial void OnSearchQueryChanged(string value)
    {
     _ = SearchUsersAsync(value);
    }

    private async Task SearchUsersAsync(string query)
    {
      if (string.IsNullOrEmpty(query))
      {
        SearchResults.Clear();
      }
      else
      {
        var results = (await _api.SearchAsync(query)).ToList();
        SearchResults.Clear();
        foreach (var user in results)
          SearchResults.Add(user);
      }
    }

    public async Task OnCreateGroupChat()
    {
      ClosePopupRequest?.Invoke();
      var chat = await _chatService.CreateGroupChat(SelectedGroupMembers.OfType<UserDTO>(), GroupName);
      //await NavigationService.GoToChatAsync(chat);
    }
  }
}


