
using NLog.Config;
using NLog;
using AGVSystemCommonNet6.GPMRosMessageNet.Messages;
using System.Globalization;
using System.IO.Compression;

namespace AGVSystem.Service
{
    public class LogDownlodService
    {
        internal async Task<string> CreateLogCompressedFile(DateTime fromDate, DateTime toDate)
        {
            string logDirectory = GetLogFolderPath();
            //Subfolder=AGVSystemLog and VMSLog
            string[] subFolders = new string[] { "AGVSystemLog", "VMSLog" };
            string tempLogStore = Path.Combine(Path.GetTempPath(), $"AgvsLog-From-{fromDate.ToString("yy-MM-dd")}-To-{toDate.ToString("yy-MM-dd")}-{DateTime.Now.Ticks}");
            string tempZipFileName = Path.Combine(Path.GetTempPath(), $"{fromDate.ToString("yyyy-MM-dd")}_to_{toDate.ToString("yyyy-MM-dd")} log.zip");
            foreach (var subFolder in subFolders)
            {
                string rootDirectory = Path.Combine(logDirectory, subFolder);
                var subDirectories = Directory.GetDirectories(rootDirectory);
                List<Task> tasks = new List<Task>();
                foreach (var subDirectory in subDirectories)
                {
                    var folderName = Path.GetFileName(subDirectory);
                    // 嘗試解析資料夾名稱為日期
                    if (DateTime.TryParseExact(folderName, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime folderDate))
                    {
                        // 如果資料夾日期在指定範圍內
                        if (folderDate >= fromDate && folderDate <= toDate)
                        {
                            tasks.Add(Task.Run(() =>
                            {
                                DirectoryCopyHelper.CopyDirectory(subDirectory, Path.Combine(tempLogStore, subFolder, folderName));
                            }));
                        }
                    }
                }
                Task.WaitAll(tasks.ToArray());

            }
            await Task.Delay(1000);
            string zipFileName = Path.Combine(Path.GetTempPath(), $"{Path.GetFileNameWithoutExtension(tempLogStore)}.zip");
            File.Delete(zipFileName);
            try
            {
                if (!Directory.Exists(tempLogStore))
                {
                    throw new Exception($"[{fromDate.ToString("yy-MM-dd")}] ~ [{toDate.ToString("yy-MM-dd")}] 沒有LOG紀錄");
                }

                ZipFile.CreateFromDirectory(tempLogStore, zipFileName);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return zipFileName;
        }

        private static string GetLogFolderPath()
        {
            string logDirectory = "";

            // Get the NLog configuration
            LoggingConfiguration config = LogManager.Configuration;

            if (config != null)
            {
                // Try to get the variable
                var variable = config.Variables.FirstOrDefault(v => v.Key == "logDirectory");

                if (variable.Key != null)
                {
                    logDirectory = (variable.Value.ToString() + "").TrimEnd(new char[1] { '/' });
                    logDirectory = Path.GetDirectoryName(logDirectory);
                    Console.WriteLine($"Log directory: {logDirectory}");
                }
                else
                {
                    Console.WriteLine("logDirectory variable not found in NLog configuration.");
                }
            }
            else
            {
                Console.WriteLine("NLog configuration not found.");
            }

            return logDirectory;
        }



        public class DirectoryCopyHelper
        {
            public static void CopyDirectory(string sourceDir, string destinationDir, bool copySubDirs = true)
            {
                DirectoryInfo dir = new DirectoryInfo(sourceDir);

                if (!dir.Exists)
                {
                    throw new DirectoryNotFoundException($"來源資料夾不存在: {sourceDir}");
                }

                DirectoryInfo[] dirs = dir.GetDirectories();
                Directory.CreateDirectory(destinationDir);

                // 複製檔案
                FileInfo[] files = dir.GetFiles();
                foreach (FileInfo file in files)
                {
                    string tempPath = Path.Combine(destinationDir, file.Name);
                    file.CopyTo(tempPath, false);
                }

                // 複製子資料夾及其內容
                if (copySubDirs)
                {
                    foreach (DirectoryInfo subdir in dirs)
                    {
                        string tempPath = Path.Combine(destinationDir, subdir.Name);
                        CopyDirectory(subdir.FullName, tempPath, copySubDirs);
                    }
                }
            }
        }

    }
}
