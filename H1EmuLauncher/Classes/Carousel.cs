using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Media.Imaging;

namespace H1EmuLauncher.Classes
{
    class Carousel
    {
        public static ManualResetEvent pauseCarousel = new(true);
        public static List<string> images = new();
        public static int currentIndex = 0;
        public static int lastIndex = 0;
        public static int progress = 0;
        public static string imagesFolder = $"{Info.APPLICATION_DATA_PATH}\\H1EmuLauncher\\CarouselImages";

        public static void BeginImageCarousel()
        {
            new Thread(() =>
            {
                if (!Directory.Exists(imagesFolder))
                {
                    Directory.CreateDirectory(imagesFolder);
                }

                DownloadImages();

                if (Directory.GetFileSystemEntries(imagesFolder).Length == 0)
                    return;

                foreach (var fileName in Directory.EnumerateFiles(imagesFolder))
                {
                    images.Add(fileName);
                }

                System.Windows.Application.Current.Dispatcher.Invoke(new Action(delegate
                {
                    Launcher.launcherInstance.carouselImage.Source = new BitmapImage(new Uri(images[currentIndex]));
                    Launcher.launcherInstance.offlineImage.Visibility = System.Windows.Visibility.Hidden;
                    Launcher.launcherInstance.imageCarousel.Visibility = System.Windows.Visibility.Visible;
                }));

            for (progress = 0; progress <= 3000; progress++)
                {
                    pauseCarousel.WaitOne();

                    System.Windows.Application.Current.Dispatcher.Invoke(new Action(delegate
                    {
                        Launcher.launcherInstance.carouselProgressBar.Value = progress;
                    }));

                    Thread.Sleep(1);

                    if (progress == 3000)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(new Action(delegate
                        {
                            ButtonAutomationPeer peer = new(Launcher.launcherInstance.nextImage);
                            IInvokeProvider invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                            invokeProv.Invoke();
                        }));

                        Thread.Sleep(1000);
                        progress = 0;
                    }
                }

            }).Start();

        }

        public static void NextImage()
        {
            if (currentIndex == images.Count - 1)
                currentIndex = 0;
            else { currentIndex++; }

            lastIndex = currentIndex - 1;
            if (lastIndex < 0)
                lastIndex = images.Count - 1;

            Launcher.launcherInstance.carouselImage.Source = new BitmapImage(new Uri(images[lastIndex]));
            Launcher.launcherInstance.carouselImageSlider.Source = new BitmapImage(new Uri(images[currentIndex]));
        }

        public static void PreviousImage()
        {
            if (currentIndex == 0)
                currentIndex = images.Count - 1;
            else { currentIndex--; }

            lastIndex = currentIndex + 1;
            if (lastIndex > images.Count - 1)
                lastIndex = 0;

            Launcher.launcherInstance.carouselImage.Source = new BitmapImage(new Uri(images[lastIndex]));
            Launcher.launcherInstance.carouselImageSlider.Source = new BitmapImage(new Uri(images[currentIndex]));
        }

        public static void DownloadImages()
        {
#pragma warning disable SYSLIB0014 // Type or member is obsolete
            WebClient wc = new();
#pragma warning restore SYSLIB0014 // Type or member is obsolete
#pragma warning disable SYSLIB0014 // Type or member is obsolete
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Info.CAROUSEL_MEDIA);
#pragma warning restore SYSLIB0014 // Type or member is obsolete

            try
            {
                using HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                using StreamReader reader = new(response.GetResponseStream());
                string html = reader.ReadToEnd();
                Regex regex = new("<a href=\".*\">(?<name>.*)</a>");
                MatchCollection matches = regex.Matches(html);
                if (matches.Count > 0)
                {
                    foreach (Match match in matches.Cast<Match>())
                    {
                        if (match.Success)
                        {
                            if (match.Groups["name"].ToString().Contains(".png"))
                                wc.DownloadFile($"{Info.CAROUSEL_MEDIA}{match.Groups["name"]}", $"{imagesFolder}\\{match.Groups["name"]}");
                        }
                    }
                }
            }
            catch { }
        }
    }
}