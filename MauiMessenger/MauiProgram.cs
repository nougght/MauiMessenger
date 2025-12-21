using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.DependencyInjection;

using MauiMessenger.Models;
using MauiMessenger.Services;
using MauiMessenger.ApiClient;

using MauiMessenger.ViewModels;
using MauiMessenger.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace MauiMessenger
{
  public static class MauiProgram
  {
    public static MauiApp CreateMauiApp()
    {
      var builder = MauiApp.CreateBuilder(); builder.Services.AddTransient<AuthMessageHandler>();

      builder.Services.AddHttpClient("Api", http =>
      {
        // 127.0.0.1
        // 10.0.2.2
        http.BaseAddress = new Uri("http://127.0.0.1:8080");
      })
.AddHttpMessageHandler<AuthMessageHandler>();

      builder.Services.AddSingleton<Client>(sp =>
      {
        var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient("Api");

        return new Client(httpClient.BaseAddress!.ToString(), httpClient);
      });

      builder.Services.AddSingleton<ChatsTabViewModel>();
      builder.Services.AddSingleton<ContactsTabViewModel>();
      builder.Services.AddSingleton<SettingsTabViewModel>();

      builder.Services.AddSingleton<SignalRService>();
      builder.Services.AddSingleton<DataRepository>();
      builder.Services.AddSingleton<AppStateService>();
      builder.Services.AddSingleton<AuthService>();
      builder.Services.AddSingleton<ChatService>();
      builder.Services.AddSingleton<MainViewModel>();


      builder
          .UseMauiApp<App>().UseMauiCommunityToolkit()
          .UseMauiCommunityToolkitMediaElement()
          .ConfigureFonts(fonts =>
          {
            fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
          });
      builder.Services.AddTransient<ChatsPage>();
      builder.Services.AddTransient<ContactsPage>();
      builder.Services.AddTransient<SettingsPage>();

      builder.Services.AddTransient<GroupCreationPopup>();


      builder.Services.AddTransient<RegisterViewModel>();
      builder.Services.AddTransient<CodeVerificationViewModel>();
      builder.Services.AddTransient<LoginViewModel>();
      builder.Services.AddTransient<ChatViewModel>();
      builder.Services.AddTransient<ChatInfoViewModel>();
      builder.Services.AddTransient<GroupCreationViewModel>();
      builder.Services.AddTransient<SearchViewModel>();
      builder.Services.AddTransient<UserPageViewModel>();

      builder.Logging.AddDebug();
      //builder.Logging.add
      var app = builder.Build();

      // 🧠 Важно: подключаем DI к CommunityToolkit
      Ioc.Default.ConfigureServices(app.Services);

      return app;
    }
  }
}
