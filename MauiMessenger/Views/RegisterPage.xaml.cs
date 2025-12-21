using MauiMessenger.ViewModels;

namespace MauiMessenger.Views;

public partial class RegisterPage : ContentPage
{
  public IEnumerable<string> IdTokenClaims { get; set; } = new string[] { "No claims found in ID token" };
  RegisterViewModel viewModel;

  public RegisterPage(RegisterViewModel vm)
  {
    InitializeComponent();
    viewModel = vm;
    BindingContext = viewModel;
    
  }

  protected override async void OnAppearing()
  {
    base.OnAppearing();
    //if (viewModel.ChatTypes.Count == 0)
    //{
    //  await viewModel.LoadEnums();

    //}
    await viewModel.Init();
  }

  //protected override bool OnBackButtonPressed() { return true; }


}