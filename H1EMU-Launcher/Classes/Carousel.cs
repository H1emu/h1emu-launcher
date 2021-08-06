using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace H1EMU_Launcher.Classes
{
    class Carousel
    {
        public static List<string> images = new List<string>();
        public static int currentIndex = 0;
        public static int progressI = 0;

        public static void BeginImageCarousel()
        {
            new Thread(() =>
            {
                DownloadImages();

                if (Directory.GetFileSystemEntries($"{Launcher.appDataPath}\\H1EmuLauncher\\CarouselImages").Length == 0) { return; }

                System.Windows.Application.Current.Dispatcher.Invoke((System.Windows.Forms.MethodInvoker)delegate
                {
                    Launcher.lncher.offlineImage.Visibility = System.Windows.Visibility.Hidden;
                    Launcher.lncher.imageCarousel.Visibility = System.Windows.Visibility.Visible;
                });

                foreach (var fileName in Directory.EnumerateFiles($"{Launcher.appDataPath}\\H1EmuLauncher\\CarouselImages"))
                {
                    images.Add(fileName);
                }

                System.Windows.Application.Current.Dispatcher.Invoke((System.Windows.Forms.MethodInvoker)delegate
                {
                    Launcher.lncher.carouselImage.Source = new BitmapImage(new Uri(images[currentIndex]));
                });

                for (progressI = 0; progressI <= 3000; progressI++)
                {
                    System.Windows.Application.Current.Dispatcher.Invoke((System.Windows.Forms.MethodInvoker)delegate
                    {
                        Launcher.lncher.carouselProgressBar.Value = progressI;
                    });

                    Thread.Sleep(1);

                    if (progressI == 3000)
                    {
                        Thread.Sleep(1000);
                        progressI = 0;

                        NextImage();
                    }
                }

            }).Start();
        }

        public static void NextImage()
        {
            if (currentIndex == images.Count - 1) { currentIndex = 0; }
            else { currentIndex++; }

            System.Windows.Application.Current.Dispatcher.Invoke((System.Windows.Forms.MethodInvoker)delegate
            {
                Launcher.lncher.carouselImage.Source = new BitmapImage(new Uri(images[currentIndex]));
            });

            progressI = 0;
        }

        public static void PreviousImage()
        {
            if (currentIndex == 0) { currentIndex = images.Count - 1; }
            else { currentIndex--; }

            Launcher.lncher.carouselImage.Source = new BitmapImage(new Uri(images[currentIndex]));

            progressI = 0;
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
