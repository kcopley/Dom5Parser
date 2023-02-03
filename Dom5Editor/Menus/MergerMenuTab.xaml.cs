using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

namespace Dom5Editor
{
    /// <summary>
    /// Interaction logic for MergerMenuTab.xaml
    /// </summary>
    public partial class MergerMenuTab : UserControl
    {
        public MergerMenuTab()
        {
            InitializeComponent();
        }

        private MergerMenuVM _managerVM;
        public MergerMenuVM MergerMenuVM
        {
            get
            {
                if (_managerVM == null) _managerVM = new MergerMenuVM();
                return _managerVM;
            }
        }

        public override void EndInit()
        {
            base.EndInit();
            MergerMenuVM.SetModFolderPath();
            FolderPath.Text = MergerMenuVM.FolderPath;
            UpdateMods();
            UpdateDisabledNationsList();
            MergerMenuVM.Logging = this.LogErrors;
            MergerMenuVM.ToggleEA = this.DisableAllEA;
            MergerMenuVM.ToggleMA = this.DisableAllMA;
            MergerMenuVM.ToggleLA = this.DisableAllLA;
            MergerMenuVM.ToggleAllMods = this.ToggleAllMods;
            MergerMenuVM.UpdateModName(ModNameText.Text);
            this.ModExportedText.Text = "";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
        }

        private void UpdateFolderPath(object sender, TextChangedEventArgs e)
        {
            UpdateFolder();
            UpdateMods();
        }

        private void RefreshClick(object sender, RoutedEventArgs e)
        {
            UpdateFolder();
            UpdateMods();
        }

        private void UpdateFolder()
        {
            MergerMenuVM.FolderPath = FolderPath.Text;
        }

        private void UpdateMods()
        {
            if (MergerMenuVM.Mods.Count > 0)
            {
                ModsInFolderList.ItemsSource = MergerMenuVM.Mods;
            }
            else
            {
                ModsInFolderList.Items.Clear();
            }
        }

        private void UpdateDisabledNationsList()
        {
            DisableEANationList.ItemsSource = MergerMenuVM.EANations;
            DisableMANationList.ItemsSource = MergerMenuVM.MANations;
            DisableLANationList.ItemsSource = MergerMenuVM.LANations;
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void ModNameText_TextChanged(object sender, TextChangedEventArgs e)
        {
            MergerMenuVM.UpdateModName(ModNameText.Text);
        }

        private void On_ExitButtonClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void MergeExportButton_Click(object sender, RoutedEventArgs e)
        {
            var ret = MergerMenuVM.MergeAndExport();
            var dialog = new Ookii.Dialogs.Wpf.VistaSaveFileDialog();
            dialog.DefaultExt = ".dm";
            dialog.InitialDirectory = ret.FolderPath;
            if (dialog.ShowDialog().GetValueOrDefault())
            {
                ret.FullFilePath = dialog.FileName;
                ret.Export();
            }
            this.ModExportedText.Text = ret + " exported successfully";
        }

        private void ToggleAllMods_Checked(object sender, RoutedEventArgs e)
        {
            MergerMenuVM.ToggleMods(ToggleAllMods.IsChecked.GetValueOrDefault());
        }

        private void DisableAllEA_Checked(object sender, RoutedEventArgs e)
        {
            MergerMenuVM.ToggleEANations(DisableAllEA.IsChecked.GetValueOrDefault());
        }

        private void DisableAllMA_Checked(object sender, RoutedEventArgs e)
        {
            MergerMenuVM.ToggleMANations(DisableAllMA.IsChecked.GetValueOrDefault());
        }

        private void DisableAllLA_Checked(object sender, RoutedEventArgs e)
        {
            MergerMenuVM.ToggleLANations(DisableAllLA.IsChecked.GetValueOrDefault());
        }

        private void FolderDialog_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            dialog.RootFolder = Environment.SpecialFolder.Desktop;
            if (!string.IsNullOrEmpty(FolderPath.Text) && Directory.Exists(FolderPath.Text))
            {
                dialog.SelectedPath = FolderPath.Text;
            }
            if (dialog.ShowDialog().GetValueOrDefault())
            {
                FolderPath.Text = dialog.SelectedPath;
            }
        }
    }
}
