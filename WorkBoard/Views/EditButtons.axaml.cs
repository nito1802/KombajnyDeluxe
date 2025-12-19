using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using KombajnDoPracy.ViewModels;

namespace KombajnDoPracy.Views
{
    public partial class EditButtons : Window
    {
        public EditButtonViewModel EditButtonViewModel { get; set; }

        public EditButtons(SerializableButtonItemViewModel serializableButtonItemViewModel)
        {
            InitializeComponent();
            EditButtonViewModel = EditButtonViewModel.GetFromSerializableButtonItemViewModel(serializableButtonItemViewModel);
            DataContext = EditButtonViewModel;
        }

        private void SaveButton_OnClick(object? sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_OnClick(object? sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Window_OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
