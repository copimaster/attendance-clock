using Avalonia.Controls;
using Avalonia.ReactiveUI;
using ReactiveUI;
using System.Threading.Tasks;
using VTACheckClock.ViewModels;

namespace VTACheckClock.Views
{
    /*public*/ partial class ConfigurationWindow : ReactiveWindow<ConfigurationViewModel> /*Window*/
    {
        public ConfigurationWindow()
        {
            InitializeComponent();
            this.FindControl<Button>("btnCancel").Click += delegate {
                Close();
            };
            this.WhenActivated(d => d(ViewModel!.ShowDBDialog.RegisterHandler(DoShowDialogAsync)));
            this.WhenActivated(d => d(ViewModel!.ShowLoginDialog.RegisterHandler(DoShowDialogAsync)));
        }

        private async Task DoShowDialogAsync(InteractionContext<DatabaseConnectionViewModel, bool> interaction)
        {
            var dialog = new DatabaseConnectionWindow {
                DataContext = interaction.Input
            };

            var result = await dialog.ShowDialog<bool>(this);
            interaction.SetOutput(result);
            ((ConfigurationViewModel?)DataContext).IsConfigured = result;
            if (result) ((ConfigurationViewModel?)DataContext).NextStep = 2;
        }

        private async Task DoShowDialogAsync(InteractionContext<LoginViewModel, bool> interaction)
        {
            var dialog = new LoginWindow {
                DataContext = interaction.Input
            };

            var result = await dialog.ShowDialog<bool>(this);
            interaction.SetOutput(result);
        }
    }
}
