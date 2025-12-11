using CommunityToolkit.Mvvm.ComponentModel;
using MauiMessenger.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MauiMessenger.Models;
using MauiMessenger.Services;
using System.Runtime.CompilerServices;
using MauiMessenger.ApiClient;

namespace MauiMessenger.ViewModels
{
  public partial class SearchViewModel : BaseViewModel
  {
    private readonly Client _api;
    private readonly ChatService _chatService;

    [ObservableProperty]
    private string searchQuery;

    
    
    public ObservableCollection<UserDTO> SearchResults { get; } = new();

    public Command BackButtonClickedCommand { get; }
    public Command ToChatPageCommand { get; }

    public SearchViewModel(Client api, ChatService chatService)
    {
      _api = api;
      _chatService = chatService;

      BackButtonClickedCommand = new Command(async () => await NavigationService.GoBackAsync());
      ToChatPageCommand = new Command<Guid>(async (userId) =>
      {

        var chat = await _chatService.GetOrCreateChatAsync(userId);
        await _chatService.LoadMessagesAsync(chat.Id);

        // go to chat page
        await NavigationService.GoToChatAsync(chat, await _chatService.GetChatItems(chat.Id), await _chatService.GetMessageIndexes(chat.Id));
      });
    }

    partial void OnSearchQueryChanged(string value)
    {
      // testing
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
  }


}
