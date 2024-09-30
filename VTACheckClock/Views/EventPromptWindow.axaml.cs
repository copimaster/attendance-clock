using Avalonia.Controls;

namespace VTACheckClock.Views
{
    public partial class EventPromptWindow : Window
    {
        public EventPromptWindow()
        {
            InitializeComponent();
            this.Get<Button>("btnExit").Click += delegate {
                this.Close(2);
            };

            this.Get<Button>("btnEnter").Click += delegate {
                this.Close(1);
            };
        }
    }
}
