using MauiMessenger.Models;
using MauiMessenger.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiMessenger.ViewModels
{
  public partial class ContactsTabViewModel : BaseViewModel
  {
    private readonly ChatService _chatService;

    public ObservableCollection<ContactDTO> Contacts { get; set; }

    public Command ContactClickedCommand { get; }

    public ContactsTabViewModel(ChatService chatService)
    {
      _chatService = chatService;
      Contacts = chatService.GetContacts();
      ContactClickedCommand = new Command<Guid>(async (userId) => await OnContactClicked(userId));
    }


    public async Task OnContactClicked(Guid userId)
    {
      var chat = await _chatService.GetOrCreateChatAsync(userId);
      await _chatService.LoadMessagesAsync(chat.Id);

      // go to chat page
      await NavigationService.GoToChatAsync(chat, await _chatService.GetChatItems(chat.Id), await _chatService.GetMessageIndexes(chat.Id),
        privateDetails: await _chatService.GetPrivateChatDetailsAsync(chat.Id), groupDetails: await _chatService.GetGroupChatDetailsAsync(chat.Id),
        channelDetails: await _chatService.GetChannelDetailsAsync(chat.Id));
      // go to chat page
    }


  }
}
