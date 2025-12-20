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
        public Command BackButtonClickedCommand { get; }


        [ObservableProperty]
        private string username;

        [ObservableProperty]
        private string password;

        public LoginViewModel(DataRepository data, AppStateService appState, AuthService authService)
        {
            _data = data;
            _appState = appState;
            _authService = authService;

            SignInButtonClickedCommand = new Command(async () => await OnSignInClicked(), () => true);

            BackButtonClickedCommand = new Command(async () => await NavigationService.GoBackAsync());
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
            await _authService.TrySignIn(this.Username, this.Password);
            Application.Current.MainPage = new AppShell();
        }


    }

}
