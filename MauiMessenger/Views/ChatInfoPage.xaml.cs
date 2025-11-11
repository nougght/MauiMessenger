using MauiMessenger.Services;
using MauiMessenger.ViewModels;

namespace MauiMessenger.Views;

public partial class ChatInfoPage : ContentPage
{
  ChatViewModel viewModel;
  ChatDTO chat;
  public Command ChatMemberClickedCommand { get; set; }
  public ChatInfoPage(ChatViewModel vm, ChatDTO chat)
  {
    InitializeComponent();
    this.viewModel = vm;
    this.chat = chat;
    BindingContext = this;
    ChatMemberClickedCommand = new Command<Guid>(async (userId) => await viewModel.ToUserPage(userId), (userId) => true);
  }
  public async void BackButton_Clicked(object sender, EventArgs e)
  {
    await Shell.Current.Navigation.PopModalAsync();
  }

}