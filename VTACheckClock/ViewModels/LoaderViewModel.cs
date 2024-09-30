using ReactiveUI;

namespace VTACheckClock.ViewModels
{
    class LoaderViewModel : ViewModelBase
    {
        string _message = "Cargando, espere...";
        public string Message
        {
            get => _message;
            set => this.RaiseAndSetIfChanged(ref _message, value);
        }

        public LoaderViewModel(string message = "") {
            Message = message != "" ? message: _message;
        }
    }
}
