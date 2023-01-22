using H1EmuLauncher.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Media;
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

        static internal ImageSource doGetImageSourceFromResource(string psResourceName)
        {
            Uri oUri = new Uri("pack://application:,,,/H1EmuLauncher;component/" + psResourceName, UriKind.RelativeOrAbsolute);
            return BitmapFrame.Create(oUri);
        }

        public static void BeginImageCarousel()
        {
            new Thread(() =>
            {
                ResourceSet resourceSet = Resources.ResourceManager.GetResourceSet(CultureInfo.CurrentUICulture, true, true);
                foreach (DictionaryEntry entry in resourceSet)
                {
                    if (int.TryParse(entry.Key.ToString().Replace("_", ""), out _))
                    {
                        images.Add($"{entry.Key.ToString().Replace("_", "")}.jpg");
                    }
                }

                Application.Current.Dispatcher.Invoke(new Action(delegate
                {
                    Launcher.launcherInstance.carouselImage.Source = doGetImageSourceFromResource($"Resources\\{images[currentIndex]}");
                }));

                for (progress = 0; progress <= 3000; progress++)
                {
                    pauseCarousel.WaitOne();

                    Application.Current.Dispatcher.Invoke(new Action(delegate
                    {
                        Launcher.launcherInstance.carouselProgressBar.Value = progress;
                    }));

                    Thread.Sleep(1);

                    if (progress == 3000)
                    {
                        Application.Current.Dispatcher.Invoke(new Action(delegate
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

            Launcher.launcherInstance.carouselImage.Source = doGetImageSourceFromResource($"Resources\\{images[lastIndex]}");
            Launcher.launcherInstance.carouselImageSlider.Source = doGetImageSourceFromResource($"Resources\\{images[currentIndex]}");
        }

        public static void PreviousImage()
        {
            if (currentIndex == 0)
                currentIndex = images.Count - 1;
            else { currentIndex--; }

            lastIndex = currentIndex + 1;
            if (lastIndex > images.Count - 1)
                lastIndex = 0;

            Launcher.launcherInstance.carouselImage.Source = doGetImageSourceFromResource($"Resources\\{images[lastIndex]}");
            Launcher.launcherInstance.carouselImageSlider.Source = doGetImageSourceFromResource($"Resources\\{images[currentIndex]}");
        }
    }
}