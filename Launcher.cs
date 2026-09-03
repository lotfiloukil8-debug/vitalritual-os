using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Forms;

namespace VitalRitual
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string appDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "VitalRitual_OS");
                if (!Directory.Exists(appDataDir))
                {
                    Directory.CreateDirectory(appDataDir);
                }

                string htmlPath = Path.Combine(appDataDir, "index.html");
                bool extracted = false;

                // 1. Extract latest embedded index.html resource
                try
                {
                    Assembly assembly = Assembly.GetExecutingAssembly();
                    using (Stream stream = assembly.GetManifestResourceStream("index.html"))
                    {
                        if (stream != null)
                        {
                            using (FileStream fs = new FileStream(htmlPath, FileMode.Create, FileAccess.Write))
                            {
                                stream.CopyTo(fs);
                            }
                            extracted = true;
                        }
                    }
                }
                catch {}

                // Copy adjacent video/videos folders to appDataDir if available
                try
                {
                    string[] videoDirs = new string[] { "video", "videos" };
                    foreach (string vd in videoDirs)
                    {
                        string srcDir = Path.Combine(baseDir, vd);
                        if (Directory.Exists(srcDir))
                        {
                            string destDir = Path.Combine(appDataDir, vd);
                            if (!Directory.Exists(destDir)) Directory.CreateDirectory(destDir);
                            foreach (string file in Directory.GetFiles(srcDir))
                            {
                                string destFile = Path.Combine(destDir, Path.GetFileName(file));
                                if (!File.Exists(destFile) || new FileInfo(file).Length != new FileInfo(destFile).Length)
                                {
                                    File.Copy(file, destFile, true);
                                }
                            }
                        }
                    }
                }
                catch {}

                // 2. Fallback to adjacent local index.html if extraction was not available
                if (!extracted)
                {
                    string localHtml = Path.Combine(baseDir, "index.html");
                    if (File.Exists(localHtml))
                    {
                        htmlPath = localHtml;
                    }
                }

                if (!File.Exists(htmlPath))
                {
                    MessageBox.Show(
                        "Impossible de trouver l'application VitalRitual OS.",
                        "VitalRitual OS",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    return;
                }

                string userDataDir = Path.Combine(appDataDir, "UserData");
                if (!Directory.Exists(userDataDir))
                {
                    Directory.CreateDirectory(userDataDir);
                }

                string[] possibleBrowsers = new string[]
                {
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Microsoft\Edge\Application\msedge.exe"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Microsoft\Edge\Application\msedge.exe"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Microsoft\Edge\Application\msedge.exe"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Google\Chrome\Application\chrome.exe"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Google\Chrome\Application\chrome.exe"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Google\Chrome\Application\chrome.exe"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"BraveSoftware\Brave-Browser\Application\brave.exe")
                };

                string browserExe = null;
                foreach (string b in possibleBrowsers)
                {
                    if (File.Exists(b))
                    {
                        browserExe = b;
                        break;
                    }
                }

                string fileUri = new Uri(htmlPath).AbsoluteUri;

                if (!string.IsNullOrEmpty(browserExe))
                {
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName = browserExe;
                    psi.Arguments = string.Format(
                        "--app=\"{0}#desktop_app\" --user-data-dir=\"{1}\" --window-size=1360,880 --enable-features=OverlayScrollbar --disable-features=Translate",
                        fileUri,
                        userDataDir
                    );
                    psi.UseShellExecute = false;
                    Process.Start(psi);
                }
                else
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = fileUri + "#desktop_app",
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Erreur lors du lancement:\n" + ex.Message,
                    "VitalRitual OS",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }
}
