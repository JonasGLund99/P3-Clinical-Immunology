using src.Data;
using Xunit;
using Xunit.Abstractions;
using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Chrome;

namespace src.Tests;

public class EndToEndTest
{
    private ChromeDriver driver;
    private Actions actions;
    public EndToEndTest()
    {
        // ChromeOptions options = new ChromeOptions();
        // options.AddArgument("headless");
        driver = new ChromeDriver();
        actions = new Actions(driver);
    }

    [Fact]
    public void CompleteFlow()
    {
        driver.Navigate().GoToUrl("http://localhost:5110");
        driver.Manage().Window.Maximize();

        // Click on add experiment button
        driver.FindElementWait(".add-element-button.experiment").Click();

        // Fill out experiment form and save experiment
        driver.FindElementWait(".modalContent");
        var inputFields = driver.FindElements(By.CssSelector(".modalContent input"));
        inputFields[0].SendKeys("EXP-NO");
        inputFields[1].SendKeys("EXP-title");
        inputFields[2].SendKeys("EXP-author");
        driver.FindElement(By.CssSelector(".green-button")).Click();
        var experimentCardInfo = driver.FindElements(By.CssSelector(".card-textcontent-experiment > span"));
        Assert.Equal("EXP-NO", experimentCardInfo[0].Text);
        Assert.Equal("EXP-author", experimentCardInfo[1].Text);
        Assert.Equal("EXP-title", experimentCardInfo[2].Text);

        // Edit experiment
        driver.FindElementWait(".card-textcontent-experiment .fa-pencil").Click();
        inputFields = driver.FindElements(By.CssSelector(".modalContent input"));
        inputFields[1].SendKeys("-modified");
        driver.FindElement(By.CssSelector(".green-button")).Click();
        experimentCardInfo = driver.FindElements(By.CssSelector(".card-textcontent-experiment > span"));
        Assert.Equal("EXP-title-modified", experimentCardInfo[2].Text);

        // Select experiment and create clinical test
        driver.FindElementWait(".add-element-button.clinicaltest").Click();
        inputFields = driver.FindElements(By.CssSelector(".modalContent input"));
        inputFields[0].SendKeys("Test title");
        driver.FindElement(By.CssSelector(".green-button")).Click();
        var clinicalTestCardInfo = driver.FindElements(By.CssSelector(".card-textcontent-clinicaltest > span"));
        Assert.Equal("Test title", clinicalTestCardInfo[0].Text);

        // // Edit clinical test
        // driver.FindElementWait(".card-textcontent-clinicaltest .fa-pencil").Click();
        // inputFields = driver.FindElements(By.CssSelector(".modalContent input"));
        // inputFields[0].SendKeys(" modified");
        // driver.FindElement(By.CssSelector(".green-button")).Click();

        // // Navigate to clinical test
        // driver.FindElementWait(".card-textcontent-clinicaltest").Click();

        // // Navigate to overview tab
        // driver.FindElementWait("button#overview").Click();








        // actions.ContextClick(driver.FindElementWait("button#overview")).Perform();

        Assert.True(true);

        // driver.Quit();
    }
}

public static class ChromeDriverExtension
{
    public static IWebElement FindElementWait(this ChromeDriver driver, string selector, int timeoutInMilliseconds)
    {
        return new WebDriverWait(driver, TimeSpan.FromMilliseconds(timeoutInMilliseconds)).Until(driver => driver.FindElement(By.CssSelector(selector)));
    }
    public static IWebElement FindElementWait(this ChromeDriver driver, string selector)
    {
        return new WebDriverWait(driver, TimeSpan.FromMilliseconds(1000)).Until(driver => driver.FindElement(By.CssSelector(selector)));
    }
}