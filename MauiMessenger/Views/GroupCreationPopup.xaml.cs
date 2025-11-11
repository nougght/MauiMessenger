using CommunityToolkit.Maui.Views;
using MauiMessenger.ViewModels;
namespace MauiMessenger.Views;

public partial class GroupCreationPopup : Popup
{
  GroupCreationViewModel viewModel;
  public GroupCreationPopup(GroupCreationViewModel vm)
  {
    InitializeComponent();
    this.viewModel = vm;
    BindingContext = this.viewModel;
    this.viewModel.ClosePopupRequest += () => CloseAsync();
  }
}
