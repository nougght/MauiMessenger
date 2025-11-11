using MauiMessenger.Views;
using MauiMessenger.ViewModels;
using MauiMessenger.Services;

namespace MauiMessenger
{
  public partial class App : Application
  {
    public App(IServiceProvider services)
    {
      InitializeComponent();

      var mainVM = services.GetService<MainViewModel>();

      var vm = services.GetService<LoginViewModel>();

      // запуск страницы входа
      MainPage = new LoginPage(vm);


      AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;


    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
      var ex = e.ExceptionObject as Exception;
      ShowError(ex);
    }
    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
      ShowError(e.Exception);
      e.SetObserved();
    }

    private void ShowError(Exception ex)
    {
    #if DEBUG
      MainThread.BeginInvokeOnMainThread(async () =>
      await Application.Current.MainPage.DisplayAlert("Ошибка", ex.ToString(), "OK"));
    #else
    // В релизе — логируем
    #endif
    }
  }


}
