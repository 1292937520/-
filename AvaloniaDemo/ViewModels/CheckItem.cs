using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AvaloniaDemo.ViewModels
{
    public class CheckItem : INotifyPropertyChanged
    {
        private string _name;
        private CheckStatus _status;

        private double? _progress;

        public string Name 
        {
            get 
            { 
                return _name; 
            }
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public CheckStatus Status
        {
            get
            {
                return _status;
            }
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        public double? Progress
        {
            get
            {
                return _progress;
            }
            set
            {
                _progress = value;
                OnPropertyChanged(nameof(Progress));
            }
        }
        public ReactiveCommand<Unit, Task> FixCommand { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public enum CheckStatus { Checking, Passed, Failed, Downloading }
}
