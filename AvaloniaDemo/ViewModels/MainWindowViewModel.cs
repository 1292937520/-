using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaDemo.ViewModels
{
    // MainWindowViewModel.cs
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly EnvironmentCheckViewModel _envCheckVm;
        private readonly WebPageViewModel _webPageVm;

        public MainWindowViewModel()
        {
            _envCheckVm = new EnvironmentCheckViewModel();
            _webPageVm = new WebPageViewModel();

            // 监听环境检查完成事件
            _envCheckVm.AllRequirementsMet += (s, e) =>
            {
                CurrentPage = _webPageVm; // 切换至Web页面
            };

            CurrentPage = _envCheckVm; // 初始显示环境检查页
        }

        private ViewModelBase _currentPage;
        public ViewModelBase CurrentPage
        {
            get => _currentPage;
            set => this.RaiseAndSetIfChanged(ref _currentPage, value);
        }
    }
}
