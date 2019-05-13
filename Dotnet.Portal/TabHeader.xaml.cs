using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Dotnet.Portal
{
    /// <summary>
    /// Interaction logic for TabHeader.xaml
    /// </summary>
    public partial class TabHeader : UserControl
    {
        public TabHeader()
        {
            InitializeComponent();
            Text = "Project 1";
            IsReadOnly = true;

            MouseUp += TabHeader_MouseUp;
            MouseDoubleClick += TabHeader_MouseDoubleClick;
            TextBox.PreviewKeyDown += TextBox_PreviewKeyDown;
            TextBox.LostFocus += TextBox_LostFocus;
        }

        public event EventHandler Closed;

        public event EventHandler NameChanged;

        public void EditName()
        {
            IsReadOnly = false;
            TextBox.Focus();
            TextBox.SelectAll();
        }

        public void Close()
        {
            Closed?.Invoke(this, EventArgs.Empty);
        }

        public bool IsReadOnly
        {
            get => Label.Visibility == Visibility.Visible;
            set
            {
                if (value)
                {
                    TextBox.Visibility = Visibility.Hidden;
                    Label.Visibility = Visibility.Visible;
                }
                else
                {
                    TextBox.Visibility = Visibility.Visible;
                    Label.Visibility = Visibility.Hidden;
                }
            }
        }

        public string Text
        {
            get => (string)Label.Content;
            set => Label.Content = TextBox.Text = value;
        }

        private bool _running;

        public bool Running
        {
            get => _running;
            set
            {
                _running = value;
                var portal = _running ? "orange" : "blue";
                Image.Source = new BitmapImage(new Uri($"images/{portal}-portal.png", UriKind.Relative));
            }
        }

        private void TabHeader_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle)
            {
                Closed?.Invoke(this, EventArgs.Empty);
            }
        }

        private void TabHeader_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left) return;

            EditName();
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            StopEditing(true);
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                StopEditing(false);
            }
            else if (e.Key == Key.Enter)
            {
                StopEditing(true);
            }
        }

        private void StopEditing(bool save)
        {
            if (IsReadOnly) return;

            if (save)
            {
                Label.Content = TextBox.Text;
                NameChanged?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                TextBox.Text = (string)Label.Content;
            }

            IsReadOnly = true;
        }
    }
}