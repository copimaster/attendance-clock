using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using ReactiveUI;
using System;
using System.Threading;
using VTACheckClock.ViewModels;

namespace VTACheckClock.Views
{
    /*public*/
    partial class WebsocketLoggerWindow : ReactiveWindow<WebsocketLoggerViewModel> /*Window*/
    {
        //private readonly ScrollViewer? _scroller;
        //private readonly WSServer _WSServer = new();

        public WebsocketLoggerWindow()
        {
            InitializeComponent();
            this.WhenActivated(d => d(ViewModel!.CancelCommand.Subscribe(model => {
                Close();
            })));

            //txtWSLogger.PropertyChanged += txtWSLoggerPropertyChanged;
            //_scroller = this.FindControl<ScrollViewer>("MessageLogScrollViewer");
            //_scroller = this.Get<ScrollViewer>("MessageLogScrollViewer");

            //txtWSLogger.GetObservable(TextBlock.TextProperty).Subscribe(text => {
            //    ScrollTextToEnd();
            //});

            //KeyDown += OnKeyDown;
        }

        private void TxtWSLoggerPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property.Name == nameof(TextBlock.Text)) {
                Dispatcher.UIThread.InvokeAsync(ScrollTextToEnd);
            }
        }

        public void ScrollTextToEnd()
        {
            Thread.Sleep(1000);
            //_scroller.ScrollToEnd();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            txtSearchLog.Text = string.Empty;
            txtSearchLog.Focus();
        }

        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if ((e.KeyModifiers & KeyModifiers.Control) != 0 && e.Key == Key.T) {
                // Acciones a realizar cuando se detecta 'Control + T'
                e.Handled = true;
                //await _WSServer.TriggerEventAsync("checkclock-offices.14", "my-event", new Employee(
                //   3709, "JHONNY GABRIEL CHABLE PAT", "20/07/2024 08:22:00", "Entrada"
                //));
            }
        }
    }
}
