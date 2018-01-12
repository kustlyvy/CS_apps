// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
//using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Internal;

using System.IO;

using System.Windows.Forms;

namespace advancedMDsync
{
    class Scraper
    {
        static ChromeDriver driver = null;
        static ChromeDriverService driverService = null;

        private string username;
        private string password;
        private string key;

        public bool close()
        {

            try {
                driver.Quit();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                //MessageBox.Show(e.Message);

                return false;
            }

            return true;
        }

        public bool readCreds()
        {
            try
            {
                string pathExe = System.Reflection.Assembly.GetEntryAssembly().Location;

                int pos = -1;
                pos = pathExe.LastIndexOf("\\");
                if (pos != -1)
                {
                    pathExe = pathExe.Remove(pos, pathExe.Length - pos);
                }

                string txtFilePath = pathExe + "\\" + "user.txt";

                using (var reader = new StreamReader(File.OpenRead(txtFilePath)))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var values = line.Split(';');

                        username = values[0];
                        password = values[1];
                        key = values[2];
                    }
                }

                return true;
            }

            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                //MessageBox.Show(e.Message);

                return false;
            }
        }

        public bool Login()
        {
            try
            {
                ChromeOptions options = new ChromeOptions();
                //options.AddArgument("--disable-extensions");
                //options.AddArguments("chrome.switches", "--disable-extensions --disable-extensions-file-access-check --disable-extensions-http-throttling --disable-infobars --enable-automation --start-maximized");
                options.AddArguments("chrome.switches", "--disable-extensions --enable-automation --start-maximized  --no-sandbox --disable-infobars");
                options.AddUserProfilePreference("credentials_enable_service", false);
              
                options.AddUserProfilePreference("profile.password_manager_enabled", false);

                driverService = ChromeDriverService.CreateDefaultService();
                driverService.HideCommandPromptWindow = true;
                driver = new ChromeDriver(driverService, options);

                driver.Navigate().GoToUrl("https://login.advancedmd.com/");

                driver.SwitchTo().Frame("frame-login");

                var userEmailField = driver.FindElementByName("username");
                var userPasswordField = driver.FindElementByName("password");
                var userOfficeKey = driver.FindElementByName("officeKey");                
 
                               
                userEmailField.SendKeys(username);
                userPasswordField.SendKeys(password);
                userOfficeKey.SendKeys(key);

                var loginButton = driver.FindElement(By.XPath("//button[@type='submit']"));
                loginButton.Click();
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message);
                Console.WriteLine(e.ToString());
                return false;
            }
            return true;
        }

        public bool SnoozeAll()
        {
            System.Threading.Thread.Sleep(3000);
            bool breakIt = true;
            while (true)
            {
                breakIt = true;
                try
                {
                    //driver.Manage().Window.Maximize();
                    //System.Threading.Thread.Sleep(3000);

                    

                    var buttons = driver.FindElements(By.TagName("button"));

                    var btnFound = true;
                    do
                    {
                        foreach (var btn in buttons)
                        {

                            btnFound = false;

                            if ((btn.Text.CompareTo("Snooze all") == 0))
                            {
                                btnFound = false;
                                btn.Click();
                                System.Threading.Thread.Sleep(5000);
                            }

                        }

                    } while (btnFound);

                }
                catch (Exception e)
                {
                    if (e.Message.Contains("element is not attached"))
                    {
                        breakIt = false;
                    }
                    else if (e.Message.Contains("Element is not clickable at point"))
                    {
                        breakIt = false;
                    }
                    else
                    {
                        //Scraper.curError = "PageFifth failed with error: " + e.ToString();
                        //MessageBox.Show("Understand : error : " + e.ToString());
                        Console.WriteLine(e.ToString());
                        return false;
                    }
                }
                if (breakIt)
                {
                    break;
                }
            }
            return true;
        }

        public bool Understand()
        {
            System.Threading.Thread.Sleep(3000);
            bool breakIt = true;
            while (true)
            {
                breakIt = true;
                try
                {
                    //driver.Manage().Window.Maximize();
                    //System.Threading.Thread.Sleep(3000);

                    var buttons = driver.FindElements(By.TagName("button"));
                    foreach (var btn in buttons)
                    {
                        if ((btn.Text.CompareTo("I understand") == 0))
                        {
                            btn.Click();
                            System.Threading.Thread.Sleep(2000);
                        }

                    }
                 }
                catch (Exception e)
                {
                    if (e.Message.Contains("element is not attached"))
                    {
                        breakIt = false;
                    }
                    else if (e.Message.Contains("Element is not clickable at point"))
                    {
                        breakIt = false;
                    }
                    else
                    {
                        //Scraper.curError = "PageFifth failed with error: " + e.ToString();
                        //MessageBox.Show("Understand : error : " + e.ToString());
                        Console.WriteLine(e.ToString());
                        return false;
                    }
                }
                if (breakIt)
                {
                    break;
                }
            }
            return true;
        }

        public void SkipNewAppointmentPopup()
        {
            System.Threading.Thread.Sleep(2000);
            //var modal = driver.FindElements(By.ClassName("modal-dialog"));

            String parentWindowHandle = driver.CurrentWindowHandle; // save the current window handle.
            IWebDriver popup = null;
            var windowIterator = driver.WindowHandles;

            foreach (var windowHandle in windowIterator)
            {
                popup = driver.SwitchTo().Window(windowHandle);

                if (popup.Title == "AdvancedMD PM - CBO Premium Billing Inc CBO Master 128716 ALEXY")
                {
                    break;
                }
            }

            try {
                 var el = driver.FindElement(By.XPath("//*[contains(text(), 'Got it')]"));
                el.Click();
             }
             catch(Exception e)
             {

             }


        }

        public bool ClickReportsMenu()
        {
            System.Threading.Thread.Sleep(2000);

            driver.ExecuteScript("window.resizeTo(1370, 768);");
            //driver.Manage().Window.Maximize();

            System.Threading.Thread.Sleep(2000);

           /* if (driver.SwitchTo().Alert() != null)
            {
                IAlert alert = driver.SwitchTo().Alert();
                //String alertText = alert.GetText();
                alert.Dismiss(); // alert.accept();

            }*/

            try
            {
                var el = driver.FindElement(By.XPath("//a[contains(text(), 'Reports')]"));
                el.Click();
            }
            catch (Exception e)
            {
                try
                {
                    //driver.FindElement(By.Id("")).SendKeys("%({F4})");
                    System.Threading.Thread.Sleep(2000);
                    var el = driver.FindElement(By.XPath("//a[contains(text(), 'Reports')]"));
                    el.Click();
                }
                catch { return false; }
            }

            return true;

        }

        public bool ClickFinancialTotalsMenu()
        {
            System.Threading.Thread.Sleep(500);
            try
            {
                var el = driver.FindElement(By.XPath("//a[contains(text(), 'Financial Totals')]"));
                el.Click();
                System.Threading.Thread.Sleep(1500);
                el.Click();
            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }

        public bool ClickTransactionDetailMenu()
        {
            System.Threading.Thread.Sleep(1500);
            try
            {
                var el = driver.FindElement(By.XPath("//a[contains(text(), 'Transaction Detail')]"));
                el.Click();
            }
            catch (Exception e)
            {
                try {
                    ClickReportsMenu();
                    ClickReportsMenu();
                    ClickFinancialTotalsMenu();
                }
                catch
                {
                    return false;
                }

            }

            return true;
        }
        public bool SwitchToReportWIndow()
        {
            System.Threading.Thread.Sleep(2000);
            String parentWindowHandle = driver.CurrentWindowHandle; // save the current window handle.
            IWebDriver popup = null;
            var windowIterator = driver.WindowHandles;

            foreach (var windowHandle in windowIterator)
            {
                popup = driver.SwitchTo().Window(windowHandle);

                if (popup.Title == "Report Center")
                {
                    return true;
                }
            }
            return false;
        }

        public bool CheckExportOnRun()
        {
            System.Threading.Thread.Sleep(2000);
            try
            {
                var el = driver.FindElement(By.XPath("//label[text()='Export on Run']/preceding-sibling::input[@type='checkbox']"));
                el.Click();
            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }

        private string MapPracticeById(string id)
        {
            Dictionary<string, string> mapNames = new Dictionary<string, string>();

            mapNames.Add("101", "128716 - Premium Billing Inc CBO Master");
            mapNames.Add("102", "129110 - Dr Vic");
            mapNames.Add("103", "129146 - Tunde DPM");
            mapNames.Add("104", "130326 - Carlos S SIlva");
            mapNames.Add("105", "131822 - Elnunu Medical PC");
            mapNames.Add("106", "131900 - HILLEL");
            mapNames.Add("107", "131976 - South Shore Podiatry");
            mapNames.Add("108", "132019 - Arthritis & Osteoporosos Care Pllc");
            mapNames.Add("109", "132492 - West End Podiatry PC");
            mapNames.Add("110", "132558 - DAVID E. GURVIS, DPM, INC., PC");
            mapNames.Add("111", "133969 - Stephanie Carter Robin DPM PC");
            mapNames.Add("112", "133984 - Healthy Steps Family Foot Care PC");
            mapNames.Add("113", "134612 - Edward Kleiman, MD PLLC");
            mapNames.Add("114", "134742 - Hanny Hernandez DPM PC");
            mapNames.Add("115", "135033 - Ernest L Isaacson DPM PC");
            mapNames.Add("116", "135257 - Weiss Medical Group");
            mapNames.Add("117", "135389 - Jungwoo Han DPM");
            mapNames.Add("118", "135392 - the Podiatric Surgeon PC");
            mapNames.Add("119", "135518 - A&T Main Foot and Ankle");
            mapNames.Add("120", "135623 - East 40th Podiatry PC");
            mapNames.Add("121", "135778 - Oleg Karpenko Podiatry PC");
            mapNames.Add("122", "135833 - Yakov Y Beim");
            mapNames.Add("123", "135839 - A and T, MaA and T PODIATRY PLLC");
            mapNames.Add("124", "135870 - ERNEST L ISAACSON DPM PC");
            mapNames.Add("125", "136049 - Santi Podiatry Group");
            mapNames.Add("126", "136237 - AMME OBSTETRICS AND GYNECOLOGY ASSOCIATES LLC");
            mapNames.Add("127", "136598 - CA Foot and Ankle Clinic");
            mapNames.Add("128", "136600 - Paul B Bernstein");





            string name_out;
            mapNames.TryGetValue(id, out name_out);

            return name_out;
        }

        public bool SaveReports(int time, string id, int suffix, string begDate, string endDate)
        {
            System.Threading.Thread.Sleep(2000);
            var selects = driver.FindElements(By.TagName("select"));
            if (selects.Count == 0)
                return false;
            SelectElement selectList = new SelectElement(selects[1]);
            IList<IWebElement> options = selectList.Options;
            IList<string> strOpt = new List<string>();

            {

                System.Threading.Thread.Sleep(3000);
                   var selects_2 = driver.FindElements(By.TagName("select"));
                    if (selects_2.Count == 0)
                        return false;
                if (selects_2.Count < 2){
                    System.Threading.Thread.Sleep(3000);
                    selects_2 = driver.FindElements(By.TagName("select"));
                }
                    SelectElement selectList_2 = new SelectElement(selects_2[1]);

                    string practice = MapPracticeById(id);
                    selectList_2.SelectByText(practice);

                    var buttons = driver.FindElements(By.TagName("button"));
                    if (buttons.Count < 6)
                        return false;

                    buttons[2].Click();
                    System.Threading.Thread.Sleep(2000);
   
                    System.Threading.Thread.Sleep(3000);

                    IWebElement dateStart = driver.FindElementById("dtpLow");
                    IWebElement dateEnd = driver.FindElementById("dtpHigh");


                    OpenQA.Selenium.Interactions.Actions actions = new OpenQA.Selenium.Interactions.Actions(driver);
                    actions.MoveToElement(dateStart);
                    actions.Click();
                    actions.SendKeys(begDate);
                    actions.Build().Perform();

                    OpenQA.Selenium.Interactions.Actions actions2 = new OpenQA.Selenium.Interactions.Actions(driver);
                    actions.MoveToElement(dateEnd);
                    actions.Click();
                    actions.SendKeys(endDate);
                    actions.Build().Perform();


                    do { //check if button clicked 
                    buttons[1].Click();
                    System.Threading.Thread.Sleep(3000);

                    buttons = driver.FindElements(By.TagName("button"));
                    
                    } while (buttons.Count >3);

                string pathUser = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string pathDownload = Path.Combine(pathUser, "Downloads");


                DateTime cTime;
                cTime = DateTime.Now;
                var fileFound = false;
                TimeSpan tDiff;

                do {
                    System.Threading.Thread.Sleep(10000);
                    string[] fileEntries = Directory.GetFiles(pathDownload, "*.xls*");
                    tDiff = DateTime.Now.Subtract(cTime);
                    if (fileEntries.Length > 0) fileFound = true;
                } while (!fileFound &&  tDiff.TotalMinutes <10);

                if (!fileFound)
                {
                    driver.Quit();
                    return false;
                }

                System.Threading.Thread.Sleep(10000);

                    while (true)
                    {
                        try
                        {
                            driver.ExecuteScript("window.history.go(-1)");
                            System.Threading.Thread.Sleep(20000);
                            while (true)
                            {
                                pathUser = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                                pathDownload = Path.Combine(pathUser, "Downloads");
                                string[] fileEntries = Directory.GetFiles(pathDownload, "*.xls.crdownload");
                                if (fileEntries.Length > 0)
                                {
                                    System.Threading.Thread.Sleep(60000);
                                }
                                else {
                                    System.Threading.Thread.Sleep(60000);
                                    break;
                                }


                            }
                            break;
                        }
                        catch
                        {
                            System.Threading.Thread.Sleep(120000);
                        }
                    }
                    System.Threading.Thread.Sleep(20000);
                    CopyFileToTempSuffix(practice, suffix.ToString());
                    System.Threading.Thread.Sleep(20000);
            }

            driver.Quit();
            return true;
        }

        bool CopyFileToTemp(string name)
        {
            try {
                string pathUser = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string pathDownload = Path.Combine(pathUser, "Downloads");

                string pattern = "*.xls";
                var dirInfo = new DirectoryInfo(pathDownload);
                //MessageBox.Show(pathDownload);
                var file = (from f in dirInfo.GetFiles(pattern) orderby f.LastWriteTime descending select f).First();
                //MessageBox.Show("2");
                string ddir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string targetDir = ddir + "//tmp";
                if (!System.IO.Directory.Exists(targetDir))
                {
                    System.IO.Directory.CreateDirectory(targetDir);
                }

                var values = name.Split('.');
                string newFile;
                if (values.Length > 1)
                {
                    newFile = targetDir + "//" + values[0] + ".xls";
                }
                else {
                    newFile = targetDir + "//" + name + ".xls";
                }

                System.IO.File.Delete(newFile);
                System.IO.File.Move(file.FullName, newFile);

                System.Threading.Thread.Sleep(5000);
            }

            catch(Exception e)
            {
                //MessageBox.Show(e.ToString());
                Console.WriteLine(e.ToString());
            }


            return true;
        }

        bool CopyFileToTempSuffix(string name, string suffix)
        {
            try
            {
                string pathUser = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string pathDownload = Path.Combine(pathUser, "Downloads");

                string pattern = "*.xls";
                var dirInfo = new DirectoryInfo(pathDownload);
                //MessageBox.Show(pathDownload);
                var file = (from f in dirInfo.GetFiles(pattern) orderby f.LastWriteTime descending select f).First();
                //MessageBox.Show("2");
                string ddir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string targetDir = ddir + "//tmp";
                if (!System.IO.Directory.Exists(targetDir))
                {
                    System.IO.Directory.CreateDirectory(targetDir);
                }

                string newFile = targetDir + "//" + name + "_" + suffix + ".xls";

                System.IO.File.Delete(newFile);
                System.IO.File.Move(file.FullName, newFile);

                System.Threading.Thread.Sleep(5000);
            }

            catch (Exception e)
            {
                //MessageBox.Show(e.ToString());
                Console.WriteLine(e.ToString());
            }


            return true;
        }
    }
}
