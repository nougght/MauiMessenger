using CommunityToolkit.Mvvm.ComponentModel;
using MauiMessenger.Models;
using MauiMessenger.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
namespace MauiMessenger.ViewModels
{

  [QueryProperty(nameof(Email), "Email")]
  [QueryProperty(nameof(IsPasswordRecovery), "IsPasswordRecovery")]
  public partial class CodeVerificationViewModel : BaseViewModel
  {

    private readonly DataRepository _data;
    private readonly AppStateService _appState;
    private readonly AuthService _authService;


    public Command VerifyCodeButtonClickedCommand { get; }
    public Command BackButtonClickedCommand { get; }

    private string username;

    [ObservableProperty]
    private string email;

    [ObservableProperty]
    private bool isPasswordRecovery;


    [NotifyPropertyChangedFor(nameof(IsCodeValid))]
    [ObservableProperty]
    private string code;

    [ObservableProperty]
    private string? codeError;

    public bool IsCodeValid => !string.IsNullOrEmpty(Code) && Code.Length == 6;


    public CodeVerificationViewModel(DataRepository data, AppStateService appState, AuthService authService)
    {
      _data = data;
      _appState = appState;
      _authService = authService;

      VerifyCodeButtonClickedCommand = new Command(async () => await OnVerifyCodeClicked(), () => true);

      BackButtonClickedCommand = new Command(async () => await NavigationService.GoBackAsync());
    }

    partial void OnCodeChanged(string? oldValue, string newValue)
    {
      string regex = newValue;
      if (String.IsNullOrEmpty(regex))
        return;

      // If the text field only contains numbers then leave.
      if (!Regex.Match(regex, "^[0-9]+$").Success)
      {
        Code = oldValue ?? string.Empty;
      }
    }

    public async Task OnVerifyCodeClicked()
    {
      AuthResponseStatus status;
      if (IsPasswordRecovery)
      {
        status = await _authService.TryVerifyCode(email: Email, code: Code);
      }
      else
      {
        status = await _authService.TrySignUp(username: _appState.tempUsername!, password: _appState.tempPassword!, email: _appState.tempEmail, code: Code);
      }




      if (status == AuthResponseStatus.InValidRecoveryCode)
      {
        codeError = "Неверный код подтверждения";
      }
      else if (status == AuthResponseStatus.Success)
      {
        if (IsPasswordRecovery)
        {
          // go to password update page
        }
        else
        {
          Application.Current.MainPage = new AppShell();
        }
      }
    }


  }

}
