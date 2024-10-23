using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using H1EmuLauncher.Classes;

namespace H1EmuLauncher
{
    public partial class AddServerWindow : Window
    {
        public static AddServerWindow addServerInstance;
        public int editIndex;

        public AddServerWindow()
        {
            InitializeComponent();
            addServerInstance = this;
            Owner = LauncherWindow.launcherInstance;

            // Adds the correct language file to the resource dictionary and then loads it.
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(SetLanguageFile.LoadFile());
        }

        private void AddServerTopBar(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        public void FillInFields(string name, string ip)
        {
            serverNameBox.Text = name;
            serverIpBox.Text = ip;
            serverNameHint.Visibility = Visibility.Hidden;
            serverIpHint.Visibility = Visibility.Hidden;
        }

        private void ServerIpBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if ((string)saveServerButton.Content == FindResource("item18").ToString())
                    AddNewServer();
                else
                    EditExistingServer(editIndex);
            }
        }

        private void AddButton(object sender, RoutedEventArgs e)
        {
            if ((string)saveServerButton.Content == FindResource("item18").ToString())
                AddNewServer();
            else
                EditExistingServer(editIndex);
        }

        private void AddNewServer()
        {
            if (string.IsNullOrEmpty(serverNameBox.Text) || string.IsNullOrEmpty(serverIpBox.Text))
            {
                CustomMessageBox.Show(FindResource("item151").ToString(), this);
                return;
            }

            if (serverNameBox.Text.Trim() == FindResource("item139").ToString() || serverNameBox.Text.Trim() == FindResource("item140").ToString() || serverNameBox.Text.Trim() == FindResource("item141").ToString())
            {
                CustomMessageBox.Show(FindResource("item143").ToString(), this);
                return;
            }

            List<LauncherWindow.ServerList> currentJson = JsonSerializer.Deserialize<List<LauncherWindow.ServerList>>(File.ReadAllText(LauncherWindow.customServersJsonFile));
            foreach (LauncherWindow.ServerList item in currentJson)
            {
                if (item.CustomServerName == serverNameBox.Text.Trim())
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
                    CustomServerName = serverNameBox.Text.Trim(),
                    CustomServerIp = serverIpBox.Text.Trim().Replace(" ", "")
                });

                string newJson = JsonSerializer.Serialize(currentJson, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(LauncherWindow.customServersJsonFile, newJson);

                ComboBoxItem newItem = new();
                newItem.Content = serverNameBox.Text.Trim();
                newItem.Style = (Style)FindResource("ComboBoxItemStyle");
                newItem.PreviewMouseRightButtonUp += LauncherWindow.launcherInstance.ItemRightMouseButtonUp;

                ContextMenu newItemContextMenu = new()
                {
                    Style = (Style)FindResource("ContextMenuStyle")
                };
                newItem.ContextMenu = newItemContextMenu;

                MenuItem editOption = new()
                {
                    Style = (Style)FindResource("CustomMenuItem"),
                    Icon = new Image { Source = new BitmapImage(new Uri("pack://application:,,,/H1EmuLauncher;component/Resources/Edit.png", UriKind.Absolute)) }
                };
                editOption.SetResourceReference(HeaderedItemsControl.HeaderProperty, "item212");
                editOption.Click += LauncherWindow.launcherInstance.EditServerInfo;

                MenuItem deleteOption = new()
                {
                    Style = (Style)FindResource("CustomMenuItem"),
                    Icon = new Image { Source = new BitmapImage(new Uri("pack://application:,,,/H1EmuLauncher;component/Resources/Delete.png", UriKind.Absolute)) }
                };
                deleteOption.SetResourceReference(HeaderedItemsControl.HeaderProperty, "item192");
                deleteOption.Click += LauncherWindow.launcherInstance.DeleteServerFromList;
                newItemContextMenu.Items.Add(editOption);
                newItemContextMenu.Items.Add(deleteOption);

                int insertIndex = LauncherWindow.launcherInstance.serverSelector.Items.Count - 1;
                if (LauncherWindow.launcherInstance.serverSelector.Items.Count > 5)
                    insertIndex--;

                LauncherWindow.launcherInstance.serverSelector.Items.Insert(insertIndex, newItem);

                if (LauncherWindow.launcherInstance.serverSelector.Items.Count == 5)
                {
                    Separator separator = new()
                    {
                        Style = (Style)FindResource("SeparatorMenuItem"),
                        Background = new SolidColorBrush(Color.FromRgb(66, 66, 66))
                    };
                    LauncherWindow.launcherInstance.serverSelector.Items.Insert(LauncherWindow.launcherInstance.serverSelector.Items.Count - 1, separator);
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
            if (string.IsNullOrEmpty(serverNameBox.Text) || string.IsNullOrEmpty(serverIpBox.Text))
            {
                CustomMessageBox.Show(FindResource("item151").ToString(), this);
                return;
            }

            if (serverNameBox.Text.Trim() == FindResource("item139").ToString() || serverNameBox.Text.Trim() == FindResource("item140").ToString() || serverNameBox.Text.Trim() == FindResource("item141").ToString())
            {
                CustomMessageBox.Show(FindResource("item143").ToString(), this);
                return;
            }

            try
            {
                List<LauncherWindow.ServerList> currentJson = JsonSerializer.Deserialize<List<LauncherWindow.ServerList>>(File.ReadAllText(LauncherWindow.customServersJsonFile));
                List<LauncherWindow.ServerList> currentJsonRecent = JsonSerializer.Deserialize<List<LauncherWindow.ServerList>>(File.ReadAllText(LauncherWindow.recentServersJsonFile));
                for (int i = currentJsonRecent.Count - 1; i >= 0; i--)
                {
                    if (currentJsonRecent[i].CustomServerName == currentJson[editIndex].CustomServerName)
                    {
                        currentJsonRecent[i].CustomServerName = serverNameBox.Text.Trim();
                        currentJsonRecent[i].CustomServerIp = serverIpBox.Text.Trim().Replace(" ", "");

                        MenuItem notifyIconContextMenuItemToEdit = (MenuItem)LauncherWindow.notifyIconContextMenu.Items[i + 3];
                        notifyIconContextMenuItemToEdit.Header = serverNameBox.Text.Trim();
                    }
                }

                currentJson[editIndex].CustomServerName = serverNameBox.Text.Trim();
                currentJson[editIndex].CustomServerIp = serverIpBox.Text.Trim().Replace(" ", "");

                string newJson = JsonSerializer.Serialize(currentJson, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(LauncherWindow.customServersJsonFile, newJson);

                string newJsonRecent = JsonSerializer.Serialize(currentJsonRecent, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(LauncherWindow.recentServersJsonFile, newJsonRecent);

                ComboBoxItem comboBoxItem = (ComboBoxItem)LauncherWindow.launcherInstance.serverSelector.Items[editIndex + 3];
                comboBoxItem.Content = serverNameBox.Text.Trim();
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

        private void ServerNameGotFocus(object sender, RoutedEventArgs e)
        {
            serverNameHint.Visibility = Visibility.Hidden;
        }

        private void ServerNameLostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(serverNameBox.Text))
                serverNameHint.Visibility = Visibility.Visible;
        }

        private void ServerIPGotFocus(object sender, RoutedEventArgs e)
        {
            serverIpHint.Visibility = Visibility.Hidden;
        }

        private void ServerIPLostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(serverIpBox.Text))
                serverIpHint.Visibility = Visibility.Visible;
        }

        private void AddServerContentRendered(object sender, EventArgs e)
        {
            SizeToContent = SizeToContent.Manual;
            SizeToContent = SizeToContent.WidthAndHeight;
        }

        private void CloseAddServer(object sender, RoutedEventArgs e)
        {
            CustomMessageBox.buttonPressed = MessageBoxResult.Cancel;
            Topmost = true;
            Close();
        }

        private void AddServerLoaded(object sender, RoutedEventArgs e)
        {
            FocusEffects.BeginUnfocusAnimation(Owner);
        }

        private void AddServerClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            editIndex = 0;
            addServerInstance = null;
            FocusEffects.BeginFocusAnimation(Owner);
            Owner.Show();
            Owner.Activate();
        }
    }
}