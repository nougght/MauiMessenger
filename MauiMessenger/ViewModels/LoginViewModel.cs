using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MauiMessenger.Services;
using MauiMessenger.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MauiMessenger.ViewModels
{
  public partial class LoginViewModel : BaseViewModel
  {

    private readonly DataRepository _data;
    private readonly AppStateService _appState;
    private readonly AuthService _authService;


    public Command SignInButtonClickedCommand;


    [ObservableProperty]
    private string _username;

    public LoginViewModel(DataRepository data, AppStateService appState, AuthService authService)
    {
      _data = data;
      _appState = appState;
      _authService = authService;

      SignInButtonClickedCommand = new Command(async () => await _authService.TrySignIn(this.Username));

    }

    // initialization data before login
    public async Task Init()
    {
      if (_data.ChatTypes.Count == 0)
      {
        await _data.LoadEnumsAsync();

      }

    }

    


  }

}
