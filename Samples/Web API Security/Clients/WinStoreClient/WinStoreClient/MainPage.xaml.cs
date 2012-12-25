using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using Thinktecture.IdentityModel.WinRT;
using Thinktecture.Samples;
using Windows.Security.Credentials;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace WinStoreClient
{

    public sealed partial class MainPage : Page
    {
        static Uri _baseAddress = new Uri(Constants.WebHostBaseAddress);
        //static Uri _baseAddress = new Uri(Constants.SelfHostBaseAddress);

        string _resourceName = "backend";
        TokenCredential _credential;

        public MainPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //ClearVault();

            RetrieveStoredToken();
        }

        private void RetrieveStoredToken()
        {
            TokenCredential credential;
            if (TokenVault.TryGetToken(_resourceName, out credential))
            {
                _credential = credential;
                _txtToken.Text = credential.AccessToken;
                _txtExpires.Text = credential.Expires.ToString();
            }
        }

        private async void SignIn_Click(object sender, RoutedEventArgs e)
        {
            var error = string.Empty;

            try
            {
                var response = await WebAuthentication.DoImplicitFlowAsync(
                    endpoint: new Uri(Constants.IdSrv.OAuth2AuthorizeEndpoint),
                    clientId: Constants.IdSrv.Win8OAuthClientName,
                    scope: Constants.Scope);

                TokenVault.StoreToken(_resourceName, response);
                RetrieveStoredToken();

            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            if (!string.IsNullOrEmpty(error))
            {
                var dialog = new MessageDialog(error);
                await dialog.ShowAsync();
            }
        }

        private async void _btnCallService_Click(object sender, RoutedEventArgs e)
        {
            var client = new HttpClient { BaseAddress = _baseAddress };
            client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", _credential.AccessToken);

            var response = await client.GetAsync("identity");
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var md = new MessageDialog(response.ReasonPhrase);
                await md.ShowAsync();
                return;
            }

            var claims = await response.Content.ReadAsAsync<IEnumerable<ViewClaim>>();

            foreach (var claim in claims)
            {
                _listClaims.Items.Add(string.Format("{0}: {1}", claim.Type, claim.Value));
            }
        }

        private void ClearVault()
        {
            try
            {
                var vault = new PasswordVault();
                var cred = vault.Retrieve(_resourceName, "token");
                vault.Remove(cred);
            }
            catch { }
        }
    }
}

