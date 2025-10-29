using System;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media;
using System.Windows;

namespace H1Emu_Launcher.Classes
{
    class FocusEffects
    {
        public static void BeginUnfocusAnimation(Window owner)
        {
            if (owner == null)
                return;

            BlurEffect be = new()
            {
                Radius = 0,
                KernelType = KernelType.Gaussian
            };

            Grid mainGrid = GetChildOfType<Grid>(owner);
            Grid childGrid = GetChildOfType<Grid>(mainGrid);
            childGrid.Effect = be;

            DoubleAnimation a = new(0.7, new Duration(TimeSpan.FromMilliseconds(200)))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };
            owner.BeginAnimation(Window.OpacityProperty, a);

            DoubleAnimation b = new(15, new Duration(TimeSpan.FromMilliseconds(200)))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };
            childGrid.Effect.BeginAnimation(BlurEffect.RadiusProperty, b);
        }

        public static void BeginFocusAnimation(Window owner)
        {
            if (owner == null)
                return;

            Grid mainGrid = GetChildOfType<Grid>(owner);
            Grid childGrid = GetChildOfType<Grid>(mainGrid);

            DoubleAnimation a = new(1, new Duration(TimeSpan.FromMilliseconds(200)))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };
            owner.BeginAnimation(Window.OpacityProperty, a);

            DoubleAnimation b = new(0, new Duration(TimeSpan.FromMilliseconds(200)))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };
            childGrid.Effect.BeginAnimation(BlurEffect.RadiusProperty, b);
        }

        public static T GetChildOfType<T>(DependencyObject depObj) where T : DependencyObject
        {
            for (int i = 0; i <= VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);
                if (child is T typedChild)
                    return typedChild;

                var childOfChild = GetChildOfType<T>(child);
                if (childOfChild != null)
                    return childOfChild;
            }

            return null;
        }
    }
}
