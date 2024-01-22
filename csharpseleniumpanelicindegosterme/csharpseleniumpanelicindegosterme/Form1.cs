using System;
using System.Diagnostics;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Windows.Forms;

namespace csharpseleniumpanelicindegosterme
{
    public partial class Form1 : Form
    {
        private Thread browserThread;
        private ChromeDriver chromeDriver;

        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            browserThread = new Thread(OpenBrowser);
            browserThread.Start();
        }

        private void OpenBrowser()
        {
            string chromeDriverPath = "chromedriver.exe";
            ChromeOptions chromeOptions = ConfigureChromeOptions();
            ChromeDriverService driverService = ChromeDriverService.CreateDefaultService(chromeDriverPath);
            driverService.HideCommandPromptWindow = true;

            chromeDriver = new ChromeDriver(driverService, chromeOptions);

            string url = "https://google.com";
            string uniqueID = DateTime.Now.Ticks.ToString();
            chromeDriver.ExecuteScript("document.title = '" + uniqueID + "';");

            int browserProcessId = 0;

            do
            {
                Thread.Sleep(100);

                foreach (Process process in Process.GetProcessesByName("chrome"))
                {
                    string titleChrome = process.MainWindowTitle;

                    if (titleChrome.StartsWith(uniqueID) && titleChrome.Contains("Chrome"))
                    {
                        process.WaitForInputIdle();
                        SetParentAndMoveWindow(process.MainWindowHandle, panel1.Handle, panel1.Width, panel1.Height);
                        browserProcessId = process.Id;
                        break;
                    }
                }
            } while (browserProcessId == 0);

            chromeDriver.Manage().Window.Maximize();
            chromeDriver.Navigate().GoToUrl(url);
        }

        private ChromeOptions ConfigureChromeOptions()
        {
            ChromeOptions options = new ChromeOptions();
            options.AddAdditionalOption("useAutomationExtension", false);
            options.AddExcludedArgument("enable-automation");
            options.AddArgument("--window-size=0,0");
            options.AddArgument("--disable-gpu"); //Siyah Sayfa Sorunu olursa bu kod çözüme ulaştıracaktır sizi

            return options;
        }

        private static void SetParentAndMoveWindow(IntPtr childHandle, IntPtr newParentHandle, int width, int height)
        {
            SetParent(childHandle, newParentHandle);
            MoveWindow(childHandle, 0, 0, width, height, false);
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool MoveWindow(IntPtr hWnd, int x, int y, int nWidth, int nHeight, bool bRepaint);

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (chromeDriver != null)
            {
                chromeDriver.Quit();
            }
        }
    }
}
