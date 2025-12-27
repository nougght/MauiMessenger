using MauiMessenger.ViewModels;

namespace MauiMessenger.Views;

public partial class UserProfilePage : ContentPage
{
	UserProfileViewModel viewModel;
	public UserProfilePage(UserProfileViewModel vm)
	{
		InitializeComponent();
		viewModel = vm;
		BindingContext = viewModel;
	}
}