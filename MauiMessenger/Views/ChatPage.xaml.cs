using MauiMessenger.ViewModels;
using MauiMessenger.ApiClient;

namespace MauiMessenger.Views;

public class MessageTemplateSelector : DataTemplateSelector
{
  public DataTemplate IncomingTemplate { get; set; }
  public DataTemplate OutgoingTemplate { get; set; }

  public static Guid CurrentUserId { get; set; } // сюда пробросим VM данные

  protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
  {
    if (item is MessageDto message)
    {
      if (message.SenderId == CurrentUserId)
      {
        return OutgoingTemplate;
      }
    }
    return IncomingTemplate;

  }
}
public partial class ChatPage : ContentPage
{

  ChatViewModel viewModel;
  public ChatPage(ChatViewModel vm)
  {
    InitializeComponent();

    this.viewModel = vm;
    BindingContext = viewModel;
    MessageTemplateSelector.CurrentUserId = vm.User.UserId;
  }

  public async void BackButton_Clicked(object sender, EventArgs e)
  {

    await Shell.Current.Navigation.PopModalAsync();
  }
  private async void MessagesView_Loaded(object sender, EventArgs e)
  {
    await ScrollMessagesToBottom();
  }
  public async Task ScrollMessagesToBottom()
  {
    if (MessagesView.ItemsSource is IList<MessageDto> items && items.Count > 0)
    {
      var last = items[items.Count - 1];
      MessagesView.ScrollTo(last, position: ScrollToPosition.End, animate: true);
    }
  }
}