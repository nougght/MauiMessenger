using CommunityToolkit.Mvvm.ComponentModel;
using MauiMessenger.ApiClient;
using MauiMessenger.Models;
using MauiMessenger.Services;
using MauiMessenger.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiMessenger.ViewModels
{
  public class SettingsTabViewModel : BaseViewModel
  {
    private readonly AppStateService _appState;
    private readonly DataRepository _data;
    private readonly AuthService _authService;

    public Command LogoutClickedCommand { get; }

    //public Command ChatWithUserCommand { get; }

    public Command BackButtonClickedCommand { get; }
    public SettingsTabViewModel(DataRepository data, AppStateService appState, Client api,
      AuthService authService)
    {
      _appState = appState;
      _data = data;
      _authService = authService;

      LogoutClickedCommand = new Command(async () => await OnLogoutAsync(), () => true);
    }



    private async Task OnLogoutAsync()
    {
      await _authService.Logout();
    }


  }
}