﻿using FRESHMusicPlayer.Handlers.Configuration;
using FRESHMusicPlayer.Handlers.Notifications;
using Squirrel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WinForms = System.Windows.Forms;

namespace FRESHMusicPlayer.Handlers
{
    public class UpdateHandler
    {
        public static async Task RealUpdateIfAvailable(bool useDeltaPatching = true)
        {
            if (App.Config.UpdateMode == UpdateMode.Manual) return;
            App.Config.UpdatesLastChecked = DateTime.Now;
            var mgr = await UpdateManager.GitHubUpdateManager("https://github.com/Royce551/FRESHMusicPlayer");
            var notification = new Notification();
            try
            {
                UpdateInfo updateInfo = await mgr.CheckForUpdate(!useDeltaPatching);
                if (updateInfo.CurrentlyInstalledVersion == null) return; // Standalone version of FMP, don't bother
                if (updateInfo.ReleasesToApply.Count == 0) return; // No updates to apply, don't bother

                notification.ContentText = $"Installing update...";
                MainWindow.NotificationHandler.Add(notification);

                await mgr.DownloadReleases(updateInfo.ReleasesToApply);
                await mgr.ApplyReleases(updateInfo);
                if (App.Config.UpdateMode == UpdateMode.Prompt)
                {
                    notification.ContentText = $"The update is ready to install!";
                    notification.ButtonText = "Restart now";
                    notification.Type = NotificationType.Success;
                    notification.OnButtonClicked = () =>
                    {
                        RestartApp();
                        return true;
                    };
                    MainWindow.NotificationHandler.Update(notification);
                }
            
                else RestartApp();
            }
            catch (Exception e)
            {
                if (useDeltaPatching)
                {
                    await RealUpdateIfAvailable(false);
                }
                notification.ContentText = $"An error occured when updating: {e.Message}";
                notification.Type = NotificationType.Failure;
                MainWindow.NotificationHandler.Update(notification);
            }
            finally
            {
                mgr.Dispose();
            }
        }
        private static void RestartApp()
        {
            WinForms.Application.Restart();
            Application.Current.Shutdown();
        }
    }
}