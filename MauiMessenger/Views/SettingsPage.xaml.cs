using MauiMessenger.ViewModels;

namespace MauiMessenger.Views;

public partial class SettingsPage : ContentPage
{
	SettingsTabViewModel viewModel;
	public SettingsPage(SettingsTabViewModel vm)
	{
		InitializeComponent();
		viewModel = vm;
		BindingContext = viewModel;
	}
}