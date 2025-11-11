using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using MauiMessenger.Models;
using MauiMessenger.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MauiMessenger.Views;

namespace MauiMessenger.ViewModels
{
  public partial class ChatsTabViewModel : BaseViewModel
  {

    private readonly ChatService _chatService;


    private IPopupService _popupService { get; set; }

    public ObservableCollection<ChatDTO> Chats { get; set; }


    public Command ChatClickedCommand { get; set; }
    public Command ToNewPrivateChatPageCommand { get; }
    public Command ToGroupCreationPopupCommand { get; }

    public delegate void Notify();
    public event Notify ClosePopupRequest;


    [ObservableProperty]
    private string newGroupName;



    public ChatsTabViewModel(ChatService chatService)
    {
      _chatService = chatService;

      _popupService = new PopupService();

      Chats = chatService.GetChats();

      ChatClickedCommand = new Command<Guid>(async (chatId) => await OnChatClicked(chatId), (chatId) => true);
      ToNewPrivateChatPageCommand = new Command(
        async () => await NavigationService.GoToSearchPageAsync() // go to search page
        , () => true);
      ToGroupCreationPopupCommand = new Command(
        async () => await _popupService.ShowPopupAsync<GroupCreationPopup>(Shell.Current)
      // go to group Creation
        , () => true);
    }

    private async Task OnChatClicked(Guid chatId)
    {
      IsBusy = true;
      var chat = _chatService.GetChat(chatId);
      await _chatService.LoadMessagesAsync(chatId);
      IsBusy = false;
      await NavigationService.GoToChatAsync(chat);
      // go to chat page

    }


  }
}
