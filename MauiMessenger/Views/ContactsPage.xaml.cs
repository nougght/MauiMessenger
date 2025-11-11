using MauiMessenger.ViewModels;

namespace MauiMessenger.Views;

public partial class ContactsPage : ContentPage
{
  ContactsTabViewModel viewModel;

  public ContactsPage(ContactsTabViewModel vm)
  {
    InitializeComponent();
    this.viewModel = vm;
    BindingContext = viewModel;
  }
}