using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Internal;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace testSelenium
{
    public static class Program
    {
        public static IWebElement SetAttribute(this IWebElement element, string attr, string value)
        {
            var driver = ((IWrapsDriver)element).WrappedDriver;
            var jsExecutor = (IJavaScriptExecutor)driver;
            jsExecutor.ExecuteScript("arguments[0].setAttribute(arguments[1], arguments[2]);", element, attr, value);
            return element;
        }

        public static IWebElement RemoveAttribute(this IWebElement element, string attr)
        {
            var driver = ((IWrapsDriver)element).WrappedDriver;
            var jsExecutor = (IJavaScriptExecutor)driver;
            jsExecutor.ExecuteScript("arguments[0].removeAttribute(arguments[1]);", element, attr);
            return element;
        }

        public static IWebElement SetFocus(this IWebElement element)
        {
            var driver = ((IWrapsDriver)element).WrappedDriver;
            var jsExecutor = (IJavaScriptExecutor)driver;
            jsExecutor.ExecuteScript("arguments[0].focus()",element);
            return element;
        }

        public static IWebDriver FindActiveDriver(string url,string debuggerAddress, out bool flag)
        {
            IWebDriver driver = null;
            flag = false;
            try
            {
                ChromeOptions opt = new ChromeOptions();
                opt.DebuggerAddress = debuggerAddress;
                driver = new ChromeDriver(opt);
                if(driver!=null)
                {
                    if (driver.Url == url)
                    {
                        flag = true;
                    }
                    else
                    {
                        return null;
                    }
                    
                }
               
            }
            catch(Exception ex)
            {

            }
            return driver;
        }
        public static void Test()
        {
            try
            {
                ChromeOptions opt = new ChromeOptions();
                opt.AddArgument("--remote-debugging-port=9222");
                IWebDriver driver = new ChromeDriver(opt);
           
                driver.Navigate().GoToUrl("http://innov8.uhg.com/AuditTrail");
                string sessionId = driver.CurrentWindowHandle;//current tab from  browser

                driver.FindElement(By.XPath("//*[@id='txtICN']")).SetAttribute("disabled", "disabled");

                driver.FindElement(By.XPath("//*[@id='txtICN']")).RemoveAttribute("disabled");

                driver.FindElement(By.XPath("//*[@id='txtICN']")).SetFocus();

                bool flag = false;
                var foundDriver = FindActiveDriver("http://innov8.uhg.com/AuditTrail", "127.0.0.1:9222", out flag);
                if (foundDriver != null && flag)
                {
                    foundDriver.SwitchTo().Window(sessionId);//swtich back to the previous tab from browser
                    foundDriver.Navigate().GoToUrl("http://innov8.uhg.com/WIM/Login");
                }
            }
            catch (Exception ex)
            {

            }
            
        }
        static void Main(string[] args)
        {
            Test();
        }
    }
}
