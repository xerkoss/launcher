using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Arquitecture check
            if (!Environment.Is64BitOperatingSystem)
            {
                CheckBox64b.IsEnabled = false;
                CheckBox64b.IsChecked = false;
                CheckBox32b.IsEnabled = false;
                CheckBox32b.IsChecked = true;
            }
            else
            {
                CheckBox32b.IsChecked = false;
                CheckBox64b.IsChecked = true;
            }

            // Get version from server
            try
            {
                WebRequest wrGETURL = WebRequest.Create("http://www.terra-golfa.com/version.html");
                Stream objStream = wrGETURL.GetResponse().GetResponseStream();

                StreamReader objReader = new StreamReader(objStream);
                
                string sLine = "";

                while (sLine!=null)
                {
                    sLine = objReader.ReadLine();
                    if (sLine != null)
                        LabelSerVer.Content = sLine;
                    if (String.Compare(LabelSerVer.Content.ToString(), "") == 0)
                    {
                        LabelSerVer.Foreground = Brushes.Red;
                        LabelSerVer.Content = "No disponible";
                    }
                }
            }
            catch
            {
                MessageBox.Show("Revisa tu conexión a internet");
            }

            // Get version from client
            try
            {
                string FileName = CheckBox32b.IsChecked == true ? "wow.exe" : "wow-64.exe";
                System.Diagnostics.FileVersionInfo myFileVersionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(Environment.CurrentDirectory + "\\" + FileName);
                if (String.Compare(myFileVersionInfo.FileVersion, LabelSerVer.Content.ToString()) == 0)
                    LabelCliVer.Foreground = Brushes.Green;
                LabelCliVer.Content = myFileVersionInfo.FileVersion;
            }
            catch
            {
                //MessageBox.Show("Ruta de archivo " + Environment.CurrentDirectory + " invalida, por favor revisa que el launcher esta en la carpeta adecuada.");
                MessageBox.Show("Si tienes instalado un cliente WoD \"mueve\" este programa al directorio del cliente\".");
                LabelCliVer.Content = "No instalado";

                ButtonPlay.Background = (ImageBrush)Application.Current.Resources["BotonInstalar"];
            }
        }
        
        // download progress
        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progressbar1.Value = e.ProgressPercentage;
        }

        // download completed
        private void Completed(object sender, AsyncCompletedEventArgs e)
        {
            MessageBox.Show("Download completed!");
            try
            {
                System.Diagnostics.ProcessStartInfo instalacion = new System.Diagnostics.ProcessStartInfo();
                instalacion.UseShellExecute = true;
                instalacion.FileName = "World-of-Warcraft-Setup-esES.exe";
                instalacion.WorkingDirectory = Environment.CurrentDirectory;
                System.Diagnostics.Process.Start(instalacion);
            }
            catch { }
        }


        private void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[1024];
            while (true)
            {
                int read = input.Read(buffer, 0, buffer.Length);
                if (read <= 0)
                    return;
                output.Write(buffer, 0, read);
            }
        }

        // Button INSTALL / PLAY
        private void ButtonPlay_Click(object sender, RoutedEventArgs e)
        {
            // Installation
            if (ButtonPlay.Background == (ImageBrush)Application.Current.Resources["BotonInstalar"])
            {
                WebClient webClient = new WebClient();
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                webClient.DownloadFileAsync(new Uri("http://dist.blizzard.com/downloads/wow-installers/full/World-of-Warcraft-Setup-esES.exe"), Environment.CurrentDirectory + "\\World-of-Warcraft-Setup-esES.exe");
            }
            else // Play
            {
                // añadir los archivos faltantes
                object patcher = WpfApplication1.Properties.Resources.connection_patcher;
                byte[] myResBytes = (byte[])patcher;
                using (FileStream fsDst = new FileStream(Environment.CurrentDirectory + "//connection_patcher.exe", FileMode.Create, FileAccess.Write))
                {
                    byte[] bytes = myResBytes;
                    fsDst.Write(bytes, 0, bytes.Length);
                    //fsDst.Close();
                    //fsDst.Dispose();
                }

                object libeay = WpfApplication1.Properties.Resources.libeay32;
                byte[] myResBytes2 = (byte[])libeay;
                using (FileStream fsDst = new FileStream(Environment.CurrentDirectory + "//libeay32.dll", FileMode.Create, FileAccess.Write))
                {
                    byte[] bytes2 = myResBytes2;
                    fsDst.Write(bytes2, 0, bytes2.Length);
                    //fsDst.Close();
                    //fsDst.Dispose();
                }

                try // realmlist update check
                {
                    string[] wtf = File.ReadAllLines(Environment.CurrentDirectory + "//WTF//Config.wtf");
                    UInt16 i = 0;
                    while (wtf[i] != null)
                    {
                        if (wtf[i].Contains("SET portal"))
                        {
                            wtf[i] = ("SET portal \"logon.golfex.net\"");
                            break;
                        }
                        i++;
                    }

                    File.WriteAllLines(Environment.CurrentDirectory + "//WTF//Config.wtf", wtf);
                }
                catch { }

                string patchedname = CheckBox32b.IsChecked == true ? "wow_patched.exe" : "wow-64_patched.exe";

                progressbar1.Value = 100;

                System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo();
                info.UseShellExecute = true;
                info.FileName = patchedname;
                info.WorkingDirectory = Environment.CurrentDirectory;

                // Patch if necessary
                try
                {
                    System.Diagnostics.FileVersionInfo myFileVersionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(Environment.CurrentDirectory + "\\" + patchedname);

                    if (String.Compare(myFileVersionInfo.FileVersion, LabelCliVer.Content.ToString()) != 0)
                        throw new Exception();
                }
                catch
                {
                    try
                    {
                       

                        string FileName = CheckBox32b.IsChecked == true ? "wow.exe" : "wow-64.exe";

                        System.Diagnostics.Process process = new System.Diagnostics.Process();
                        System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                        startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                        startInfo.FileName = "cmd.exe";
                        startInfo.Arguments = "/c connection_patcher.exe " + FileName;
                        process.StartInfo = startInfo;
                        process.Start();
                        process.WaitForExit();
                    }
                    catch
                    {
                        MessageBox.Show("Falta el archivo connection_patcher");
                    }

                }
                try
                {
                    System.Diagnostics.Process.Start(info);
                }
                catch
                {
                    MessageBox.Show("Falta el archivo parcheado");
                }
            }
        }

        private void CheckBox32b_Checked(object sender, RoutedEventArgs e)
        {
            if (CheckBox64b.IsChecked == true)
                CheckBox64b.IsChecked = false;
        }

        private void CheckBox64b_Checked(object sender, RoutedEventArgs e)
        {
            if (CheckBox32b.IsChecked == true)
                CheckBox32b.IsChecked = false;
        }
    }
}
