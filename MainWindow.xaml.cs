using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using binarization.Scripts;
using Microsoft.Win32;

namespace binarization;

public partial class MainWindow : Window {
    private ImageSource _originalImage = null!;
    private int _windowSize = 5;
    private BinarizationType _binarizationType = BinarizationType.Threshold;
    private Button _selectedButton;
    private const int MinValue = 0;
    private int MaxValue = 25;
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

    private void Choose_Binarization(object sender, RoutedEventArgs e) {
        WindowSizeStackPanel.Visibility = Visibility.Visible;
        var button = sender as Button;

        if(button?.CommandParameter is not BinarizationType type) 
            return;
        
        _binarizationType = type;
        switch (_binarizationType) {
            case BinarizationType.Otsu:
                NumberTextBox.Visibility = Visibility.Visible;
                WindowSizeStackPanel.Visibility = Visibility.Visible;
                SliderStackPanel.Visibility = Visibility.Collapsed;
                break;
            case BinarizationType.Threshold:
                WindowSizeStackPanel.Visibility = Visibility.Collapsed;
                NumberTextBox.Visibility = Visibility.Collapsed;
                SliderStackPanel.Visibility = Visibility.Visible;
                MaxValue = 255;
                break;
            case BinarizationType.Sauvola:
            case BinarizationType.Phansalkara:
            case BinarizationType.Niblack:
            default:
                SliderStackPanel.Visibility = Visibility.Collapsed;
                WindowSizeStackPanel.Visibility = Visibility.Visible;
                NumberTextBox.Visibility = Visibility.Visible;
                MaxValue = 25;
                break;
        }
        
        var clickedButton = sender as Button;

        if (_selectedButton != null)
        {
            _selectedButton.ClearValue(BorderBrushProperty);
            _selectedButton.ClearValue(BorderThicknessProperty);
        }
        
        if (clickedButton != null)
        {
            clickedButton.BorderBrush = Brushes.Blue;
            clickedButton.BorderThickness = new Thickness(2);
            _selectedButton = clickedButton;
        }
    }
    
    private void NumberValidationTextBox(object sender, TextCompositionEventArgs e) {
        var textBox = sender as TextBox;
        var newText = textBox.Text.Insert(textBox.SelectionStart, e.Text);

        if (int.TryParse(newText, out var result))
        {
            if (result < MinValue || result > MaxValue)
            {
                e.Handled = true;
            }
        }
        else
        {
            e.Handled = true;
        }
    }

    private void TextChangeTextBox(object sender, TextChangedEventArgs e) {
        var textBox = sender as TextBox;
        if(int.TryParse(textBox.Text, out var result)) {
            if (result < MinValue) {
                textBox.Text = MinValue.ToString();
                textBox.CaretIndex = textBox.Text.Length;
            }
            else if (result > MaxValue) {
                textBox.Text = MaxValue.ToString();
                textBox.CaretIndex = textBox.Text.Length;
            }
            _windowSize = int.Parse(textBox.Text);
        }
        else {
            textBox.Text = string.Empty;
            _windowSize = 0;
        }
    }

    private void Apply_Binarization(object sender, RoutedEventArgs e) {
        DisplayedImage.Source = _binarizationType switch {
            BinarizationType.Threshold => ThresholdBinarization.Binarize(_originalImage, threshold: (byte)_windowSize),
            BinarizationType.Sauvola => SauvolaBinarization.Binarize(_originalImage, windowSize: _windowSize),
            BinarizationType.Phansalkara => PhansalkaraBinarization.Binarize(_originalImage, windowSize: _windowSize),
            BinarizationType.Otsu => OtsuBinarization.Binarize(_originalImage),
            BinarizationType.Niblack => NiblackBinarization.Binarize(_originalImage, windowSize: _windowSize),
            _ => throw new ArgumentOutOfRangeException($"Type {_binarizationType} not found")
        };
    }
    
    private void ThresholdSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
        if (sender is not Slider slider || CurrentThresholdValue == null) 
            return;
        
        CurrentThresholdValue.Text = $"Current Value: {slider.Value:F0}";
        DisplayedImage.Source = ThresholdBinarization.Binarize(_originalImage, (byte)slider.Value);
    }
}
