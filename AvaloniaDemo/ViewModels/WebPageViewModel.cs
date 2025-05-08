using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaDemo.ViewModels
{
    // WebPageViewModel.cs
    public class WebPageViewModel : ViewModelBase
    {
        private string _url = "http://10.0.20.102/lm-devops";
        public string Url
        {
            get => _url;
            set => this.RaiseAndSetIfChanged(ref _url, value);
        }
    }
}
