using Microsoft.Extensions.Logging;
using MauiMessenger.ViewModels;
using MauiMessenger.Views;
using MauiMessenger.Services;
using Microsoft.Extensions.Configuration;
using CommunityToolkit.Maui;
using MauiMessenger.Models;

namespace MauiMessenger
{
  public static class MauiProgram
  {
    public static MauiApp CreateMauiApp()
    {
      var builder = MauiApp.CreateBuilder();
      builder.Services.AddTransient<GroupCreationPopup>();
      builder.Services.AddSingleton<MainViewModel>();
      builder.Services.AddSingleton<SignalRService>();
      builder.Services.AddSingleton<DataRepository>();
      builder.Services.AddSingleton<AppStateService>();
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
      builder.Services.AddTransient<LoginViewModel>();
      builder.Services.AddTransient<ChatViewModel>();
#if DEBUG
      builder.Logging.AddDebug();
#endif

      return builder.Build();
    }
  }
}
