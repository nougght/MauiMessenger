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


    public Command SignInButtonClickedCommand { get; }
    public Command ToSignUpButtonClickedCommand { get; }
    public Command BackButtonClickedCommand { get; }


    [ObservableProperty]
    private string username;

    [ObservableProperty]
    private string password;


    public bool IsSignInButtonEnabled
    {
      get
      {
        return UsernameError == null && PasswordError == null;
      }
    }


    [NotifyPropertyChangedFor(nameof(IsSignInButtonEnabled))]
    [ObservableProperty]
    private string? usernameError;

    [NotifyPropertyChangedFor(nameof(IsSignInButtonEnabled))]
    [ObservableProperty]
    private string? passwordError;

    [NotifyPropertyChangedFor(nameof(IsSignInButtonEnabled))]
    [ObservableProperty]
    private string? responseError;

    public LoginViewModel(DataRepository data, AppStateService appState, AuthService authService)
    {
      _data = data;
      _appState = appState;
      _authService = authService;

      ToSignUpButtonClickedCommand = new Command(async () => await NavigationService.GoToRegisterPage(), () => true);

      SignInButtonClickedCommand = new Command(async () => await OnSignInClicked(), () => true);

      BackButtonClickedCommand = new Command(async () => await NavigationService.GoBackAsync());
    }


    partial void OnUsernameChanged(string value)
    {
      UsernameError = _authService.ValidateUsername(value);
    }

    partial void OnPasswordChanged(string value)
    {
      PasswordError = _authService.ValidatePassword(value);
    }

    // initialization data before login
    public async Task Init()
    {
      if (_data.ChatTypes.Count == 0)
      {
        await _data.LoadEnumsAsync();

      }

    }


    public async Task OnSignInClicked()
    {
      var username = Username.Contains('@') ? null : Username;
      var email = Username.Contains('@') ? Username : null;
      var status = await _authService.TrySignIn(username: username, password: this.Password, email: email);
      if (status != AuthResponseStatus.Success)
      {
        ResponseError = status switch
        {
          AuthResponseStatus.Error => "Ошибка",
          _ => "Пользователь с таким логином не найден"
        };
      }
      else
      {
        Application.Current.MainPage = new AppShell();
      }
    }


  }

}
