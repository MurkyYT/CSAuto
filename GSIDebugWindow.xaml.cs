using Murky.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CSAuto
{
    /// <summary>
    /// Interaction logic for GSIDebugWindow.xaml
    /// </summary>
    public partial class GSIDebugWindow : Window
    {
        MainWindow main;
        public GSIDebugWindow(MainWindow main)
        {
            InitializeComponent();
            this.main = main;
        }
        public void UpdateText(string data)
        {
            this.Dispatcher.Invoke(() =>
            {
                outputBox.Text = data;
                lastRecieveTime.Text = $"Last recieved data from GSI: {DateTime.Now.Hour}:{DateTime.Now.Minute}:{DateTime.Now.Second}";
            });
           
        }
        public void UpdateDebug(string data)
        {
            this.Dispatcher.Invoke(() =>
            {
                debugBox.Text += data+'\n';
                debugBox.ScrollToEnd();
            });

        }

        private void debugWind_Closed(object sender, EventArgs e)
        {
            main.debugWind = null;
            Log.debugWind = null;
        }
        void ParseGameState(string JSON)
        {
            try
            {
                GameState gs = new GameState(JSON);
                Log.WriteLine("-----------------------------");
                foreach (PropertyInfo pi in gs.GetType().GetProperties())
                {
                    Log.WriteLine(
                        string.Format("{0} | {1}",
                                pi.Name,
                                pi.GetValue(gs, null)
                            )
                    );
                    Type type = pi.PropertyType;
                    switch (type.ToString())
                    {
                        case "CSAuto.Player":
                            ParsePlayerGSI(gs, pi);
                            break;
                        case "CSAuto.Match":
                            ParseMatchGSI(gs, pi);
                            break;
                        case "CSAuto.Round":
                            ParseRoundGSI(gs, pi);
                            break;
                    }
                    Log.WriteLine("-----------------------------");
                }
            }
            catch
            {
                Log.WriteLine("There was an error while parsing the GSI file, check if the file is correct");
            }
        }

        private static void ParseRoundGSI(GameState gs, PropertyInfo pi)
        {
            foreach (PropertyInfo pi2 in pi.PropertyType.GetProperties())
            {
                Log.WriteLine(
                    string.Format("{0} | {1}",
                            pi2.Name,
                            pi2.GetValue(TypeConvertor.ConvertPropertyInfoToOriginalType<Round>(pi, gs), null)
                        )
                );
            }
        }

        private static void ParseMatchGSI(GameState gs, PropertyInfo pi)
        {
            foreach (PropertyInfo pi2 in pi.PropertyType.GetProperties())
            {
                Log.WriteLine(
                    string.Format("{0} | {1}",
                            pi2.Name,
                            pi2.GetValue(TypeConvertor.ConvertPropertyInfoToOriginalType<Match>(pi, gs), null)
                        )
                );
            }
        }

        private static void ParsePlayerGSI(GameState gs, PropertyInfo pi)
        {
            Player player = TypeConvertor.ConvertPropertyInfoToOriginalType<Player>(pi, gs);
            foreach (PropertyInfo pi2 in pi.PropertyType.GetProperties())
            {
                Log.WriteLine(
                    string.Format("{0} | {1}",
                            pi2.Name,
                            pi2.GetValue(player, null)
                        )
                );
                Type type = pi2.PropertyType;
                switch (type.ToString())
                {
                    case "CSAuto.Weapon[]":
                        ParseWeaponsGSI(player);
                        break;
                }
            }
            
        }

        private static void ParseWeaponsGSI(Player player)
        {
            foreach (Weapon wep in player.Weapons)
            {
                foreach (PropertyInfo pi3 in wep.GetType().GetProperties())
                {
                    Log.WriteLine(
                        string.Format("{0} | {1}",
                                pi3.Name,
                                pi3.GetValue(wep, null)
                            )
                    );
                }
                Log.WriteLine("-----------------------------");
            }
        }

        private static void ParseWeaponGSI(Player player, PropertyInfo pi2)
        {
            foreach (PropertyInfo pi3 in pi2.PropertyType.GetProperties())
            {
                Log.WriteLine(
                    string.Format("Name: {0} | Value: {1}",
                            pi3.Name,
                            pi3.GetValue(TypeConvertor.ConvertPropertyInfoToOriginalType<Weapon>(pi2, player), null)
                        )
                );
            }
            Log.WriteLine("-----------------------------");
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                InitialDirectory = @"D:\",
                Title = "Browse Text Files",

                CheckFileExists = true,
                CheckPathExists = true,

                DefaultExt = "txt",
                Filter = "txt files (*.txt)|*.txt",
                FilterIndex = 2,
                RestoreDirectory = true,

                ReadOnlyChecked = true,
                ShowReadOnly = true
            };

            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ParseGameState(File.ReadAllText(openFileDialog1.FileName));
            }
        }

        private void SaveFile_Click(object sender, RoutedEventArgs e)
        {
            string strWorkPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string fileName = DateTime.Now.Day.ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Year.ToString() + $" - {DateTime.Now.ToString("HH;mm;ss")}.txt";
            string path = strWorkPath + "/DEBUG/GSI_OUTPUT/"+ fileName;
            if (outputBox.Text != "None")
            {
                Directory.CreateDirectory(strWorkPath + "/DEBUG/GSI_OUTPUT/");
                using (FileStream fs = File.Create(path))
                {
                    Byte[] title = new UTF8Encoding(true).GetBytes(outputBox.Text);
                    fs.Write(title, 0, title.Length);
                }
            }
        }
    }
}
