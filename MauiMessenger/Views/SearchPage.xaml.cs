using MauiMessenger.ViewModels;

namespace MauiMessenger.Views;

public partial class SearchPage : ContentPage
{
  ChatViewModel viewModel;
  public SearchPage(ChatViewModel vm)
  {
    InitializeComponent();
    this.viewModel = vm;
    BindingContext = viewModel;
  }

}