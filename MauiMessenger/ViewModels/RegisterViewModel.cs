using CommunityToolkit.Mvvm.ComponentModel;
using MauiMessenger.Models;
using MauiMessenger.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiMessenger.ViewModels
{
  public partial class RegisterViewModel : BaseViewModel
  {

    private readonly DataRepository _data;
    private readonly AppStateService _appState;
    private readonly AuthService _authService;


    public Command SignUpButtonClickedCommand { get; }
    public Command ToSignInButtonClickedCommand { get; }
    public Command BackButtonClickedCommand { get; }


    [ObservableProperty]
    private string username;

    [ObservableProperty]
    private string password;

    [ObservableProperty]
    private string passwordConfirmation;

    [NotifyPropertyChangedFor(nameof(IsSignUpButtonEnabled))]
    [ObservableProperty]
    private bool useEmail = false;

    [ObservableProperty]
    private string email;



    public bool IsSignUpButtonEnabled
    {
      get
      {
        return UsernameError == null && PasswordError == null && PasswordConfirmationError == null && (!UseEmail || EmailError == null);
      }
    }


    [NotifyPropertyChangedFor(nameof(IsSignUpButtonEnabled))]
    [ObservableProperty]
    private string? usernameError;

    [NotifyPropertyChangedFor(nameof(IsSignUpButtonEnabled))]
    [ObservableProperty]
    private string? passwordError;

    [NotifyPropertyChangedFor(nameof(IsSignUpButtonEnabled))]
    [ObservableProperty]
    private string? passwordConfirmationError;

    [NotifyPropertyChangedFor(nameof(IsSignUpButtonEnabled))]
    [ObservableProperty]
    private string? emailError;

    [NotifyPropertyChangedFor(nameof(IsSignUpButtonEnabled))]
    [ObservableProperty]
    private string? responseError;


    public RegisterViewModel(DataRepository data, AppStateService appState, AuthService authService)
    {
      _data = data;
      _appState = appState;
      _authService = authService;

      ToSignInButtonClickedCommand = new Command(async () => await NavigationService.GoToLoginPage(), () => true);

      SignUpButtonClickedCommand = new Command(async () => await OnSignUpClicked(), () => true);

      BackButtonClickedCommand = new Command(async () => await NavigationService.GoBackAsync());

      UsernameError = _authService.ValidateUsername(Username);
    }

    partial void OnUsernameChanged(string value)
    {
      UsernameError = _authService.ValidateUsername(value);
    }

    partial void OnPasswordChanged(string value)
    {
      PasswordError = _authService.ValidatePassword(value);
    }

    partial void OnPasswordConfirmationChanged(string value)
    {
      var error = _authService.ValidatePassword(value);
      PasswordConfirmationError = error ?? (value != Password ? "Пароли должны совпадать" : null);
    }

    partial void OnEmailChanged(string value)
    {
      EmailError = _authService.ValidateEmail(value);
    }

    public async Task Init()
    {
      if (_data.ChatTypes.Count == 0)
      {
        await _data.LoadEnumsAsync();
      }
    }

    public async Task OnSignUpClicked()
    {
      var response = await _authService.CheckForRegister(Username, UseEmail ? Email : null);
      if (!response.IsUsernameFree)
      {
        UsernameError = "Такое имя пользователя уже существует";
      }
      if (response.IsEmailFree != null && !response.IsEmailFree.Value)
      {
        EmailError = "К данной почте уже привязан аккаунт";
      }

      // if errors occurred
      if (IsSignUpButtonEnabled)
      {
        if (UseEmail)
        {
          await _authService.SendVerificationCode(Email);
          _appState.tempUsername = Username;
          _appState.tempPassword = Password;
          _appState.tempEmail = Email;
          NavigationService.GoToEmailVerificationPage(email: Email, isPasswordRecovery: false);
        }
        else
        {
          var status = await _authService.TrySignUp(username: Username, password: Password, email: Email);
          if (status != AuthResponseStatus.Success)
          {
            ResponseError = status switch
            {
              _ => "Ошибка"
            };
          }
          else
          {
            Application.Current.MainPage = new AppShell();
          }
        }
      }
      //else
      //{
      //  var status = await _authService.TrySignUp(username: this.Username, email: this.UseEmail ? Email : null, password: this.Password);

      //  if (status != AuthResponseStatus.Success)
      //  {
      //    ResponseError = status switch
      //    {
      //      AuthResponseStatus.ExistingUsername => "Такое имя пользователя уже существует",
      //      AuthResponseStatus.ExistingEmail => "К данной почте уже привязан аккаунт",
      //      AuthResponseStatus.Error => "Ошибка",
      //      _ => "-------------"
      //    };
      //  }
      //  else
      //  {
      //    Application.Current.MainPage = new AppShell();
      //  }
      //}
    }


  }
}
