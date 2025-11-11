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

namespace MauiMessenger.ViewModels
{
  public partial class SearchViewModel : BaseViewModel
  {
    private readonly Client _api;

    [ObservableProperty]
    private string searchQuery;

    
    
    public ObservableCollection<UserDTO> SearchResults { get; } = new();

    public SearchViewModel(Client api)
    {
      _api = api;
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
