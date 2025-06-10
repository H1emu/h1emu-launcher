using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace H1EmuLauncher.Classes
{
    class Carousel
    {
        public static Storyboard playCarousel;
        public static List<string> images = [];
        public static int currentIndex = 0;
        public static int lastIndex = 0;

        public static ImageSource ConvertResourceToImageSource(string psResourceName)
        {
            try
            {
                return BitmapFrame.Create(new Uri("pack://application:,,,/H1Emu Launcher;component/" + psResourceName, UriKind.Absolute));
            }
            catch
            {
                return null;
            }
        }

        public static void BeginImageCarousel()
        {
            new Thread(() =>
            {
                int i = 1;
                ResourceSet resourceSet = Properties.Resources.ResourceManager.GetResourceSet(CultureInfo.CurrentUICulture, true, true);
                foreach (DictionaryEntry resource in resourceSet)
                {
                    if (ConvertResourceToImageSource($"Resources\\{i}.jpg") != null)
                    {
                        images.Add($"{i}.jpg");
                        i++;
                    }
                    else
                        break;
                }

                Application.Current.Dispatcher.Invoke(new Action(delegate
                {
                    DoubleAnimation carouselImageRectangle = new(0, LauncherWindow.launcherInstance.carouselRectangleGrid.ActualWidth, new Duration(TimeSpan.FromSeconds(6)))
                    {
                        EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
                    };
                    carouselImageRectangle.SetValue(Storyboard.TargetProperty, LauncherWindow.launcherInstance.carouselRectangle);
                    carouselImageRectangle.SetValue(Storyboard.TargetPropertyProperty, new PropertyPath(FrameworkElement.WidthProperty));

                    playCarousel = new Storyboard();
                    playCarousel.Children.Add(carouselImageRectangle);
                    playCarousel.Completed += (s, o) => 
                    {
                        NextImage();
                    };
                    playCarousel.Begin();

                    if (LauncherWindow.launcherInstance.imageCarousel.Visibility == Visibility.Hidden)
                        playCarousel.Stop();

                }));
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

            LauncherWindow.launcherInstance.carouselImage.Source = ConvertResourceToImageSource($"Resources\\{images[lastIndex]}");
            LauncherWindow.launcherInstance.carouselImageFollow.Source = ConvertResourceToImageSource($"Resources\\{images[currentIndex]}");
            LauncherWindow.launcherInstance.CarouselNextAnimation.Begin();
            LauncherWindow.launcherInstance.CarouselNextAnimationFollow.Begin();
        }

        public static void PreviousImage()
        {
            if (currentIndex == 0)
                currentIndex = images.Count - 1;
            else { currentIndex--; }

            lastIndex = currentIndex + 1;
            if (lastIndex > images.Count - 1)
                lastIndex = 0;

            LauncherWindow.launcherInstance.carouselImage.Source = ConvertResourceToImageSource($"Resources\\{images[lastIndex]}");
            LauncherWindow.launcherInstance.carouselImageFollow.Source = ConvertResourceToImageSource($"Resources\\{images[currentIndex]}");
            LauncherWindow.launcherInstance.CarouselPreviousAnimation.Begin();
            LauncherWindow.launcherInstance.CarouselPreviousAnimationFollow.Begin();
        }
    }
}