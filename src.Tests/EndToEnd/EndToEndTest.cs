using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using src.Data;
using System.Collections.ObjectModel;
using Xunit;
using Microsoft.Azure.Cosmos;
using OpenQA.Selenium.DevTools.V106.Overlay;
using Xunit.Abstractions;

namespace src.Tests.OrderedTests
{

    [TestCaseOrderer("src.Tests.TestCaseOrdering.PriorityOrderer", "src.Tests")]
    public class EndToEndTest
    {

        private static readonly ChromeDriver driver = new ChromeDriver();
        private readonly ITestOutputHelper _output;
        private Actions actions { get; set; }
        public EndToEndTest(ITestOutputHelper output)
        {
            _output = output;
            actions = new Actions(driver);
        }


        [Fact, TestPriority(1)]
        public void TestCase1()
        {
            driver.Navigate().GoToUrl("http://localhost:5110");
            driver.Manage().Window.Maximize();

            // Click on add experiment button
            driver.FindElementWait(".add-element-button.experiment").Click();

            // Fill out experiment form and save experiment
            driver.FindElementWait(".modalContent");
            var inputFields = driver.FindElementsWait(".modalContent input");
            inputFields[0].SendKeys("EXP-NO");
            inputFields[1].SendKeys("EXP-title");
            inputFields[2].SendKeys("EXP-author");
            driver.FindElementWait(".green-button", 1000).Click();
            System.Threading.Thread.Sleep(1000);
            var experimentCardInfo = driver.FindElementsWait("#experiment-grid > div.all-experiment-cards > div > div > div > span");
            Assert.Equal("EXP-NO", experimentCardInfo[0].Text);
            Assert.Equal("EXP-author", experimentCardInfo[1].Text);
            Assert.Equal("EXP-title", experimentCardInfo[2].Text);

            // driver.Quit();
        }

        [Fact, TestPriority(2)]
        public void TestCase2()
        {
            // Edit experiment
            driver.FindElementWait(".card-textcontent-experiment .fa-pencil").Click();
            var inputFields = driver.FindElementsWait(".modalContent input");
            inputFields[1].SendKeys("-modified");
            driver.FindElementWait(".green-button").Click();
            var experimentCardInfo = driver.FindElementsWait(".card-textcontent-experiment > span");
            Assert.Equal("EXP-title-modified", experimentCardInfo[2].Text);
        }

        [Fact, TestPriority(3)]
        public void TestCase3()
        {
            // Create clinical test
            driver.FindElementWait(".add-element-button.clinicaltest").Click();
            var inputFields = driver.FindElementsWait(".modalContent input");
            inputFields[0].SendKeys("Test title");
            driver.FindElementWait(".green-button").Click();
            var clinicalTestCardInfo = driver.FindElementsWait("#clinical-test-grid > div.all-clinicaltest-cards > div > div > div > span:nth-child(1)");
            Assert.Equal("Test title", clinicalTestCardInfo[0].Text);
        }

        [Fact, TestPriority(4)]
        public void TestCase4()
        {
            // Edit clinical test
            driver.FindElementWait(".card-textcontent-clinicaltest .fa-pencil").Click();
            var inputFields = driver.FindElementsWait(".modalContent input");
            inputFields[0].SendKeys(" modified");
            driver.FindElementWait(".green-button").Click();

            var title = driver.FindElementWait("#clinical-test-grid > div.all-clinicaltest-cards > div > div > div > span:nth-child(1)");
            Assert.Equal("Test title modified", title.Text);

            //driver.Quit();
        }

        [Fact, TestPriority(5)]
        public void TestCase5()
        {
            // Navigate to clinical test
            driver.FindElementWait(".card-textcontent-clinicaltest").Click();

            // Navigate to overview tab
            driver.FindElementWait("button#overview").Click();

            driver.FindElementWait("body > div.page > main > div > div > div.plate > div > button").Click();

            var firstRowTds = driver.FindElementsWait("#overview-table > thead > tr > td");

            firstRowTds[1].SendKeys("RUC ID");
            Assert.Equal("RUC ID", firstRowTds[1].Text);
            firstRowTds[2].SendKeys("Patient ID");
            Assert.Equal("Patient ID", firstRowTds[2].Text);
            firstRowTds[3].SendKeys("Plasma Aalborg ID");
            Assert.Equal("Plasma Aalborg ID", firstRowTds[3].Text);
            firstRowTds[4].SendKeys("EV Aalborg ID");
            Assert.Equal("EV Aalborg ID", firstRowTds[4].Text);


            for (int i = 0; i < PatientData.Length - 1; i++)
            {
                var ithRowTds = driver.FindElementsWait($"#overview-table > tbody > tr:nth-child({i + 1}) > td");

                string[] pd = (PatientData[(i)].Split("\t"));
                for (int j = 0; j < pd.Length; j++)
                {
                    if (pd[j] == "") continue;
                    ithRowTds[j + 1].SendKeys(pd[j].TrimEnd());
                    Assert.Equal(pd[j].TrimEnd(), ithRowTds[j + 1].Text);
                }
            }

            //actions.ContextClick(driver.FindElementWait("button#overview")).Perform();
        }

        [Fact, TestPriority(6)]
        public void TestCase6()
        {
            string[] chosenTableTitles = { "RUC ID", "Patient ID", "Plasma Aalborg ID" };
            driver.FindElementWait("#check-button > i").Click();
            driver.FindElementWait("body > div.page > main > div > div.modal.fade.show > div > div > div.modal-body > input[type=checkbox]").Click();
            driver.FindElementWait("body > div.page > main > div > div.modal.fade.show > div > div > div.modal-footer > button.btn.btn-primary").Click();

            driver.FindElementWait("#key-selectors > div:nth-child(1) > select > option:nth-child(2)").Click();
            driver.FindElementWait("#key-selectors > div:nth-child(2) > select > option:nth-child(3)").Click();
            driver.FindElementWait("#key-selectors > div:nth-child(3) > select > option:nth-child(4)").Click();

            for (int i = 0; i < 3; i++)
            {
                SelectElement select = new SelectElement(driver.FindElementWait($"#key-selectors > div:nth-child({i + 1}) > select"));
                Assert.Equal(chosenTableTitles[i], select.SelectedOption.Text);
            }

            var overview = driver.FindElementWait("body > div.page > main > div > div > div.plates > div");
            string overviewClass = overview.GetAttribute("class");
            Assert.Equal("plate numSlides3", overviewClass);

            int pdIndex = 0;
            bool shouldRun = true;
            int secondDivNthChildIndex = 0;
            while (shouldRun)
            {
                for (int i = 1; i < 4; i++)
                {
                    for (int j = 1; j < 4; j++)
                    {
                        var pd = PatientData[pdIndex++].Split("\t");
                        if (pd[0] == "")
                        {
                            shouldRun = false;
                            break;
                        }
                        for (int k = 1; k < 4; k++)
                        {
                            var divElem = driver.FindElementWait($"" +
                                $"body > div.page > main > div > div > div.plates > div > div > div.Slide-{i} > div:nth-child({j+secondDivNthChildIndex}) > div:nth-child({k})");
                            _output.WriteLine($"div.Slide-{i} > div:nth-child({j + secondDivNthChildIndex}) > div:nth-child({k}) text was: {divElem.Text}");
                            _output.WriteLine($"pd[k-1] = {pd[k - 1].TrimEnd()}");
                            _output.WriteLine($"");

                            Assert.Equal(pd[k - 1].TrimEnd(), divElem.Text);
                        }
                    }
                    if (!shouldRun) break;
                }
                secondDivNthChildIndex += 3;
            }


        }


        [Fact, TestPriority(1000)]
        public async void CleanUp()
        {
            DatabaseService.EnableTestMode();
            await DatabaseService.Instance.SetupDatabase();
            if (DatabaseService.Instance.Database == null) throw new Exception("Database is null");

            Container ctContainer = await DatabaseService.Instance.Database.CreateContainerIfNotExistsAsync("ClinicalTest", "/PartitionKey");
            Container eContainer = await DatabaseService.Instance.Database.CreateContainerIfNotExistsAsync("Experiment", "/PartitionKey");
            await ctContainer.DeleteContainerAsync();
            await eContainer.DeleteContainerAsync();
            await DatabaseService.Instance.Database.CreateContainerIfNotExistsAsync("ClinicalTest", "/PartitionKey");
            await DatabaseService.Instance.Database.CreateContainerIfNotExistsAsync("Experiment", "/PartitionKey");

        }


        public string[] PatientData = "1\t10b\t1P\t1EV\r\n2\t13a\t2P\t2EV\r\n3\t17a\t3P\t3EV\r\n4\t19a\t4P\t4EV\r\n5\t20a\t5P\t5EV\r\n6\t25b\t6P\t6EV\r\n7\t27a\t7P\t7EV\r\n8\t27b\t8P\t8EV\r\n9\t28b\t9P\t9EV\r\n10\t29b\t10P\t10EV\r\n11\t32a\t11P\t11EV\r\n12\t32b\t12P\t12EV\r\n13\t39a\t13P\t13EV\r\n14\t3a\t14P\t14EV\r\n15\t43a\t15P\t15EV\r\n16\t45a\t16P\t16EV\r\n17\t45b\t17P\t17EV\r\n18\t46b\t18P\t18EV\r\n19\t48b\t19P\t19EV\r\n20\t4b\t20P\t20EV\r\n21\t50a\t21P\t21EV\r\n22\t5b\t22P\t22EV\r\n23\t10c\t23P\t23EV\r\n24\t10d\t24P\t24EV\r\n25\t11c\t25P\t25EV\r\n26\t11d\t26P\t26EV\r\n27\t12c\t27P\t27EV\r\n28\t12d\t28P\t28EV\r\n29\t13c\t29P\t29EV\r\n30\t13d\t30P\t30EV\r\n31\t1d\t31P\t31EV\r\n32\t23b\t32P\t32EV\r\n33\t2c\t33P\t33EV\r\n34\t3c\t34P\t34EV\r\n35\t3d\t35P\t35EV\r\n36\t4c\t36P\t36EV\r\n37\t4d\t37P\t37EV\r\n38\t5c\t38P\t38EV\r\n39\t5d\t39P\t39EV\r\n40\t6c\t40P\t40EV\r\n41\t6d\t41P\t41EV\r\n42\t7c\t42P\t42EV\r\n43\t7d\t43P\t43EV\r\n44\t8c\t44P\t44EV\r\n45\t8d\t45P\t45EV\r\n46\t9c\t46P\t46EV\r\n47\t9d\t47P\t47EV\r\n48\t24a\t48P\t48EV\r\n".Split("\n");

    }

    public static class ChromeDriverExtension
    {
        public static IWebElement FindElementWait(this ChromeDriver driver, string selector, int timeoutInMilliseconds)
        {
            IWebElement? webElement = null;
            do
            {

                try
                {
                    do
                    {
                        webElement = new WebDriverWait(driver, TimeSpan.FromMilliseconds(timeoutInMilliseconds)).Until(driver => driver.FindElement(By.CssSelector(selector)));
                    }
                    while (webElement == null);
                }
                catch (Exception ex)
                {

                }
            }
            while (webElement == null);

            return webElement;
        }
        public static IWebElement FindElementWait(this ChromeDriver driver, string selector)
        {
            IWebElement? webElement = null;
            do
            {

                try
                {
                    do
                    {
                        webElement = new WebDriverWait(driver, TimeSpan.FromMilliseconds(1000)).Until(driver => driver.FindElement(By.CssSelector(selector)));
                    }
                    while (webElement == null);
                }
                catch (Exception ex)
                {

                }
            }
            while (webElement == null);

            return webElement;
        }

        public static ReadOnlyCollection<IWebElement> FindElementsWait(this ChromeDriver driver, string selector)
        {
            ReadOnlyCollection<IWebElement> webElements = null;
            do
            {

                try
                {
                    do
                    {
                        webElements = new WebDriverWait(driver, TimeSpan.FromMilliseconds(1000)).Until(driver => driver.FindElements(By.CssSelector(selector)));
                    }
                    while (webElements == null);
                }
                catch (Exception ex)
                {

                }
            }
            while (webElements == null || webElements.Count == 0);

            return webElements;
        }
    }


}
