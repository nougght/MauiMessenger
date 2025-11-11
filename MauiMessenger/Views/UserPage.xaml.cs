using MauiMessenger.ViewModels;
using MauiMessenger.Services;

namespace MauiMessenger.Views;

public partial class UserPage : ContentPage
{
  UserPageViewModel viewModel;

  //public Command ChatClickedCommand { get; set; }

  public UserPage(UserPageViewModel vm)
  {
    InitializeComponent();
    this.viewModel = vm;
    this.user = user;
    BindingContext = this;

  }


  public async void BackButton_Clicked(object sender, EventArgs e)
  {
    await Shell.Current.Navigation.PopModalAsync();
  }

  public async void ChatButton_Clicked(object sender, EventArgs e)
  {
    //await Shell.Current.Navigation.PopModalAsync();
    await viewModel.OnChatWithUserClicked(user.UserId);
  }
  public async void AddToContact_Clicked(object sender, EventArgs e)
  {
    await viewModel.AddContact(user.UserId);
  }

}