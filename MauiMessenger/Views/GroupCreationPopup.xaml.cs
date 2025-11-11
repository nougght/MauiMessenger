using CommunityToolkit.Maui.Views;
using MauiMessenger.ViewModels;
namespace MauiMessenger.Views;

public partial class GroupCreationPopup : Popup
{
  ChatsTabViewModel viewModel;
  public GroupCreationPopup(ChatsTabViewModel viewModel)
  {
    InitializeComponent();
    this.viewModel = viewModel;
    BindingContext = this.viewModel;
    this.viewModel.ClosePopupRequest += () => CloseAsync();
  }
}
