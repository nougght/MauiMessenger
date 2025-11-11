using MauiMessenger.Services;
using MauiMessenger.ViewModels;

namespace MauiMessenger.Views;

public partial class ChatInfoPage : ContentPage
{
  ChatInfoViewModel viewModel;
  public ChatInfoPage(ChatInfoViewModel vm)
  {
    InitializeComponent();
    this.viewModel = vm;
    BindingContext = viewModel;
    //ChatMemberClickedCommand = new Command<Guid>(async (userId) => await viewModel.ToUserPage(userId), (userId) => true);
  }
  //public async void BackButton_Clicked(object sender, EventArgs e)
  //{
  //  await Shell.Current.Navigation.PopModalAsync();
  //}

}