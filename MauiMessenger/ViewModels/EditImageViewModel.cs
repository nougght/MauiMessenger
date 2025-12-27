using CommunityToolkit.Mvvm.ComponentModel;
using MauiMessenger.Models;
using MauiMessenger.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MauiMessenger.Models;

namespace MauiMessenger.ViewModels
{

  public partial class EditImageViewModel : BaseViewModel
  {
    private readonly AppStateService _appState;
    private readonly DataRepository _data;


    [ObservableProperty]
    private Stream image;

    public Command BackButtonClickedCommand { get; }


    public EditImageViewModel(AppStateService appState, DataRepository data, Stream image)
    {
      _appState = appState;
      _data = data;
      Image = image;

      //UserProfileClickedCommand = new Command(async () =>
      //  await NavigationService.GoToUserProfileAsync(), () => true);

      BackButtonClickedCommand = new Command(async () => await Shell.Current.Navigation.PopModalAsync());

    }


    public async Task SaveResult(Stream res)
    {
      await Shell.Current.Navigation.PopModalAsync();
    }
  }
}