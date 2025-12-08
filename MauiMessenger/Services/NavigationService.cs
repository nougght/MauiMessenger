using CommunityToolkit.Mvvm.DependencyInjection;
using MauiMessenger.Models;
using MauiMessenger.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MauiMessenger.ViewModels;
using System.Collections.ObjectModel;

namespace MauiMessenger.Services
{
  public static class NavigationService
  {

    public static async Task GoToChatAsync(ChatDTO chat, ObservableCollection<ChatItem> chatItems, List<int> indexes)
    {

      var chatVm = Ioc.Default.GetRequiredService<ChatViewModel>();
      await MainThread.InvokeOnMainThreadAsync(() =>
      {
        chatVm.Chat = chat;
        chatVm.ChatItems = chatItems;
        chatVm.Indexes = indexes;
        chatVm.LastReadMessageId = chat.LastReadMessageId;
      });

      var page = new ChatPage(chatVm);
      await Shell.Current.Navigation.PushModalAsync(page);
      //var param = new ShellNavigationQueryParameters
      //{
      //    { "Chat", chat },

      //};
      //await Shell.Current.GoToAsync(nameof(ChatPage), param);
    }

    public static async Task GoToUserPageAsync(UserDTO user)
    {
      var param = new ShellNavigationQueryParameters
      {
          { "User", user }
      };
      await Shell.Current.GoToAsync(nameof(UserPage), true, param);
    }

    public static async Task GoToChatInfoPageAsync(ChatDTO chat)
    {
      var param = new ShellNavigationQueryParameters
      {
        { "Chat", chat }
      };
      await Shell.Current.GoToAsync(nameof(ChatInfoPage), true, param);
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
