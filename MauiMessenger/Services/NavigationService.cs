using CommunityToolkit.Mvvm.DependencyInjection;
using MauiMessenger.Models;
using MauiMessenger.ViewModels;
using MauiMessenger.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MauiMessenger.Services
{
  public static class NavigationService
  {

    public static async Task GoToChatAsync(ConversationDTO chat, ObservableCollection<ChatItem> chatItems, List<int> indexes, 
      GroupChatDetailsDTO? groupDetails = null, PrivateChatDetailsDTO? privateDetails = null, ChannelDetailsDTO? channelDetails = null)
    {
      var chatVm = Ioc.Default.GetRequiredService<ChatViewModel>();
      await MainThread.InvokeOnMainThreadAsync(() =>
      {
        chatVm.Conversation = chat;
        chatVm.ChatItems = chatItems;
        chatVm.Indexes = indexes;
        chatVm.LastReadMessageId = chat.LastReadMessageId;
        chatVm.LastReadMessageIndex = chatItems.IndexOf(chatItems.FirstOrDefault(x => x is MessageItem msg && msg.Message.Id == chat.LastReadMessageId));
        chatVm.GroupChatDetails = groupDetails;
        chatVm.PrivateChatDetails = privateDetails;
        chatVm.ChannelDetails = channelDetails;
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

    public static async Task GoToChatInfoPageAsync(ConversationDTO chat)
    {
      var param = new ShellNavigationQueryParameters
      {
        { "Chat", chat }
      };
      await Shell.Current.GoToAsync(nameof(ChatInfoPage), true, param);
    }

    public static async Task GoToSearchPageAsync()
    {
      //var vm = Ioc.Default.GetRequiredService<SearchViewModel>();
      //var page = new SearchPage(vm);
      //await Shell.Current.Navigation.PushModalAsync(page);
      await Shell.Current.GoToAsync(nameof(SearchPage));
    }

    public static async Task GoBackAsync() => await Shell.Current.GoToAsync("..");

    //public static async Task GoToLoginAsync()
    //{
    //  Application.Current.MainPage = new LoginPage();
    //}


    //Shell.Current.GoToAsync("//login");







    public static async Task GoToLoginPage()
    {
      await MainThread.InvokeOnMainThreadAsync(() =>
      {
        var loginVm = Ioc.Default.GetRequiredService<LoginViewModel>();
        Application.Current!.MainPage = new NavigationPage(new LoginPage(loginVm));
      });
    }

    public static async Task GoToRegisterPage()
    {
      await MainThread.InvokeOnMainThreadAsync(() =>
      {
        var registerVm = Ioc.Default.GetRequiredService<RegisterViewModel>();
        Application.Current!.MainPage = new NavigationPage(new RegisterPage(registerVm));
      });
    }

    public static async Task GoToEmailVerificationPage(string email, bool isPasswordRecovery)
    {
      var verifyVm = Ioc.Default.GetRequiredService<CodeVerificationViewModel>();
      await MainThread.InvokeOnMainThreadAsync(() =>
      {
        verifyVm.Email = email;
        verifyVm.IsPasswordRecovery = isPasswordRecovery;
      });

      var page = new CodeVerificationPage(verifyVm);
      await MainThread.InvokeOnMainThreadAsync(async () =>
      {
        await Application.Current!
            .MainPage!
            .Navigation
            .PushAsync(page);
      });
    }

  }

}
