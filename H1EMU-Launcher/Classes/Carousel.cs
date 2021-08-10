using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Media.Imaging;

namespace H1EMU_Launcher.Classes
{
    class Carousel
    {
        public static List<string> images = new List<string>();
        public static int currentIndex = 0;
        public static int lastIndex = 0;
        public static int progressI = 0;
        public static ManualResetEvent pauseCarousel = new ManualResetEvent(true);

        public static void BeginImageCarousel()
        {
            new Thread(() =>
            {
                DownloadImages();

                if (Directory.GetFileSystemEntries($"{Launcher.appDataPath}\\H1EmuLauncher\\CarouselImages").Length == 0) { return; }

                foreach (var fileName in Directory.EnumerateFiles($"{Launcher.appDataPath}\\H1EmuLauncher\\CarouselImages"))
                {
                    images.Add(fileName);
                }

                System.Windows.Application.Current.Dispatcher.Invoke((System.Windows.Forms.MethodInvoker)delegate
                {
                    Launcher.lncher.carouselImage.Source = new BitmapImage(new Uri(images[currentIndex]));
                    Launcher.lncher.offlineImage.Visibility = System.Windows.Visibility.Hidden;
                    Launcher.lncher.imageCarousel.Visibility = System.Windows.Visibility.Visible;
                });

                for (progressI = 0; progressI <= 3000; progressI++)
                {
                    pauseCarousel.WaitOne();

                    System.Windows.Application.Current.Dispatcher.Invoke((System.Windows.Forms.MethodInvoker)delegate
                    {
                        Launcher.lncher.carouselProgressBar.Value = progressI;
                    });

                    Thread.Sleep(1);

                    if (progressI == 3000)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke((System.Windows.Forms.MethodInvoker)delegate
                        {
                            ButtonAutomationPeer peer = new ButtonAutomationPeer(Launcher.lncher.nextImage);
                            IInvokeProvider invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                            invokeProv.Invoke();
                        });

                        Thread.Sleep(1000);
                        progressI = 0;
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
            int number = 1;

            WebClient wc = new WebClient();

            while (true)
            {
                try
                {
                    wc.DownloadString($"https://h1emu.com/Public/uploads/media/embed/{number}.png");
                    wc.DownloadFile($"https://h1emu.com/Public/uploads/media/embed/{number}.png", $"{Launcher.appDataPath}\\H1EmuLauncher\\CarouselImages\\{number}.png");
                    number++;
                }
                catch
                {
                    break;
                }
            }
        }
    }
}
