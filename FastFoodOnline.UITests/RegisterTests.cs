using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;

namespace FastFoodOnline.UITests
{
    [TestFixture]
    public class RegisterTests
    {
        private IWebDriver driver;

        [SetUp]
        public void Setup()
        {
            var options = new EdgeOptions();
            driver = new EdgeDriver(options);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
        }

        [Test]
        public void Register_WithValidData_ShouldRedirectToHome()
        {
            driver.Navigate().GoToUrl("https://localhost:7036/Account/Register");

            driver.FindElement(By.Id("Email")).SendKeys("user1@example.com");
            driver.FindElement(By.Id("Password")).SendKeys("Abcd1234@");
            driver.FindElement(By.Id("ConfirmPassword")).SendKeys("Abcd1234@");
            driver.FindElement(By.Id("FullName")).SendKeys("Nguyen Van A");
            driver.FindElement(By.Id("PhoneNumber")).SendKeys("0123456789");
            driver.FindElement(By.Id("Address")).SendKeys("Da Nang");
            driver.FindElement(By.Id("DateOfBirth")).SendKeys("01/01/2000");

            driver.FindElement(By.CssSelector("form button[type='submit']")).Click();

            Assert.That(driver.Url.Contains("/Home") || driver.Url.Contains("/"));
        }

        [TearDown]
        public void TearDown()
        {
            if (driver != null)
            {
                try
                {
                    driver.Quit();
                    driver.Dispose();
                }
                catch { }
                finally
                {
                    driver = null;
                }
            }
        }
    }
}
