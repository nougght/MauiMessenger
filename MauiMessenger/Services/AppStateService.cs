using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using MauiMessenger.Models;



namespace MauiMessenger.Services
{
  public partial class AppStateService : ObservableObject
  {
    [ObservableProperty]
    private UserDTO? currentUser;

    [ObservableProperty]
    private ChatDTO? selectedChat;

    public bool IsAuthorised { get => CurrentUser != null; }



  }
}
