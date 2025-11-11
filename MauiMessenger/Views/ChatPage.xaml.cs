using MauiMessenger.Services;
using MauiMessenger.ViewModels;
using System.Collections.ObjectModel;
using MauiMessenger.Models;

namespace MauiMessenger.Views;

public class MessageTemplateSelector : DataTemplateSelector
{
  public DataTemplate IncomingTemplate { get; set; }
  public DataTemplate OutgoingTemplate { get; set; }

  public static Guid CurrentUserId { get; set; }

  protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
  {
    if (item is MessageDTO message)
    {
      if (message.SenderId == CurrentUserId)
      {
        return OutgoingTemplate;
      }
    }
    return IncomingTemplate;

  }}



public partial class ChatPage : ContentPage
{

  public ChatViewModel viewModel;
  public ChatDTO chat;

  //public ObservableCollection<MessageDto> Messages { get => viewModel.Messages; };
  //public string Message
  public ChatPage(ChatViewModel vm, ChatDTO chat)
  {
    InitializeComponent();

    this.viewModel = vm;
    this.chat = chat;
    BindingContext = viewModel;
    MessageTemplateSelector.CurrentUserId = vm.User.UserId;

  }

  public async void BackButton_Clicked(object sender, EventArgs e)
  {

    await Shell.Current.Navigation.PopModalAsync();
  }
  public async void MessagesView_Loaded(object sender, EventArgs e)
  {
    await ScrollMessagesToBottom();
  }

  public async Task ScrollMessagesToBottom()
  {
    if (MessagesView.ItemsSource is IList<MessageDTO> items && items.Count > 0)
    {
      var last = items[items.Count - 1];
      MessagesView.ScrollTo(last, position: ScrollToPosition.End, animate: true);
    }
  }

}