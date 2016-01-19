using System;
using System.Configuration;
using System.Printing;
using System.Windows;

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

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            // the page to print -- this page use a media print stylesheet
            Browser.Navigate("http://www.meadroid.com");
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
                secMgr.Apply(licenseUrl, ConfigurationManager.AppSettings["ScriptXLicenseGuid"], Int32.Parse(ConfigurationManager.AppSettings["ScriptXLicenseRevision"]));
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
            
        }
    }
}
