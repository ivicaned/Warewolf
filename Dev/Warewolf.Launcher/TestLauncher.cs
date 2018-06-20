﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Threading;
using System.Xml;
using Warewolf.Launcher.TestRunners;

namespace Warewolf.Launcher
{
    class TestLauncher
    {
        public string DoServerStart { get; set; }
        public string DoStudioStart { get; set; }
        public string ServerPath { get; set; }
        public string StudioPath { get; set; }
        public string ResourcesType { get; set; }
        public string DotCoverPath { get; set; }
        public string ServerUsername { get; set; }
        public string ServerPassword { get; set; }
        public string RunAllJobs { get; set; }
        public bool Cleanup { get; set; }
        public string AssemblyFileVersionsTest { get; set; }
        public string RecordScreen { get; set; }
        string Parallelize { get; set; }
        public string Category { get; set; }
        public string ProjectName { get; set; }
        public string RunAllUnitTests { get; set; }
        public string RunAllServerTests { get; set; }
        public string RunAllReleaseResourcesTests { get; set; }
        public string RunAllDesktopUITests { get; set; }
        public string RunAllWebUITests { get; set; }
        public string RunWarewolfServiceTests { get; set; }
        public string DomywarewolfioStart { get; set; }
        public string JobName { get; set; }
        public string MergeDotCoverSnapshotsInDirectory { get; set; }
        public string StartDocker { get; set; }
        public int RetryCount { get; internal set; } = 0;
        public bool StartServerAsConsole { get; internal set; } = false;
        public bool AdminMode { get; internal set; } = false;
        public ITestRunner TestRunner { get; internal set; }

        public string ServerExeName;
        public string StudioExeName;
        public List<string> ServerPathSpecs;
        public List<string> StudioPathSpecs;
        public bool ApplyDotCover;

        public Dictionary<string, Tuple<string, string>> JobSpecs;
        public string WebsPath;
        ContainerLauncher ciRemoteContainerLauncher;
        private string RunServerWithDotcoverScript;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        static extern IntPtr FindWindowByCaption(IntPtr zeroOnly, string lpWindowName);

        string FindFileInParent(List<string> FileSpecs, int NumberOfParentsToSearch = 7)
        {
            var NumberOfParentsSearched = -1;
            var FilePath = "";
            var CurrentDirectory = TestRunner.TestsPath;
            while (FilePath == "" && NumberOfParentsSearched++ < NumberOfParentsToSearch && CurrentDirectory != "")
            {
                var NumberOfFileSpecsSearched = -1;
                var FileSpec = "";
                while (FilePath == "" && ++NumberOfFileSpecsSearched < FileSpecs.Count)
                {
                    FileSpec = FileSpecs[NumberOfFileSpecsSearched];
                    if (Path.IsPathRooted(FileSpec))
                    {
                        if (CurrentDirectory != "")
                        {
                            CurrentDirectory = Path.GetDirectoryName(FileSpec);
                        }
                        FileSpec = Path.GetFileName(FileSpec);
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(CurrentDirectory))
                        {
                            CurrentDirectory = TestRunner.TestsPath;
                        }
                    }
                    if (Directory.Exists(Path.GetDirectoryName(Path.Combine(CurrentDirectory, FileSpec))))
                    {
                        string[] files = Directory.GetFiles(CurrentDirectory, FileSpec, SearchOption.TopDirectoryOnly);
                        string[] folders = Directory.GetDirectories(CurrentDirectory, FileSpec, SearchOption.TopDirectoryOnly);
                        if (files.Length > 0 || folders.Length > 0)
                        {
                            FilePath = Path.Combine(CurrentDirectory, FileSpec);
                        }
                    }
                }
                if (CurrentDirectory != null && CurrentDirectory != "" && (Path.GetFileNameWithoutExtension(CurrentDirectory)) != "\\")
                {
                    if (FilePath == "" && Path.GetPathRoot(CurrentDirectory) != CurrentDirectory)
                    {
                        CurrentDirectory = Directory.GetParent(CurrentDirectory).FullName;
                    }
                }
                else
                {
                    CurrentDirectory = "";
                }
            }
            return FilePath;
        }

        public static void CopyOnWrite(string FileSpec)
        {
            if (File.Exists(FileSpec))
            {
                var num = 1;
                var FileExtention = Path.GetExtension(FileSpec);
                var FileSpecWithoutExtention = FileSpec.Substring(0, FileSpec.LastIndexOf('.') + 1);
                while (File.Exists($"{FileSpecWithoutExtention}{num}{FileExtention}"))
                {
                    num++;
                }
                File.Move(FileSpec, $"{FileSpecWithoutExtention}{num}{FileExtention}");
            }
            else if (Directory.Exists(FileSpec))
            {
                var num = 1;
                while (Directory.Exists($"{FileSpec}{num}"))
                {
                    num++;
                }
                Directory.Move(FileSpec, $"{FileSpec}{num}");
            }
        }

        void MoveFileToTestResults(string SourceFilePath, string DestinationFileName)
        {
            var DestinationFilePath = Path.Combine(TestRunner.TestsResultsPath, DestinationFileName);
            if (File.Exists(SourceFilePath))
            {
                CopyOnWrite(DestinationFilePath);
                Console.WriteLine($"Moving \"{SourceFilePath}\" to \"{DestinationFilePath}\"");
                var DestinationFolderPath = Path.GetDirectoryName(DestinationFilePath);
                if (!Directory.Exists(DestinationFolderPath))
                {
                    Directory.CreateDirectory(DestinationFolderPath);
                }
                File.Move(SourceFilePath, DestinationFilePath);
            }
        }

        void MoveFolderToTestResults(string SourceFolderPath, string DestinationFolderName)
        {
            var DestinationFolderPath = Path.Combine(TestRunner.TestsResultsPath, DestinationFolderName);
            if (Directory.Exists(SourceFolderPath))
            {
                CopyOnWrite(DestinationFolderPath);
                Console.WriteLine($"Moving \"{SourceFolderPath}\" to \"{DestinationFolderPath}\"");
                if (!Directory.Exists(DestinationFolderPath))
                {
                    Directory.CreateDirectory(DestinationFolderPath);
                }
                Directory.Move(SourceFolderPath, DestinationFolderPath);
            }
        }

        public void CleanupServerStudio(bool Force = true)
        {
            if (!string.IsNullOrEmpty(ServerPath) && File.Exists(ServerPath))
            {
                string serverStartedFile = Path.Combine(Path.GetDirectoryName(ServerPath), "ServerStarted");
                if (File.Exists(serverStartedFile))
                {
                    File.Delete(serverStartedFile);
                }
            }

            //Find Webs
            if (string.IsNullOrEmpty(WebsPath))
            {
                WebsPath = Path.Combine(TestRunner.TestsPath, "_PublishedWebsites", "Dev2.Web");
                if (!File.Exists(WebsPath) && !String.IsNullOrEmpty(ServerPath))
                {
                    WebsPath = Path.Combine(Path.GetDirectoryName(ServerPath), "_PublishedWebsites", "Dev2.Web");
                }
            }
            else
            {
                if (!Directory.Exists(WebsPath))
                {
                    throw new ArgumentException("No webs folder found at " + WebsPath);
                }
            }

            //Find Studio
            if (string.IsNullOrEmpty(StudioPath))
            {
                bool foundStudio = TryFindWarewolfStudioExe(out string studioPath);
                if (foundStudio)
                {
                    StudioPath = studioPath;
                }
            }
            else
            {
                if (!File.Exists(StudioPath))
                {
                    throw new ArgumentException("No studio found at " + StudioPath);
                }
            }
            if (!string.IsNullOrEmpty(StudioPath))
            {
                string studioStartedFile = Path.Combine(Path.GetDirectoryName(StudioPath), "StudioStarted");
                if (File.Exists(studioStartedFile))
                {
                    File.Delete(studioStartedFile);
                }
            }

            int WaitForCloseTimeout = Force ? 10 : 1800;
            int WaitForCloseRetryCount = Force ? 1 : 10;

            //Stop Studio
            Process process = StartProcess("taskkill", "/im \"Warewolf Studio.exe\"");
            var Output = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();

            //Soft Kill
            int i = 0;
            string WaitTimeoutMessage = "This command stopped operation because process ";
            string WaitOutput = WaitTimeoutMessage;
            while (!(Output.StartsWith("ERROR: ")) && WaitOutput.StartsWith(WaitTimeoutMessage) && i < WaitForCloseRetryCount)
            {
                i++;
                Console.WriteLine(Output);
                Process.GetProcessesByName("Warewolf Studio")[0].WaitForExit(WaitForCloseTimeout);
                var FormatWaitForCloseTimeoutMessage = WaitOutput.Replace(WaitTimeoutMessage, "");
                if (FormatWaitForCloseTimeoutMessage != "" && !(FormatWaitForCloseTimeoutMessage.StartsWith("Cannot find a process with the name ")))
                {
                    Console.WriteLine(FormatWaitForCloseTimeoutMessage);
                }
                process.Start();
                process.WaitForExit();
                Output = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
            }

            //Force Kill
            process.StartInfo.Arguments = "/im \"Warewolf Studio.exe\" /f";
            process.Start();
            process.WaitForExit();
            Output = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
            if (!(Output.StartsWith("ERROR: ")))
            {
                Console.WriteLine(Output);
            }

            //Stop my.warewolf.io
            process.StartInfo.Arguments = "/im iisexpress.exe /f";
            process.Start();
            process.WaitForExit();
            Output = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
            if (!(Output.StartsWith("ERROR: ")))
            {
                Console.WriteLine(Output);
            }

            if (!StartServerAsConsole)
            {
                //Stop Server
                var stopServerService = new Process();
                stopServerService.StartInfo.UseShellExecute = false;
                stopServerService.StartInfo.RedirectStandardOutput = true;
                stopServerService.StartInfo.RedirectStandardError = true;
                stopServerService.StartInfo.FileName = "sc.exe";
                stopServerService.StartInfo.Arguments = "stop \"Warewolf Server\"";
                stopServerService.Start();
                stopServerService.WaitForExit();
                var ServiceOutput = stopServerService.StandardOutput.ReadToEnd() + stopServerService.StandardError.ReadToEnd();
                if (ServiceOutput != "[SC] ControlService FAILED 1062:\r\n\r\nThe service has not been started.\r\n\r\n")
                {
                    Console.WriteLine(ServiceOutput.TrimStart('\n'));
                    var allServerProcesses = Process.GetProcessesByName("Warewolf Server");
                    if (allServerProcesses.Length > 0)
                    {
                        allServerProcesses[0].WaitForExit(WaitForCloseTimeout);
                    }
                }
            }
            process.StartInfo.Arguments = "/im \"Warewolf Server.exe\" /f";
            process.Start();
            process.StartInfo.Arguments = "/im \"operadriver.exe\" /f";
            process.Start();
            process.StartInfo.Arguments = "/im \"geckodriver.exe\" /f";
            process.Start();
            process.StartInfo.Arguments = "/im \"IEDriverServer.exe\" /f";
            process.Start();

            //Delete CI Remote Container
            if (ciRemoteContainerLauncher != null)
            {
                try
                {
                    ciRemoteContainerLauncher.Dispose();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error disposing CI Remote server container: " + e.Message);
                }
            }

            //Delete Certain Studio and Server Resources
            var ToClean = new[]
            {
                "%LOCALAPPDATA%\\Warewolf\\DebugData\\PersistSettings.dat",
                "%LOCALAPPDATA%\\Warewolf\\UserInterfaceLayouts\\WorkspaceLayout.xml",
                "%PROGRAMDATA%\\Warewolf\\Workspaces",
                "%PROGRAMDATA%\\Warewolf\\Server Settings",
                "%PROGRAMDATA%\\Warewolf\\VersionControl"
            };

            foreach (var FileOrFolder in ToClean)
            {
                var ActualPath = Environment.ExpandEnvironmentVariables(FileOrFolder);
                if (File.Exists(ActualPath))
                {
                    File.Delete(ActualPath);
                }
                if (Directory.Exists(ActualPath))
                {
                    Directory.Delete(ActualPath, true);
                }
                if ((File.Exists(FileOrFolder) || Directory.Exists(FileOrFolder)))
                {
                    Console.Error.WriteLine("Cannot delete " + FileOrFolder);
                }
            }

            if (String.IsNullOrEmpty(JobName))
            {
                JobName = "Test Run";
            }

            MoveFolderToTestResults(Environment.ExpandEnvironmentVariables(@"%PROGRAMDATA%\Warewolf\Resources"), $"{JobName} Server Resources Folder");
            MoveFolderToTestResults(Environment.ExpandEnvironmentVariables(@"%PROGRAMDATA%\Warewolf\Tests"), $"{JobName} Server Tests Folder");
            MoveFolderToTestResults(Environment.ExpandEnvironmentVariables(@"%PROGRAMDATA%\Warewolf\VersionControl"), $"{JobName} Server VersionControl Folder");
        }

        bool TryFindWarewolfServerExe(out string serverPath)
        {
            serverPath = FindFileInParent(ServerPathSpecs);
            if (serverPath.EndsWith(".zip"))
            {
                ZipFile.ExtractToDirectory(serverPath, TestRunner.TestsResultsPath + "\\Server");
                serverPath = TestRunner.TestsResultsPath + "\\Server\\" + ServerExeName;
            }
            return (!string.IsNullOrEmpty(serverPath) && File.Exists(serverPath));
        }

        bool TryFindWarewolfStudioExe(out string studioPath)
        {
            studioPath = FindFileInParent(StudioPathSpecs);
            if (studioPath.EndsWith(".zip"))
            {
                ZipFile.ExtractToDirectory(studioPath, TestRunner.TestsResultsPath + "\\Studio");
                studioPath = $"{TestRunner.TestsResultsPath}\\Studio\\{StudioExeName}";
            }
            return (!string.IsNullOrEmpty(studioPath) && (File.Exists(studioPath)));
        }

        internal void TryStartLocalCIRemoteContainer()
        {
            try
            {
                ciRemoteContainerLauncher = new ContainerLauncher("localhost", "", "latest", true);
                if (!string.IsNullOrEmpty(ciRemoteContainerLauncher.IP))
                {
                    CIRemoteOverloading(ciRemoteContainerLauncher.IP);
                }
                else
                {
                    throw new Exception("Cannot get container IP address.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to start CI Remote server container.\n" + e.Message.Replace(".",": ") + e.InnerException??e.InnerException.Message);
            }
        }

        void CIRemoteOverloading(string ip)
        {
            var ServerFolderPath = Path.GetDirectoryName(ServerPath);
            string ServerTestsCIRemote = Path.Combine(ServerFolderPath, $"Resources - ServerTests", "Resources", "Remote Connection Integration.xml");
            string UITestsCIRemote = Path.Combine(ServerFolderPath, $"Resources - UITests", "Resources", "Acceptance Testing Resources", "Remote Connection Integration.xml");
            Console.WriteLine($"Redirecting to containerized CI remote server in {ServerTestsCIRemote} and {UITestsCIRemote} to {ip}");
            var ServerTestsCIRemoteContents = File.ReadAllText(ServerTestsCIRemote);
            var UITestsCIRemoteContents = File.ReadAllText(UITestsCIRemote);
            ServerTestsCIRemoteContents = InsertServerSourceAddress(ServerTestsCIRemoteContents, ip);
            UITestsCIRemoteContents = InsertServerSourceAddress(UITestsCIRemoteContents, ip);
            ServerTestsCIRemoteContents = ServerTestsCIRemoteContents
                .Replace(";AuthenticationType=Windows", $";AuthenticationType=User;UserName={ContainerLauncher.Username};Password={ContainerLauncher.Password}");
            UITestsCIRemoteContents = UITestsCIRemoteContents
                .Replace(";AuthenticationType=Windows", $";AuthenticationType=User;UserName={ContainerLauncher.Username};Password={ContainerLauncher.Password}");
            File.WriteAllText(ServerTestsCIRemote, ServerTestsCIRemoteContents);
            File.WriteAllText(UITestsCIRemote, UITestsCIRemoteContents);
        }

        string InsertServerSourceAddress(string serverSourceXML, string newAddress)
        {
            var startFrom = "AppServerUri=http://";
            var subStringTo = ":3142/dsf;";
            int startIndex = serverSourceXML.IndexOf(startFrom) + startFrom.Length;
            int length = serverSourceXML.IndexOf(subStringTo) - startIndex;
            string oldAddress = serverSourceXML.Substring(startIndex, length);
            if (!string.IsNullOrEmpty(oldAddress))
            {
                serverSourceXML = serverSourceXML.Replace(oldAddress, "");
            }
            return serverSourceXML.Substring(0, startIndex) + newAddress + serverSourceXML.Substring(startIndex, serverSourceXML.Length - startIndex);
        }

        bool WaitForFileUnlock(string FileSpec)
        {
            var locked = true;
            var RetryCount = 0;
            while (locked && RetryCount < 100)
            {
                RetryCount++;
                try
                {
                    File.OpenWrite(FileSpec).Close();
                    locked = false;
                }
                catch
                {
                    Console.WriteLine($"Still waiting for {FileSpec} file to unlock.");
                    Thread.Sleep(3000);
                }
            }
            return locked;
        }

        bool WaitForFileExist(string FileSpec)
        {
            var exists = false;
            var RetryCount = 0;
            while (!exists && RetryCount < 100)
            {
                RetryCount++;
                if (File.Exists(FileSpec))
                {
                    exists = true;
                }
                else
                {
                    Console.WriteLine($"Still waiting for {FileSpec} file to exist.");
                   Thread.Sleep(3000);
                }
            }
            return exists;
        }

        public void MergeDotCoverSnapshots(List<string> DotCoverSnapshots, string DestinationFilePath, string LogFilePath)
        {
            if (DotCoverSnapshots != null)
            {
                if (DotCoverSnapshots.Count > 1)
                {
                    var DotCoverSnapshotsString = String.Join("\";\"", DotCoverSnapshots);
                    CopyOnWrite(LogFilePath + ".merge.log");
                    CopyOnWrite(LogFilePath + ".report.log");
                    CopyOnWrite(DestinationFilePath + ".dcvr");
                    CopyOnWrite(DestinationFilePath + ".html");
                    Process.Start(DotCoverPath, $"merge /Source=\"{DotCoverSnapshotsString}\" /Output=\"{DestinationFilePath}.dcvr\" /LogFile=\"{LogFilePath}.merge.log\"");
                }
                if (DotCoverSnapshots.Count == 1)
                {
                    var LoneSnapshot = DotCoverSnapshots[0];
                    if (DotCoverSnapshots.Count == 1 && (File.Exists(LoneSnapshot)))
                    {
                        Process.Start(DotCoverPath, $"report /Source=\"{LoneSnapshot}\" /Output=\"{DestinationFilePath}\\DotCover Report.html\" /ReportType=HTML /LogFile=\"{LogFilePath}.report.log\"");
                        Console.WriteLine($"DotCover report written to {DestinationFilePath}\\DotCover Report.html");
                    }
                }
            }
            if (File.Exists(DestinationFilePath + ".dcvr"))
            {
                Process.Start(DotCoverPath, $"report /Source=\"{DestinationFilePath}.dcvr\" /Output=\"{DestinationFilePath}\\DotCover Report.html\" /ReportType=HTML /LogFile=\"{LogFilePath}.report.log\"");
                Console.WriteLine($"DotCover report written to{DestinationFilePath}\\DotCover Report.html");
            }
        }

        public void MoveArtifactsToTestResults(bool DotCover, bool Server, bool Studio)
        {
            if (Cleanup)
            {
                //Write failing tests playlist.
                Console.WriteLine($"Writing all test failures in \"{TestRunner.TestsResultsPath}\" to a playlist file.");

                var PlayList = "<Playlist Version=\"1.0\">";
                foreach (var FullTRXFilePath in Directory.GetFiles(TestRunner.TestsResultsPath, "*.trx"))
                {
                    XmlDocument trxContent = new XmlDocument();
                    trxContent.Load(FullTRXFilePath);
                    var namespaceManager = new XmlNamespaceManager(trxContent.NameTable);
                    namespaceManager.AddNamespace("a", "http://microsoft.com/schemas/VisualStudio/TeamTest/2010");
                    if (trxContent.DocumentElement.SelectNodes("/a:TestRun/a:Results/a:UnitTestResult", namespaceManager).Count > 0)
                    {
                        foreach (XmlNode TestResult in trxContent.DocumentElement.SelectNodes("/a:TestRun/a:Results/a:UnitTestResult", namespaceManager))
                        {
                            if (TestResult.Attributes["outcome"].InnerText == "Failed")
                            {
                                if (trxContent.DocumentElement.SelectNodes("/a:TestRun/a:TestDefinitions/a:UnitTest/a:TestMethod", namespaceManager).Count > 0)
                                {
                                    foreach (XmlNode TestDefinition in trxContent.DocumentElement.SelectNodes("/a:TestRun/a:TestDefinitions/a:UnitTest/a:TestMethod", namespaceManager))
                                    {
                                        if (TestResult.Attributes["testName"] != null && TestDefinition.Name == TestResult.Attributes["testName"].InnerText)
                                        {
                                            PlayList += "<Add Test=\"" + TestDefinition.Attributes["className"] + "." + TestDefinition.Name + "\" />";
                                        }
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Error parsing /TestRun/TestDefinitions/UnitTest/TestMethod from trx file at trxFile");
                                    continue;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (trxContent.DocumentElement.SelectSingleNode("/a:TestRun/a:Results/a:UnitTestResult", namespaceManager).Attributes["outcome"].InnerText == "Failed")
                        {
                            PlayList += "<Add Test=\"" + trxContent.DocumentElement.SelectSingleNode("/a:TestRun/a:TestDefinitions/a:UnitTest/a:TestMethod", namespaceManager).Attributes["className"].InnerText + "." + trxContent.DocumentElement.SelectSingleNode("/a:TestRun/a:TestDefinitions/a:UnitTest/a:TestMethod", namespaceManager).Name + "\" />";
                        }
                        else
                        {
                            if (trxContent.DocumentElement.SelectSingleNode("/a:TestRun/a:Results/a:UnitTestResult", namespaceManager) == null)
                            {
                                Console.WriteLine("Error parsing /TestRun/Results/UnitTestResult from trx file at " + FullTRXFilePath);
                            }
                        }
                    }
                }
                PlayList += "</Playlist>";
                var OutPlaylistPath = $"{TestRunner.TestsResultsPath}\\{JobName} Failures.playlist";
                CopyOnWrite(OutPlaylistPath);
                File.WriteAllText(OutPlaylistPath, PlayList);
                Console.WriteLine($"Playlist file written to \"{OutPlaylistPath}\".");
            }

            if (Studio)
            {
                string studioLogFile = Environment.ExpandEnvironmentVariables(@"%LocalAppData%\Warewolf\Studio Logs\Warewolf Studio.log");
                WaitForFileUnlock(studioLogFile);
                MoveFileToTestResults(studioLogFile, $"{JobName} Studio.log");
            }
            if (Studio && DotCover)
            {
                var StudioSnapshot = Environment.ExpandEnvironmentVariables(@"%LocalAppData%\Warewolf\Studio Logs\dotCover.dcvr");
                Console.WriteLine($"Trying to move Studio coverage snapshot file from {StudioSnapshot} to {TestRunner.TestsResultsPath}\\{JobName} Studio DotCover.dcvr");
                var exists = WaitForFileExist(StudioSnapshot);
                if (exists)
                {
                    var locked = WaitForFileUnlock(StudioSnapshot);
                    if (!(locked))
                    {
                        Console.WriteLine($"Moving Studio coverage snapshot file from StudioSnapshot to {TestRunner.TestsResultsPath}\\{JobName} Studio DotCover.dcvr");
                        CopyOnWrite($"{TestRunner.TestsResultsPath}\\{JobName} Studio DotCover.dcvr");
                        File.Move(StudioSnapshot, $"{TestRunner.TestsResultsPath}\\{JobName} Studio DotCover.dcvr");
                    }
                    else
                    {
                        Console.WriteLine("Studio Coverage Snapshot File is locked.");
                    }
                }
                else
                {
                    throw new FileNotFoundException($"Studio coverage snapshot not found at {StudioSnapshot}");
                }
                if (File.Exists(Environment.ExpandEnvironmentVariables(@"%LocalAppData%\Warewolf\Studio Logs\dotCover.log")))
                {
                    MoveFileToTestResults(Environment.ExpandEnvironmentVariables(@"%LocalAppData%\Warewolf\Studio Logs\dotCover.log"), $"{JobName} Studio DotCover.log");
                }
            }
            if (Server)
            {
                string serverLogFile = Environment.ExpandEnvironmentVariables(@"%ProgramData%\Warewolf\Server Log\wareWolf-Server.log");
                WaitForFileUnlock(serverLogFile);
                MoveFileToTestResults(serverLogFile, $"{JobName} Server.log");

                string myWarewolfIoLogFile = Environment.ExpandEnvironmentVariables(@"%ProgramData%\Warewolf\Server Log\my.warewolf.io.log");
                WaitForFileUnlock(serverLogFile);
                MoveFileToTestResults(myWarewolfIoLogFile, $"{JobName} my.warewolf.io Server.log");

                string myWarewolfIoErrorsLogFile = Environment.ExpandEnvironmentVariables(@"%ProgramData%\Warewolf\Server Log\my.warewolf.io.errors.log");
                WaitForFileUnlock(myWarewolfIoErrorsLogFile);
                MoveFileToTestResults(myWarewolfIoErrorsLogFile, $"{JobName} my.warewolf.io Server Errors.log");
            }
            if (Server && DotCover)
            {
                var ServerSnapshot = Environment.ExpandEnvironmentVariables(@"%ProgramData%\Warewolf\Server Log\dotCover.dcvr");
                Console.WriteLine($"Trying to move Server coverage snapshot file from {ServerSnapshot} to {TestRunner.TestsResultsPath}\\{JobName} Server DotCover.dcvr");
                var exists = WaitForFileExist(ServerSnapshot);
                if (exists)
                {
                    var locked = WaitForFileUnlock(ServerSnapshot);
                    if (!locked)
                    {
                        Console.WriteLine($"Moving Server coverage snapshot file from {ServerSnapshot} to {TestRunner.TestsResultsPath}\\{JobName} Server DotCover.dcvr");
                        MoveFileToTestResults(ServerSnapshot, $"{JobName} Server DotCover.dcvr");
                    }
                    else
                    {
                        Console.WriteLine("Server Coverage Snapshot File still locked after retrying for 2 minutes.");
                    }
                }
                if (File.Exists(Environment.ExpandEnvironmentVariables(@"%ProgramData%\Warewolf\Server Log\dotCover.log")))
                {
                    MoveFileToTestResults(Environment.ExpandEnvironmentVariables(@"%ProgramData%\Warewolf\Server Log\dotCover.log"), $"{JobName} Server DotCover.log");
                }
                if (File.Exists(Environment.ExpandEnvironmentVariables(@"%ProgramData%\Warewolf\Server Log\my.warewolf.io.log")))
                {
                    MoveFileToTestResults(Environment.ExpandEnvironmentVariables(@"%ProgramData%\Warewolf\Server Log\my.warewolf.io.log"), $"{JobName} my.warewolf.io.log");
                }
                if (File.Exists(Environment.ExpandEnvironmentVariables(@"%ProgramData%\Warewolf\Server Log\my.warewolf.io.errors.log")))
                {
                    MoveFileToTestResults(Environment.ExpandEnvironmentVariables(@"%ProgramData%\Warewolf\Server Log\my.warewolf.io.errors.log"), $"{JobName} my.warewolf.io Errors.log");
                }
            }
            if (Server && Studio && DotCover)
            {
                MergeDotCoverSnapshots(new List<string> { Path.Combine(TestRunner.TestsResultsPath, $"{JobName} Server DotCover.dcvr"), Path.Combine(TestRunner.TestsResultsPath, $"{JobName} Studio DotCover.dcvr") }, Path.Combine(TestRunner.TestsResultsPath, $"{JobName} Merged Server and Studio DotCover"), Path.Combine(TestRunner.TestsResultsPath, "ServerAndStudioDotCoverSnapshot"));
            }
            if (RecordScreen != null)
            {
                MoveScreenRecordingsToTestResults();
            }
            foreach (var scriptFile in Directory.GetFiles(Path.GetDirectoryName(TestRunner.TestsResultsPath)))
            {
                if (Path.GetFileName(scriptFile).StartsWith("Run ") && Path.GetExtension(scriptFile) == ".bat")
                {
                    MoveFileToTestResults(scriptFile, Path.GetFileName(scriptFile));
                }
            }
        }

        public void RetryTestFailures(string jobName, string testAssembliesList, List<string> TestAssembliesDirectories, string testSettingsFile, string FullTRXFilePath, int currentRetryCount)
        {
            TestRunner.TestsResultsPath = Path.Combine(TestRunner.TestsResultsPath, NumberToWords(currentRetryCount) + "RetryTestResults");

            WaitForFileUnlock(FullTRXFilePath);
            TestRunner.TestList = "";
            XmlDocument trxContent = new XmlDocument();
            trxContent.Load(FullTRXFilePath);
            var namespaceManager = new XmlNamespaceManager(trxContent.NameTable);
            namespaceManager.AddNamespace("a", "http://microsoft.com/schemas/VisualStudio/TeamTest/2010");
            if (trxContent.DocumentElement.SelectNodes("/a:TestRun/a:TestDefinitions/a:UnitTest/a:TestMethod", namespaceManager).Count > 0)
            {
                foreach (XmlNode TestResult in trxContent.DocumentElement.SelectNodes("/a:TestRun/a:Results/a:UnitTestResult", namespaceManager))
                {
                    if (TestResult.Attributes["outcome"] != null && TestResult.Attributes["outcome"].InnerText == "Failed")
                    {
                        TestRunner.TestList += "," + TestResult.Attributes["testName"].InnerXml;
                    }
                }
            }
            else
            {
                Console.WriteLine($"Error parsing /TestRun/Results/UnitTestResult from trx file at {FullTRXFilePath}");
            }
            string TestRunnerPath;
            if (TestRunner.TestList.StartsWith(","))
            {
                TestRunnerPath = TestRunner.WriteTestRunner(jobName, "", "", testAssembliesList, testSettingsFile, Path.Combine(TestRunner.TestsResultsPath, "RetryResults"), RecordScreen != null, JobSpecs);
            }
            else
            {
                Console.WriteLine($"No failing tests found to retry in trx file at {FullTRXFilePath}");
                return;
            }
            Console.WriteLine($"Re-running all test failures in \"{FullTRXFilePath}\".");
            var retryResults = RunTests(jobName, testAssembliesList, TestAssembliesDirectories, testSettingsFile, TestRunnerPath);
            if (retryResults != FullTRXFilePath)
            {
                MergeRetryResults(FullTRXFilePath, retryResults);
            }
            else
            {
                Console.WriteLine($"{TestRunnerPath} did not produce a test result trx file in {TestRunner.TestsResultsPath}");
            }
        }

        public static string NumberToWords(int number) => new[] { "None", "First", "Second", "Third", "Fourth", "Fifth", "Sixth", "Seventh", "Eighth", "Nineth", "Tenth", "Eleventh", "Twelveth", "Thirteenth", "Fourteenth", "Fifteenth", "Sixteenth", "Seventeenth", "Eighteenth", "Nineteenth" }[number];

        void MergeRetryResults(string originalResults, string retryResults)
        {
            var trxContent = new XmlDocument();
            trxContent.Load(retryResults);
            var newNamespaceManager = new XmlNamespaceManager(trxContent.NameTable);
            newNamespaceManager.AddNamespace("a", "http://microsoft.com/schemas/VisualStudio/TeamTest/2010");
            if (trxContent.DocumentElement.SelectNodes("/a:TestRun/a:Results/a:UnitTestResult", newNamespaceManager).Count > 0)
            {
                var originalTrxContent = new XmlDocument();
                originalTrxContent.Load(originalResults);
                var originalNamespaceManager = new XmlNamespaceManager(originalTrxContent.NameTable);
                originalNamespaceManager.AddNamespace("a", "http://microsoft.com/schemas/VisualStudio/TeamTest/2010");
                foreach (XmlNode TestResult in trxContent.DocumentElement.SelectNodes("/a:TestRun/a:Results/a:UnitTestResult", newNamespaceManager))
                {
                    if (TestResult.Attributes["outcome"] != null && TestResult.Attributes["outcome"].InnerText == "Failed")
                    {
                        foreach (XmlNode OriginalTestResult in originalTrxContent.DocumentElement.SelectNodes("/a:TestRun/a:Results/a:UnitTestResult", originalNamespaceManager))
                        {
                            if (OriginalTestResult.Attributes["testName"] != null && TestResult.Attributes["testName"] != null && OriginalTestResult.Attributes["testName"].InnerXml == TestResult.Attributes["testName"].InnerXml)
                            {
                                XmlNode originalOutputNode = OriginalTestResult.SelectSingleNode("//a:Output", originalNamespaceManager);
                                XmlNode newOutputNode = TestResult.SelectSingleNode("//a:Output", newNamespaceManager);
                                if (newOutputNode != null)
                                {
                                    if (originalOutputNode != null)
                                    {
                                        XmlNode originalStdErrNode = originalOutputNode.SelectSingleNode("//a:StdErr", originalNamespaceManager);
                                        XmlNode newStdErrNode = newOutputNode.SelectSingleNode("//a:StdErr", newNamespaceManager);
                                        if (newStdErrNode != null)
                                        {
                                            if (originalStdErrNode != null)
                                            {
                                                originalStdErrNode.InnerText += "\n" + newStdErrNode.InnerText;
                                            }
                                            else
                                            {
                                                originalOutputNode.AppendChild(newStdErrNode);
                                            }
                                        }
                                        XmlNode originalStdOutNode = originalOutputNode.SelectSingleNode("//a:StdOut", originalNamespaceManager);
                                        XmlNode newStdOutNode = newOutputNode.SelectSingleNode("//a:StdOut", newNamespaceManager);
                                        if (newStdOutNode != null)
                                        {
                                            if (originalStdOutNode != null)
                                            {
                                                originalStdOutNode.InnerText += "\n" + newStdOutNode.InnerText;
                                            }
                                            else
                                            {
                                                originalOutputNode.AppendChild(newStdOutNode);
                                            }
                                        }
                                        XmlNode originalErrorInfoNode = originalOutputNode.SelectSingleNode("//a:ErrorInfo", originalNamespaceManager);
                                        XmlNode newErrorInfoNode = newOutputNode.SelectSingleNode("//a:ErrorInfo", newNamespaceManager);
                                        if (newErrorInfoNode != null)
                                        {
                                            if (originalErrorInfoNode != null)
                                            {
                                                XmlNode originalMessageNode = originalErrorInfoNode.SelectSingleNode("//a:Message", originalNamespaceManager);
                                                XmlNode newMessageNode = newErrorInfoNode.SelectSingleNode("//a:Message", newNamespaceManager);
                                                if (newErrorInfoNode != null)
                                                {
                                                    if (originalMessageNode != null)
                                                    {
                                                        originalMessageNode.InnerText += "\n" + newErrorInfoNode.InnerText;
                                                    }
                                                    else
                                                    {
                                                        originalMessageNode.AppendChild(newErrorInfoNode);
                                                    }
                                                }
                                                XmlNode originalStackTraceNode = originalErrorInfoNode.SelectSingleNode("//a:StackTrace", originalNamespaceManager);
                                                XmlNode newStackTraceNode = newErrorInfoNode.SelectSingleNode("//a:StackTrace", newNamespaceManager);
                                                if (newStackTraceNode != null)
                                                {
                                                    if (originalStackTraceNode != null)
                                                    {
                                                        originalStackTraceNode.InnerText += "\n" + newStackTraceNode.InnerText;
                                                    }
                                                    else
                                                    {
                                                        originalStackTraceNode.AppendChild(newStackTraceNode);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                originalOutputNode.AppendChild(newErrorInfoNode);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        OriginalTestResult.AppendChild(newOutputNode);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (XmlNode OriginalTestResult in originalTrxContent.DocumentElement.SelectNodes("/a:TestRun/a:Results/a:UnitTestResult", originalNamespaceManager))
                        {
                            if (OriginalTestResult.Attributes["testName"] != null && TestResult.Attributes["testName"] != null && OriginalTestResult.Attributes["testName"].InnerXml == TestResult.Attributes["testName"].InnerXml)
                            {
                                if (OriginalTestResult.Attributes["outcome"] == null)
                                {
                                    var newOutcomeAttribute = originalTrxContent.CreateAttribute("outcome");
                                    newOutcomeAttribute.Value = "Passed";
                                    OriginalTestResult.Attributes.Append(newOutcomeAttribute);
                                }
                                else
                                {
                                    OriginalTestResult.Attributes["outcome"].InnerText = "Passed";
                                }
                                XmlNode originalOutputNode = OriginalTestResult.SelectSingleNode("//a:Output", originalNamespaceManager);
                                XmlNode newOutputNode = TestResult.SelectSingleNode("//a:Output", newNamespaceManager);
                                if (newOutputNode != null)
                                {
                                    if (originalOutputNode != null)
                                    {
                                        XmlNode originalStdErrNode = originalOutputNode.SelectSingleNode("//a:StdErr", originalNamespaceManager);
                                        if (originalStdErrNode != null)
                                        {
                                            try
                                            {
                                                originalOutputNode.RemoveChild(originalStdErrNode);
                                            }
                                            catch (ArgumentException) { }
                                        }
                                        XmlNode originalStdOutNode = originalOutputNode.SelectSingleNode("//a:StdOut", originalNamespaceManager);
                                        XmlNode newStdOutNode = newOutputNode.SelectSingleNode("//a:StdOut", newNamespaceManager);
                                        if (newStdOutNode != null)
                                        {
                                            if (originalStdOutNode != null)
                                            {
                                                originalStdOutNode.InnerText += "\n" + newStdOutNode.InnerText;
                                            }
                                            else
                                            {
                                                originalOutputNode.AppendChild(newStdOutNode);
                                            }
                                        }
                                        XmlNode originalErrorInfoNode = originalOutputNode.SelectSingleNode("//a:ErrorInfo", originalNamespaceManager);
                                        if (originalErrorInfoNode != null)
                                        {
                                            try
                                            {
                                                originalOutputNode.RemoveChild(originalErrorInfoNode);
                                            }
                                            catch (ArgumentException) { }
                                        }
                                    }
                                    else
                                    {
                                        OriginalTestResult.AppendChild(newOutputNode);
                                    }
                                }
                            }
                        }
                        var countersNodes = originalTrxContent.DocumentElement.SelectNodes("/a:TestRun/a:ResultSummary/a:Counters", originalNamespaceManager);
                        if (countersNodes.Count > 0)
                        {
                            var countersNode = countersNodes.Item(0);
                            var failuresBefore = int.Parse(countersNode.Attributes["failed"].InnerText);
                            var passesBefore = int.Parse(countersNode.Attributes["passed"].InnerText);
                            if (--failuresBefore <= 0)
                            {
                                var resultsSummaryNodes = originalTrxContent.DocumentElement.SelectNodes("/a:TestRun/a:ResultSummary", originalNamespaceManager);
                                if (resultsSummaryNodes.Count > 0)
                                {
                                    var resultsSummaryNode = resultsSummaryNodes.Item(0);
                                    resultsSummaryNode.Attributes["outcome"].InnerText = "Completed";
                                }
                            }
                            countersNode.Attributes["failed"].InnerText = failuresBefore.ToString();
                            countersNode.Attributes["passed"].InnerText = (++passesBefore).ToString();
                        }
                    }
                }
                originalTrxContent.Save(originalResults);
                File.Delete(retryResults);
            }
            else
            {
                Console.WriteLine("Error parsing /TestRun/TestDefinitions/UnitTest/TestMethod from trx file at " + retryResults);
            }
        }

        void MoveScreenRecordingsToTestResults()
        {
            Console.WriteLine("Getting UI test screen recordings from \"" + TestRunner.TestsResultsPath + "\"");
            var ScreenRecordingsFolder = GetLatestScreenRecordingsFolder();
            if (!string.IsNullOrEmpty(ScreenRecordingsFolder))
            {
                string directoryToRemove = Path.Combine(ScreenRecordingsFolder + "\\In");
                if (Directory.Exists(directoryToRemove))
                {
                    foreach (var subDir in Directory.GetDirectories(directoryToRemove))
                    {
                        string subDirName = Path.GetFileName(subDir);
                        string newDirFullPath = Path.Combine(ScreenRecordingsFolder, subDirName);
                        Directory.Move(subDir, newDirFullPath);
                    }
                    Directory.Delete(directoryToRemove);
                }
                else
                {
                    Console.WriteLine(directoryToRemove + " not found.");
                }
            }
        }

        string GetLatestScreenRecordingsFolder()
        {
            var directory = new DirectoryInfo(TestRunner.TestsResultsPath);
            var screenRecordingFolders = directory.GetDirectories().Where((folderPath) => { return folderPath.Name.StartsWith("ScreenRecordings"); });
            if (screenRecordingFolders.Count() > 0)
            {
                return screenRecordingFolders.OrderByDescending(f => f.LastWriteTime).First().FullName;
            }
            return "";
        }

        public void InstallServer()
        {
            //Find Server
            if (string.IsNullOrEmpty(ServerPath))
            {
                bool foundServer = TryFindWarewolfServerExe(out string serverPath);
                if (foundServer)
                {
                    ServerPath = serverPath;
                }
                else
                {
                    throw new ArgumentException($"No server found. Make sure your server is compiled and try again.");
                }
            }
            else
            {
                if (!File.Exists(ServerPath))
                {
                    throw new ArgumentException($"No server found at {ServerPath}. Make sure your server is compiled and try again.");
                }
            }
            Console.WriteLine("Will now stop any currently running Warewolf servers and studios. Resources will be backed up to " + TestRunner.TestsResultsPath + ".");
            if (string.IsNullOrEmpty(ResourcesType))
            {
                Console.WriteLine("\nWhat type of resources would you like to install the server with?");
                var options = new[] {
                    "[u]UITests: Use these resources for running UI Tests. (This is the default)",
                    "[s]ServerTests: Use these resources for running everything except unit tests and Coded UI tests.",
                    "[r]Release: Use these resources for Warewolf releases.",
                    "[l]Load: Use these resources for Desktop UI Load Testing."
                };
                foreach (var option in options)
                {
                    Console.WriteLine();
                    var originalColour = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write(option.Substring(0, 3));
                    Console.ForegroundColor = originalColour;
                    Console.Write(option.Substring(3, option.Length-3));
                }
                Console.WriteLine("\n\nOr Press Enter to use default (UITest)...");
                string originalTitle = Console.Title;
                string uniqueTitle = Guid.NewGuid().ToString();
                Console.Title = uniqueTitle;
                Thread.Sleep(50);
                IntPtr handle = FindWindowByCaption(IntPtr.Zero, uniqueTitle);

                if (handle != IntPtr.Zero)
                {
                    Console.Title = originalTitle;
                    SetForegroundWindow(handle);
                }
                ResourcesType = Console.ReadLine();
                if (ResourcesType == "" || ResourcesType.ToLower() == "u")
                {
                    ResourcesType = "UITests";
                }
                if (ResourcesType.ToLower() == "s")
                {
                    ResourcesType = "ServerTests";
                }
                if (ResourcesType.ToLower() == "r")
                {
                    ResourcesType = "Release";
                }
                if (ResourcesType.ToLower() == "l")
                {
                    ResourcesType = "Load";
                }
            }
            
            string resourcesPath = Path.Combine(Path.GetDirectoryName(ServerPath), $"Resources - {ResourcesType}");
            if (!Directory.Exists(resourcesPath))
            {
                throw new ArgumentException($"Invalid resource type. Folder not found {resourcesPath}");
            }

            if (!StartServerAsConsole)
            {
                var ServerService = ServiceController.GetServices().Any(serviceController => serviceController.ServiceName.Equals("Warewolf Server"));
                if (!ApplyDotCover)
                {
                    if (!ServerService)
                    {
                        Process.Start("sc.exe", "create \"Warewolf Server\" binPath= \"" + ServerPath + "\" start= demand");
                    }
                    else
                    {
                        Console.WriteLine("Configuring service to " + ServerPath);
                        Process.Start("sc.exe", "config \"Warewolf Server\" binPath= \"" + ServerPath + "\" start= demand");
                    }
                }
                else
                {
                    var ServerBinDir = Path.GetDirectoryName(ServerPath);
                    var RunnerXML = @"<AnalyseParams>
    <TargetExecutable>" + ServerPath + @"</TargetExecutable>
    <Output>" + Environment.ExpandEnvironmentVariables("%ProgramData%") + @"\Warewolf\Server Log\dotCover.dcvr</Output>
    <Scope>
	    <ScopeEntry>" + ServerBinDir + @"\*.dll</ScopeEntry>
	    <ScopeEntry>" + ServerBinDir + @"\*.exe</ScopeEntry>
    </Scope>
    <Filters>
        <ExcludeFilters>
            <FilterEntry>
                <ModuleMask>*.tests</ModuleMask>
                <ModuleMask>*.specs</ModuleMask>
            </FilterEntry>
        </ExcludeFilters>
        <AttributeFilters>
            <AttributeFilterEntry>
                <ClassMask>System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute</ClassMask>
            </AttributeFilterEntry>
        </AttributeFilters>
    </Filters>
</AnalyseParams>";

                    if (string.IsNullOrEmpty(JobName))
                    {
                        if (ProjectName != "")
                        {
                            JobName = ProjectName;
                        }
                        else
                        {
                            JobName = "Manual Tests";
                        }
                    }
                    var DotCoverRunnerXMLPath = TestRunner.TestsResultsPath + "\\Server DotCover Runner.xml";
                    CopyOnWrite(DotCoverRunnerXMLPath);
                    File.WriteAllText(DotCoverRunnerXMLPath, RunnerXML);
                    RunServerWithDotcoverScript = "\\\"" + DotCoverPath + "\\\" cover \\\"" + DotCoverRunnerXMLPath + "\\\" /LogFile=\\\"" + TestRunner.TestsResultsPath + "\\ServerDotCover.log\\\"";
                    if (!ServerService)
                    {
                        Process.Start("sc.exe", "create \"Warewolf Server\" binPath= \"" + RunServerWithDotcoverScript + "\" start= demand");
                    }
                    else
                    {
                        Console.WriteLine("Configuring service to " + RunServerWithDotcoverScript);
                        Process.Start("sc.exe", "config \"Warewolf Server\" binPath= \"" + RunServerWithDotcoverScript + "\"");
                    }
                }
            }
            if (!string.IsNullOrEmpty(ServerUsername) && string.IsNullOrEmpty(ServerPassword))
            {
                Process.Start("sc.exe", "config \"Warewolf Server\" obj= \"" + ServerUsername + "\"");
            }
            if (!string.IsNullOrEmpty(ServerUsername) && !string.IsNullOrEmpty(ServerPassword))
            {
                Process.Start("sc.exe", "config \"Warewolf Server\" obj= \"" + ServerUsername + "\" password= \"" + ServerPassword + "\"");
            }

            var ResourcePathSpecs = new List<string>();
            foreach (var ServerPathSpec in ServerPathSpecs)
            {
                if (ServerPathSpec.EndsWith(ServerExeName))
                {
                    ResourcePathSpecs.Add(ServerPathSpec.Replace(ServerExeName, "Resources - " + ResourcesType));
                }
            }
            var ResourcesDirectory = FindFileInParent(ResourcePathSpecs);

            if (ResourcesDirectory != "" && ResourcesDirectory != Path.GetDirectoryName(ServerPath) + "\\" + Path.GetFileName(ResourcesDirectory))
            {
                RecursiveFolderCopy(ResourcesDirectory, Path.GetDirectoryName(ServerPath));
            }
        }

        public void StartServer()
        {
            var ServerFolderPath = Path.GetDirectoryName(ServerPath);
            Console.WriteLine($"Deploying New resources from {ServerFolderPath}\\Resources - {ResourcesType}\\*");
            RecursiveFolderCopy(Path.Combine(ServerFolderPath, $"Resources - {ResourcesType}"), Environment.ExpandEnvironmentVariables("%ProgramData%\\Warewolf"));

            if (!StartServerAsConsole)
            {
                try
                {
                    ServiceController.GetServices().FirstOrDefault(serviceController => serviceController.ServiceName.Equals("Warewolf Server"))?.Start();
                }
                catch (InvalidOperationException e)
                {
                    Console.WriteLine(e.Message);
                }

                var process = StartProcess("sc.exe", "interrogate \"Warewolf Server\"");
                var Output = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
                if (!(Output.EndsWith("RUNNING ")))
                {
                    Console.WriteLine(Output);
                    process.StartInfo.Arguments = "start \"Warewolf Server\"";
                    process.Start();
                    process.WaitForExit();
                }
            }
            else
            {
                if (!ApplyDotCover)
                {
                    Process.Start(ServerPath);
                }
                else
                {
                    Process.Start(RunServerWithDotcoverScript);
                }
            }

            WaitForServerStart(ServerFolderPath);
        }

        void WaitForServerStart(string ServerFolderPath)
        {
            var ServerStartedFilePath = ServerFolderPath + "\\ServerStarted";
            WaitForFileExist(ServerStartedFilePath);
            if (!(File.Exists(ServerStartedFilePath)))
            {
                throw new Exception("Server Cannot Start.");
            }
            else
            {
                Console.WriteLine("Server has started.");
            }
        }

        public void Startmywarewolfio()
        {
            if (Directory.Exists(WebsPath))
            {
                var IISExpressPath = "C:\\Program Files (x86)\\IIS Express\\iisexpress.exe";
                if (!(File.Exists(IISExpressPath)))
                {
                    Console.WriteLine("my.warewolf.io cannot be hosted. IISExpressPath not found.");
                }
                else
                {
                    Console.WriteLine("\"" + IISExpressPath + "\" /path:\"" + WebsPath + "\" /port:18405 /trace:error");
                    Process.Start(IISExpressPath, "/path:\"" + WebsPath + "\" /port:18405 /trace:error");
                    Console.WriteLine("my.warewolf.io has started.");
                }
            }
            else
            {
                Console.WriteLine("my.warewolf.io cannot be hosted. Webs not found at " + TestRunner.TestsPath + "\\_PublishedWebsites\\Dev2.Web");
                if (!string.IsNullOrEmpty(ServerPath))
                {
                    Console.Write(" or at " + Path.GetDirectoryName(ServerPath) + "\\_PublishedWebsites\\Dev2.Web");
                }
            }
        }

        public void StartStudio()
        {
            if (string.IsNullOrEmpty(StudioPath))
            {
                throw new FileNotFoundException("Cannot find Warewolf Studio. To run the studio provide a path to the Warewolf Studio exe file as a commandline parameter like this: -StudioPath");
            }
            var StudioLogFile = Environment.ExpandEnvironmentVariables("%LocalAppData%\\Warewolf\\Studio Logs\\Warewolf Studio.log");
            CopyOnWrite(StudioLogFile);
            if (!ApplyDotCover)
            {
                Process.Start(StudioPath);
            }
            else
            {
                var StudioBinDir = Path.GetDirectoryName(StudioPath);
                var RunnerXML = @"
<AnalyseParams>
    <TargetExecutable>" + StudioPath + @"</TargetExecutable>
    <Output>" + Environment.ExpandEnvironmentVariables("%LocalAppData%") + @"\Warewolf\Studio Logs\dotCover.dcvr</Output>
    <Scope>
    	<ScopeEntry>" + StudioBinDir + @"\*.dll</ScopeEntry>
    	<ScopeEntry>" + StudioBinDir + @"\*.exe</ScopeEntry>
    </Scope>
    <Filters>
        <ExcludeFilters>
            <FilterEntry>
                <ModuleMask>*.tests</ModuleMask>
                <ModuleMask>*.specs</ModuleMask>
            </FilterEntry>
        </ExcludeFilters>
        <AttributeFilters>
            <AttributeFilterEntry>
                <ClassMask>System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute</ClassMask>
            </AttributeFilterEntry>
        </AttributeFilters>
    </Filters>
</AnalyseParams>
";
                var DotCoverRunnerXMLPath = TestRunner.TestsResultsPath + "\\Studio DotCover Runner.xml";
                CopyOnWrite(DotCoverRunnerXMLPath);
                File.WriteAllText(DotCoverRunnerXMLPath, RunnerXML);
                Process.Start(DotCoverPath, "cover \"" + DotCoverRunnerXMLPath + "\" /LogFile=\"" + TestRunner.TestsResultsPath + "\\StudioDotCover.log\"");
            }
            try
            {
                WaitForStudioStart(Path.GetDirectoryName(StudioPath));
            }
            catch (Exception)
            {
                if (!ApplyDotCover)
                {
                    Process.Start(StudioPath);
                }
                else
                {
                    Process.Start(DotCoverPath, "cover \"" + TestRunner.TestsResultsPath + "\\Studio DotCover Runner.xml\" /LogFile=\"" + TestRunner.TestsResultsPath + "\\StudioDotCover.log\"");
                }
                WaitForStudioStart(Path.GetDirectoryName(StudioPath));
            }
        }

        void WaitForStudioStart(string StudioFolderPath)
        {
            var StudioStartedFilePath = Path.Combine(StudioFolderPath, "StudioStarted");
            WaitForFileExist(StudioStartedFilePath);
            if (!(File.Exists(StudioStartedFilePath)))
            {
                throw new Exception("Studio Cannot Start.");
            }
            else
            {
                Console.WriteLine("Studio has started.");
            }
        }

        bool AssemblyIsNotAlreadyDefinedWithoutWildcards(string AssemblyNameToCheck)
        {
            var JobAssemblySpecs = new List<string>();
            foreach (var Job in JobSpecs.Values)
            {
                if (!Job.Item1.Contains("*") && !JobAssemblySpecs.Contains(Job.Item1))
                {
                    JobAssemblySpecs.Add(Job.Item1);
                }
            }
            return !JobAssemblySpecs.Contains(AssemblyNameToCheck);
        }

        public Tuple<string, List<string>> ResolveProjectFolderSpecs(string ProjectFolderSpec)
        {
            var TestAssembliesList = "";
            var TestAssembliesDirectories = new List<string>();
            var ProjectFolderSpecInParent = FindFileInParent(new List<string> { ProjectFolderSpec });
            if (ProjectFolderSpecInParent != "")
            {
                if (ProjectFolderSpecInParent.Contains("*"))
                {
                    foreach (var projectFolder in Directory.GetDirectories(ProjectFolderSpecInParent))
                    {
                        TestAssembliesList += TestRunner.AppendProjectFolder(projectFolder);
                        if (!TestAssembliesDirectories.Contains(projectFolder + "\\bin\\Debug"))
                        {
                            TestAssembliesDirectories.Add(projectFolder + "\\bin\\Debug");
                        }
                    }
                }
                else
                {
                    TestAssembliesList += TestRunner.AppendProjectFolder(ProjectFolderSpecInParent);
                    if (!TestAssembliesDirectories.Contains(ProjectFolderSpecInParent + "\\bin\\Debug"))
                    {
                        TestAssembliesDirectories.Add(ProjectFolderSpecInParent + "\\bin\\Debug");
                    }
                }
                return new Tuple<string, List<string>>(TestAssembliesList, TestAssembliesDirectories);
            }
            throw new Exception("Cannot resolve spec: " + ProjectFolderSpec);
        }

        public Tuple<string, List<string>> ResolveTestAssemblyFileSpecs(string TestAssemblyFileSpecs)
        {
            var TestAssembliesList = "";
            var TestAssembliesDirectories = new List<string>();
            var TestAssembliesFileSpecsInParent = FindFileInParent(new List<string> { TestAssemblyFileSpecs });
            if (!string.IsNullOrEmpty(TestAssembliesFileSpecsInParent))
            {
                List<string> resolveCommaNotation = new List<string>();
                if (TestAssembliesFileSpecsInParent.Contains(','))
                {
                    resolveCommaNotation = TestAssembliesFileSpecsInParent.Split(',').ToList();
                }
                else
                {
                    resolveCommaNotation.Add(TestAssembliesFileSpecsInParent);
                }
                List<string> resolveStarNotation = new List<string>();
                foreach (var file in resolveCommaNotation)
                {
                    if (TestAssembliesFileSpecsInParent.Contains('*'))
                    {
                        resolveStarNotation = Directory.GetFiles(Path.GetDirectoryName(TestAssembliesFileSpecsInParent), Path.GetFileName(TestAssembliesFileSpecsInParent), SearchOption.TopDirectoryOnly).ToList();
                    }
                    else
                    {
                        resolveStarNotation.Add(TestAssembliesFileSpecsInParent);
                    }
                }
                foreach (var file in resolveStarNotation)
                {
                    var AssemblyNameToCheck = Path.GetFileNameWithoutExtension(file);
                    if (!TestAssembliesFileSpecsInParent.Contains("*") || (AssemblyIsNotAlreadyDefinedWithoutWildcards(AssemblyNameToCheck)))
                    {
                        TestAssembliesList = TestRunner.AppendTestAssembly(TestAssembliesList, file);
                        if (!TestAssembliesDirectories.Contains(Path.GetDirectoryName(file)))
                        {
                            TestAssembliesDirectories.Add(Path.GetDirectoryName(file));
                        }
                    }
                }
                return new Tuple<string, List<string>>(TestAssembliesList, TestAssembliesDirectories);
            }
            throw new Exception($"Cannot find test assemblies at {TestAssemblyFileSpecs}. Make sure your test assemblies are compiled and try again.");
        }

        public string ScreenRecordingTestSettingsFile(string JobName)
        {
            var TestSettingsFile = "";
            if (RecordScreen != null)
            {
                var TestSettingsId = Guid.NewGuid();

                // Create test settings.
                TestSettingsFile = TestRunner.TestsResultsPath + "\\" + JobName + ".testsettings";
                CopyOnWrite(TestSettingsFile);
                File.WriteAllText(TestSettingsFile, @"<?xml version=""1.0"" encoding=""UTF-8""?>
<TestSettings id=""" + TestSettingsId + @""" name=""JobName"" xmlns=""http://microsoft.com/schemas/VisualStudio/TeamTest/2010"">
    <Description>Run " + JobName + @" With Screen Recording.</Description>
    <NamingScheme baseName=""ScreenRecordings"" appendTimeStamp=""false"" useDefault=""false""/>
    <Execution>
    <AgentRule name=""LocalMachineDefaultRole"">
        <DataCollectors>
        <DataCollector uri=""datacollector://microsoft/VideoRecorder/1.0"" assemblyQualifiedName=""Microsoft.VisualStudio.TestTools.DataCollection.VideoRecorder.VideoRecorderDataCollector, Microsoft.VisualStudio.TestTools.DataCollection.VideoRecorder, Version=12.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"" friendlyName=""Screen and Voice Recorder"">
            <Configuration>
            <MediaRecorder sendRecordedMediaForPassedTestCase=""false"" xmlns=""""/>
            </Configuration>
        </DataCollector>
        </DataCollectors>
    </AgentRule>
    </Execution>
</TestSettings>
");
            }
            return TestSettingsFile;
        }

        public string DotCoverRunner(string JobName, List<string> TestAssembliesDirectories)
        {
            // Write DotCover Runner XML 
            var DotCoverSnapshotFile = Path.Combine(TestRunner.TestsResultsPath, $"{JobName} DotCover Output.dcvr");
            CopyOnWrite(DotCoverSnapshotFile);
            var DotCoverArgs = @"<AnalyseParams>
    <TargetExecutable>" + TestRunner.TestsResultsPath + "\\..\\Run " + JobName + @".bat</TargetExecutable>
    <Output>" + DotCoverSnapshotFile + @"</Output>
    <Scope>";
            foreach (var TestAssembliesDirectory in TestAssembliesDirectories)
            {
                DotCoverArgs += @"
        <ScopeEntry>" + TestAssembliesDirectory + @"\*.dll</ScopeEntry>
        <ScopeEntry>" + TestAssembliesDirectory + @"\*.exe</ScopeEntry>";
            }
            DotCoverArgs += @"
    </Scope>
    <Filters>
        <ExcludeFilters>
            <FilterEntry>
                <ModuleMask>*.tests</ModuleMask>
                <ModuleMask>*.specs</ModuleMask>
            </FilterEntry>
        </ExcludeFilters>
        <AttributeFilters>
            <AttributeFilterEntry>
                <ClassMask>System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute</ClassMask>
            </AttributeFilterEntry>
        </AttributeFilters>
    </Filters>
</AnalyseParams>";
            var DotCoverRunnerXMLPath = Path.Combine(TestRunner.TestsResultsPath, JobName + " DotCover Runner.xml");
            CopyOnWrite(DotCoverRunnerXMLPath);
            File.WriteAllText(DotCoverRunnerXMLPath, DotCoverArgs);

            // Create full DotCover argument string.
            var DotCoverLogFile = TestRunner.TestsResultsPath + "\\DotCover.xml.log";
            CopyOnWrite(DotCoverLogFile);
            var FullArgsList = $" cover \"{DotCoverRunnerXMLPath}\" /LogFile=\"{DotCoverLogFile}\"";

            // Write DotCover Runner Batch File
            var DotCoverRunnerPath = $"{TestRunner.TestsResultsPath}\\Run {JobName} DotCover.bat";
            CopyOnWrite(DotCoverRunnerPath);
            File.WriteAllText(DotCoverRunnerPath, $"\"{DotCoverPath}\"{FullArgsList}");
            return DotCoverRunnerPath;
        }

        public string RunTests(string JobName, string TestAssembliesList, List<string> TestAssembliesDirectories, string TestSettingsFile, string TestRunnerPath)
        {
            var trxTestResultsFile = "";
            if (File.Exists(TestRunnerPath))
            {
                if (!string.IsNullOrEmpty(DoServerStart) || !string.IsNullOrEmpty(DoStudioStart) || !string.IsNullOrEmpty(DomywarewolfioStart))
                {
                    CleanupServerStudio();
                    Startmywarewolfio();
                    TryStartLocalCIRemoteContainer();
                    if (!string.IsNullOrEmpty(DoServerStart) || !string.IsNullOrEmpty(DoStudioStart))
                    {
                        StartServer();
                        if (!string.IsNullOrEmpty(DoStudioStart))
                        {
                            StartStudio();
                        }
                    }
                }
                if (ApplyDotCover && string.IsNullOrEmpty(DoServerStart) && string.IsNullOrEmpty(DoStudioStart))
                {
                    string DotCoverRunnerPath = DotCoverRunner(JobName, TestAssembliesDirectories);

                    // Run DotCover Runner Batch File
                    trxTestResultsFile = StartTestRunnerProcess(DotCoverRunnerPath);
                    if (!string.IsNullOrEmpty(DoServerStart) || !string.IsNullOrEmpty(DoStudioStart) || !string.IsNullOrEmpty(DomywarewolfioStart))
                    {
                        CleanupServerStudio(false);
                    }
                }
                else
                {
                    // Run Test Runner Batch File
                    trxTestResultsFile = StartTestRunnerProcess(TestRunnerPath);
                    if (!string.IsNullOrEmpty(DoServerStart) || !string.IsNullOrEmpty(DoStudioStart) || !string.IsNullOrEmpty(DomywarewolfioStart))
                    {
                        CleanupServerStudio(!ApplyDotCover);
                    }
                }
                MoveArtifactsToTestResults(ApplyDotCover, (!string.IsNullOrEmpty(DoServerStart) || !string.IsNullOrEmpty(DoStudioStart)), !string.IsNullOrEmpty(DoStudioStart));
            }
            return trxTestResultsFile;
        }

        string StartTestRunnerProcess(string TestRunnerPath)
        {
            ProcessStartInfo startinfo = new ProcessStartInfo();
            startinfo.FileName = TestRunnerPath;
            Process process = new Process();
            process.StartInfo = startinfo;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.Start();
            var trxFilePath = "";

            while (!process.StandardOutput.EndOfStream)
            {
                string testRunLine = process.StandardOutput.ReadLine();
                Console.WriteLine(testRunLine);
                if (testRunLine.StartsWith("Results File: "))
                {
                    trxFilePath = ParseTrxFilePath(testRunLine);
                }
            }
            process.WaitForExit();
            string allErrors = process.StandardError.ReadToEnd();
            Console.WriteLine(allErrors);
            return trxFilePath;
        }

        string ParseTrxFilePath(string standardOutput)
        {
            const string parseFrom = "Results File: ";
            int StartIndex = standardOutput.IndexOf(parseFrom) + parseFrom.Length;
            return standardOutput.Substring(StartIndex, standardOutput.Length-StartIndex);
        }

        void RecursiveFolderCopy(string sourceDir, string targetDir)
        {
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                File.Copy(file, Path.Combine(targetDir, Path.GetFileName(file)), true);
            }

            foreach (var directory in Directory.GetDirectories(sourceDir))
            {
                RecursiveFolderCopy(directory, Path.Combine(targetDir, Path.GetFileName(directory)));
            }
        }

        Process StartProcess(string exeName, string args)
        {
            var process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.FileName = exeName;
            process.StartInfo.Arguments = args;
            process.Start();
            process.WaitForExit();
            return process;
        }

        public void MergeDotCoverSnapshots()
        {
            var DotCoverSnapshots = Directory.GetFiles(MergeDotCoverSnapshotsInDirectory, "*.dcvr", SearchOption.AllDirectories).ToList();
            if (string.IsNullOrEmpty(JobName))
            {
                JobName = "DotCover";
            }
            var MergedSnapshotFileName = JobName.Split(',')[0];
            MergedSnapshotFileName = "Merged " + MergedSnapshotFileName + " Snapshots";
            MergeDotCoverSnapshots(DotCoverSnapshots, MergeDotCoverSnapshotsInDirectory + "\\" + MergedSnapshotFileName, MergeDotCoverSnapshotsInDirectory + "\\DotCover");
        }

        public void RunAllUnitTestJobs(int startIndex, int NumberOfUnitTestJobs)
        {
            JobName = string.Join(",", JobSpecs.Keys.ToList().GetRange(startIndex, NumberOfUnitTestJobs));
            RunTestJobs();
            CleanupServerStudio(ApplyDotCover);
        }

        public void RunAllServerTestJobs(int startIndex, int NumberOfServerTestJobs)
        {
            JobName = string.Join(",", JobSpecs.Keys.ToList().GetRange(startIndex, NumberOfServerTestJobs));
            ResourcesType = "ServerTests";
            DoServerStart = "true";
            RunTestJobs();
            CleanupServerStudio(ApplyDotCover);
        }

        public void RunAllReleaseResourcesTestJobs(int startIndex, int NumberOfReleaseResourcesTestJobs)
        {
            JobName = string.Join(",", JobSpecs.Keys.ToList().GetRange(startIndex, NumberOfReleaseResourcesTestJobs));
            ResourcesType = "Release";
            DoServerStart = "true";
            RunTestJobs();
            CleanupServerStudio(ApplyDotCover);
        }

        public void RunAllDesktopUITestJobs(int startIndex, int NumberOfDesktopUITestJobs)
        {
            JobName = string.Join(",", JobSpecs.Keys.ToList().GetRange(startIndex, NumberOfDesktopUITestJobs));
            ResourcesType = "UITests";
            DoStudioStart = "true";
            RunTestJobs();
            CleanupServerStudio(ApplyDotCover);
        }

        public void RunAllWebUITestJobs(int startIndex, int NumberOfWebUITestJobs)
        {
            JobName = string.Join(",", JobSpecs.Keys.ToList().GetRange(startIndex, NumberOfWebUITestJobs));
            DomywarewolfioStart = "true";
            RunTestJobs();
            CleanupServerStudio(ApplyDotCover);
        }

        public void RunAllLoadTestJobs(int startIndex, int NumberOfLoadTestJobs)
        {
            JobName = string.Join(",", JobSpecs.Keys.ToList().GetRange(startIndex, NumberOfLoadTestJobs));
            ResourcesType = "Load";
            DoStudioStart = "true";
            RunTestJobs();
            CleanupServerStudio(ApplyDotCover);
        }

        public void RunTestJobs(string jobName = "")
        {
            if (jobName != "")
            {
                JobName = jobName;
            }

            // Unpack jobs
            var JobNames = new List<string>();
            var JobAssemblySpecs = new List<string>();
            var JobCategories = new List<string>();
            if (!string.IsNullOrEmpty(JobName) && string.IsNullOrEmpty(MergeDotCoverSnapshotsInDirectory))
            {
                foreach (var Job in JobName.Split(','))
                {
                    var TrimJobName = Job.TrimEnd('1', '2', '3', '4', '5', '6', '7', '8', '9', '0', ' ');
                    if (JobSpecs.ContainsKey(TrimJobName))
                    {
                        JobNames.Add(TrimJobName);
                        if (JobSpecs[TrimJobName].Item2 == null)
                        {
                            JobAssemblySpecs.Add(JobSpecs[TrimJobName].Item1);
                            JobCategories.Add("");
                        }
                        else
                        {
                            JobAssemblySpecs.Add(JobSpecs[Job].Item1);
                            JobCategories.Add(JobSpecs[Job].Item2);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Unrecognized Job " + Job + " was ignored from the run");
                    }
                }
            }
            if (!string.IsNullOrEmpty(ProjectName))
            {
                JobNames.Add(ProjectName);
                JobAssemblySpecs.Add(ProjectName);
                if (!string.IsNullOrEmpty(Category))
                {
                    JobCategories.Add(Category);
                }
                else
                {
                    JobCategories.Add("");
                }
            }
            if (!string.IsNullOrEmpty(TestRunner.Path) && !File.Exists(TestRunner.Path))
            {
                if (File.Exists(TestRunner.Path.Replace("Enterprise", "Professional")))
                {
                    TestRunner.Path = TestRunner.Path.Replace("Enterprise", "Professional");
                }
                if (File.Exists(TestRunner.Path.Replace("Enterprise", "Community")))
                {
                    TestRunner.Path = TestRunner.Path.Replace("Enterprise", "Community");
                }
            }
            if (!File.Exists(TestRunner.Path))
            {
                throw new ArgumentException("Error cannot find VSTest.console.exe or MSTest.exe. Use either --TestRunner.Path or --MSTestPath parameters to pass paths to one of those files.");
            }

            if (ApplyDotCover && DotCoverPath != "" && !(File.Exists(DotCoverPath)))
            {
                throw new ArgumentException("Error cannot find dotcover.exe. Use -build.DotCoverPath parameter to pass a path to that file.");
            }

            if (File.Exists(Environment.ExpandEnvironmentVariables("%vs140comntools%..\\IDE\\CommonExtensions\\Microsoft\\TestWindow\\TestResults\\*.trx")))
            {
                File.Move(Environment.ExpandEnvironmentVariables("%vs140comntools%..\\IDE\\CommonExtensions\\Microsoft\\TestWindow\\TestResults\\*.trx"), TestRunner.TestsResultsPath);
                Console.WriteLine("Removed loose TRX files from VS install directory.");
            }

            if (!string.IsNullOrEmpty(DoServerStart) || !string.IsNullOrEmpty(DoStudioStart))
            {
                InstallServer();
            }

            TestRunner.ReadPlaylist();

            for (var i = 0; i < JobName.Split(',').Count(); i++)
            {
                var JobName = JobNames[i].ToString();
                var ProjectSpec = JobAssemblySpecs[i].ToString();
                var TestCategories = JobCategories[i].ToString();
                var TestAssembliesList = "";
                var TestAssembliesDirectories = new List<string>();
                if (!TestRunner.TestsPath.EndsWith("\\"))
                {
                    TestRunner.TestsPath += "\\";
                }
                foreach (var Project in ProjectSpec.Split(','))
                {
                    Tuple<string, List<string>> UnPackTestAssembliesListAndDirectories = ResolveTestAssemblyFileSpecs(TestRunner.TestsPath + Project + ".dll");
                    TestAssembliesList += UnPackTestAssembliesListAndDirectories.Item1;
                    if (UnPackTestAssembliesListAndDirectories.Item2.Count > 0)
                    {
                        TestAssembliesDirectories = TestAssembliesDirectories.Concat(UnPackTestAssembliesListAndDirectories.Item2).ToList();
                    }
                    if (TestAssembliesList == "")
                    {
                        UnPackTestAssembliesListAndDirectories = ResolveProjectFolderSpecs(TestRunner.TestsPath + Project);
                        TestAssembliesList += UnPackTestAssembliesListAndDirectories.Item1;
                        if (UnPackTestAssembliesListAndDirectories.Item2.Count > 0)
                        {
                            TestAssembliesDirectories = TestAssembliesDirectories.Concat(UnPackTestAssembliesListAndDirectories.Item2).ToList();
                        }
                    }
                }
                if (string.IsNullOrEmpty(TestAssembliesList) || string.IsNullOrEmpty(TestAssembliesList))
                {
                    throw new Exception("Cannot find any " + ProjectSpec + " project folders or assemblies at " + TestRunner.TestsPath + ".");
                }

                // Setup for screen recording
                var TestSettingsFile = ScreenRecordingTestSettingsFile(JobName);

                string TestRunnerPath = TestRunner.WriteTestRunner(JobName, ProjectSpec, TestCategories, TestAssembliesList, TestSettingsFile, TestRunner.TestsResultsPath, RecordScreen != null, JobSpecs);

                //Run Tests
                var TrxFile = RunTests(JobName, TestAssembliesList, TestAssembliesDirectories, TestSettingsFile, TestRunnerPath);

                //Re-try Failures
                for (var count = 0; count < RetryCount; count++)
                {
                    RetryTestFailures(JobName, TestAssembliesList, TestAssembliesDirectories, TestSettingsFile, TrxFile, count + 1);
                }
            }
            if (ApplyDotCover && JobName.Split(',').Count() > 1)
            {
                MergeDotCoverSnapshots();
            }
        }
    }
}
