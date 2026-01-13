using CommunityToolkit.Mvvm.DependencyInjection;
using MauiMessenger.Services;
using MauiMessenger.ViewModels;
using MauiMessenger.Views;

namespace MauiMessenger
{
  public partial class App : Application
  {
    public App(IServiceProvider services)
    {

      AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

      TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;



      //App.Current.Dispatcher.UnhandledException += (sender, e) =>
      //{
      //  File.WriteAllText("crash_ui.txt", e.Exception.ToString());
      //  e.Handled = true;
      //};

      InitializeComponent();

      var mainVM = services.GetService<MainViewModel>();
      // запуск страницы входа
      NavigationService.GoToRegisterPage();
      mainVM.TryEnter();

    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
      var window = base.CreateWindow(activationState);
      window.Width = 500;
      window.Height = 700;
      return window;
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
      var ex = e.ExceptionObject as Exception;
      //File.WriteAllText("crash_unhandled.txt", e.ExceptionObject.ToString());
      ShowError(ex);
    }
    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
      //File.WriteAllText("crash_task.txt", e.Exception.ToString());
      ShowError(e.Exception);
      e.SetObserved();
    }

    private void ShowError(Exception ex)
    {
    //#if DEBUG
      MainThread.BeginInvokeOnMainThread(async () =>
      await Application.Current.MainPage.DisplayAlert("Ошибка", ex.ToString(), "OK"));
    //#else
    //// В релизе — логируем
    //#endif
    }
  }


}
