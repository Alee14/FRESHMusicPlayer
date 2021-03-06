﻿using ATL.Playlist;
using FRESHMusicPlayer.Utilities;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FRESHMusicPlayer.Forms.Playlists
{
    /// <summary>
    /// Interaction logic for PlaylistEntry.xaml
    /// </summary>
    public partial class PlaylistEntry : UserControl
    {
        private readonly bool trackExists = true;
        private readonly string playlist;
        private readonly string path;
        public PlaylistEntry(string playlist, string path)
        {
            InitializeComponent();
            if (path is null) trackExists = false;
            TitleLabel.Text = playlist;
            this.playlist = playlist;
            this.path = path;
            CheckIfPlaylistExists();
        }
        private void CheckIfPlaylistExists()
        {
            foreach (var thing in DatabaseUtils.ReadTracksForPlaylist(playlist))
            {
                if (thing.Path == path)
                {
                    AddButton.IsEnabled = false;
                    RemoveButton.IsEnabled = true;
                    return;
                }
            }
            AddButton.IsEnabled = true;
            RemoveButton.IsEnabled = false;
        }
        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            RenameButton.Visibility = DeleteButton.Visibility = ExportButton.Visibility = Visibility.Visible;
            if (trackExists)
            {
                AddButton.Visibility = RemoveButton.Visibility = Visibility.Visible;
            }
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            AddButton.Visibility = RemoveButton.Visibility = RenameButton.Visibility = DeleteButton.Visibility = ExportButton.Visibility = Visibility.Collapsed;
        }

        private void RenameButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FMPTextEntryBox("Playlist Name", playlist);
            dialog.ShowDialog();
            if (dialog.OK)
            {
                var x = MainWindow.Libraryv2.GetCollection<DatabasePlaylist>("playlists").FindOne(y => y.Name == playlist);
                x.Name = dialog.Response;
                TitleLabel.Text = dialog.Response;
                MainWindow.Libraryv2.GetCollection<DatabasePlaylist>("playlists").Update(x);
            }
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            DatabaseUtils.RemoveTrackFromPlaylist(playlist, path);
            CheckIfPlaylistExists();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            DatabaseUtils.AddTrackToPlaylist(playlist, path);
            CheckIfPlaylistExists();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e) => DatabaseUtils.DeletePlaylist(playlist);

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            var tracks = DatabaseUtils.ReadTracksForPlaylist(playlist);
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "M3U UTF-8 Playlist|*.m3u8|Other|*";
            if (saveFileDialog.ShowDialog() == true)
            {
                IPlaylistIO pls = PlaylistIOFactory.GetInstance().GetPlaylistIO(saveFileDialog.FileName);
                IList<string> pathsToWrite = new List<string>();
                foreach (var track in tracks)
                {
                    pathsToWrite.Add(track.Path);
                }
                pls.FilePaths = pathsToWrite;
            }
        }

        
    }
}
