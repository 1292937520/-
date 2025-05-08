using AvaloniaDemo.Common;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AvaloniaDemo.ViewModels;

public class EnvironmentCheckViewModel : ViewModelBase
{
    public ObservableCollection<CheckItem> CheckItems { get; } = new();
    public ICommand RecheckCommand { get; }

    public event EventHandler? AllRequirementsMet;

    public EnvironmentCheckViewModel()
    {
        // 初始化检测项
        CheckItems.Add(new CheckItem { Name = "Docker容器" });
        CheckItems.Add(new CheckItem { Name = "实验镜像(java:8)" });
        CheckItems.Add(new CheckItem { Name = "实验代码包" });
        RecheckCommand = ReactiveCommand.Create(StartCheck);
        StartCheck();
    }

    private async void StartCheck()
    {
        foreach (var item in CheckItems)
        {
            item.Status = CheckStatus.Checking;

            // 模拟检测逻辑
            bool exists = item.Name switch
            {
                "Docker容器" => await DockerService.CheckDockerExists(),
                "实验镜像(java:8)" => await DockerService.CheckImageExists("java:8"),
                "实验代码包" => File.Exists("experiment_package.zip"),
                _ => false
            };

            item.Status = exists ? CheckStatus.Passed : CheckStatus.Failed;

            if (!exists)
            {
                item.FixCommand =  ReactiveCommand.Create(async () => await FixItem(item));
            }
        }

        // 全部通过后自动跳转
        if (CheckItems.All(x => x.Status == CheckStatus.Passed))
        {
            await Task.Delay(2000);
            AllRequirementsMet?.Invoke(this, EventArgs.Empty);

        }
    }

    private async Task FixItem(CheckItem item)
    {
        item.Status = CheckStatus.Downloading;

        try
        {
            if (item.Name == "Docker容器")
            {
                await DockerService.InstallDockerAsync(progress => item.Progress = progress);
            }
            else if (item.Name == "实验镜像(java:8)")
            {
                await DockerService.PullImageAsync("java:8",
                    progress => item.Progress = progress);
            }
            else if (item.Name == "实验代码包")
            {
                await DownloadService.DownloadFileAsync(
                    "https://example.com/experiment_package.zip",
                    "experiment_package.zip",
                    progress => item.Progress = progress);
            }

            item.Status = CheckStatus.Passed;
        }
        catch
        {
            item.Status = CheckStatus.Failed;
        }
    }
}