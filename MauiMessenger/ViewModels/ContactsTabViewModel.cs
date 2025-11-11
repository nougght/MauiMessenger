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
  public partial class  ContactsTabViewModel : BaseViewModel
  {
    private readonly ChatService _chatService;

    public ObservableCollection<ContactDTO> Contacts { get; set; }

    public Command ContactClickedCommand { get; }

    public ContactsTabViewModel(ChatService chatService)
    {
      _chatService = chatService;
      Contacts = chatService.GetContacts();
      ContactClickedCommand = new Command<Guid> (async (userId) => 
    }


    public async Task OnContactClicked(Guid userid)
    {
      await _chatService.GetOrCreateChatAsync(userid);
      // go to chat page
    }


  }
}
