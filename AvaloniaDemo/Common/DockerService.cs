using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AvaloniaDemo.Common
{
    public static class DockerService
    {
        public static async Task InstallDockerAsync(Action<double> progressCallback)
        {
            try
            {
                if (File.Exists(GetDockerExecutablePath()))
                {
                    await StartAndWaitForDockerAsync();
                    return;
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    await InstallForWindowsAsync(progressCallback);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    await InstallForMacAsync(progressCallback);
                }
                else
                {
                    await InstallForLinuxAsync(progressCallback);
                }
               
                await StartAndWaitForDockerAsync();
            }
            catch (Exception ex)
            {                
                throw ex;
            }
        }

        private static async Task StartAndWaitForDockerAsync()
        {
            try
            {
                // 启动 Docker 服务
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // 尝试通过快捷方式启动 Docker Desktop
                    var shortcutPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
                        "Programs", "Docker", "Docker Desktop.lnk");

                    if (File.Exists(shortcutPath))
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = shortcutPath,
                            UseShellExecute = true
                        });
                    }
                    else
                    {
                        // 直接启动 Docker 服务
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = "sc",
                            Arguments = "start docker",
                            Verb = "runas",
                            UseShellExecute = true
                        });
                    }
                }
                else
                {
                    // Linux/macOS 使用 systemctl 启动
                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "sudo",
                            Arguments = "systemctl start docker",
                            UseShellExecute = false
                        }
                    };

                    process.Start();
                    await process.WaitForExitAsync();

                    if (process.ExitCode != 0)
                    {
                        throw new InvalidOperationException("启动 Docker 服务失败");
                    }
                }

                // 等待最多60秒，每隔2秒检查一次
                for (int i = 0; i < 30; i++)
                {
                    if (await CheckDockerRunningAsync())
                    {
                        Console.WriteLine("Docker 服务已成功启动");
                        return;
                    }

                    await Task.Delay(2000);
                }

                throw new TimeoutException("Docker 服务启动超时");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"启动 Docker 时发生错误: {ex}");
                throw;
            }
        }

        private static string GetDockerExecutablePath()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // 默认安装路径
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    "Docker", "Docker", "resources", "bin", "docker.exe");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return "/usr/bin/docker";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return "/usr/local/bin/docker";
            }

            throw new PlatformNotSupportedException("不支持的操作系统");
        }

        /// <summary>
        /// linux安装
        /// </summary>
        /// <param name="progressCallback"></param>
        /// <returns></returns>
        private static async Task InstallForLinuxAsync(Action<double> progressCallback)
        {
            string chinaMirror = "https://mirrors.aliyun.com/docker-ce/linux/ubuntu";

            var installScript = """
                #!/bin/bash
                sudo apt-get update
                sudo apt-get install -y \
                    apt-transport-https \
                    ca-certificates \
                    curl \
                    gnupg \
                    lsb-release
                curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /usr/share/keyrings/docker-archive-keyring.gpg
                echo \
                  "deb [arch=amd64 signed-by=/usr/share/keyrings/docker-archive-keyring.gpg] https://download.docker.com/linux/ubuntu \
                  $(lsb_release -cs) stable" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null
                sudo apt-get update
                sudo apt-get install -y docker-ce docker-ce-cli containerd.io
                sudo usermod -aG docker $USER
                """;

            string scriptPath = Path.Combine(Path.GetTempPath(), "install_docker.sh");
            await File.WriteAllTextAsync(scriptPath, installScript);

            var chmodProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "chmod",
                    Arguments = $"+x {scriptPath}",
                    UseShellExecute = false
                }
            };
            chmodProcess.Start();
            await chmodProcess.WaitForExitAsync();

            var installProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "sudo",
                    Arguments = $"bash {scriptPath}",
                    UseShellExecute = true
                }
            };
            installProcess.Start();
            await installProcess.WaitForExitAsync();
        }

        /// <summary>
        /// Window安装
        /// </summary>
        /// <param name="progress"></param>
        /// <returns></returns>
        private static async Task InstallForWindowsAsync(Action<double> progressCallback)
        {
            string tempPath = Path.GetTempPath();
            string installerPath = Path.Combine(tempPath, "DockerDesktopInstaller.exe");
            string downloadUrl = "https://desktop.docker.com/win/stable/Docker%20Desktop%20Installer.exe";

            try
            {
                // 配置TLS版本
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

                using (var httpClient = new HttpClient())
                {
                    // 设置更长的超时时间
                    httpClient.Timeout = TimeSpan.FromMinutes(30);

                    // 获取文件大小
                    using (var headResponse = await httpClient.SendAsync(
                        new HttpRequestMessage(HttpMethod.Head, downloadUrl)))
                    {
                        headResponse.EnsureSuccessStatusCode();
                        var contentLength = headResponse.Content.Headers.ContentLength;

                        // 下载文件并显示进度
                        using (var response = await httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead))
                        {
                            response.EnsureSuccessStatusCode();

                            await using var downloadStream = await response.Content.ReadAsStreamAsync();
                            await using var fileStream = new FileStream(installerPath, FileMode.Create);

                            var buffer = new byte[8192];
                            long totalBytesRead = 0;
                            int bytesRead;

                            while ((bytesRead = await downloadStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                await fileStream.WriteAsync(buffer, 0, bytesRead);
                                totalBytesRead += bytesRead;

                                // 报告进度
                                if (contentLength.HasValue && progressCallback != null)
                                {
                                    double progress = (double)totalBytesRead / contentLength.Value * 100;
                                    progressCallback(progress);
                                }
                            }
                        }
                    }
                }

                // 验证文件大小（可选）
                var fileInfo = new FileInfo(installerPath);
                if (fileInfo.Length < 100 * 1024 * 1024) // 小于100MB可能不完整
                {
                    throw new InvalidOperationException("下载的安装包不完整");
                }

                // 执行静默安装
                using (var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = installerPath,
                        Arguments = "install --quiet --accept-license",
                        Verb = "runas",
                        UseShellExecute = true
                    }
                })
                {
                    process.Start();
                    await process.WaitForExitAsync();

                    if (process.ExitCode != 0)
                    {
                        throw new InvalidOperationException($"安装失败，退出代码: {process.ExitCode}");
                    }
                }

                // 清理临时文件
                 File.Delete(installerPath);
            }
            catch (Exception ex)
            {
                // 记录详细错误信息
                Console.WriteLine($"安装过程中发生错误: {ex}");

                // 清理可能残留的临时文件
                try { if (File.Exists(installerPath)) File.Delete(installerPath); }
                catch { /* 忽略清理错误 */ }

                throw; // 重新抛出异常，由调用者处理
            }
        }

        /// <summary>
        /// Mac安装
        /// </summary>
        /// <param name="progressCallback"></param>
        /// <returns></returns>
        private static async Task InstallForMacAsync(Action<double> progressCallback)
        {
            var brewProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = "-c \"$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)\"",
                    RedirectStandardOutput = true
                }
            };
            brewProcess.Start();
            await brewProcess.WaitForExitAsync();

            var dockerProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = "-c \"brew install --cask docker\"",
                    RedirectStandardOutput = true
                }
            };
            dockerProcess.Start();
            await dockerProcess.WaitForExitAsync();
        }


        public static async Task<bool> CheckDockerExists()
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "docker",
                        Arguments = "--version",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        UseShellExecute = false
                    }
                };

                process.Start();
                process.WaitForExit(2000); // 超时2秒

                // 退出码为0且输出包含"version"即认为安装成功
                return process.ExitCode == 0 &&
                       process.StandardOutput.ReadToEnd().Contains("version");
            }
            catch
            {
                // 如果连docker命令都找不到会抛出异常
                return false;
            }
        }

        public static async Task<bool> CheckDockerRunningAsync()
        {
            try
            {
                // 根据平台选择不同的检测命令
                var (fileName, arguments) = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? ("cmd", "/c docker info")
                    : ("docker", "info");

                using var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = fileName,
                        Arguments = arguments,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        UseShellExecute = false
                    }
                };

                process.Start();
                await process.WaitForExitAsync(); 

                // 退出码为0且无错误输出表示运行正常
                return process.ExitCode == 0 &&
                       string.IsNullOrEmpty(process.StandardError.ReadToEnd());
            }
            catch
            {
                return false; // 任何异常都视为未运行
            }
        }

        public static async Task<bool> CheckImageExists(string imageName)
        {
            try
            {
                using var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "docker",
                        Arguments = $"image inspect {imageName}",
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                await process.WaitForExitAsync();
                return process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        public static async Task PullImageAsync(string imageName, Action<double> progressCallback)
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "docker",
                    Arguments = $"pull {imageName}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            process.OutputDataReceived += (_, e) =>
            {
                if (e.Data?.Contains("Downloading") == true)
                {
                    var match = Regex.Match(e.Data, @"(\d+)%");
                    if (match.Success)
                        progressCallback(double.Parse(match.Groups[1].Value));
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            await process.WaitForExitAsync();
        }
    }
}
