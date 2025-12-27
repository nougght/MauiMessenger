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
  public partial class UserProfileViewModel : BaseViewModel
  {
    private readonly AppStateService _appState;
    private readonly DataRepository _data;


    [ObservableProperty]
    private UserDTO user;

    [ObservableProperty]
    private string? avatarURL;

    public Command ChangeAvatarClickedCommand { get; }

    //public Command ChatWithUserCommand { get; }

    public Command BackButtonClickedCommand { get; }
    public UserProfileViewModel(DataRepository data, AppStateService appState, Client api)
    {
      _appState = appState;
      _data = data;
      User = _appState.CurrentUser;

      LoadAvatarURL();

      ChangeAvatarClickedCommand = new Command(async () => await OnChangeAvatarAsync(), () => true);
      //ChatWithUserCommand = new Command(async () => await ChatWithUser(), () => true);

      BackButtonClickedCommand = new Command(async () => await NavigationService.GoBackAsync());
    }
     
    partial void OnUserChanged(UserDTO value)
    {
      //IsInContacts = _data.GetContactByUser(value.UserId) == null ? false : true;
    }


    private async Task OnChangeAvatarAsync()
    {
      var res = await FileService.PickFileAsync();
      await MainThread.InvokeOnMainThreadAsync(async () =>
      {
        byte[] bytes;
        using (var inputStream = await res[0].OpenReadAsync())
        {
          using var ms = new MemoryStream();
          await inputStream.CopyToAsync(ms);
          bytes = ms.ToArray();
        }
        var editPage = new EditImagePage(ImageSource.FromStream(() => new MemoryStream(bytes)));

        await Shell.Current.Navigation.PushModalAsync(editPage);

        var result = await editPage.ShowAsync();

        AvatarURL = await _data.UpdateAvatarAsync(
          ownerId: User.UserId,
          ownerType: "user",
          contentType: res[0].ContentType,
          stream: result == null ? await res[0].OpenReadAsync() : result
          );
      });
    }


    public async Task LoadAvatarURL()
    {
      var url = await _data.GetAvatarURLAsync(_appState.CurrentUser.UserId.ToString());
      //if(!string.IsNullOrEmpty(url))
      {
        AvatarURL = url;
      }

    }


  }
}