using Serilog;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace Pixel.Automation.Core
{
    public class Application
    {
        [NonSerialized]
        Process process;
        public Process Process
        {
            get
            {
                return process;
            }

            set
            {
                process = value;
            }
        }

        /// <summary>
        /// Name of the process
        /// </summary>
        public virtual string Name
        {
            get
            {
                try
                {
                    string processName = Process.ProcessName;
                    return processName;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, ex.Message);
                }
                return string.Empty;
            }
        }

        /// <summary>
        /// Returns whether process has exited
        /// </summary>
        public virtual bool HasExited
        {
            get
            {
                try
                {
                    return Process.HasExited;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, ex.Message);
                }
                return true;
            }
        }


        protected Application()
        {
        }

        private Application(Process process)
        {
            this.Process = process;
        }


        /// <summary>
        /// Runs the process identified by the executable and creates Application object for this executable
        /// </summary>
        /// <param name="executable">Path to the executable</param>
        /// <exception cref="ArgumentNullException">No process info passed</exception>      
        public static Application Launch(string executable)
        {
            var processStartInfo = new ProcessStartInfo(executable);
            return Launch(processStartInfo);
        }

        /// <summary>
        /// Lauches the process and creates and Application object for it
        /// </summary>
        /// <exception cref="ArgumentNullException">No process info passed</exception>
        /// <exception cref="WhiteException">White Failed to Launch or Attached to process</exception>
        public static Application Launch(ProcessStartInfo processStartInfo)
        {          
            Process process;
            try
            {
                process = Process.Start(processStartInfo);
            }
            catch (Win32Exception ex)
            {
                var error =
                    string.Format(
                        "[Failed Launching process:{0}] [Working directory:{1}] [Process full path:{2}] [Current Directory:{3}]",
                        processStartInfo.FileName,
                        new DirectoryInfo(processStartInfo.WorkingDirectory).FullName,
                        new FileInfo(processStartInfo.FileName).FullName,
                        Environment.CurrentDirectory);
                Log.Error(ex, error);
                throw;
            }
            return Attach(process);
        }

        /// <summary>
        /// Attaches White to an existing process by process id 
        /// </summary>      
        public static Application Attach(int processId)
        {
            Process process = Process.GetProcessById(processId);     
            return new Application(process);
        }

        /// <summary>
        /// Attaches White to an existing process
        /// </summary>
        /// <exception cref="WhiteException">White Failed to Attach to process</exception>
        public static Application Attach(Process process)
        {
            return new Application(process);
        }

        /// <summary>
        /// Attaches with existing process
        /// </summary>
        /// <exception cref="WhiteException">White Failed to Attach to process with specified name</exception>
        public static Application Attach(string executable)
        {
            Process[] processes = Process.GetProcessesByName(executable);
            if (processes.Length == 0) throw new ArgumentException("Could not find process named: " + executable);
            return Attach(processes[0]);
        }

        /// <summary>
        /// Attaches to the process if it is running or launches a new process
        /// </summary>
        /// <param name="processStartInfo"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="WhiteException">White Failed to Launch or Attach to process</exception>
        public static Application AttachOrLaunch(ProcessStartInfo processStartInfo)
        {
            string processName = ReplaceLast(processStartInfo.FileName, ".exe", string.Empty);
            processName = Path.GetFileName(processName);
            Process[] processes = Process.GetProcessesByName(processName);
            if (processes.Length == 0) return Launch(processStartInfo);
            return Attach(processes[0]);
        }

        private static string ReplaceLast(string replaceIn, string replace, string with)
        {
            int index = replaceIn.LastIndexOf(replace);
            if (index == -1) return replaceIn;
            return replaceIn.Substring(0, index) + with + replaceIn.Substring(index + replace.Length);
        }


        /// <summary>
        /// Tries to find the main window, then close it. If it hasn't closed in 5 seconds, kill the process
        /// </summary>
        public virtual void Close()
        {
            if (Process.HasExited)
            {
                Process.Dispose();
                return;
            }
            if(Process.CloseMainWindow())
            {
                Process.WaitForExit(5000);
            }
            if (!Process.HasExited)
            {
                Process.Kill();
                Process.WaitForExit(5000);
            }
            Process.Dispose();
        }

        /// <summary>
        /// Kills the applications and waits till it is closed
        /// </summary>
        public virtual void Kill()
        {
            try
            {
                if (Process.HasExited) return;
                Process.Kill();
                Process.WaitForExit();
                Process.Dispose();
            }
            catch { }
        }

    }
}
