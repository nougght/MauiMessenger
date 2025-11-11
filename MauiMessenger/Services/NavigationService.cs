using MauiMessenger.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MauiMessenger.Views;

namespace MauiMessenger.Services
{
  public static class NavigationService
  {

    public static async Task GoToChatAsync(ChatDTO chat)
    {
      var param = new ShellNavigationQueryParameters
      {
          { "Chat", chat },

      };
      await Shell.Current.GoToAsync(nameof(ChatPage), param);
    }

    public static async Task GoToUserPageAsync(UserDTO user)
    {
      var param = new ShellNavigationQueryParameters
      {
          { "User", user }
      };
      await Shell.Current.GoToAsync(nameof(UserPage), true, param);
    }

    public static async Task GoToSearchPageAsync()
    {
      await Shell.Current.GoToAsync(nameof(SearchPage));
    }

    public static async Task GoBackAsync() => await Shell.Current.GoToAsync("..");

    //public static async Task GoToLoginAsync()
    //{
    //  Application.Current.MainPage = new LoginPage();
    //}

    
    //Shell.Current.GoToAsync("//login");
  }

}
