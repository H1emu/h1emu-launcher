using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Media.Imaging;

namespace H1EMU_Launcher.Classes
{
    class Carousel
    {
        public static ManualResetEvent pauseCarousel = new ManualResetEvent(true);
        public static List<string> images = new List<string>();
        public static int currentIndex = 0;
        public static int lastIndex = 0;
        public static int progress = 0;

        public static void BeginImageCarousel()
        {
            new Thread(() =>
            {
                DownloadImages();

                if (!Directory.Exists($"{Info.APPLICATION_DATA_PATH}\\H1EmuLauncher\\CarouselImages"))
                {
                    Directory.CreateDirectory($"{Info.APPLICATION_DATA_PATH}\\H1EmuLauncher\\CarouselImages");
                }

                if (Directory.GetFileSystemEntries($"{Info.APPLICATION_DATA_PATH}\\H1EmuLauncher\\CarouselImages").Length == 0) { return; }

                foreach (var fileName in Directory.EnumerateFiles($"{Info.APPLICATION_DATA_PATH}\\H1EmuLauncher\\CarouselImages"))
                {
                    images.Add(fileName);
                }

                System.Windows.Application.Current.Dispatcher.Invoke((System.Windows.Forms.MethodInvoker)delegate
                {
                    Launcher.lncher.carouselImage.Source = new BitmapImage(new Uri(images[currentIndex]));
                    Launcher.lncher.offlineImage.Visibility = System.Windows.Visibility.Hidden;
                    Launcher.lncher.imageCarousel.Visibility = System.Windows.Visibility.Visible;
                });

            for (progress = 0; progress <= 3000; progress++)
                {
                    pauseCarousel.WaitOne();

                    System.Windows.Application.Current.Dispatcher.Invoke((System.Windows.Forms.MethodInvoker)delegate
                    {
                        Launcher.lncher.carouselProgressBar.Value = progress;
                    });

                    Thread.Sleep(1);

                    if (progress == 3000)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke((System.Windows.Forms.MethodInvoker)delegate
                        {
                            ButtonAutomationPeer peer = new ButtonAutomationPeer(Launcher.lncher.nextImage);
                            IInvokeProvider invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                            invokeProv.Invoke();
                        });

                        Thread.Sleep(1000);
                        progress = 0;
                    }
                }

            }).Start();

        }

        public static void NextImage()
        {
            if (currentIndex == images.Count - 1) { currentIndex = 0; }
            else { currentIndex++; }

            lastIndex = currentIndex - 1;
            if (lastIndex < 0) { lastIndex = images.Count - 1; }

            Launcher.lncher.carouselImage.Source = new BitmapImage(new Uri(images[lastIndex]));
            Launcher.lncher.carouselImageSlider.Source = new BitmapImage(new Uri(images[currentIndex]));
        }

        public static void PreviousImage()
        {
            if (currentIndex == 0) { currentIndex = images.Count - 1; }
            else { currentIndex--; }

            lastIndex = currentIndex + 1;
            if (lastIndex > images.Count - 1) { lastIndex = 0; }

            Launcher.lncher.carouselImage.Source = new BitmapImage(new Uri(images[lastIndex]));
            Launcher.lncher.carouselImageSlider.Source = new BitmapImage(new Uri(images[currentIndex]));
        }

        public static void DownloadImages()
        {
            WebClient wc = new WebClient();
            int number = 0;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Info.CAROUSEL_MEDIA);

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        string html = reader.ReadToEnd();
                        Regex regex = new Regex("<a href=\".*\">(?<name>.*)</a>");
                        MatchCollection matches = regex.Matches(html);
                        if (matches.Count > 0)
                        {
                            foreach (Match match in matches)
                            {
                                if (match.Success)
                                {
                                    number++;

                                    if (number != 1 && number != 2)
                                    {
                                        wc.DownloadFile($"{Info.CAROUSEL_MEDIA + match.Groups["name"]}", $"{Info.APPLICATION_DATA_PATH}\\H1EmuLauncher\\CarouselImages\\{match.Groups["name"]}");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                return;
            }
        }
    }
}