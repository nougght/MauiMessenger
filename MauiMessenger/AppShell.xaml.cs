using MauiMessenger.Views;

namespace MauiMessenger
{
  public partial class AppShell : Shell
  {
    public AppShell()
    {

#if WINDOWS || MACCATALYST
    FlyoutBehavior = FlyoutBehavior.Locked; // боковое меню
#else
      FlyoutBehavior = FlyoutBehavior.Disabled; // только TabBar
#endif
      InitializeComponent();

      FlyoutWidth = 150;
      Items.Add(
        new FlyoutItem
        {
          Title = "Чаты",
          Items =
          {
            new ShellContent
            {
              Content = IPlatformApplication.Current.Services.GetService<ChatsPage>()
            }
          },
          Route="Chats"
        }
      );

      Items.Add(
        new FlyoutItem
        {
          Title = "Контакты",
          Items =
          {
            new ShellContent
            {
              Content = IPlatformApplication.Current.Services.GetService<ContactsPage>()
            }
          },
          Route = "Contacts"
        }
      );

      Items.Add(
        new FlyoutItem
        {
          Title = "Настройки",
          Items =
          {
            new ShellContent
            {
              Content = IPlatformApplication.Current.Services.GetService<SettingsPage>()
            }
          },
          Route = "Settings"
        }
      );

      Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
      Routing.RegisterRoute(nameof(ChatPage), typeof(ChatPage));
      Routing.RegisterRoute(nameof(ChatInfoPage), typeof(ChatInfoPage));
      Routing.RegisterRoute(nameof(UserPage), typeof(UserPage));
      Routing.RegisterRoute(nameof(SearchPage), typeof(SearchPage));
      //Routing.RegisterRoute(nameof(), typeof());
    }

  }
}
