using Microsoft.WindowsAzure.ActiveDirectory.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Thinktecture.Samples;

namespace WpfWebViewWithAalAcs
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static Uri _baseAddress = new Uri(Constants.WebHostBaseAddress);
        //static Uri _baseAddress = new Uri(Constants.SelfHostBaseAddress);

        string _token;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void GetTokenButton_Click(object sender, RoutedEventArgs e)
        {
            var context = new AuthenticationContext(Constants.ACS.IssuerUri);
            var credential = context.AcquireToken(Constants.Realm);

            if (credential != null)
            {
                if (!string.IsNullOrWhiteSpace(credential.Assertion))
                {
                    _token = credential.Assertion;
                    OutputTextBox.Text = _token;
                }
            }
        }

        private void CallServiceButton_Click(object sender, RoutedEventArgs e)
        {
            var client = new HttpClient
            {
                BaseAddress = _baseAddress
            };

            client.SetToken(Constants.ACS.Scheme, _token);

            var response = client.GetAsync("identity").Result;
            response.EnsureSuccessStatusCode();

            var claims = response.Content.ReadAsAsync<ViewClaims>().Result;
            var sb = new StringBuilder(128);

            foreach (var claim in claims)
            {
                sb.AppendFormat(" {0}\n  {1}\n\n", claim.Type, claim.Value);
            }

            OutputTextBox.Text = sb.ToString();
        }
    }
}
