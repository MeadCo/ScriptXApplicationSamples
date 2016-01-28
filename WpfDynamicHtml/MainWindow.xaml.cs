using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Printing;
using System.Runtime.InteropServices;
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

namespace WpfDynamicHtml
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // select the default printer ...
            var defaultPrinter = new LocalPrintServer().DefaultPrintQueue;
            if (defaultPrinter != null)
            {
                CmbPrinters.SelectedValue = defaultPrinter.FullName;
            }
        }

        private void NameTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PrintLabel();
            }
        }

        private void BtnPrint_OnClick(object sender, RoutedEventArgs e)
        {
            PrintLabel();
        }

        private void PrintLabel()
        {
            PrintLabel(NameTextBox.Text);
            NameTextBox.Text = "";
        }

        // Print a very simplistic label with the persons name on it
        private async void PrintLabel(string personName)
        {
            if (!string.IsNullOrWhiteSpace(personName))
            {
                // license the app so we can use advanced features
                if (await ApplyScriptXLicenseAsync())
                {
                    try
                    {
                        // create a scriptx factory
                        var factory = new ScriptX.Factory();

                        // get the printing object and configure required parameters
                        ScriptX.printing printer = factory.printing;

                        // select the printer and paper size.
                        printer.printer = CmbPrinters.SelectedValue.ToString();
                        printer.paperSize = "A4";

                        printer.header = this.Title;
                        printer.footer = "&D&b&b&P of &p";

                        // use some advanced features ...
                        printer.SetMarginMeasure(2); // set units to inches
                        printer.leftMargin = 1.5f;
                        printer.topMargin = 1;
                        printer.rightMargin = 1;
                        printer.bottomMargin = 1;

                        // build the html to print
                        StringBuilder sHtml = new StringBuilder("html://");
                        sHtml.Append(
                            "<!DOCTYPEe html><html><head><meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\"><title>ScriptX Sample</title></head><body>");
                        sHtml.Append("<table border=\"0\" width=\"100%\" height=\"100%\"><tr>");
                        sHtml.Append("<td align=\"center\">");
                        sHtml.Append("<h1>ScriptX Printing of HTML</h1><p>This label is for:</p><h2>");
                        sHtml.Append(personName);
                        sHtml.Append("</h2><p>and was printed on:</p><h4>");
                        sHtml.Append(System.DateTime.Now.ToLongDateString());
                        sHtml.Append("</h4></td></tr></table></body></html>");

                        // and print it -- this job will be queued and printed
                        // in an external process (so this call returns as soon
                        // as the content has been stored in the queue).
                        printer.PrintHTML(sHtml.ToString(), 0);
                    }
                    catch (COMException e)
                    {
                        MessageBox.Show("Printing failed: " + e.Message, this.Title);
                    }

                }

            }
            else
            {
                MessageBox.Show("Please enter a name to print on the label", this.Title);
            }

        }


        #region licensing

        /// <summary>
        /// Async wrapper on the process of downloading the license file and validation
        /// </summary>
        /// <returns></returns>
        private async Task<bool> ApplyScriptXLicenseAsync()
        {
            bool bAccepted = false;

            try
            {
                bAccepted = await Task.Run<bool>(() => ApplyScriptXLicense());
            }
            catch (COMException comEx)
            {
                MessageBox.Show("Unable to apply the license: " + comEx.Message, this.Title);
            }

            return bAccepted;
        }

        /// <summary>
        /// Take the license details from app.config and license this application for use of ScriptX Advanced features
        /// </summary>
        private bool ApplyScriptXLicense()
        {
            Guid licenseGuid = new Guid(ConfigurationManager.AppSettings["ScriptXLicenseGuid"]);

            UriBuilder uriBuilder = new UriBuilder();
            uriBuilder.Scheme = "http";
            uriBuilder.Host = ConfigurationManager.AppSettings["ScriptXLicenseHost"];
            uriBuilder.Port = Int32.Parse(ConfigurationManager.AppSettings["ScriptXLicensePort"]);
            uriBuilder.Path = string.Format("download/{0}/mlf", licenseGuid.ToString());

            return ApplyScriptXLicense(uriBuilder.Uri,
                licenseGuid,
                Int32.Parse(ConfigurationManager.AppSettings["ScriptXLicenseRevision"]));
        }

        /// <summary>
        /// License this application for use of ScriptX Advanced features.
        /// *NOTE* When debugging, must *not* Enable the Visual Studio Hosting Process (Project properties -> Debug page)
        /// Not running on the UI thread here (assuming called by ApplicatScriptXLicenseAsync())
        /// </summary>
        /// <param name="licenseUri">The Uri of the license file, can be null/empty if using a machine license</param>
        /// <param name="licenseGuid">The unique id of the license</param>
        /// <param name="licenseRevision">The license revision</param>
        private bool _bLicenseApplied = false;
        private bool ApplyScriptXLicense(Uri licenseUri, Guid licenseGuid, int licenseRevision)
        {
            if (!_bLicenseApplied)
            {
                var secMgr = new SecMgr.SecMgr();
                secMgr.Apply(licenseUri == null ? "" : licenseUri.ToString(), "{" + licenseGuid.ToString() + "}",
                    licenseRevision);
                _bLicenseApplied = true;
            }

            return _bLicenseApplied;
        }
        #endregion
    }
}
