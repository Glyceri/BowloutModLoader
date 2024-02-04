using BowloutModLoader.Enums;
using BowloutModLoader.JSON;
using BowloutModLoader.Utils;
using Microsoft.Win32;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BowloutModLoader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const string BowloutModManagerJson = "https://raw.githubusercontent.com/Glyceri/BowloutModManager/main/BowloutModManager.json";

        string modHandlerPath = null!;

        List<string> mods = new List<string>();

        List<string> finalMods = new List<string>();

        string harmonyPath = null!;
        string winhttpPath = null!;

        bool canNeverRun = false;

        public MainWindow()
        {
            InitializeComponent();
            textBox.Text = SteamPathFinder.GetPath(BaseString.GAME_NAME);
            path = textBox.Text!;

            try
            {
                DownloadJsonFile(BowloutModManagerJson, (jsonResponse) => DoParseJson<MainElement>(jsonResponse, (mainElement) => OnGetMainElement(mainElement), (error) => Trace.WriteLine(error)), (error) => Trace.WriteLine(error));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            try
            {
                if (File.Exists(Environment.CurrentDirectory + @"\winhttp.dll"))
                {
                    winhttpPath = Environment.CurrentDirectory + @"\winhttp.dll";
                }
                if (File.Exists(Environment.CurrentDirectory + @"\0Harmony.dll"))
                {
                    harmonyPath = Environment.CurrentDirectory + @"\0Harmony.dll";
                }

                Trace.WriteLine(harmonyPath);
                Trace.WriteLine(winhttpPath);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
            }

            if (winhttpPath == null || harmonyPath == null)
            {
                canNeverRun = true;
            }
        }

        void OnGetMainElement(MainElement mainElement)
        {
            Trace.WriteLine(mainElement.latest_url);
            Trace.WriteLine(mainElement.latest_version);

            DownloadDLL(mainElement.latest_url, "ModHandler", (succesPath) =>
            {
                modHandlerPath = succesPath;
                InvalidateVisual();
            },
            (error) =>
            {
                canNeverRun = true;
                InvalidateVisual();
            });

            foreach (string url in mainElement.recommended_mods)
            {
                DownloadJsonFile(url, (json) =>
                DoParseJson<ModJson>(json, (modJson) =>
                {
                    DownloadDLL(modJson.latest_url, "Mods", (successfullCallback) =>
                    {
                        mods.Add(successfullCallback);
                        finalMods.Add(successfullCallback);
                        InvalidateVisual();
                    },
                    (error) => Trace.WriteLine("Download DLL Failed: " + error));
                },
                (error) => Trace.WriteLine("Parse Json failed: " + error)),
                (error) => Trace.WriteLine("Json Download failed: " + error));
            }
        }

        async void DownloadDLL(string downloadURL, string subFolder, Action<string> successfullCallback, Action<string> errorCallback)
        {
            Trace.WriteLine(downloadURL);
            string directory = Environment.CurrentDirectory + @$"\Downloads\{subFolder}\";
            string filePath = directory + @"\" + Path.GetFileName(downloadURL);
            try
            {
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
                using HttpClient client = new HttpClient();
                using HttpResponseMessage response = await client.GetAsync(downloadURL);
                using FileStream fs = new FileStream(filePath, FileMode.Create);
                await response.Content.CopyToAsync(fs);
                successfullCallback?.Invoke(filePath);
            }
            catch (Exception e)
            {
                errorCallback?.Invoke(e.ToString());
                return;
            }
        }

        async void DownloadJsonFile(string downloadURL, Action<string> successfulCallback, Action<string> errorCallback)
        {
            try
            {
                using HttpClient client = new HttpClient();
                using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, downloadURL);
                using HttpResponseMessage response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                if (response == null)
                {
                    errorCallback?.Invoke("Response Null");
                    return;
                }

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    errorCallback?.Invoke("HTTP NOT FOUND!");
                    return;
                }

                string stringResponse = await response.Content.ReadAsStringAsync();

                if (stringResponse == null || stringResponse == string.Empty)
                {
                    errorCallback?.Invoke("stringResponse == null || empty");
                    return;
                }

                successfulCallback?.Invoke(stringResponse);
            }
            catch (Exception e)
            {
                errorCallback?.Invoke(e.ToString());
                return;
            }
        }

        void DoParseJson<T>(string json, Action<T> onSuccess, Action<string> onError)
        {
            Trace.WriteLine($"{json}");

            try
            {
                onSuccess?.Invoke(JsonConvert.DeserializeObject<T>(json)!);
            }
            catch (Exception e)
            {
                onError?.Invoke(e.ToString());
            }
        }

        bool isBowloutPath = false;
        string path = string.Empty;

        void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            isBowloutPath = File.Exists(textBox.Text + "\\BOWLOUT.exe");
            textBox.Background = isBowloutPath ? Brushes.Green : Brushes.Red;

            if (isBowloutPath) SetVisible();
            else SetInvisible();
        }

        void button_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog dialog = new OpenFolderDialog()
            {
                Title = "Bowlout Game Directory",
                Multiselect = false,
                InitialDirectory = textBox.Text == null ? Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + @"\steam" + BaseString.steamApps : textBox.Text
            };
            if (!dialog.ShowDialog() ?? true) return;

            path = dialog.FolderName;

            path = path.Replace("\\select.this.directory", "");
            path = path.Replace(".this.directory", "");

            textBox.Text = path;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (canNeverRun)
            {
                object label2 = FindName("InfoLabel1");
                if (label2 is not Label l2) return;
                l2.Content = "An Error Occured. Please restart the program and try again.\nIf this keeps happening, redownload the program.";

                object installButton = FindName("InstallButton");
                if (installButton is not Button iButton) return;
                iButton.Visibility = Visibility.Hidden;

                return;
            }

            object label = FindName("InfoLabel1");
            if (label is not Label l) return;
            l.Content = modHandlerPath == null ? "Mod loader is being downloaded. Please wait." : "Mod Loader Found. Please select mods to install.";

            object modList = FindName("modlist");
            if (modList is not ListView mList) return;
            mList.Items.Clear();
            foreach (string mod in mods)
            {
                CheckBox b = new CheckBox();
                b.IsChecked = finalMods.Contains(mod);
                b.Content = Path.GetFileName(mod);

                b.Unchecked += (object sender, RoutedEventArgs e) =>
                {
                    finalMods.Remove(mod);
                };

                b.Checked += (object sender, RoutedEventArgs e) =>
                {
                    finalMods.Add(mod);
                };

                mList.Items.Add(b);
            }
        }

        void SetVisible()
        {
            object panel = FindName("Download_Succeeded_Panel");
            if (panel is not StackPanel sPanel) return;
            sPanel.Visibility = Visibility.Visible;
        }

        void SetInvisible()
        {
            object panel = FindName("Download_Succeeded_Panel");
            if (panel is not StackPanel sPanel) return;
            sPanel.Visibility = Visibility.Hidden;
        }

        private void modList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                try
                {
                    File.Delete(path + @"\winhttp.dll");
                    string[] files = Directory.GetFiles(path + @"\Mods\");
                    foreach (string file in files)
                    {
                        File.Delete(file);
                    }

                    File.Delete(path + @"\BOWLOUT_Data\Managed\0Harmony.dll");
                    File.Delete(path + @"\BOWLOUT_Data\Managed\BowloutModManager.dll");
                }
                catch (Exception ee)
                {
                    Trace.WriteLine("Error deleting old files: " + ee.ToString());
                }

                try
                {
                    using FileStream fs = new FileStream(path + @"\doorstop_config.ini", FileMode.Create);
                    fs.Close();
                    using StreamWriter writer = new StreamWriter(path + @"\doorstop_config.ini");
                    writer.Write("[General]\r\nenabled=true\r\ntarget_assembly=BOWLOUT_Data\\managed\\BowloutModManager.dll");
                    writer.Close();
                }
                catch (Exception ee)
                {
                    Trace.WriteLine("Error Creating ini file: " + ee.ToString());
                }


                try
                {
                    File.Copy(winhttpPath, path + @"\winhttp.dll", true);
                    File.Copy(modHandlerPath, path + @"\BOWLOUT_Data\Managed\BowloutModManager.dll", true);
                    File.Copy(harmonyPath, path + @"\BOWLOUT_Data\Managed\0Harmony.dll", true);
                }
                catch (Exception ee)
                {
                    Trace.WriteLine("Copying core files went wrong: " + ee.ToString());
                }

                try
                {
                    Directory.CreateDirectory(path + @"\Mods\");
                    foreach(string finalMod in finalMods)
                    {
                        File.Copy(finalMod, path + @"\Mods\" + Path.GetFileName(finalMod), true);
                    }
                }
                catch (Exception ee)
                {
                    Trace.WriteLine("Copying mod files went wrong: " + ee.ToString());
                }
            }
            catch (Exception ee)
            {
                Trace.WriteLine(ee);
            }
        }
    }
}