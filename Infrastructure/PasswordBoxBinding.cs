using System.Windows;
using System.Windows.Controls;

namespace X.Desktop.Infrastructure;

public static class PasswordBoxBinding
{
    private static readonly DependencyProperty IsUpdatingProperty =
        DependencyProperty.RegisterAttached("IsUpdating", typeof(bool), typeof(PasswordBoxBinding));

    public static readonly DependencyProperty BindPasswordProperty =
        DependencyProperty.RegisterAttached("BindPassword", typeof(bool), typeof(PasswordBoxBinding),
            new PropertyMetadata(false, OnBindPasswordChanged));

    public static readonly DependencyProperty BoundPasswordProperty =
        DependencyProperty.RegisterAttached("BoundPassword", typeof(string), typeof(PasswordBoxBinding),
            new FrameworkPropertyMetadata(string.Empty,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnBoundPasswordChanged));

    public static bool GetBindPassword(DependencyObject obj) => (bool)obj.GetValue(BindPasswordProperty);
    public static void SetBindPassword(DependencyObject obj, bool value) => obj.SetValue(BindPasswordProperty, value);
    public static string GetBoundPassword(DependencyObject obj) => (string)obj.GetValue(BoundPasswordProperty);
    public static void SetBoundPassword(DependencyObject obj, string value) => obj.SetValue(BoundPasswordProperty, value);

    private static void OnBindPasswordChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is not PasswordBox box) return;
        box.PasswordChanged -= HandlePasswordChanged;
        if ((bool)e.NewValue) box.PasswordChanged += HandlePasswordChanged;
    }

    private static void OnBoundPasswordChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is not PasswordBox box || (bool)box.GetValue(IsUpdatingProperty)) return;
        box.Password = e.NewValue as string ?? string.Empty;
    }

    private static void HandlePasswordChanged(object sender, RoutedEventArgs e)
    {
        var box = (PasswordBox)sender;
        box.SetValue(IsUpdatingProperty, true);
        SetBoundPassword(box, box.Password);
        box.SetValue(IsUpdatingProperty, false);
    }
}
