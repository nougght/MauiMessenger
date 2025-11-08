using MauiMessenger.ViewModels;

namespace MauiMessenger.Views;

public partial class ChatsPage : ContentPage
{
  ChatViewModel viewModel;
  public Command ChatClickedCommand {  get; set; }
  public ChatsPage(ChatViewModel vm)
  {

    InitializeComponent();
    this.viewModel = vm;
    BindingContext = viewModel;
    ChatClickedCommand = new Command<Guid>(OnChatClicked);
  }
  protected override async void OnAppearing()
  {
    base.OnAppearing();

  }

  private void OnChatClicked(Guid chatId)
  {
    viewModel.SetChat(chatId);
    Shell.Current.Navigation.PushModalAsync(new ChatPage(viewModel));

  }
}