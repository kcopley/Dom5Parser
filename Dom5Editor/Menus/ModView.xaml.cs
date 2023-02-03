using Dom5Edit;
using Dom5Edit.Entities;
using Ookii.Dialogs.Wpf;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Dom5Editor
{
    /// <summary>
    /// Interaction logic for EditorMenuTab.xaml
    /// </summary>
    public partial class ModView : UserControl
    {
        private ModViewModel _vm;

        public ModView()
        {
            InitializeComponent();
            var m = VanillaLoader.Vanilla;
        }


        private void SaveModButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = GetSaveFileDialog();
            if (dialog.ShowDialog().GetValueOrDefault())
            {
                var result = dialog.FileName;
                _vm.Save(result);
            }
        }

        private void LoadModButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = GetOpenFileDialog();
            if (dialog.ShowDialog().GetValueOrDefault())
            {
                var result = dialog.FileName;
                _vm = new ModViewModel(result);
                this.DataContext = _vm;
            }
        }

        public VistaOpenFileDialog GetOpenFileDialog()
        {
            var dialog = new VistaOpenFileDialog();
            dialog.DefaultExt = ".dm";
            dialog.InitialDirectory = MergerMenuVM.GetDefaultFolderPath();
            return dialog;
        }

        public VistaSaveFileDialog GetSaveFileDialog()
        {
            var dialog = new VistaSaveFileDialog();
            dialog.DefaultExt = ".dm";
            dialog.InitialDirectory = MergerMenuVM.GetDefaultFolderPath();
            return dialog;
        }

        private void EditorMonsterList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var monster = (sender as ListBox).SelectedItem as MonsterViewModel;
            _vm.OpenMonster = monster;
        }

        private void Icon_File_Changed(object sender, TextChangedEventArgs e)
        {
            BindingExpression be = IconImage.GetBindingExpression(Image.SourceProperty);
            be.UpdateTarget();
        }

        private void OpenBanner_Click(object sender, RoutedEventArgs e)
        {
            var dialog = GetOpenFileDialog();
            if (dialog.ShowDialog().GetValueOrDefault())
            {
                var result = dialog.FileName;
                var filePath = Path.GetDirectoryName(_vm.FullFilePath);
                result = result.Replace(filePath, "");
                result = result.Trim('/').Trim('\\');
                IconText.Text = result;
            }
        }
    }
}
