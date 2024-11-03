using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using binarization.Scripts;
using Microsoft.Win32;

namespace binarization;

public partial class MainWindow : Window {
    private ImageSource _originalImage = null!; 
    public MainWindow() {
        InitializeComponent();
    }
    
    private void LoadImage_Click(object sender, RoutedEventArgs e) {
        var openFileDialog = new OpenFileDialog {
            Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp"
        };

        if(openFileDialog.ShowDialog() == false) 
            return;
        
        var bitmap = new BitmapImage(new Uri(openFileDialog.FileName));
        _originalImage = bitmap;
        DisplayedImage.Source = bitmap;
    }
    
    private void ResetImage_Click(object sender, RoutedEventArgs e) {
        DisplayedImage.Source = _originalImage;
    }

    private void Apply_Binarization(object sender, RoutedEventArgs e) {
        var button = sender as Button;

        if(button?.CommandParameter is not BinarizationType type) 
            return;
        
        try {
            switch (type) {
                case BinarizationType.Threshold:
                    SliderStackPanel.Visibility = Visibility.Visible;
                    break;
                case BinarizationType.Sauvola:
                    DisplayedImage.Source = SauvolaBinarization.Binarize(_originalImage);
                    break;
                case BinarizationType.Phansalkara:
                    DisplayedImage.Source = PhansalkaraBinarization.Binarize(_originalImage);
                    break;
                case BinarizationType.Otsu:
                    break;
                case BinarizationType.Niblack:
                    DisplayedImage.Source = NiblackBinarization.Binarize(_originalImage);
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Type {type} not found");
            }
        }
        catch (Exception ex) {
            Console.WriteLine(ex);
        }
    }
    
    private void ThresholdSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
        if (sender is not Slider slider || CurrentThresholdValue == null) 
            return;
        
        CurrentThresholdValue.Text = $"Current Value: {slider.Value:F0}";
        DisplayedImage.Source = ThresholdBinarization.Binarize(_originalImage, (byte)slider.Value);
    }
}
