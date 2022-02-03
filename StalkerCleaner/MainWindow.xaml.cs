using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using Ookii.Dialogs.Wpf;
using StalkerCleaner.Annotations;
using StalkerCleaner.Services;

namespace StalkerCleaner
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public string GamedataPath { get; set; }

        private CleanService _cleanService;
        private ProgressDialog progressDialog = new ProgressDialog()
        {
            WindowTitle = "КЛИНИНГ",
            Description = "Все будет хорошо..",
            ShowTimeRemaining = true,
        };

        public MainWindow()
        {
            _cleanService = new CleanService();
            InitializeComponent();

            progressDialog.DoWork += new DoWorkEventHandler(_sampleProgressDialog_DoWork);
        }

        private void SelectFolderClick(object sender, RoutedEventArgs e)
        {
            ShowFolderBrowserDialog();
        }
        
        private void ShowFolderBrowserDialog()
        {
            var dialog = new VistaFolderBrowserDialog
            {
                Description = "Выберите папку gamedata", 
                UseDescriptionForTitle = true
            };

            // ReSharper disable once PossibleInvalidOperationException
            if ((bool) dialog.ShowDialog(this))
            {
                GamedataPath = dialog.SelectedPath;
                OnPropertyChanged(nameof(GamedataPath));
                _cleanService.InitFileService(GamedataPath);
                _dialogComboBox.IsEnabled = true;
                _showDialogButton.IsEnabled = true;
            }
        }

        private void SelectTaskClick(object sender, RoutedEventArgs e)
        {
            switch (_dialogComboBox.SelectedIndex)
            {
                case 0:
                    ShowProgressDialogClearDirectories();
                    break;
                case 1:
                    ShowProgressDialogClearScripts();
                    break;
                case 2:
                    ShowProgressDialogClearConfigs();
                    break;
                case 3:
                    ShowProgressDialogClearSounds();
                    break;
                case 4:
                    ShowFolderBrowserDialog();
                    break;
                case 5:
                    ShowOpenFileDialog();
                    break;
                case 6:
                    ShowSaveFileDialog();
                    break;
            }
        }

        private void ShowTaskDialog()
        {
            if (TaskDialog.OSSupportsTaskDialogs)
            {
                using (TaskDialog dialog = new TaskDialog())
                {
                    dialog.WindowTitle = "Task dialog sample";
                    dialog.MainInstruction = "This is an example task dialog.";
                    dialog.Content =
                        "Task dialogs are a more flexible type of message box. Among other things, task dialogs support custom buttons, command links, scroll bars, expandable sections, radio buttons, a check box (useful for e.g. \"don't show this again\"), custom icons, and a footer. Some of those things are demonstrated here.";
                    dialog.ExpandedInformation =
                        "Ookii.org's Task Dialog doesn't just provide a wrapper for the native Task Dialog API; it is designed to provide a programming interface that is natural to .Net developers.";
                    dialog.Footer =
                        "Task Dialogs support footers and can even include <a href=\"http://www.ookii.org\">hyperlinks</a>.";
                    dialog.FooterIcon = TaskDialogIcon.Information;
                    dialog.EnableHyperlinks = true;
                    TaskDialogButton customButton = new TaskDialogButton("A custom button");
                    TaskDialogButton okButton = new TaskDialogButton(ButtonType.Ok);
                    TaskDialogButton cancelButton = new TaskDialogButton(ButtonType.Cancel);
                    dialog.Buttons.Add(customButton);
                    dialog.Buttons.Add(okButton);
                    dialog.Buttons.Add(cancelButton);
                    dialog.HyperlinkClicked += new EventHandler<HyperlinkClickedEventArgs>(TaskDialog_HyperLinkClicked);
                    TaskDialogButton button = dialog.ShowDialog(this);
                    if (button == customButton)
                        MessageBox.Show(this, "You clicked the custom button", "Task Dialog Sample");
                    else if (button == okButton)
                        MessageBox.Show(this, "You clicked the OK button.", "Task Dialog Sample");
                }
            }
            else
            {
                MessageBox.Show(this, "This operating system does not support task dialogs.", "Task Dialog Sample");
            }
        }

        private void ShowTaskDialogWithCommandLinks()
        {
            if (TaskDialog.OSSupportsTaskDialogs)
            {
                using (TaskDialog dialog = new TaskDialog())
                {
                    dialog.WindowTitle = "Task dialog sample";
                    dialog.MainInstruction = "This is a sample task dialog with command links.";
                    dialog.Content =
                        "Besides regular buttons, task dialogs also support command links. Only custom buttons are shown as command links; standard buttons remain regular buttons.";
                    dialog.ButtonStyle = TaskDialogButtonStyle.CommandLinks;
                    TaskDialogButton elevatedButton = new TaskDialogButton("An action requiring elevation");
                    elevatedButton.CommandLinkNote =
                        "Both regular buttons and command links can show the shield icon to indicate that the action they perform requires elevation. It is up to the application to actually perform the elevation.";
                    elevatedButton.ElevationRequired = true;
                    TaskDialogButton otherButton = new TaskDialogButton("Some other action");
                    TaskDialogButton cancelButton = new TaskDialogButton(ButtonType.Cancel);
                    dialog.Buttons.Add(elevatedButton);
                    dialog.Buttons.Add(otherButton);
                    dialog.Buttons.Add(cancelButton);
                    dialog.ShowDialog(this);
                }
            }
            else
            {
                MessageBox.Show(this, "This operating system does not support task dialogs.", "Task Dialog Sample");
            }
        }

        private void ShowProgressDialogClearDirectories()
        {
            progressDialog.Text = "Очищаем пустые папки..";
            if (progressDialog.IsBusy)
                MessageBox.Show(this, "The progress dialog is already displayed.", "Progress dialog sample");
            else
            {
                progressDialog.Show();
                var deletedDirectories =  _cleanService.ClearDirectories(GamedataPath);
                progressDialog.Dispose();
                MessageBox.Show(this, $"Удалили пустые папки:\n{string.Join("\n", deletedDirectories)}", "Удаленные папки");
            }
        }
        
        private void ShowProgressDialogClearScripts()
        {
            progressDialog.Text = "Очищаем скрипты..";
            if (progressDialog.IsBusy)
                MessageBox.Show(this, "The progress dialog is already displayed.", "Progress dialog sample");
            else
            {
                progressDialog.Show();
                var deletedScripts =  _cleanService.ClearScripts();
                progressDialog.Dispose();
                MessageBox.Show(this, $"Удалили мусорные скрипты:\n{string.Join("\n", deletedScripts)}", "Удаленные скрипты");
            }
        }
        
        private void ShowProgressDialogClearConfigs()
        {
            progressDialog.Text = "Очищаем конфиги..";
            if (progressDialog.IsBusy)
                MessageBox.Show(this, "The progress dialog is already displayed.", "Progress dialog sample");
            else
            {
                progressDialog.Show();
                var deletedConfigs =  _cleanService.ClearConfigs();
                progressDialog.Dispose();
                MessageBox.Show(this, $"Удалили мусорные конфиги:\n{string.Join("\n", deletedConfigs)}", "Удаленные конфиги");
            }
        }
        
        private void ShowProgressDialogClearSounds()
        {
            progressDialog.Text = "Очищаем звуки..";
            if (progressDialog.IsBusy)
                MessageBox.Show(this, "The progress dialog is already displayed.", "Progress dialog sample");
            else
            {
                progressDialog.Show();
                var deletedConfigs =  _cleanService.ClearSounds();
                progressDialog.Dispose();
                MessageBox.Show(this, $"Удалили мусорные звуки:\n{string.Join("\n", deletedConfigs)}", "Удаленные звуки");
            }
        }

        private void ShowCredentialDialog()
        {
            using (CredentialDialog dialog = new CredentialDialog())
            {
                // The window title will not be used on Vista and later; there the title will always be "Windows Security".
                dialog.WindowTitle = "Credential dialog sample";
                dialog.MainInstruction = "Please enter your username and password.";
                dialog.Content =
                    "Since this is a sample the credentials won't be used for anything, so you can enter anything you like.";
                dialog.ShowSaveCheckBox = true;
                dialog.ShowUIForSavedCredentials = true;
                // The target is the key under which the credentials will be stored.
                // It is recommended to set the target to something following the "Company_Application_Server" pattern.
                // Targets are per user, not per application, so using such a pattern will ensure uniqueness.
                dialog.Target = "Ookii_DialogsWpfSample_www.example.com";
                if (dialog.ShowDialog(this))
                {
                    MessageBox.Show(this,
                        string.Format("You entered the following information:\nUser name: {0}\nPassword: {1}",
                            dialog.Credentials.UserName, dialog.Credentials.Password), "Credential dialog sample");
                    // Normally, you should verify if the credentials are correct before calling ConfirmCredentials.
                    // ConfirmCredentials will save the credentials if and only if the user checked the save checkbox.
                    dialog.ConfirmCredentials(true);
                }
            }
        }

        private void ShowOpenFileDialog()
        {
            // As of .Net 3.5 SP1, WPF's Microsoft.Win32.OpenFileDialog class still uses the old style
            VistaOpenFileDialog dialog = new VistaOpenFileDialog();
            dialog.Filter = "All files (*.*)|*.*";
            if (!VistaFileDialog.IsVistaFileDialogSupported)
                MessageBox.Show(this,
                    "Because you are not using Windows Vista or later, the regular open file dialog will be used. Please use Windows Vista to see the new dialog.",
                    "Sample open file dialog");
            if ((bool) dialog.ShowDialog(this))
                MessageBox.Show(this, "The selected file was: " + dialog.FileName, "Sample open file dialog");
        }

        private void ShowSaveFileDialog()
        {
            VistaSaveFileDialog dialog = new VistaSaveFileDialog();
            dialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            dialog.DefaultExt = "txt";
            // As of .Net 3.5 SP1, WPF's Microsoft.Win32.SaveFileDialog class still uses the old style
            if (!VistaFileDialog.IsVistaFileDialogSupported)
                MessageBox.Show(this,
                    "Because you are not using Windows Vista or later, the regular save file dialog will be used. Please use Windows Vista to see the new dialog.",
                    "Sample save file dialog");
            if ((bool) dialog.ShowDialog(this))
                MessageBox.Show(this, "The selected file was: " + dialog.FileName, "Sample save file dialog");
        }

        private void TaskDialog_HyperLinkClicked(object sender, HyperlinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Href);
        }

        private void _sampleProgressDialog_DoWork(object sender, DoWorkEventArgs e)
        {
            // Implement the operation that the progress bar is showing progress of here, same as you would do with a background worker.
            for (int x = 0; x <= 100; ++x)
            {
                Thread.Sleep(500);
                // Periodically check CancellationPending and abort the operation if required.
                if (progressDialog.CancellationPending)
                    return;
                // ReportProgress can also modify the main text and description; pass null to leave them unchanged.
                // If _sampleProgressDialog.ShowTimeRemaining is set to true, the time will automatically be calculated based on
                // the frequency of the calls to ReportProgress.
                progressDialog.ReportProgress(x, null,
                    string.Format(System.Globalization.CultureInfo.CurrentCulture, "Ждемс..: {0}%", x));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}