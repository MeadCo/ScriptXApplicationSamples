using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Xml;

// available from added reference to COM :: Microsoft HTML Object Library (4.0)
using mshtml;

namespace WpfApplicationFreeUse
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
        private string HomeURL = "http://scriptxsamples.meadroid.com/Licensed/SalesInfo/Release/DOCTYPE";

        // the html to insert to put the ScriptX factory on the document.
        private string factoryObjectHtml = "<object id=\"factory\" style=\"display:none\" classid=\"clsid:1663ed61-23eb-11d2-b92f-008048fdd814\"></object>";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            WebBrowser.Navigate(HomeURL);
        }

        // simple progress monitoring
        #region progressMonitor
        private void EnableUI(bool bEnable)
        {
            PreviewButton.IsEnabled = bEnable;
            PrintButton.IsEnabled = bEnable;
            NavigationUrl.IsEnabled = bEnable;
        }


        private void WebBrowser_OnNavigated(object sender, NavigationEventArgs e)
        {
            EnableUI(false);
            NavigationUrl.Text = String.Format("Loading :: {0}",e.Uri.ToString());
        }

        private void WebBrowser_OnNavigating(object sender, NavigatingCancelEventArgs e)
        {
            EnableUI(false);
            if (NavigationUrl != null) NavigationUrl.Text = String.Format("Finding :: {0}",e.Uri.ToString());
        }

        private void WebBrowser_OnLoadCompleted(object sender, NavigationEventArgs e)
        {
            EnableUI(true);
            NavigationUrl.Text = String.Format("Loaded :: {0}",e.Uri.ToString());
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
                MessageBox.Show(String.Format("Invalid address: {0}",exception.Message),this.Title);
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
        /// Defines custom headers and footers for the print.
        /// </summary>
        /// <param name="operation">Print or Preview?</param>
        private void PrintOrPreview(PrintOperation operation)
        {
            var document = (IHTMLDocument3) WebBrowser.Document;

            // 'de-facto' id is 'factory'
            var factoryElement = document.getElementById("factory");

            // does the factory object exist?
            if (factoryElement == null)
            {
                // If not then create it.
                ((IHTMLDocument2)WebBrowser.Document).body.insertAdjacentHTML("beforeEnd",
                    factoryObjectHtml);

                factoryElement = document.getElementById("factory");
            }

            if (factoryElement != null)
            {
                var factoryObject = (IHTMLObjectElement) factoryElement;

                // an element 'factory' exists, but is the object loaded (it may not be installed)?

                ScriptX.Factory factory = (ScriptX.Factory) factoryObject.@object;
                if (factory != null)
                {
                    ScriptX.printing printer = factory.printing;

                    printer.header = this.Title;
                    printer.footer = "&D&b&b&P of &p";
                    switch (operation)
                    {
                        case PrintOperation.Print:
                            printer.Print(false); // prompt will only be obeyed on intranet
                            break;

                        case PrintOperation.Preview:
                            printer.Preview();
                            break;
                    }
                }
                else
                {
                    MessageBox.Show("Unable to find or create MeadCo ScriptX.\n\nIs MeadCo ScriptX installed?",this.Title);
                }
            }
        }

    }
}
