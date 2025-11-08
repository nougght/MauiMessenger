using MauiMessenger.Views;
using MauiMessenger.ViewModels;
using MauiMessenger.ApiClient;
namespace MauiMessenger
{
  public partial class App : Application
  {
    public App(IServiceProvider services)
    {
      InitializeComponent();

      var vm = services.GetService<ChatViewModel>();
      // запуск страницы входа
      MainPage = new LoginPage(vm);


    }
  }
}
