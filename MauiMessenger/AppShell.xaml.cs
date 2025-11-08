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
      Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));

    }

  }
}
