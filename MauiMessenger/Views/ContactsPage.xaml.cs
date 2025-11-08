using MauiMessenger.ViewModels;

namespace MauiMessenger.Views;

public partial class ContactsPage : ContentPage
{
  ChatViewModel viewModel;

  public ContactsPage(ChatViewModel vm)
  {
    InitializeComponent();
    this.viewModel = vm;
    BindingContext = viewModel;
  }
}