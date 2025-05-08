using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaDemo.Common
{
    public static class DownloadService
    {
        public static async Task DownloadFileAsync(string url, string localPath, Action<double> progressCallback)
        {
            using var client = new HttpClient();
            using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

            await using var stream = await response.Content.ReadAsStreamAsync();
            await using var fileStream = new FileStream(localPath, FileMode.Create);

            var totalBytes = response.Content.Headers.ContentLength ?? 0;
            var buffer = new byte[8192];
            var bytesRead = 0;
            var totalRead = 0L;

            while ((bytesRead = await stream.ReadAsync(buffer)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                totalRead += bytesRead;
                progressCallback(totalBytes > 0 ? (totalRead * 100d / totalBytes) : 0);
            }
        }
    }
}
