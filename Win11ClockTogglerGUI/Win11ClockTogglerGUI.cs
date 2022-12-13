﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Resources;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Win11ClockToggler;

namespace Win11ClockTogglerGUI
{
    public partial class Win11ClockTogglerGUI : Form
    {
        //Allows to register HotKeys globally in Windows (not included in Win11ClockTogglerLib 
        //because is specific to this app, and not the CLI one)
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);
        // Catch the WM_HOTKEY message to handle any hotkeys being pressed
        // https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-hotkey
        const int WM_HOTKEY = 0x0312;

        private bool IsDirty = false;
        private List<IntPtr> CurrentMonitoredControls = new List<IntPtr>();
        private string LatestVersion;
        //Registry keys to save the latest status of the checks
        private readonly string REG_CHKNOTIFAREA_STATUS = "chkNotifArea_Status";
        private readonly string REG_CHKALLLDISPLAYS_STATUS = "chkAllDisplays_Status";
        //Internal IDs for the global hot keys (associated with this window)
        private static int TOGGLE_KEY_ID = 1;
        private static int STEALTH_KEY_ID = 2;
        private bool isShown = false;
        private DateTime lastMouseOvered = DateTime.MinValue;
        public Win11ClockTogglerGUI()
        {
            InitializeComponent();

            // Register hotkeys
            int keyModifiers = 0x008 + 0x004; // Win + Shift
            RegisterHotKey(this.Handle, TOGGLE_KEY_ID, keyModifiers, (int)Keys.F6);
            RegisterHotKey(this.Handle, STEALTH_KEY_ID, keyModifiers, (int)Keys.F7);
        }




        private void CheckBoxes_Paint(object sender, PaintEventArgs e)
        {
            CheckBox current = ((CheckBox)sender);
            //Draw border
            ControlPaint.DrawBorder(e.Graphics, current.ClientRectangle, Color.Black, ButtonBorderStyle.Solid);
        }

        private void DisableCheckBox(CheckBox chkBox)
        {
            chkBox.Enabled = false;
            chkBox.Parent.ForeColor = Theme.DisabledForeground;
            chkBox.Parent.BackColor = Theme.DisabledPanelBackground;
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // Process any messages sent to this window
        protected override void WndProc(ref Message m)
        {
            bool passThroughMsg = true;

            if (m.Msg == WM_HOTKEY)
            {
                int id = m.WParam.ToInt32();
                if (id == TOGGLE_KEY_ID)
                {
                    btnHideShow_Click(null, null);
                }
                else if (id == STEALTH_KEY_ID)
                {
                    toggleStealthMode();
                }
            }

            // Catch the WM_SYSCOMMAND message
            // https://docs.microsoft.com/en-us/windows/win32/menurc/wm-syscommand
            const int WM_SYSCOMMAND = 0x0112;
            if (m.Msg == WM_SYSCOMMAND)
            {
                // When the window is being minimized (SC_MINIMIZE)
                const int SC_MINIMIZE = 0xf020;
                if (m.WParam.ToInt32() == SC_MINIMIZE)
                {
                    passThroughMsg = false; // To prevent the regular minimize behaviour
                    toggleStealthMode();
                }
            }

            if (passThroughMsg)
            {
                base.WndProc(ref m);
            }
        }

        private void ShowClockElements()
        {

            //By default, hide at least the clock
            Helper.TaskbarElement tbeToToggle = Helper.TaskbarElement.Clock;
            //If the user wants, toggle the full notification area
            if (NotificationAreaToggle.Checked) tbeToToggle = Helper.TaskbarElement.FullNotificationArea;
            Helper.ShowTaskbarElements(tbeToToggle);

            IsDirty = false;
            ExitText.Text = "Exit";
            pnlCheckBoxes.Enabled = true;
            //Stop monitoring the notificaton area
            tmrShowMonitor.Enabled = false;
            CurrentMonitoredControls = new List<IntPtr>();
            //This is a hack: dispose the notification icon (although it's not visible) to force a redraw of the notification area in Windows 10
            if (Helper.IsWindows10)
                notifyIcon.Visible = false;
        }

        private void HideClockElements()
        {
            //By default, hide at least the clock
            Helper.TaskbarElement tbeToToggle = Helper.TaskbarElement.Clock;
            //If the user wants, toggle the full notification area
            if (NotificationAreaToggle.Checked) tbeToToggle = Helper.TaskbarElement.FullNotificationArea;
            Helper.HideTaskbarElements(tbeToToggle);

            IsDirty = true;
            ExitText.Text = "Restore && Exit";
            //pnlCheckBoxes.Enabled = false;
            //Monitor the notification area in case it pops up again for any reason
            //(if the user hasn't enabled Focus Assist, any notification or any new icon added to the tray will show all again)
            CurrentMonitoredControls.Add(Helper.GetDateTimeControlHWnd());  //Always monitor the Datetime control
            if (NotificationAreaToggle.Checked)
                CurrentMonitoredControls.AddRange(Helper.GetNotificationAreaHWnds());   //It's a different list depending on the Windows version
            tmrShowMonitor.Enabled = true;
            //Add notification icon (hack in Win10 to be able to restore the real width of the taskbar when showing it again)
            if (Helper.IsWindows10)
                notifyIcon.Visible = true;
        }

        private void btnHideShow_Click(object sender, EventArgs e)
        {
            //By default, hide at least the clock
            Helper.TaskbarElement tbeToToggle = Helper.TaskbarElement.Clock;
            //If the user wants, toggle the full notification area
            if (NotificationAreaToggle.Checked) tbeToToggle = Helper.TaskbarElement.FullNotificationArea;

            //Operation performed (hide or show) (received as information of the operation that has been done)
            Helper.SWOperation operation = Helper.SWOperation.None;

            //Toggle Date/time and/or notification areas
            operation = Helper.ToggleTaskbarElements(tbeToToggle);

            switch (operation)
            {
                case Helper.SWOperation.Hide:
                    HideClockElements();
                    break;
                case Helper.SWOperation.Show:
                    ShowClockElements();
                    break;
                default:  //Controls can't be found: something has changed in the underlying structure: notify
                    MessageBox.Show(@"The notification area and/or the Date/Time controls have not been found.

This program is designed for Windows 11 (although it works with Windows 10 too). 

Maybe you're using a newer version of Windows. 
Or maybe your version of Windows 11 is newer and the layout for building the taskbar has been changed.

Please, contact me through GitHub (https://github.com/jmalarcon/Win11ClockToggler) 
and let me know about this issue. Thanks!",
"Problem!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }

            //Disable clock in secondary taskbars
            if (SecondaryToggle.Checked)
                Helper.ShowOrHideSecondaryTaskbarsElementWindow();

        }

        //Form load 
        private void Win11ClockTogglerGUI_Load(object sender, EventArgs e)
        {
            SetTheme();
            DisableCheckBox(DateTimeToggle);   //This is always fixed, for information purposes, because the Date/Time is always toggled
            //Get the latest state of the option checkboxes to keep them the same
            NotificationAreaToggle.Checked = (Helper.ReadRegValue(REG_CHKNOTIFAREA_STATUS, "1") == "1");
            SecondaryToggle.Checked = (Helper.ReadRegValue(REG_CHKALLLDISPLAYS_STATUS, "1") == "1");

            //Check if there are secondary taskbars in secondary windows
            if (!Helper.AreThereSecondaryTaskbars())
            {
                //Disable checkbox if there are not secondary taskbars
                SecondaryToggle.Checked = false;
                DisableCheckBox(SecondaryToggle);
            }

            SetAllCheckboxes();
            //Check for new version in background
            bgwCheckVersion.RunWorkerAsync();
        }

        private void SetTheme()
        {
            Theme = new ColorThemes.LightTheme();
            try
            {
                int res = (int)Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", "AppsUseLightTheme", -1);
                if (res == 0)
                {
                       Theme = new ColorThemes.DarkTheme();
                }
            }
            catch
            {
                //Exception Handling     
            }

            foreach (Panel panel in new List<Panel> { UpdatePanel, pnlNotifArea, ShowOnHoverPanel, pnlSecondary, pnlDateTime, AboutPanel, VisibilityPanel, ExitPanel })
            {
                panel.ForeColor = Theme.Foreground;
                panel.BackColor = Theme.PanelBackground;
            }

            this.BackColor = Theme.Background;
        }

        private ColorThemes.Theme Theme;

        private void StartAutoTimer()
        {
            autoTimer = new System.Windows.Forms.Timer();
            autoTimer.Interval = 300;
            autoTimer.Tick += AutoTimer_Tick;
            autoTimer.Start();
        }

        private void AutoTimer_Tick(object sender, EventArgs e)
        {
            var res = Screen.GetBounds(this);
            float dx, dy;

            Graphics g = this.CreateGraphics();
            try
            {
                dx = g.DpiX;
                dy = g.DpiY;
            }
            finally
            {
                g.Dispose();
            }

            float dpiFactor = dx / 96f;

            int left = res.Right - (int)(170 * dpiFactor);
            int top = res.Bottom - (int)(48 * dpiFactor);

            var pos = System.Windows.Forms.Control.MousePosition;

            if (pos.X >= left && pos.Y >= top && pos.X <= res.Right && pos.Y <= res.Bottom)
            {
                lastMouseOvered = DateTime.Now;
                if (!isShown)
                {
                    ShowClockElements();
                    isShown = true;
                }

            }
            else
            {
                if (isShown && (DateTime.Now - lastMouseOvered).TotalSeconds > 5)
                {
                    HideClockElements();
                    isShown = false;
                }
            }
        }

        System.Windows.Forms.Timer autoTimer;

        //Form closing
        private void Win11ClockTogglerGUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Save the status of the checks to keep the latest option
            Helper.SaveRegValue(REG_CHKNOTIFAREA_STATUS, NotificationAreaToggle.Checked ? "1" : "0");
            Helper.SaveRegValue(REG_CHKALLLDISPLAYS_STATUS, SecondaryToggle.Checked ? "1" : "0");

            if (IsDirty)
                btnHideShow_Click(null, null);

            //Dispose Notify icons because of Windows 10 hack
            if (Helper.IsWindows10 && NotificationAreaToggle.Checked)
            {
                try
                {
                    notifyIcon.Icon.Dispose();
                    notifyIcon.Dispose();
                }
                catch { }
            }
        }

        private void toggleStealthMode()
        {
            if (this.Visible)
            {
                this.HideClockElements();
                MessageBox.Show("The Win11ClockToggler window is now completely hidden.\nWhenever you want to bring it back, press Win+Shift+F7.",
                    "Windows 11 Date//Time & Notification Area Toggler",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Visible = false;
            }
            else
            {
                this.Visible = true;
                this.ShowClockElements();
                this.WindowState = FormWindowState.Normal;
                BringToFront();
            }
        }

        //Timer to monitor if the current notification area pops up again because of a new icon or notification
        private void tmrShowMonitor_Tick(object sender, EventArgs e)
        {
            if (CurrentMonitoredControls.Count > 0) //Monitoring on
            {
                CurrentMonitoredControls.ForEach(ctrlHWnd =>
                {
                    //If it has been put visible again, hide it
                    if (Helper.IsControlVisible(ctrlHWnd))
                        Helper.HideControl(ctrlHWnd);
                });
            }
            else    //Monitoring off
                return;
        }

        //Check the latest version available in the background
        private void bgwCheckVersion_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            LatestVersion = VersionChecker.GetLatestAvailableVersion();
        }

        //When the latest version info is available
        private void bgwCheckVersion_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            //If there's a new version, show the label with the infomation and link
            if (LatestVersion != null && LatestVersion != string.Empty)
            {
                lnkNewVersion.Text = $"New version {LatestVersion} available! Click here to download...";
                lnkNewVersion.LinkArea = new LinkArea(0, lnkNewVersion.Text.Length);
                lnkNewVersion.Visible = true;
                UpdatePanel.ForeColor = Theme.Foreground;
                UpdatePanel.BackColor = Theme.PanelBackground;
            }
            else
            {
                lnkNewVersion.Visible = true;
                lnkNewVersion.Text = "You are on the latest version";
                lnkNewVersion.LinkArea = new LinkArea();

                UpdatePanel.ForeColor = Theme.DisabledForeground;
                UpdatePanel.BackColor = Theme.DisabledPanelBackground;
            }
        }

        private void lnkNewVersion_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/jmalarcon/Win11ClockToggler/releases");
        }

        private void notifyIcon_Click(object sender, EventArgs e)
        {
            this.ShowClockElements();
        }

        private void cmdAbout_Click(object sender, EventArgs e)
        {
            About about = new About();
            about.ShowDialog(this);
        }

        private void AutoHideToggle_CheckedChanged(object sender, EventArgs e)
        {
            SetImageFromToggle(AutoHideToggle, AutoHideImage, AutoHideLabel);

            if (AutoHideToggle.Checked)
            {
                HideClockElements();
                isShown = false;
                StartAutoTimer();
            }
            else
            {
                autoTimer.Stop();
                autoTimer = null;

            }              
        }

        private void DateTimeToggle_CheckedChanged(object sender, EventArgs e)
        {
            SetImageFromToggle(DateTimeToggle, DateTimeImage, DateTimeLabel);
        }

        private void DateTimeToggle_Layout(object sender, LayoutEventArgs e)
        {
            SetImageFromToggle(DateTimeToggle, DateTimeImage, DateTimeLabel);
        }

        private void SetImageFromToggle(CheckBox checkBox, PictureBox pictureBox, Label label)
        {
            pictureBox.Image = checkBox.Checked ? ToggleOnSource.Image : ToggleOffSource.Image;

            pictureBox.Enabled = checkBox.Enabled;
            checkBox.Visible = false;
            label.Text = checkBox.Checked ? "On" : "Off";

        }

        private void NotificationAreaToggle_CheckedChanged(object sender, EventArgs e)
        {
            SetImageFromToggle(NotificationAreaToggle, NotificationAreaImage, NotificationAreaLabel);
        }

        private void SecondaryToggle_CheckedChanged(object sender, EventArgs e)
        {
            SetImageFromToggle(SecondaryToggle, SecondaryImage, SecondaryLabel);
        }

        private void SetAllCheckboxes()
        {
            SetImageFromToggle(DateTimeToggle, DateTimeImage, DateTimeLabel);
            SetImageFromToggle(NotificationAreaToggle, NotificationAreaImage, NotificationAreaLabel);
            SetImageFromToggle(SecondaryToggle, SecondaryImage, SecondaryLabel);
            SetImageFromToggle(AutoHideToggle, AutoHideImage, AutoHideLabel);
        }

        private void SecondaryImage_LoadCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            
        }

        private void DateTimeImage_Click(object sender, EventArgs e)
        {
            ClickToggleImage(DateTimeToggle);
        }

        private void ClickToggleImage(CheckBox checkBox)
        {
            if (checkBox.Enabled)
            {
                checkBox.Checked = !checkBox.Checked;
            }
        }

        private void NotificationAreaImage_Click(object sender, EventArgs e)
        {
            ClickToggleImage(NotificationAreaToggle);
        }

        private void SecondaryImage_Click(object sender, EventArgs e)
        {
            ClickToggleImage(SecondaryToggle);
        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void AutoHideImage_Click(object sender, EventArgs e)
        {
            ClickToggleImage(AutoHideToggle);
        }

        private void panel1_Click(object sender, EventArgs e)
        {
            About about = new About();
            about.ShowDialog(this);
        }

        private void panel2_Click(object sender, EventArgs e)
        {
            btnHideShow_Click(null, null);
        }

        private void panel3_Click(object sender, EventArgs e)
        {
            this.Close();
            Application.Exit();
        }

        private void label14_Click(object sender, EventArgs e)
        {
            About about = new About();
            about.ShowDialog(this);
        }

        private void label2_Click(object sender, EventArgs e)
        {
            About about = new About();
            about.ShowDialog(this);
        }

        private void label17_Click(object sender, EventArgs e)
        {
            About about = new About();
            about.ShowDialog(this);
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label20_Click(object sender, EventArgs e)
        {
            btnHideShow_Click(null, null);
        }

        private void label18_Click(object sender, EventArgs e)
        {
            btnHideShow_Click(null, null);
        }

        private void label19_Click(object sender, EventArgs e)
        {
            btnHideShow_Click(null, null);
        }

        private void ExitPanel_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
