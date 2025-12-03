using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.DependencyInjection;
using MauiMessenger.Models;
using MauiMessenger.Services;
using MauiMessenger.ViewModels;
using MauiMessenger.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MauiMessenger
{
  public static class MauiProgram
  {
    public static MauiApp CreateMauiApp()
    {
      var builder = MauiApp.CreateBuilder();
      builder.Services.AddSingleton<MainViewModel>();

      builder.Services.AddSingleton<ChatsTabViewModel>();
      builder.Services.AddSingleton<ContactsTabViewModel>();
      builder.Services.AddSingleton<SettingsTabViewModel>();

      builder.Services.AddSingleton<SignalRService>();
      builder.Services.AddSingleton<DataRepository>();
      builder.Services.AddSingleton<AppStateService>();
      builder.Services.AddSingleton<AuthService>();
      builder.Services.AddSingleton<ChatService>();
      builder.Services.AddSingleton<Client>(
        s =>
        {
          //var baseUrl = "http://10.0.2.2:8080
          var baseUrl = "http://127.0.0.1:8080";
          var httpClient = new HttpClient
          {
            BaseAddress = new Uri(baseUrl)
          };
          return new Client(baseUrl, httpClient);
        });

      builder
          .UseMauiApp<App>().UseMauiCommunityToolkit()
          .ConfigureFonts(fonts =>
          {
            fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
          });
      builder.Services.AddTransient<ChatsPage>();
      builder.Services.AddTransient<ContactsPage>();
      builder.Services.AddTransient<SettingsPage>();

      builder.Services.AddTransient<GroupCreationPopup>();


      builder.Services.AddTransient<LoginViewModel>();
      builder.Services.AddTransient<ChatViewModel>();
      builder.Services.AddTransient<ChatInfoViewModel>();
      builder.Services.AddTransient<GroupCreationViewModel>();
      builder.Services.AddTransient<SearchViewModel>();
      builder.Services.AddTransient<UserPageViewModel>();

#if DEBUG
      builder.Logging.AddDebug();
#endif
      var app = builder.Build();

      // 🧠 Важно: подключаем DI к CommunityToolkit
      Ioc.Default.ConfigureServices(app.Services);

      return app;
    }
  }
}
