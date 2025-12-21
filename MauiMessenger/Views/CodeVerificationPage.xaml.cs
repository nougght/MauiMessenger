using MauiMessenger.ViewModels;

namespace MauiMessenger.Views;

public partial class CodeVerificationPage : ContentPage
{
	CodeVerificationViewModel viewModel;

	public CodeVerificationPage(CodeVerificationViewModel vm)
	{
		InitializeComponent();
		viewModel = vm;
		BindingContext = viewModel;
	}
}