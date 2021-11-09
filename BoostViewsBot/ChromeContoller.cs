using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BoostViewsBot
{
    class ChromeContoller
    {
        public string ViewsCount { get; private set; }

        private List<ChromeDriver> _chromeDrivers;
        private Thread[] _threads;
        private ChromeOptions _chromeOptions;
        private string _url;
        private uint _driversDefaultCount;

        public ChromeContoller()
        {
            _driversDefaultCount = 4;
            _threads = new Thread[_driversDefaultCount];
            _chromeDrivers = new List<ChromeDriver>();
            _chromeOptions = new ChromeOptions();
            _chromeOptions.AddArgument("start-maximized");
            _chromeOptions.AddArgument("mute-audio");
            _chromeOptions.AddArgument("no-sandbox");
            _chromeOptions.AddArgument("disable-dev-shm-usage");
            _chromeOptions.AddArgument("disable-extensions");
            _chromeOptions.AddArgument("incognito");
            _chromeOptions.AddArgument("disable-gpu");
            _chromeOptions.AddArgument("headless");
        }

        public void SetUrl(string url)
        {
            _url = url;
        }

        public async void BoostViews()
        {
            await Task.Run(() =>
            {
                CreateChromeDrivers();
                _chromeDrivers[0].Navigate().GoToUrl(_url);
                SetViewsCount(_chromeDrivers[0]);

                for (int i = 0; i < _chromeDrivers.Count; i++)
                {
                    ParameterizedThreadStart parameterizedThread = new ParameterizedThreadStart(StartRefreshing);
                    _threads[i] = new Thread(parameterizedThread);
                    _threads[i].Start(_chromeDrivers[i]);
                }
            });
        }

        public void CloseAllDrivers()
        {
            foreach (Thread thread in _threads)
            {
                thread.Abort();
            }

            foreach (ChromeDriver driver in _chromeDrivers)
            {
                try
                {
                    driver.Quit();
                }
                catch
                {
                    break;
                }
            }

            _chromeDrivers.Clear();
        }

        private void RestartDrivers()
        {
            CloseAllDrivers();
            _chromeDrivers.Clear();
            Thread.Sleep(1000);
            BoostViews();
        }

        private void CreateChromeDrivers()
        {
            for (int i = 0; i < _driversDefaultCount; i++)
            {
                ChromeDriverService chromeDriverService = ChromeDriverService.CreateDefaultService();
                _chromeDrivers.Add(new ChromeDriver(chromeDriverService, _chromeOptions));
                _chromeDrivers[i].Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);
            }
        }

        private void StartRefreshing(object o)
        {
            bool isYouTubePlayer = IsYoutubePlayer();
            ChromeDriver chromeDriver = o as ChromeDriver;

            try
            {
                if (isYouTubePlayer == false)
                    RefreshVKPlayer(chromeDriver);
                else
                    RefreshYouTubePlayer(chromeDriver);
            }
            catch
            {
                return;
            }
        }


        private void RefreshVKPlayer(ChromeDriver chromeDriver)
        {
            while (_chromeDrivers.Count > 0)
            {
                chromeDriver.Navigate().GoToUrl(_url);
                Thread.Sleep(1000);

                lock (ViewsCount)
                {
                    SetViewsCount(chromeDriver);
                }
            }
        }

        private void RefreshYouTubePlayer(ChromeDriver chromeDriver)
        {
            while (_chromeDrivers.Count > 0)
            {
                chromeDriver.Navigate().GoToUrl(_url);
                PlayYoutubeVideo(chromeDriver);
                Thread.Sleep(1000);

                lock (ViewsCount)
                {
                    SetViewsCount(chromeDriver);
                }
            }
        }

        private bool IsYoutubePlayer()
        {
            try
            {
                _chromeDrivers[0].FindElement(By.Id("video_yt"));
                return true;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }

        private void PlayYoutubeVideo(ChromeDriver chromeDriver)
        {
            try
            {
                chromeDriver.SwitchTo().DefaultContent();
                IWebElement webElement = chromeDriver.FindElement(By.ClassName("video_yt_player"));
                webElement.Click();
            }
            catch
            {
                return;
            }
        }

        private void SetViewsCount(ChromeDriver chromeDriver)
        {
            try
            {
                IWebElement views = chromeDriver.FindElement(By.XPath("//*[@id=\"mv_views\"]"));
                ViewsCount = views.Text;
            }
            catch
            {
                CloseAllDrivers();
            }
        }
    }
}
