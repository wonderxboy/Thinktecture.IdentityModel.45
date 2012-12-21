using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Thinktecture.IdentityModel.WinRT;
using Windows.Security.Authentication.Web;
using Windows.Security.Credentials;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace WinStoreClient
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
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
                    endpoint: new Uri("https://adfs.leastprivilege.vm/idsrv/issue/oauth2/authorize"),
                    clientId: "test",
                    scope: "https://test/rp/");

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
            var client = new HttpClient { BaseAddress = new Uri("https://roadie/webapisecurity/api/") };

            client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Win8", _credential.AccessToken);

            var response = await client.GetAsync("identity");
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var md = new MessageDialog(response.ReasonPhrase);
                await md.ShowAsync();
                return;
            }

            var identity = await response.Content.ReadAsAsync<Identity>();

            foreach (var claim in identity.Claims)
            {
                _listClaims.Items.Add(string.Format("{0}: {1}", claim.ClaimType, claim.Value));
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

