using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using PasswordManagerBlazor.Client.Helpers;
using PasswordManagerBlazor.Shared.DTOs;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using static PasswordManagerBlazor.Client.Pages.Login;
using System.Net.Http.Json;

namespace PasswordManagerBlazor.Client.Service
{
    public interface IAuthService
    {
        Task<RegisterResult> Register(UserRegistrationDto registerModel);
        Task<LoginResult> Login(LoginModel loginModel);
        Task Logout();
    }

    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly ILocalStorageService _localStorage;

        public AuthService(HttpClient httpClient,
                           AuthenticationStateProvider authenticationStateProvider,
                           ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _authenticationStateProvider = authenticationStateProvider;
            _localStorage = localStorage;
        }

        public async Task<RegisterResult> Register(UserRegistrationDto registerModel)
        {
            var result = await _httpClient.PostAsJsonAsync("api/user/registration", registerModel);
            if (!result.IsSuccessStatusCode)
                return new RegisterResult { Successful = true, Error = null };
            return new RegisterResult { Successful = false, Error = "Error occured"  };
        }

        public async Task<LoginResult> Login(LoginModel loginModel)
        {
            var loginAsJson = JsonSerializer.Serialize(new UserLoginDto
            {
                Email = loginModel.Username,
                Password = loginModel.Password
            });
            var response = await _httpClient.PostAsync("api/user/login", new StringContent(loginAsJson, Encoding.UTF8, "application/json"));
            var loginResult = JsonSerializer.Deserialize<LoginResult>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (!response.IsSuccessStatusCode)
            {
                return loginResult!;
            }

            await _localStorage.SetItemAsync("authToken", loginResult!.Token);
            ((ApiAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsAuthenticated(loginModel.Username!);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", loginResult.Token);

            return loginResult;
        }

        public async Task Logout()
        {
            await _localStorage.RemoveItemAsync("authToken");
            ((ApiAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsLoggedOut();
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }
}
