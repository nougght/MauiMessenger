using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    private ConversationDTO? selectedChat;


    public ObservableCollection<UserStatus> Statuses { get; } = new();

    public string? tempUsername;
    public string? tempPassword;
    public string? tempEmail;

    public string? AccessToken { get; private set; }
    public string? RefreshToken { get; private set; }
    public Guid? SavedUserId { get; private set; }

    public bool IsAuthorised { get => CurrentUser != null; }

    public void UpdateUserStatuses(HashSet<UserStatus> statuses)
    {
      foreach (var status in statuses)
      {
        var existing = Statuses.FirstOrDefault(s => s.UserId == status.UserId);
        if (existing == null)
        {
          Statuses.Add(status);
        }
        else
        {
          existing.IsOnline = status.IsOnline;
        }
      }
    }

    public UserStatus? GetUserStatusById(Guid userId)
    {
      return Statuses.FirstOrDefault(s => userId == s.UserId);
    }

    public async Task LoadTokenAndUserId()
    {
      RefreshToken = await SecureStorage.Default.GetAsync("refresh_token");
      var userId = await SecureStorage.Default.GetAsync("user_id");
      SavedUserId = userId == null ? null : new Guid(userId);
    }


    public async Task SetSession(string accessToken, string refreshToken, UserDTO user)
    {
      AccessToken = accessToken;
      RefreshToken = refreshToken;
      CurrentUser = user;
      await SaveCurrentTokenAndUserId();
    }

    public void SetAccessToken(string accessToken)
    {
      AccessToken = accessToken;
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

    public async Task ResetSession()
    {
      await MainThread.InvokeOnMainThreadAsync(() =>
      {
        AccessToken = null;
        RefreshToken = null;
        SavedUserId = null;
        CurrentUser = null;
        SelectedChat = null;
        SecureStorage.Default.Remove("refresh_token");
        SecureStorage.Default.Remove("user_id");
      });
    }
  }
}
