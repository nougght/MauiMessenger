using MauiMessenger.ViewModels;

namespace MauiMessenger.Views;

public partial class TopBar : ContentView
{
	TopBarViewModel viewModel;
  public TopBar()
	{
		InitializeComponent();
		viewModel = IPlatformApplication.Current.Services.GetService<TopBarViewModel>();
    BindingContext = viewModel;
  }

	public void OnAppearing()
	{
		viewModel.LoadAvatarURL();
  }
}