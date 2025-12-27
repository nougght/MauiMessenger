using CommunityToolkit.Mvvm.ComponentModel;
using MauiMessenger.ApiClient;
using MauiMessenger.Models;
using MauiMessenger.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiMessenger.ViewModels
{
  public partial class TopBarViewModel : BaseViewModel
  {
    private readonly AppStateService _appState;
    private readonly DataRepository _data;



    public Command UserProfileClickedCommand { get; }
    //public Command BackButtonClickedCommand { get; }

    [ObservableProperty]
    private string? avatarURL;


    public TopBarViewModel(AppStateService appState, DataRepository data)
    {
      _appState = appState;
      _data = data;

      LoadAvatarURL();

      UserProfileClickedCommand = new Command(async () =>
        await NavigationService.GoToUserProfileAsync(), () => true);

      //BackButtonClickedCommand = new Command(async () => await NavigationService.GoBackAsync());

    }

    public async Task LoadAvatarURL()
    {
      AvatarURL = await _data.GetAvatarURLAsync(_appState.CurrentUser.UserId.ToString());
    }
  }
}