using mshtml;
using System;
using System.Configuration;
using System.Printing;
using System.Windows;
using System.Windows.Controls;


namespace WpfApplicationWalkthrough
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // availableprint operations ..
        private enum PrintOperation
        {
            Print,
            Preview
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        //private readonly string _homeUrl = "http://scriptxsamples.meadroid.com/Licensed/SalesInfo/Release/DOCTYPE";
        private readonly string _homeUrl = "http://www.meadroid.com";

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            // the page to print -- this page uses a media print stylesheet
            Browser.Navigate(_homeUrl);
            ApplyScriptXLicense();

            // select the default printer ...
            var defaultPrinter = new LocalPrintServer().DefaultPrintQueue;
            if (defaultPrinter != null)
            {
                CmbPrinters.SelectedValue = defaultPrinter.FullName;
            }

        }

        /// <summary>
        /// License this application for use of ScriptX Advanced features.
        /// *NOTE* When debugging, must *not* Enable the Visual Studio Hosting Process (Project properties -> Debug page)
        /// </summary>
        private void ApplyScriptXLicense()
        {
            var secMgr = new SecMgr.SecMgr();
            string licenseUrl = AppDomain.CurrentDomain.BaseDirectory + "sxlic.mlf";
            try
            {
                secMgr.Apply(licenseUrl, ConfigurationManager.AppSettings["ScriptXLicenseGuid"],
                    Int32.Parse(ConfigurationManager.AppSettings["ScriptXLicenseRevision"]));
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format("Failed to license this application: {0}", e.Message), this.Title);
            }

        }

        private void BtnPrint_OnClick(object sender, RoutedEventArgs e)
        {
            PrintDocument(PrintOperation.Print);
        }

        private void BtnPreview_OnClick(object sender, RoutedEventArgs e)
        {
            PrintDocument(PrintOperation.Preview);
        }

        /// <summary>
        /// Print or preview the document displayed in the web browser
        /// </summary>
        /// <param name="operation"></param>
        private void PrintDocument(PrintOperation operation)
        {
            var printer = HtmlPrinter;
            if (printer != null)
            {
                printer.printer = CmbPrinters.SelectedValue.ToString();
                printer.paperSource = "A4";

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
            }
            else
            {
                MessageBox.Show("Unable to find a printer", this.Title);
            }
        }

        private ScriptX.printing HtmlPrinter
        {
            get
            {
                var document = (IHTMLDocument3) Browser.Document;

                // 'de-facto' id is 'factory'
                var factoryElement = (IHTMLObjectElement) document.getElementById("factory");

                // does the factory object exist?
                if (factoryElement == null)
                {
                    // the html to insert to put the ScriptX factory on the document.
                    const string factoryObjectHtml = "<object id=\"factory\" style=\"display:none\" classid=\"clsid:1663ed61-23eb-11d2-b92f-008048fdd814\"></object>";

                    // If not then create it.
                    ((IHTMLDocument2) Browser.Document).body.insertAdjacentHTML("beforeEnd",
                        factoryObjectHtml);

                    factoryElement = (IHTMLObjectElement) document.getElementById("factory");
                }

                if (factoryElement != null)
                {
                    // an object 'factory' exists, but is the object loaded (it may not be installed)?
                    ScriptX.Factory factory = factoryElement.@object;
                    if (factory != null)
                    {
                        // we have a factory, get printing object
                        ScriptX.printing printer = factory.printing;
                        return printer;
                    }
                }

                return null;
            }
        }
    }
}
