using H1Emu_Launcher.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace H1Emu_Launcher.SettingsPages
{
    public partial class AssetPacks : Page
    {
        public static AssetPacks assetPacksInstance;

        public AssetPacks()
        {
            InitializeComponent();
            assetPacksInstance = this;

            // Adds the correct language file to the resource dictionary and then loads it
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());
        }

        private void AssetPacksInitialized(object sender, EventArgs e)
        {
            LoadAssetPacks();
        }

        private void LoadAssetPacks()
        {
            if (!File.Exists(LauncherWindow.assetPacksJsonFile) || string.IsNullOrEmpty(File.ReadAllText(LauncherWindow.assetPacksJsonFile)))
                File.WriteAllText(LauncherWindow.assetPacksJsonFile, "[]");

            try
            {
                // Load all of the asset packs into the asset pack selector
                List<LauncherWindow.AssetPackList> currentJson = JsonSerializer.Deserialize<List<LauncherWindow.AssetPackList>>(File.ReadAllText(LauncherWindow.assetPacksJsonFile));
                foreach (LauncherWindow.AssetPackList server in currentJson)
                {
                    ComboBoxItem newItem = new()
                    {
                        Content = server.AssetPackName,
                        Style = (Style)FindResource("ComboBoxItemStyle")
                    };
                    assetPacksBox.Items.Insert(assetPacksBox.Items.Count - 1, newItem);
                }

                if (assetPacksBox.Items.Count > 3)
                {
                    Separator separator = new()
                    {
                        Style = (Style)FindResource("SeparatorMenuItem"),
                        Background = new SolidColorBrush(Color.FromRgb(66, 66, 66))
                    };
                    assetPacksBox.Items.Insert(assetPacksBox.Items.Count - 1, separator);
                }

                // Add an event for only user added asset packs in the list to delete on right click
                for (int i = 0; i <= assetPacksBox.Items.Count - 1; i++)
                {
                    if (assetPacksBox.Items[i] is ComboBoxItem serverItem && i > 1 && i < assetPacksBox.Items.Count - 2)
                    {
                        ContextMenu itemContextMenu = new()
                        {
                            Style = (Style)FindResource("ContextMenuStyle")
                        };
                        serverItem.ContextMenu = itemContextMenu;

                        MenuItem editOptionCustom = new()
                        {
                            Style = (Style)FindResource("CustomMenuItem"),
                            Margin = new Thickness(0, 0, 0, 6)
                        };
                        System.Windows.Shapes.Path pathCustom = new()
                        {
                            Data = (PathGeometry)FindResource("EditIcon"),
                            Stretch = Stretch.Uniform
                        };
                        Binding bindingCustom = new("Foreground")
                        {
                            Source = editOptionCustom,
                            Mode = BindingMode.OneWay
                        };
                        BindingOperations.SetBinding(pathCustom, System.Windows.Shapes.Path.FillProperty, bindingCustom);

                        editOptionCustom.Icon = pathCustom;
                        editOptionCustom.SetResourceReference(HeaderedItemsControl.HeaderProperty, "item212");
                        editOptionCustom.Click += (s, e) => { EditAssetPack(serverItem); };

                        Separator separator = new()
                        {
                            Style = (Style)FindResource("SeparatorMenuItem"),
                            Background = new SolidColorBrush(Color.FromRgb(66, 66, 66)),
                            Margin = new Thickness(10, 2, 10, 10)
                        };

                        MenuItem deleteOptionCustom = new()
                        {
                            Style = (Style)FindResource("CustomMenuItem"),
                        };
                        System.Windows.Shapes.Path pathDeleteCustom = new()
                        {
                            Data = (PathGeometry)FindResource("BinIcon"),
                            Stretch = Stretch.Uniform
                        };
                        Binding bindingDeleteCustom = new("Foreground")
                        {
                            Source = deleteOptionCustom,
                            Mode = BindingMode.OneWay
                        };
                        BindingOperations.SetBinding(pathDeleteCustom, System.Windows.Shapes.Path.FillProperty, bindingDeleteCustom);

                        deleteOptionCustom.Icon = pathDeleteCustom;
                        deleteOptionCustom.SetResourceReference(HeaderedItemsControl.HeaderProperty, "item192");
                        deleteOptionCustom.Click += (s, e) => { DeleteAssetPack(serverItem); };

                        itemContextMenu.Items.Add(editOptionCustom);
                        itemContextMenu.Items.Add(separator);
                        itemContextMenu.Items.Add(deleteOptionCustom);
                    }
                }
            }
            catch (Exception e)
            {
                CustomMessageBox.Show($"{FindResource("item184")} \"{e.Message}\".", SettingsWindow.settingsInstance);
            }

            assetPacksBox.SelectedIndex = Properties.Settings.Default.selectedAssetPack;
        }

        public async void EditAssetPack(ComboBoxItem assetPackItem)
        {
            List<LauncherWindow.AssetPackList> assetPackJson = JsonSerializer.Deserialize<List<LauncherWindow.AssetPackList>>(File.ReadAllText(LauncherWindow.assetPacksJsonFile));
            for (int i = assetPackJson.Count - 1; i >= 0; i--)
            {
                if (assetPackJson[i].AssetPackName == (string)assetPackItem.Content)
                {
                    AddItemWindow editServer = new()
                    {
                        Owner = SettingsWindow.settingsInstance,
                        itemType = 2
                    };
                    editServer.primaryTextbox.Text = assetPackJson[i].AssetPackName;
                    editServer.secondaryTextbox.Text = assetPackJson[i].AssetPackURL;
                    editServer.primaryTextboxHint.Visibility = Visibility.Hidden;
                    editServer.secondaryTextboxHint.Visibility = Visibility.Hidden;
                    editServer.saveServerButton.SetResourceReference(ContentProperty, "item213");
                    editServer.editIndex = i;

                    await Task.Run(() =>
                    {
                        Dispatcher.Invoke(new Action(delegate
                        {
                            editServer.ShowDialog();
                        }));
                    });

                    assetPacksBox.IsDropDownOpen = false;
                    break;
                }
            }
        }

        public void DeleteAssetPack(ComboBoxItem assetPackItem)
        {
            MessageBoxResult mbr = CustomMessageBox.Show(FindResource("item230").ToString(), SettingsWindow.settingsInstance, false, true, true);
            if (mbr != MessageBoxResult.Yes)
                return;

            // Delete the server from the custom servers file list
            List<LauncherWindow.AssetPackList> assetPackJson = JsonSerializer.Deserialize<List<LauncherWindow.AssetPackList>>(File.ReadAllText(LauncherWindow.assetPacksJsonFile));
            for (int i = assetPackJson.Count - 1; i >= 0; i--)
            {
                if (assetPackJson[i].AssetPackName == (string)assetPackItem.Content)
                {
                    assetPackJson.Remove(assetPackJson[i]);
                    if (assetPacksBox.SelectedItem == assetPackItem)
                    {
                        if (i + 3 < assetPacksBox.Items.Count - 1)
                            assetPacksBox.SelectedIndex = i + 3;
                        else
                            assetPacksBox.SelectedIndex = i + 1;
                    }
                    break;
                }
            }

            string newJson = JsonSerializer.Serialize(assetPackJson, LauncherWindow.jsonSerializerOptions);
            File.WriteAllText(LauncherWindow.assetPacksJsonFile, newJson);

            assetPacksBox.Items.Remove(assetPackItem);

            if (assetPacksBox.Items.Count == 4)
                assetPacksBox.Items.RemoveAt(assetPacksBox.Items.Count - 2);
        }

        private void AddAssetPackClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            AddItemWindow addAssetPack = new()
            {
                Owner = SettingsWindow.settingsInstance,
                itemType = 2
            };
            addAssetPack.ShowDialog();
        }

        private void AssetPacksBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!assetPacksBox.IsLoaded)
                return;

            if (assetPacksBox.SelectedIndex == assetPacksBox.Items.Count - 1 ||
                assetPacksBox.SelectedIndex == assetPacksBox.Items.Count - 2 ||
                assetPacksBox.SelectedIndex == 1 || assetPacksBox.SelectedIndex == -1)
            {
                assetPacksBox.SelectedIndex = 0;
            }

            Properties.Settings.Default.selectedAssetPack = assetPacksBox.SelectedIndex;
            Properties.Settings.Default.Save();
        }
    }
}
