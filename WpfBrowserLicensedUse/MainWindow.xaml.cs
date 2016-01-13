using mshtml;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace WpfBrowserLicensedUse
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private enum PrintOperation
        {
            Print,
            Preview
        }

        // a document we will print - it doesnt have scriptx on it.
        private readonly string _homeUrl = "http://scriptxsamples.meadroid.com/Licensed/SalesInfo/Release/DOCTYPE";

        // the html to insert to put the ScriptX factory on the document.
        private string _factoryObjectHtml = "<object id=\"factory\" style=\"display:none\" classid=\"clsid:1663ed61-23eb-11d2-b92f-008048fdd814\"></object>";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            WebBrowser.Navigate(_homeUrl);

            // license this application so that everything is possible.
            ApplyScriptXLicense();
        }

        // simple progress monitoring
        #region progressMonitor
        private void EnableUi(bool bEnable)
        {
            PreviewButton.IsEnabled = bEnable;
            PrintButton.IsEnabled = bEnable;
            NavigationUrl.IsEnabled = bEnable;
        }


        private void WebBrowser_OnNavigated(object sender, NavigationEventArgs e)
        {
            EnableUi(false);
            NavigationUrl.Text = $"Loading :: {e.Uri.ToString()}";
        }

        private void WebBrowser_OnNavigating(object sender, NavigatingCancelEventArgs e)
        {
            EnableUi(false);
            if (NavigationUrl != null) NavigationUrl.Text = $"Finding :: {e.Uri.ToString()}";
        }

        private void WebBrowser_OnLoadCompleted(object sender, NavigationEventArgs e)
        {
            EnableUi(true);
            NavigationUrl.Text = $"Loaded :: {e.Uri.ToString()}";
        }
        #endregion

        // trivial naviation UI
        #region navUI
        private void NavigationUrl_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GoButton_OnClick(this, new RoutedEventArgs());
            }
        }

        private void GoButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Uri uri = new Uri(NavigationUrl.Text);
                WebBrowser.Navigate(uri);
            }
            catch (Exception exception)
            {
                MessageBox.Show($"Invalid address: {exception.Message}", this.Title);
            }
        }
        #endregion

        private void PrintButton_OnClick(object sender, RoutedEventArgs e)
        {
            PrintOrPreview(PrintOperation.Print);
        }

        private void PreviewButton_OnClick(object sender, RoutedEventArgs e)
        {
            PrintOrPreview(PrintOperation.Preview);
        }

        /// <summary>
        /// PrintOrPreview the document displayed in the WebBrowser.
        /// Adds ScriptX to the page if required, or uses it if already there
        /// Defines custom headers and footers for the print and as we are licensed
        /// we can set margin measure (and a whole lot of other things!)
        /// 
        /// Note that we dont add security manager to the html document, the app is licensed
        /// so it isnt needed.
        /// </summary>
        /// <param name="operation">Print or Preview?</param>
        private void PrintOrPreview(PrintOperation operation)
        {
            var document = (IHTMLDocument3)WebBrowser.Document;

            // 'de-facto' id is 'factory'
            var factoryElement = document.getElementById("factory");

            // does the factory object exist?
            if (factoryElement == null)
            {
                // If not then create it.
                ((IHTMLDocument2)WebBrowser.Document).body.insertAdjacentHTML("beforeEnd",
                    _factoryObjectHtml);

                factoryElement = document.getElementById("factory");
            }

            if (factoryElement != null)
            {
                var factoryObject = (IHTMLObjectElement)factoryElement;

                // an element 'factory' exists, but is the object loaded (it may not be installed)?

                ScriptX.Factory factory = (ScriptX.Factory)factoryObject.@object;
                if (factory != null)
                {
                    ScriptX.printing printer = factory.printing;

                    printer.header = this.Title;
                    printer.footer = "&D&b&b&P of &p";

                    // use some advanced features ...
                    printer.SetMarginMeasure(2); // set units to inches
                    printer.leftMargin = 1.5f;
                    printer.topMargin = 1;
                    printer.rightMargin = 1;
                    printer.bottomMargin = 1;
                    switch (operation)
                    {
                        case PrintOperation.Print:
                            printer.Print(false); // prompt will only be obeyed on intranet
                            break;

                        case PrintOperation.Preview:
                            printer.Preview();
                            break;
                    }

                    // and we can wait untiol the job is spooled and so let the user know its on its way
                    printer.WaitForSpoolingComplete();
                    MessageBox.Show("The print has completed.", this.Title);
                }
                else
                {
                    MessageBox.Show("Unable to find or create MeadCo ScriptX.\n\nIs MeadCo ScriptX installed?", this.Title);
                }
            }

        }

        /// <summary>
        /// Take the license details from app.config and license this application for use of ScriptX Advanced features
        /// </summary>
        private void ApplyScriptXLicense()
        {
            Guid licenseGuid = new Guid(ConfigurationManager.AppSettings["ScriptXLicenseGuid"]);

            UriBuilder uriBuilder = new UriBuilder();
            uriBuilder.Scheme = "http";
            uriBuilder.Host = ConfigurationManager.AppSettings["ScriptXLicenseHost"];

            uriBuilder.Path = $"download/{licenseGuid.ToString()}/mlf";
        
            ApplyScriptXLicense(uriBuilder.Uri,
                licenseGuid, 
                Int32.Parse(ConfigurationManager.AppSettings["ScriptXLicenseRevision"]));
        }

        /// <summary>
        /// License this application for use of ScriptX Advanced features.
        /// *NOTE* When debugging, must *not* Enable the Visual Studio Hosting Process (Project properties -> Debug page)
        /// </summary>
        /// <param name="licenseUri">The Uri of the license file, can be null/empty if using a machine license</param>
        /// <param name="licenseGuid">The unique id of the license</param>
        /// <param name="licenseRevision">The license revision</param>
        private void ApplyScriptXLicense(Uri licenseUri, Guid licenseGuid, int licenseRevision)
        {
            var secMgr = new SecMgr.SecMgr();

            try
            {
                secMgr.Apply(licenseUri == null ? "" : licenseUri.ToString(), "{" + licenseGuid.ToString() + "}", licenseRevision);
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format("Failed to license this application using: {0}\n\nThe error was: {1}", licenseUri == null ? "the machine license" : licenseUri.ToString(),e.Message), this.Title);
            }

        }
    }
}
