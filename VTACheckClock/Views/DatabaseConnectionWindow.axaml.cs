using Avalonia.Controls;

namespace VTACheckClock.Views
{
    public partial class DatabaseConnectionWindow : Window
    {
        public DatabaseConnectionWindow()
        {
            InitializeComponent();
            btnCancel.Click += delegate {
                Close();
            };
        }
    }
}
