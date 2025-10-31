using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using System.Net;
using System.Net.Http.Headers;

namespace DemoProject.Client.Service
{
    public class AuthTokenHandler : DelegatingHandler
    {
        private readonly ILocalStorageService _localStorage;
        private readonly JwtAuthenticationStateProvider _authProvider;
        private readonly NavigationManager _navigation;

        public AuthTokenHandler(ILocalStorageService localStorage,
                                JwtAuthenticationStateProvider authProvider,
                                NavigationManager navigation)
        {
            _localStorage = localStorage;
            _authProvider = authProvider;
            _navigation = navigation;
        }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Add access token if available
            var token = await _localStorage.GetItemAsync<string>("authToken");
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                // Try refreshing token
                var refreshToken = await _localStorage.GetItemAsync<string>("refreshToken");
                if (!string.IsNullOrEmpty(refreshToken))
                {
                    var newToken = await _authProvider.RefreshTokenAsync();
                    if (!string.IsNullOrEmpty(newToken))
                    {
                        // Retry original request with new token
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
                        response = await base.SendAsync(request, cancellationToken);
                        return response;
                    }
                }

                // No refresh token or refresh failed: log out
                _authProvider.NotifyUserLogout();
                _navigation.NavigateTo("auth/login", true);
            }

            return response;
        }
    }
}
