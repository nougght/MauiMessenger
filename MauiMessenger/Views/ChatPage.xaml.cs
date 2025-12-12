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
  public DataTemplate ServiceTemplate { get; set; }
  public DataTemplate DaySeparatorTemplate { get; set; }
  public DataTemplate UnreadMarkerTemplate { get; set; }
  public DataTemplate LoadingTemplate { get; set; }
  public static Guid CurrentUserId { get; set; }

  protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
  {
    return item switch
    {
      MessageItem msg when msg.Message.SenderId == CurrentUserId => OutgoingTemplate,
      MessageItem msg when msg.Message.SenderId != CurrentUserId => IncomingTemplate,
      ServiceMessageItem => ServiceTemplate,
      DaySeparatorItem => DaySeparatorTemplate,
      UnreadMarkerItem => UnreadMarkerTemplate,
      _ => LoadingTemplate
    };

  }
}

public class VisibleBehavior : Behavior<VisualElement>
{
  protected override void OnAttachedTo(VisualElement bindable)
  {
    base.OnAttachedTo(bindable);
    bindable.Loaded += Bindable_Loaded;
    bindable.Unloaded += Bindable_Unloaded;
  }

  private void Bindable_Loaded(object sender, EventArgs e)
  {
    if (sender is VisualElement ve && ve.BindingContext is ChatItem item)
      item.IsVisible = true;
  }

  private void Bindable_Unloaded(object sender, EventArgs e)
  {
    if (sender is VisualElement ve && ve.BindingContext is ChatItem item)
      item.IsVisible = false;
  }

  protected override void OnDetachingFrom(VisualElement bindable)
  {
    bindable.Loaded -= Bindable_Loaded;
    bindable.Unloaded -= Bindable_Unloaded;
    base.OnDetachingFrom(bindable);
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
    Debug.WriteLine($"[ChatPage] Messages = {viewModel.ChatItems?.Count}");
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
    await Task.Delay(500);
    ScrollMessagesToUnread(this, EventArgs.Empty);

  }

  bool _scrollEventWorked = false;

  void OnScrolled(object sender, ItemsViewScrolledEventArgs e)
  {
    _scrollEventWorked = true;

    var firstVisivle = e.FirstVisibleItemIndex;
    var lastVisible = e.LastVisibleItemIndex;

    viewModel.OnVisibleRangeChanged(firstVisivle, lastVisible, DateTime.UtcNow);

  }

  private async void ScrollMessagesToIndex(int index)
  {
        if (index == 0)
            return;

    if (MessagesView.ItemsSource is ObservableCollection<ChatItem> items && items.Count > 0)
    {
      if (viewModel.Indexes.Count <= index)
      {
        index = viewModel.Indexes[viewModel.Indexes.Count - 1];
      }
      var item = items[index];
      await MainThread.InvokeOnMainThreadAsync(() => 
      MessagesView.ScrollTo(item, position: ScrollToPosition.MakeVisible, animate: false)
      );
    }
  }



  private async void ScrollMessagesToUnread(object sender, EventArgs e)
  {
    var readPosition = viewModel.GetReadPosition();
    //viewModel.OnVisibleRangeChanged(readPosition, readPosition + 3, DateTime.UtcNow);

    _scrollEventWorked = false;

    // Просим MAUI проскроллить
    ScrollMessagesToIndex(readPosition == -1 ? viewModel.Indexes.LastOrDefault(i => true) : readPosition + 3);


    // Даём MAUI время вызвать событие (они вызываются на следующем UI тикe)
    await Task.Delay(50);

    if (!_scrollEventWorked)
    {
      var chatItems = MessagesView.ItemsSource as ObservableCollection<ChatItem>;
      var msgs = chatItems.OfType<MessageItem>().ToList();

      var i = 0;
      while (i < msgs.Count && msgs[i].IsVisible)
      {
        ++i;
      }
      if (i > 0)
      {
        i = i == msgs.Count ? i - 1 : i;
        await viewModel.OnVisibleRangeChanged(readPosition, chatItems.IndexOf(msgs[i] as ChatItem), DateTime.UtcNow);
      }
    }

  }


  private async void ScrollMessagesToBottom(object sender, EventArgs e)
  {
    if (MessagesView.ItemsSource is ObservableCollection<ChatItem> items && items.Count > 0)
    {
      var last = items[items.Count - 1];
      MessagesView.ScrollTo(last, position: ScrollToPosition.MakeVisible, animate: false);
    }
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