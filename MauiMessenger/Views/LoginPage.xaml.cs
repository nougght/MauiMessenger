using MauiMessenger.ViewModels;

namespace MauiMessenger.Views;

public partial class LoginPage : ContentPage
{
  public IEnumerable<string> IdTokenClaims { get; set; } = new string[] { "No claims found in ID token" };
  ChatViewModel viewModel;
  public string Username { get; set; }
  public LoginPage(ChatViewModel vm)
  {
    InitializeComponent();
    viewModel = vm;
    BindingContext = this;
  }

  protected override async void OnAppearing()
  {
    base.OnAppearing();
    if (viewModel.ChatTypes.Count == 0)
    {
      await viewModel.LoadEnums();

    }
  }

  protected override bool OnBackButtonPressed() { return true; }

  private async void SignInButton_Clicked(object sender, EventArgs e)
  {
    Application.Current.MainPage = new AppShell();
    await viewModel.Login(Username);
    await viewModel.Connect();
    await viewModel.RegisterInHub();
  }
}