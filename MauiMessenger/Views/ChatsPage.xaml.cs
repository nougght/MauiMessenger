using MauiMessenger.ViewModels;

namespace MauiMessenger.Views;

public partial class ChatsPage : ContentPage
{
  ChatsTabViewModel viewModel;
  public ChatsPage(ChatsTabViewModel vm)
  {

    InitializeComponent();
    this.viewModel = vm;
    BindingContext = viewModel;
  }
  protected override async void OnAppearing()
  {
    base.OnAppearing();

  }

  //private void OnChatClicked(Guid chatId)
  //{
  //  viewModel.SetChat(chatId);
  //  Shell.Current.Navigation.PushModalAsync(new ChatPage(viewModel, viewModel.Chat));

  //}
}