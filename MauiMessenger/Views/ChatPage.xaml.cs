using MauiMessenger.Models;
using MauiMessenger.Services;
using MauiMessenger.ViewModels;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;

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

  }
}



public partial class ChatPage : ContentPage
{

  public ChatViewModel viewModel;

  //public ObservableCollection<MessageDto> Messages { get => viewModel.Messages; };
  //public string Message
  public ChatPage(ChatViewModel vm)
  {
    InitializeComponent();

    this.viewModel = vm;
    BindingContext = viewModel;
    viewModel.MessageSent += () => 
      { ScrollMessagesToBottom(viewModel, EventArgs.Empty); };

    MessageTemplateSelector.CurrentUserId = vm.User.UserId;
    //vm.MessageSent += ScrollMessagesToBottom;
#if DEBUG
    Debug.WriteLine($"[ChatPage] BindingContext = {BindingContext?.GetType().Name}");
    Debug.WriteLine($"[ChatPage] Messages = {viewModel.Messages?.Count}");
#endif
  }

  protected override async void OnAppearing()
  {
    base.OnAppearing();
    //this.viewModel.OnAppearing();
    //await Task.Delay(1000);
    //if (viewModel.Messages.Count > 0)
    //{
    //  ScrollMessagesToBottom();
    //}
    ScrollMessagesToUnread(this, EventArgs.Empty);

  }

  void OnScrolled(object sender, ItemsViewScrolledEventArgs e)
  {
    var firstVisivle = e.FirstVisibleItemIndex;
    var lastVisible = e.LastVisibleItemIndex;

    viewModel.OnVisibleRangeChanged(firstVisivle, lastVisible);
  }

  private async void ScrollMessagesToIndex(int index)
  {
    if (MessagesView.ItemsSource is ObservableCollection<MessageDTO> items && items.Count > 0)
    {
      if (items.Count <= index)
      {
        index = items.Count - 1;
      }
      var item = items[index];
      MessagesView.ScrollTo(index, position: ScrollToPosition.MakeVisible, animate: false);
    }
  }

  private async void ScrollMessagesToUnread(object sender, EventArgs e)
  {
    var readPosition = viewModel.GetReadPosition();
    viewModel.OnVisibleRangeChanged(readPosition, readPosition + 3);
    ScrollMessagesToIndex(readPosition + 3);

  }


  private async void ScrollMessagesToBottom(object sender, EventArgs e)
  {
    if (MessagesView.ItemsSource is ObservableCollection<MessageDTO> items && items.Count > 0)
    {
      var last = items[items.Count - 1];
      MessagesView.ScrollTo(last, position: ScrollToPosition.MakeVisible, animate: false);
    }
  }

  public async void BackButton_Clicked(object sender, EventArgs e)
  {

    await Shell.Current.Navigation.PopModalAsync();
  }

  private void MessagesView_Scrolled(object sender, ItemsViewScrolledEventArgs e)
  {

  }
  //public async void MessagesView_Loaded(object sender, EventArgs e)
  //{
  //  await ScrollMessagesToBottom();
  //}

  //public async Task ScrollMessagesToBottom()
  //{
  //  if (MessagesView.ItemsSource is ObservableCollection<MessageDTO> items && items.Count > 0)
  //  {
  //    var last = items[items.Count - 1];
  //    MessagesView.ScrollTo(last, position: ScrollToPosition.End, animate: true);
  //  }
  //}

}