using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using binarization.Scripts;
using Microsoft.Win32;

namespace binarization;

public partial class MainWindow : Window {
    private ImageSource _orginalImage = null!; 
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
        _orginalImage = bitmap;
        DisplayedImage.Source = bitmap;
    }

    private void Apply_Binarization(object sender, RoutedEventArgs e) {
        var button = sender as Button;

        if(button?.CommandParameter is not BinarizationType type) 
            return;
        
        try {
            switch (type) {
                case BinarizationType.Threshold:
                    DisplayedImage.Source = ThresholdBinarization.Binarize(DisplayedImage.Source);
                    break;
                case BinarizationType.Sauvola:
                    break;
                case BinarizationType.Phansalkara:
                    break;
                case BinarizationType.Otsu:
                    break;
                case BinarizationType.Niblack:
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Type {type} not found");
            }
        }
        catch (Exception ex) {
            Console.WriteLine(ex);
        }
    }
}


