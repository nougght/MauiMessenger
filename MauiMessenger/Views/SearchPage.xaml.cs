using MauiMessenger.ViewModels;

namespace MauiMessenger.Views;

public partial class SearchPage : ContentPage
{
  SearchViewModel viewModel;
  public SearchPage(SearchViewModel vm)
  {
    InitializeComponent();
    this.viewModel = vm;
    BindingContext = viewModel;
  }

}