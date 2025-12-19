using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace MauiMessenger.Services
{
  public class AuthMessageHandler : DelegatingHandler
  {
    private readonly AppStateService _state;

    public AuthMessageHandler(AppStateService state)
    {
      _state = state;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
      var token = _state.AccessToken;

      if (!string.IsNullOrEmpty(token))
      {
        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
      }

      return await base.SendAsync(request, cancellationToken);
    }
  }

}
