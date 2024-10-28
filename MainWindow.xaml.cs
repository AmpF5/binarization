using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace binarization;

public partial class MainWindow : Window {
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
        DisplayedImage.Source = bitmap;
    }
}


