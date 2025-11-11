using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiMessenger.Services
{
  public class NavigationService
  {
    public async Task GoToChatAsync(Guid chatId)
    {
      // MAUI Shell
      await Shell.Current.GoToAsync($"chat?chatId={chatId}");
    }

    public Task GoBackAsync() => Shell.Current.GoToAsync("..");

    public Task GoToLoginAsync() => Shell.Current.GoToAsync("//login");
  }

}
