using MauiMessenger.ViewModels;

namespace MauiMessenger.Views;

public partial class ChatInfoPage : ContentPage
{
  ChatViewModel viewModel;
  public Command ChatClickedCommand { get; set; }
  public ChatInfoPage(ChatViewModel vm)
  {
    InitializeComponent();
    this.viewModel = vm;
    BindingContext = viewModel;

  }
  public async void BackButton_Clicked(object sender, EventArgs e)
  {
    await Shell.Current.Navigation.PopModalAsync();
  }

}