using CommunityToolkit.Maui.Views;
using MauiMessenger.ViewModels;
namespace MauiMessenger.Views;

public partial class GroupCreationPopup : Popup
{
  ChatViewModel viewModel;
  public GroupCreationPopup(ChatViewModel viewModel)
  {
    InitializeComponent();
    this.viewModel = viewModel;
    BindingContext = this.viewModel;
    this.viewModel.ClosePopupRequest += () => CloseAsync();
  }
}
