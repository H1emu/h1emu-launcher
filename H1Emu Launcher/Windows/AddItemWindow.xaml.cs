using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using H1Emu_Launcher.Classes;
using H1Emu_Launcher.SettingsPages;

namespace H1Emu_Launcher
{
    public partial class AddItemWindow : Window
    {
        public static AddItemWindow addItemWindowInstance;
        public bool editItem;
        public int editIndex;

        // 1 = Server
        // 2 = Asset Pack
        public int itemType;

        public AddItemWindow()
        {
            InitializeComponent();
            addItemWindowInstance = this;

            // Adds the correct language file to the resource dictionary and then loads it
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());
        }

        private void AddServerTopBar(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        public void FillInFields(string primaryText, string secondaryText)
        {
            primaryTextBox.Text = primaryText;
            secondaryTextBox.Text = secondaryText;
            primaryTextBoxHint.Visibility = Visibility.Hidden;
            secondaryTextBoxHint.Visibility = Visibility.Hidden;
        }

        private void secondaryTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                switch (itemType)
                {
                    case 1:
                        if (!editItem)
                            AddNewServer();
                        else
                            EditExistingServer(editIndex);
                        break;
                    case 2:
                        if (!editItem)
                            AddNewAssetPack();
                        else
                            EditExistingAssetPack(editIndex);
                        break;
                }
            }
        }

        private void AddButton(object sender, RoutedEventArgs e)
        {
            switch (itemType)
            {
                case 1:
                    if (!editItem)
                        AddNewServer();
                    else
                        EditExistingServer(editIndex);
                    break;
                case 2:
                    if (!editItem)
                        AddNewAssetPack();
                    else
                        EditExistingAssetPack(editIndex);
                    break;
            }
        }

        private void AddNewServer()
        {
            if (string.IsNullOrEmpty(primaryTextBox.Text))
            {
                CustomMessageBox.Show(FindResource("item151").ToString(), this);
                return;
            }

            if (string.IsNullOrEmpty(secondaryTextBox.Text))
            {
                CustomMessageBox.Show(FindResource("item161").ToString(), this);
                return;
            }

            if (primaryTextBox.Text.Trim() == FindResource("item139").ToString() || primaryTextBox.Text.Trim() == FindResource("item140").ToString() || primaryTextBox.Text.Trim() == FindResource("item141").ToString())
            {
                CustomMessageBox.Show(FindResource("item143").ToString(), this);
                return;
            }

            List<LauncherWindow.ServerList> currentJson = JsonSerializer.Deserialize<List<LauncherWindow.ServerList>>(File.ReadAllText(LauncherWindow.customServersJsonFile));
            foreach (LauncherWindow.ServerList item in currentJson)
            {
                if (item.CustomServerName == primaryTextBox.Text.Trim())
                {
                    CustomMessageBox.Show(FindResource("item143").ToString(), this);
                    return;
                }
            }

            try
            {
                // Save the new custom server to the custom servers file list
                currentJson.Add(new LauncherWindow.ServerList()
                {
                    CustomServerName = primaryTextBox.Text.Trim(),
                    CustomServerIp = secondaryTextBox.Text.Trim().Replace(" ", "")
                });

                string newJson = JsonSerializer.Serialize(currentJson, LauncherWindow.jsonSerializerOptions);
                File.WriteAllText(LauncherWindow.customServersJsonFile, newJson);

                ComboBoxItem newItem = new()
                {
                    Content = primaryTextBox.Text.Trim(),
                    Style = (Style)FindResource("ComboBoxItemStyle")
                };

                ContextMenu newItemContextMenu = new()
                {
                    Style = (Style)FindResource("ContextMenuStyle")
                };
                newItem.ContextMenu = newItemContextMenu;

                MenuItem editOption = new()
                {
                    Style = (Style)FindResource("CustomMenuItem"),
                };
                System.Windows.Shapes.Path pathEdit = new()
                {
                    Data = (PathGeometry)FindResource("EditIcon"),
                    Stretch = Stretch.Uniform
                };
                Binding bindingEdit = new("Foreground")
                {
                    Source = editOption,
                    Mode = BindingMode.OneWay
                };
                BindingOperations.SetBinding(pathEdit, System.Windows.Shapes.Path.FillProperty, bindingEdit);

                editOption.Icon = pathEdit;
                editOption.SetResourceReference(HeaderedItemsControl.HeaderProperty, "item212");
                editOption.Click += (s, e) => { LauncherWindow.launcherInstance.EditServer(newItem); };

                Separator contextMenuSeparatorSeparator = new()
                {
                    Style = (Style)FindResource("SeparatorMenuItem"),
                    Background = new SolidColorBrush(Color.FromRgb(66, 66, 66)),
                    Margin = new Thickness(10, 2, 10, 10)
                };

                MenuItem deleteOption = new()
                {
                    Style = (Style)FindResource("CustomMenuItem"),
                };
                System.Windows.Shapes.Path pathDelete = new()
                {
                    Data = (PathGeometry)FindResource("BinIcon"),
                    Stretch = Stretch.Uniform
                };
                Binding bindingDelete = new("Foreground")
                {
                    Source = deleteOption,
                    Mode = BindingMode.OneWay
                };
                BindingOperations.SetBinding(pathDelete, System.Windows.Shapes.Path.FillProperty, bindingDelete);

                deleteOption.Icon = pathDelete;
                deleteOption.SetResourceReference(HeaderedItemsControl.HeaderProperty, "item192");
                deleteOption.Click += (s, e) => { LauncherWindow.launcherInstance.DeleteServer(newItem); };

                newItemContextMenu.Items.Add(editOption);
                newItemContextMenu.Items.Add(contextMenuSeparatorSeparator);
                newItemContextMenu.Items.Add(deleteOption);

                int insertIndex = LauncherWindow.launcherInstance.serverSelector.Items.Count - 1;
                if (LauncherWindow.launcherInstance.serverSelector.Items.Count > 5)
                    insertIndex--;

                LauncherWindow.launcherInstance.serverSelector.Items.Insert(insertIndex, newItem);

                if (LauncherWindow.launcherInstance.serverSelector.Items.Count == 5)
                {
                    Separator serverSelectorSeparator = new()
                    {
                        Style = (Style)FindResource("SeparatorMenuItem"),
                        Background = new SolidColorBrush(Color.FromRgb(66, 66, 66))
                    };
                    LauncherWindow.launcherInstance.serverSelector.Items.Insert(LauncherWindow.launcherInstance.serverSelector.Items.Count - 1, serverSelectorSeparator);
                }

                LauncherWindow.launcherInstance.serverSelector.SelectedIndex = insertIndex;
            }
            catch (Exception e)
            {
                CustomMessageBox.Show($"{FindResource("item142")} {e.Message}", this);
            }

            Topmost = true;
            Close();
        }

        private void EditExistingServer(int editIndex)
        {
            if (string.IsNullOrEmpty(primaryTextBox.Text) || string.IsNullOrEmpty(secondaryTextBox.Text))
            {
                CustomMessageBox.Show(FindResource("item151").ToString(), this);
                return;
            }

            if (primaryTextBox.Text.Trim() == FindResource("item139").ToString() || primaryTextBox.Text.Trim() == FindResource("item140").ToString() || primaryTextBox.Text.Trim() == FindResource("item141").ToString())
            {
                CustomMessageBox.Show(FindResource("item143").ToString(), this);
                return;
            }

            List<LauncherWindow.ServerList> currentJson = JsonSerializer.Deserialize<List<LauncherWindow.ServerList>>(File.ReadAllText(LauncherWindow.customServersJsonFile));
            List<LauncherWindow.ServerListRecent> currentJsonRecent = JsonSerializer.Deserialize<List<LauncherWindow.ServerListRecent>>(File.ReadAllText(LauncherWindow.recentServersJsonFile));
            foreach (LauncherWindow.ServerList item in currentJson)
            {
                if (item.CustomServerName == primaryTextBox.Text.Trim())
                {
                    CustomMessageBox.Show(FindResource("item143").ToString(), this);
                    return;
                }
            }

            foreach (LauncherWindow.ServerListRecent item in currentJsonRecent)
            {
                if (item.CustomServerNameRecent == primaryTextBox.Text.Trim())
                {
                    CustomMessageBox.Show(FindResource("item143").ToString(), this);
                    return;
                }
            }

            try
            {
                for (int i = currentJsonRecent.Count - 1; i >= 0; i--)
                {
                    if (currentJsonRecent[i].CustomServerNameRecent == currentJson[editIndex].CustomServerName)
                    {
                        currentJsonRecent[i].CustomServerNameRecent = primaryTextBox.Text.Trim();

                        MenuItem notifyIconContextMenuItemToEdit = (MenuItem)LauncherWindow.notifyIconContextMenu.Items[i + 3];
                        notifyIconContextMenuItemToEdit.Header = primaryTextBox.Text.Trim();
                        break;
                    }
                }

                currentJson[editIndex].CustomServerName = primaryTextBox.Text.Trim();
                currentJson[editIndex].CustomServerIp = secondaryTextBox.Text.Trim().Replace(" ", "");

                ComboBoxItem comboBoxItem = (ComboBoxItem)LauncherWindow.launcherInstance.serverSelector.Items[editIndex + 3];
                comboBoxItem.Content = primaryTextBox.Text.Trim();

                string newJson = JsonSerializer.Serialize(currentJson, LauncherWindow.jsonSerializerOptions);
                File.WriteAllText(LauncherWindow.customServersJsonFile, newJson);

                string newJsonRecent = JsonSerializer.Serialize(currentJsonRecent, LauncherWindow.jsonSerializerOptions);
                File.WriteAllText(LauncherWindow.recentServersJsonFile, newJsonRecent);
            }
            catch (Exception e)
            {
                CustomMessageBox.Show($"{FindResource("item142")} {e.Message}", this);
            }

            Topmost = true;
            Close();
        }

        private void AddNewAssetPack()
        {
            if (string.IsNullOrEmpty(primaryTextBox.Text))
            {
                CustomMessageBox.Show(FindResource("item233").ToString(), this);
                return;
            }

            if (string.IsNullOrEmpty(secondaryTextBox.Text))
            {
                CustomMessageBox.Show(FindResource("item234").ToString(), this);
                return;
            }

            if (primaryTextBox.Text.Trim() == FindResource("item226").ToString() || primaryTextBox.Text.Trim() == FindResource("item227").ToString())
            {
                CustomMessageBox.Show(FindResource("item235").ToString(), this);
                return;
            }

            List<LauncherWindow.AssetPackList> assetPackJson = JsonSerializer.Deserialize<List<LauncherWindow.AssetPackList>>(File.ReadAllText(LauncherWindow.assetPacksJsonFile));
            foreach (LauncherWindow.AssetPackList item in assetPackJson)
            {
                if (item.AssetPackName == primaryTextBox.Text.Trim())
                {
                    CustomMessageBox.Show(FindResource("item235").ToString(), this);
                    return;
                }
            }

            try
            {
                // Save the new custom server to the custom servers file list
                assetPackJson.Add(new LauncherWindow.AssetPackList()
                {
                    AssetPackName = primaryTextBox.Text.Trim(),
                    AssetPackURL = secondaryTextBox.Text.Trim().Replace(" ", "")
                });

                string newJson = JsonSerializer.Serialize(assetPackJson, LauncherWindow.jsonSerializerOptions);
                File.WriteAllText(LauncherWindow.assetPacksJsonFile, newJson);

                ComboBoxItem newItem = new()
                {
                    Content = primaryTextBox.Text.Trim(),
                    Style = (Style)FindResource("ComboBoxItemStyle")
                };

                ContextMenu newItemContextMenu = new()
                {
                    Style = (Style)FindResource("ContextMenuStyle")
                };
                newItem.ContextMenu = newItemContextMenu;

                MenuItem editOption = new()
                {
                    Style = (Style)FindResource("CustomMenuItem"),
                };
                System.Windows.Shapes.Path pathEdit = new()
                {
                    Data = (PathGeometry)FindResource("EditIcon"),
                    Stretch = Stretch.Uniform
                };
                Binding bindingEdit = new("Foreground")
                {
                    Source = editOption,
                    Mode = BindingMode.OneWay
                };
                BindingOperations.SetBinding(pathEdit, System.Windows.Shapes.Path.FillProperty, bindingEdit);

                editOption.Icon = pathEdit;
                editOption.SetResourceReference(HeaderedItemsControl.HeaderProperty, "item212");
                editOption.Click += (s, e) => { AssetPacks.assetPacksInstance.EditAssetPack(newItem); };

                Separator contextMenuSeparatorSeparator = new()
                {
                    Style = (Style)FindResource("SeparatorMenuItem"),
                    Background = new SolidColorBrush(Color.FromRgb(66, 66, 66)),
                    Margin = new Thickness(10, 2, 10, 10)
                };

                MenuItem deleteOption = new()
                {
                    Style = (Style)FindResource("CustomMenuItem"),
                };
                System.Windows.Shapes.Path pathDelete = new()
                {
                    Data = (PathGeometry)FindResource("BinIcon"),
                    Stretch = Stretch.Uniform
                };
                Binding bindingDelete = new("Foreground")
                {
                    Source = deleteOption,
                    Mode = BindingMode.OneWay
                };
                BindingOperations.SetBinding(pathDelete, System.Windows.Shapes.Path.FillProperty, bindingDelete);

                deleteOption.Icon = pathDelete;
                deleteOption.SetResourceReference(HeaderedItemsControl.HeaderProperty, "item192");
                deleteOption.Click += (s, e) => { AssetPacks.assetPacksInstance.DeleteAssetPack(newItem); };

                newItemContextMenu.Items.Add(editOption);
                newItemContextMenu.Items.Add(contextMenuSeparatorSeparator);
                newItemContextMenu.Items.Add(deleteOption);

                int insertIndex = AssetPacks.assetPacksInstance.assetPacksBox.Items.Count - 1;
                if (AssetPacks.assetPacksInstance.assetPacksBox.Items.Count > 4)
                    insertIndex--;

                AssetPacks.assetPacksInstance.assetPacksBox.Items.Insert(insertIndex, newItem);

                if (AssetPacks.assetPacksInstance.assetPacksBox.Items.Count == 4)
                {
                    Separator serverSelectorSeparator = new()
                    {
                        Style = (Style)FindResource("SeparatorMenuItem"),
                        Background = new SolidColorBrush(Color.FromRgb(66, 66, 66))
                    };
                    AssetPacks.assetPacksInstance.assetPacksBox.Items.Insert(AssetPacks.assetPacksInstance.assetPacksBox.Items.Count - 1, serverSelectorSeparator);
                }

                AssetPacks.assetPacksInstance.assetPacksBox.SelectedIndex = insertIndex;
            }
            catch (Exception e)
            {
                CustomMessageBox.Show($"{FindResource("item142")} {e.Message}", this);
            }

            Topmost = true;
            Close();
        }

        private void EditExistingAssetPack(int editIndex)
        {
            if (string.IsNullOrEmpty(primaryTextBox.Text) || string.IsNullOrEmpty(secondaryTextBox.Text))
            {
                CustomMessageBox.Show(FindResource("item233").ToString(), this);
                return;
            }

            if (primaryTextBox.Text.Trim() == FindResource("item139").ToString() || primaryTextBox.Text.Trim() == FindResource("item140").ToString() || primaryTextBox.Text.Trim() == FindResource("item141").ToString())
            {
                CustomMessageBox.Show(FindResource("item235").ToString(), this);
                return;
            }

            List<LauncherWindow.AssetPackList> assetPackJson = JsonSerializer.Deserialize<List<LauncherWindow.AssetPackList>>(File.ReadAllText(LauncherWindow.assetPacksJsonFile));
            foreach (LauncherWindow.AssetPackList item in assetPackJson)
            {
                if (item.AssetPackName == primaryTextBox.Text.Trim())
                {
                    CustomMessageBox.Show(FindResource("item235").ToString(), this);
                    return;
                }
            }

            try
            {
                assetPackJson[editIndex].AssetPackName = primaryTextBox.Text.Trim();
                assetPackJson[editIndex].AssetPackURL = secondaryTextBox.Text.Trim().Replace(" ", "");

                ComboBoxItem comboBoxItem = (ComboBoxItem)AssetPacks.assetPacksInstance.assetPacksBox.Items[editIndex + 2];
                comboBoxItem.Content = primaryTextBox.Text.Trim();

                string newJson = JsonSerializer.Serialize(assetPackJson, LauncherWindow.jsonSerializerOptions);
                File.WriteAllText(LauncherWindow.assetPacksJsonFile, newJson);
            }
            catch (Exception e)
            {
                CustomMessageBox.Show($"{FindResource("item142")} {e.Message}", this);
            }

            Topmost = true;
            Close();
        }

        private void CancelButton(object sender, RoutedEventArgs e)
        {
            Topmost = true;
            Close();
        }

        private void PrimaryTextBoxGotFocus(object sender, RoutedEventArgs e)
        {
            primaryTextBoxHint.Visibility = Visibility.Hidden;
        }

        private void PrimaryTextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(primaryTextBox.Text))
                primaryTextBoxHint.Visibility = Visibility.Visible;
        }

        private void SecondaryTextBoxGotFocus(object sender, RoutedEventArgs e)
        {
            secondaryTextBoxHint.Visibility = Visibility.Hidden;
        }

        private void secondaryTextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(secondaryTextBox.Text))
                secondaryTextBoxHint.Visibility = Visibility.Visible;
        }

        private void CloseAddItemWindow(object sender, RoutedEventArgs e)
        {
            CustomMessageBox.buttonPressed = MessageBoxResult.Cancel;
            Topmost = true;
            Close();
        }

        private void AddItemWindowLoaded(object sender, RoutedEventArgs e)
        {
            FocusEffects.BeginUnfocusAnimation(Owner);

            switch (itemType)
            {
                case 1:
                    primaryTextBoxHint.Text = LauncherWindow.launcherInstance.FindResource("item144").ToString();
                    secondaryTextBoxHint.Text = LauncherWindow.launcherInstance.FindResource("item145").ToString();
                    Title = LauncherWindow.launcherInstance.FindResource("item207").ToString();
                    break;
                case 2:
                    primaryTextBoxHint.Text = LauncherWindow.launcherInstance.FindResource("item228").ToString();
                    secondaryTextBoxHint.Text = LauncherWindow.launcherInstance.FindResource("item229").ToString();
                    Title = LauncherWindow.launcherInstance.FindResource("item231").ToString();
                    break;
            }

            if (!editItem)
                addOrSaveButton.Content = LauncherWindow.launcherInstance.FindResource("item18").ToString();
            else
                addOrSaveButton.Content = LauncherWindow.launcherInstance.FindResource("item213").ToString();
        }

        private void AddItemWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            editIndex = 0;
            addItemWindowInstance = null;
            FocusEffects.BeginFocusAnimation(Owner);
            Owner.Show();
            Owner.Activate();
        }
    }
}