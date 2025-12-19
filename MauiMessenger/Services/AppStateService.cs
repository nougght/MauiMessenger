using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using MauiMessenger.Models;
using Microsoft.Maui.Storage;



namespace MauiMessenger.Services
{
  public partial class AppStateService : ObservableObject
  {
    [ObservableProperty]
    private UserDTO? currentUser;

    [ObservableProperty]
    private ChatDTO? selectedChat;


    public string? AccessToken { get; private set; }
    public string? RefreshToken { get; private set; }
    public string? SavedUserId { get; private set; }

    public bool IsAuthorised { get => CurrentUser != null; }


    public async Task LoadTokenAndUserId()
    {
      RefreshToken = await SecureStorage.Default.GetAsync("refresh_token");
      SavedUserId = await SecureStorage.Default.GetAsync("user_id");
    }

    public async Task SetSession(string accessToken, string refreshToken, UserDTO user)
    {
      AccessToken = accessToken;
      RefreshToken = refreshToken;
      CurrentUser = user;
      await SaveCurrentTokenAndUserId();
    }
    public async Task SaveCurrentTokenAndUserId()
    {
      if (RefreshToken != null)
      {
        await SecureStorage.Default.SetAsync("refresh_token", RefreshToken);
      }
      if (CurrentUser != null)
      {
        await SecureStorage.Default.SetAsync("user_id", CurrentUser.UserId.ToString());
      }
    }

    public async void ResetSession()
    {
      AccessToken = null;
      RefreshToken = null;
      SavedUserId = null;
      CurrentUser = null;
      SelectedChat = null;
      SecureStorage.Default.Remove("refresh_token");
      SecureStorage.Default.Remove("user_id");
    }
  }
}
