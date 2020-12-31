using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Google.Cloud.Speech.V1;
using NAudio.Wave;
using System.Threading;
using System.Net;

namespace YtCaptchaBypass
{
    class Program
    {
        static Random rnd = new Random();
        static void Main(string[] args)
        {
            var driver = new ChromeDriver();
            driver.Navigate().GoToUrl("https://www.google.com/recaptcha/api2/demo");
            waitRandom(2000, 4000);
            switchIframe("a-",driver);
            clickElement("span", "id", "recaptcha-anchor",driver);
            waitRandom(2000, 4000);
            driver.SwitchTo().DefaultContent();
            switchIframe("c-", driver);
            clickElement("button", "id", "recaptcha-audio-button", driver);
            waitRandom(1000, 2000);
            downloadAudio(driver);
            convertMp3ToWav("a.mp3", "o.wav");
            waitRandom(1000, 2000);
            typefield("input", "id", "audio-response", driver, speechToText("o.wav"));
            waitRandom(1000, 3000);
            clickElement("button", "id", "recaptcha-verify-button", driver);

        }
        static string speechToText(string input)
        {
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "crendi.json");

            var speech = SpeechClient.Create();
            var config = new RecognitionConfig
            {
                Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
                SampleRateHertz = 22050,
                LanguageCode = LanguageCodes.German.Germany
            };
            var audio = RecognitionAudio.FromFile(input);
            var response = speech.Recognize(config, audio);
            foreach(var result in response.Results)
            {
                return result.Alternatives[0].Transcript;
            }
            return "";
        }
        static void convertMp3ToWav(string input,string output)
        {
            using(Mp3FileReader mp3 = new Mp3FileReader(input))
            {
                using(WaveStream pcm = WaveFormatConversionStream.CreatePcmStream(mp3))
                {
                    var outFormat = new WaveFormat(pcm.WaveFormat.SampleRate, 1);
                    using(var resampler = new MediaFoundationResampler(pcm, outFormat))
                    {
                        WaveFileWriter.CreateWaveFile(output, resampler);
                    }
                }
            }
        }
        static void downloadAudio(ChromeDriver driver)
        {
            using (var client = new WebClient())
            {

                client.DownloadFile(driver.FindElement(By.XPath("//audio[@id='audio-source']")).GetAttribute("src"), "a.mp3");
            }
        }
        static void switchIframe(string value,ChromeDriver driver)
        {
            driver.SwitchTo().Frame(driver.FindElement(By.CssSelector("iframe[name^='" + value + "']")));
        }
        static void clickElement(string tag,string type,string value, ChromeDriver driver)
        {
            driver.FindElement(By.XPath("//" + tag + "[@" + type + "='" + value + "']")).Click();
        }
        static void waitRandom(int min,int max)
        {
              Thread.Sleep(rnd.Next(min, max));
        }
        static void typefield(string tag, string type, string value, ChromeDriver driver,string input)
        {
            driver.FindElement(By.XPath("//" + tag + "[@" + type + "='" + value + "']")).SendKeys(input);
        }
        
    }
}
