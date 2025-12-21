using MauiMessenger.ApiClient;
using MauiMessenger.Models;
using MauiMessenger.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MauiMessenger.Services
{
  public class AuthService
  {
    readonly DataRepository _data;
    readonly AppStateService _appState;
    readonly SignalRService _signalR;
    readonly Client _api;

    public AuthService(DataRepository data, Client api, AppStateService appState, SignalRService signalR)
    {
      _data = data;
      _api = api;
      _appState = appState;
      _signalR = signalR;
    }

    public async Task InitSession(string accessToken, string refreshToken, UserDTO user)
    {
      await _appState.SetSession(accessToken, refreshToken, user);

      await _signalR.Connect();
      await _signalR.RegisterInHub(user.UserId);
      await _data.LoadChatsAsync();
      await _data.LoadContactsAsync();
    }

    public async Task<AuthResponseStatus> TrySignUp(string username, string password, string? email = null, string? code = null)
    {
      var response = await _api.RegisterAsync(
        new RegisterRequest
        {
          Username = username,
          Password = password,
          Email = email,
          Code = code
        });

      if (response.Status == AuthResponseStatus.Success)
      {
        if (email != null)
        {
          // Navigate to email verification page
          //await NavigationService.GoToVerifyPage(email);

        }
        await InitSession(accessToken: response.AccessToken,
          refreshToken: response.RefreshToken,
          user: response.User
        );
      }
      return response.Status;
    }

    public async Task<RegisterCheckResponse> CheckForRegister(string username, string? email)
    {
      var response = await _api.CheckAsync(
        new RegisterCheckRequest
        {
          Username = username,
          Email = email
        });
      return response;
    }
    public async Task<AuthResponseStatus> TrySignIn(string? username, string password, string? email = null)
    {
      if (username == null && email == null)
        throw new Exception();

      var response = await _api.LoginAsync(
        new LoginRequest
        {
          Username = username,
          Password = password,
          Email = email
        });

      if (response.Status == AuthResponseStatus.Success)
      {
        await InitSession(accessToken: response.AccessToken,
          refreshToken: response.RefreshToken,
          user: response.User
        );
      }
      return response.Status;
    }

    public async Task SendVerificationCode(string email)
    {
      await _api.RecoverAsync(email);
    }

    public async Task<AuthResponseStatus> TryVerifyCode(string email, string code)
    {
      var response = await _api.VarifyCodeAsync(
        new RecoverCodeVerifyRequest
        {
          Email = email,
          Code = code
        });

      if (response.Status == AuthResponseStatus.Success)
      {
        await InitSession(accessToken: response.AccessToken,
          refreshToken: response.RefreshToken,
          user: response.User
        );
      }
      return response.Status;
    }


    public string? ValidateEmail(string value)
    {
      if (string.IsNullOrWhiteSpace(value))
        return "Email не может быть пустым";
      else if (value.Contains(' '))
        return "Email не может содержать пробел";
      else if (value.Count(c => c == '@') != 1)
        return "Email должен содержать @ и только один";
      else if (!value.Contains('.'))
        return "Email должен содержать '.'";
      else if (value.Length < 3)
        return "Email не может быть короче 3 символов";
      else
        return null;

    }


    public string? ValidateUsername(string value)
    {
      if (string.IsNullOrWhiteSpace(value))
        return "Логин не может быть пустым";
      else if (value.Contains(' '))
        return "Логин не может содержать пробел";
      else if (value.Length < 3)
        return "Логин не может быть короче 3 символов";
      else if (value.Any(c => c > 127))
        return "Логин может содержать буквы только латинского алфавита";
      else
        return null;

    }

    public string? ValidatePassword(string value)
    {
      if (string.IsNullOrWhiteSpace(value))
        return "Пароль не может быть пустым";
      else if (value.Contains(' '))
        return "Пароль не может содержать пробел";
      else if (value.Length < 8)
        return "Пароль не может быть короче 8 символов";
      else if (value.Any(c => c > 127))
        return "Пароль может содержать буквы только латинского алфавита";
      else
        return null;

    }

  }
}
