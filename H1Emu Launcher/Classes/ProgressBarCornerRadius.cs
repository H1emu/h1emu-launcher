using System.Windows;

namespace H1Emu_Launcher.Classes
{
    public static class ProgressBarCornerRadius
    {
        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.RegisterAttached(
                                                                                            "CornerRadius",
                                                                                            typeof(CornerRadius),
                                                                                            typeof(ProgressBarCornerRadius),
                                                                                            new FrameworkPropertyMetadata(new CornerRadius(0)));

        public static void SetCornerRadius(DependencyObject element, CornerRadius value) => element.SetValue(CornerRadiusProperty, value);

        public static CornerRadius GetCornerRadius(DependencyObject element) => (CornerRadius)element.GetValue(CornerRadiusProperty);
    }
}
