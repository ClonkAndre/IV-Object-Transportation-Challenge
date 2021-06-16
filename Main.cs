using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GTA;
using GTA.Native;
using IVObjectTransportationChallenge;
using Un4seen.Bass;

namespace GTAIVObjectTransportationChallenge {
    public class Main : Script {

        #region Variables
        // Mission
        private bool isMissionActive;
        private bool isCreatingMission;
        private bool isUsingCustomMusic;

        private bool areVehiclesAllowedToUse;
        private bool isWantedAfterObjectPickup;
        private bool removeWeaponsOnMissionStart;

        private string startingObject;
        private int targetWantedLevel;
        private int remainingTimeUntilLose;
        private string allowedVehicles;
        private string missionName;
        private Vector3 endPoint;
        private Vector3 objectPoint;

        private Blip objectLocationBlip;
        private Blip endLocationBlip;
        private Checkpoint endLocationCheckpoint;

        // Mission temps
        private bool msgTemp;
        private bool hasPickedUpObject;
        private int tempCountdownTime;

        // Timer
        private GTA.Timer countdownTimer;
        private GTA.Timer textDrawingTimer1;

        // Forms
        #region Forms
        #region MainForm
        private GTA.Forms.Form mainForm;
        private GTA.Forms.Label createLabel;
        private GTA.Forms.Button createNewMissionButton;
        private GTA.Forms.Label allMissionsLabel;
        private GTA.Forms.Listbox allMissionsListBox;
        private GTA.Forms.Button startMissionButton;
        private GTA.Forms.Button abortMissionButton;
        #endregion
        #region missionEditor
        private GTA.Forms.Form missionEditor;
        private GTA.Forms.Label missionNameLabel;
        private GTA.Forms.Textbox missionNameTextBox;
        private GTA.Forms.Label missionSettingsLabel;
        private GTA.Forms.Button areVehiclesAllowedCheckBox;
        private GTA.Forms.Label areVehiclesAllowedInfoLabel;
        private GTA.Forms.Textbox allowedVehiclesTextBox;
        private GTA.Forms.Button isPlayerWantedCheckBox;
        private GTA.Forms.Label isPlayerWantedInfoLabel;
        private GTA.Forms.Textbox wantedLevelTextBox;
        private GTA.Forms.Label secondsUntilLoseInfoLabel;
        private GTA.Forms.Textbox secondsUntilLoseTextBox;
        private GTA.Forms.Button removeAllWeaponsOnStartCheckBox;
        private GTA.Forms.Label objectNeededInfoLabel;
        private GTA.Forms.Textbox objectNeededTextBox;
        private GTA.Forms.Label objectPositionInfoLabel;
        private GTA.Forms.Textbox objectPositionTextBox;
        private GTA.Forms.Label finishPointInfoLabel;
        private GTA.Forms.Textbox finishPointTextBox;
        private GTA.Forms.Button abortButton;
        private GTA.Forms.Button createMissionButton;
        #endregion
        #endregion

        // Sounds
        private Int32 defaultMissionCompleteSound;
        private Int32 customMissionCompleteSound;

        // Settings
        private int soundVolume;
        private Keys openMenuKey;

        // Other
        private bool allowDrawing;
        private drawingMode selectedDrawingMode = drawingMode.None;
        private string DataFolder = Game.InstallFolder + "\\scripts\\IVObjectTransportationChallenge";
        private string MissionFolder = Game.InstallFolder + "\\scripts\\IVObjectTransportationChallenge\\Missions";
        private enum drawingMode { None, StartingText, VehicleWarningText }
        #endregion

        #region Timer
        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            if (tempCountdownTime == 0) {
                countdownTimer.Stop();
                EndMission(false, true);
            }
            else {
                tempCountdownTime--;
            }
        }
        private void TextDrawingTimer1_Tick(object sender, EventArgs e)
        {
            textDrawingTimer1.Stop();
            selectedDrawingMode = drawingMode.None;
        }
        #endregion

        #region FormsEventHandler

        #region mainForm
        private void CreateNewMissionButton_Click(object sender, GTA.MouseEventArgs e)
        {
            if (isMissionActive) {
                ShowMessage("You are currently on a mission!", 3500);
            }
            else {
                isCreatingMission = true;
                mainForm.Close();
            }
        }
        private void StartMissionButton_Click(object sender, GTA.MouseEventArgs e)
        {
            if (!isMissionActive) {
                if (allMissionsListBox.SelectedValue == null) {
                    ShowMessage("Please select a mission from the list.", 2000);
                }
                else {
                    // Checking
                    ShowMessage("Loading mission: " + allMissionsListBox.SelectedItem.DisplayText, 1000); // Info
                    mainForm.Close();

                    string missionPath = MissionFolder + "\\" + allMissionsListBox.SelectedItem.DisplayText;
                    if (!System.IO.File.Exists(missionPath + "\\mission.ini")) {
                        ShowMessage(string.Format("Could not load mission {0}. mission.ini doesn't exists.", allMissionsListBox.SelectedItem.DisplayText), 3500); // Info
                    }
                    else {
                        try {
                            // Load mission data
                            areVehiclesAllowedToUse = bool.Parse(IniHelper.ReadValueFromFile("Mission", "AreVehiclesAllowed", "false", missionPath + "\\mission.ini"));
                            isWantedAfterObjectPickup = bool.Parse(IniHelper.ReadValueFromFile("Mission", "PlayerWillBeWantedByPolice", "false", missionPath + "\\mission.ini"));
                            removeWeaponsOnMissionStart = bool.Parse(IniHelper.ReadValueFromFile("Mission", "RemoveWeapons", "false", missionPath + "\\mission.ini"));

                            targetWantedLevel = int.Parse(IniHelper.ReadValueFromFile("Mission", "WantedLevels", "0", missionPath + "\\mission.ini"));
                            remainingTimeUntilLose = int.Parse(IniHelper.ReadValueFromFile("Mission", "SecondsUntilLose", "5", missionPath + "\\mission.ini"));
                            tempCountdownTime = remainingTimeUntilLose;

                            allowedVehicles = IniHelper.ReadValueFromFile("Mission", "AllowedVehicles", "ALL", missionPath + "\\mission.ini");
                            missionName = IniHelper.ReadValueFromFile("Mission", "Name", "Unknown", missionPath + "\\mission.ini");
                            startingObject = IniHelper.ReadValueFromFile("Mission", "Object", "Unknown", missionPath + "\\mission.ini");

                            string endPointVector3Row = IniHelper.ReadValueFromFile("Mission", "FinishLocation", "0|0|0", missionPath + "\\mission.ini");
                            endPoint = new Vector3(float.Parse(endPointVector3Row.Split('|')[0]), float.Parse(endPointVector3Row.Split('|')[1]), float.Parse(endPointVector3Row.Split('|')[2]));
                            string objectPointVector3Row = IniHelper.ReadValueFromFile("Mission", "ObjectLocation", "0|0|0", missionPath + "\\mission.ini");
                            objectPoint = new Vector3(float.Parse(objectPointVector3Row.Split('|')[0]), float.Parse(objectPointVector3Row.Split('|')[1]), float.Parse(objectPointVector3Row.Split('|')[2]));

                            // Create blips etc
                            objectLocationBlip = BlipObj(objectPoint, BlipColor.Yellow, BlipDisplay.MapOnly, BlipIcon.Misc_Destination, false, "Object location");
                            endLocationBlip = BlipObj(endPoint, BlipColor.Red, BlipDisplay.MapOnly, BlipIcon.Misc_Destination1, false, "Object delivery point");
                            endLocationCheckpoint = new Checkpoint(endPoint.ToGround(), Color.Red, 1.5f);

                            // Check for custom mission sound
                            if (System.IO.File.Exists(missionPath + "\\default.mp3")) {
                                customMissionCompleteSound = Bass.BASS_StreamCreateFile(missionPath + "\\default.mp3", 0, 0, BASSFlag.BASS_STREAM_PRESCAN & BASSFlag.BASS_STREAM_AUTOFREE);
                                Bass.BASS_ChannelSetAttribute(customMissionCompleteSound, Un4seen.Bass.BASSAttribute.BASS_ATTRIB_VOL, soundVolume / 100.0f);
                                isUsingCustomMusic = true;
                            }
                            else {
                                isUsingCustomMusic = false;
                            }

                            // Activate mission
                            allowDrawing = true;
                            isMissionActive = true;
                            ShowMessage("~s~Search the ~y~Area~s~ for the object.", 5000); // Info
                            selectedDrawingMode = drawingMode.StartingText;
                            textDrawingTimer1.Start();

                            if (removeWeaponsOnMissionStart) { Player.Character.Weapons.RemoveAll(); }
                        }
                        catch (Exception ex) {
                            Game.Console.Print("Error while loading mission: " + ex.Message);
                            ShowMessage("Error while loading mission. Check console for details.", 3000);
                        }
                    }
                }
            }
            else {
                ShowMessage("End your current mission in order to start a new one.", 3500);
            }
        }
        private void AbortMissionButton_Click(object sender, GTA.MouseEventArgs e)
        {
            EndMission(false);
        }
        #endregion

        #region missionEditor
        private void CreateMissionButton_Click(object sender, GTA.MouseEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(missionNameTextBox.Text)) {
                GTAMessageBox msgbox = new GTAMessageBox("Please enter a mission name.", "Info", MessageBoxButtons.OK);
                msgbox.ShowDialog();
            }
            else {
                if (System.IO.Directory.Exists(DataFolder + "\\Missions\\" + missionNameTextBox.Text)) {
                    GTAMessageBox msgbox = new GTAMessageBox(string.Format("A mission with the name '{0}' already exists.\n\rPlease choose another name.", missionNameTextBox.Text), "Info", MessageBoxButtons.OK);
                    msgbox.ShowDialog();
                }
                else {
                    if (string.IsNullOrWhiteSpace(objectNeededTextBox.Text)) {
                        GTAMessageBox msgbox = new GTAMessageBox("Please enter a valid object.", "Info", MessageBoxButtons.OK);
                        msgbox.ShowDialog();
                    }
                    else {
                        if (string.IsNullOrWhiteSpace(objectPositionTextBox.Text)) {
                            GTAMessageBox msgbox = new GTAMessageBox("Please enter a valid object location.", "Info", MessageBoxButtons.OK);
                            msgbox.ShowDialog();
                        }
                        else {
                            if (string.IsNullOrWhiteSpace(finishPointTextBox.Text)) {
                                GTAMessageBox msgbox = new GTAMessageBox("Please enter a valid finish point location.", "Info", MessageBoxButtons.OK);
                                msgbox.ShowDialog();
                            }
                            else
                            {
                                try {
                                    // Create mission
                                    isCreatingMission = false;
                                    missionEditor.Close();

                                    ShowMessage(string.Format("Creating mission '{0}' ...", missionNameTextBox.Text), 2500); // Info

                                    System.IO.Directory.CreateDirectory(MissionFolder + "\\" + missionNameTextBox.Text); // Create Directory
                                    IniHelper.WriteValueToFile("Mission", "Name", missionNameTextBox.Text, MissionFolder + "\\" + missionNameTextBox.Text + "\\mission.ini"); // Save name
                                    IniHelper.WriteValueToFile("Mission", "Object", objectNeededTextBox.Text, MissionFolder + "\\" + missionNameTextBox.Text + "\\mission.ini"); // Save object
                                    IniHelper.WriteValueToFile("Mission", "ObjectLocation", objectPositionTextBox.Text, MissionFolder + "\\" + missionNameTextBox.Text + "\\mission.ini"); // Save object location
                                    IniHelper.WriteValueToFile("Mission", "FinishLocation", finishPointTextBox.Text, MissionFolder + "\\" + missionNameTextBox.Text + "\\mission.ini"); // Save finish location
                                    IniHelper.WriteValueToFile("Mission", "AllowedVehicles", allowedVehiclesTextBox.Text, MissionFolder + "\\" + missionNameTextBox.Text + "\\mission.ini"); // Save vehicles
                                    IniHelper.WriteValueToFile("Mission", "WantedLevels", wantedLevelTextBox.Text.ToString(), MissionFolder + "\\" + missionNameTextBox.Text + "\\mission.ini"); // Save wanted levels
                                    IniHelper.WriteValueToFile("Mission", "SecondsUntilLose", secondsUntilLoseTextBox.Text.ToString(), MissionFolder + "\\" + missionNameTextBox.Text + "\\mission.ini"); // Save seconds

                                    // Save states
                                    if (areVehiclesAllowedCheckBox.BackColor == Color.DarkRed) {
                                        IniHelper.WriteValueToFile("Mission", "AreVehiclesAllowed", "False", MissionFolder + "\\" + missionNameTextBox.Text + "\\mission.ini");
                                    }
                                    else {
                                        IniHelper.WriteValueToFile("Mission", "AreVehiclesAllowed", "True", MissionFolder + "\\" + missionNameTextBox.Text + "\\mission.ini");
                                    }

                                    if (isPlayerWantedCheckBox.BackColor == Color.DarkRed) {
                                        IniHelper.WriteValueToFile("Mission", "PlayerWillBeWantedByPolice", "False", MissionFolder + "\\" + missionNameTextBox.Text + "\\mission.ini");
                                    }
                                    else {
                                        IniHelper.WriteValueToFile("Mission", "PlayerWillBeWantedByPolice", "True", MissionFolder + "\\" + missionNameTextBox.Text + "\\mission.ini");
                                    }

                                    if (removeAllWeaponsOnStartCheckBox.BackColor == Color.DarkRed) {
                                        IniHelper.WriteValueToFile("Mission", "RemoveWeapons", "False", MissionFolder + "\\" + missionNameTextBox.Text + "\\mission.ini");
                                    }
                                    else {
                                        IniHelper.WriteValueToFile("Mission", "RemoveWeapons", "True", MissionFolder + "\\" + missionNameTextBox.Text + "\\mission.ini");
                                    }

                                    // Final check
                                    if (System.IO.File.Exists(MissionFolder + "\\" + missionNameTextBox.Text + "\\mission.ini")) {
                                        ShowMessage(string.Format("Mission '{0}' created!", missionNameTextBox.Text), 2500); // Info
                                        missionNameTextBox.Text = string.Empty;
                                        areVehiclesAllowedCheckBox.BackColor = Color.DarkRed;
                                        allowedVehiclesTextBox.Text = string.Empty;
                                        isPlayerWantedCheckBox.BackColor = Color.DarkRed;
                                        wantedLevelTextBox.Text = string.Empty;
                                        secondsUntilLoseTextBox.Text = string.Empty;
                                        removeAllWeaponsOnStartCheckBox.BackColor = Color.DarkRed;
                                        objectNeededTextBox.Text = string.Empty;
                                        objectPositionTextBox.Text = string.Empty;
                                        finishPointTextBox.Text = string.Empty;
                                    }
                                    else {
                                        GTAMessageBox msgbox = new GTAMessageBox(string.Format("Unknown error while creating mission '{0}' ...", missionNameTextBox.Text), "Error", MessageBoxButtons.OK);
                                        msgbox.ShowDialog();
                                    }
                                }
                                catch (Exception ex) {
                                    GTAMessageBox msgbox = new GTAMessageBox(string.Format("Error while creating your mission...\n\rYour settings should still be saved in the editor window.\n\rError details: {0}", ex.Message), "Error", MessageBoxButtons.OK);
                                    msgbox.ShowDialog();
                                }
                            }
                        }
                    }
                }
            }
        }
        private void AbortButton_Click(object sender, GTA.MouseEventArgs e)
        {
            isCreatingMission = false;
            missionNameTextBox.Text = string.Empty;
            areVehiclesAllowedCheckBox.BackColor = Color.DarkRed;
            allowedVehiclesTextBox.Text = string.Empty;
            isPlayerWantedCheckBox.BackColor = Color.DarkRed;
            wantedLevelTextBox.Text = string.Empty;
            secondsUntilLoseTextBox.Text = string.Empty;
            removeAllWeaponsOnStartCheckBox.BackColor = Color.DarkRed;
            objectNeededTextBox.Text = string.Empty;
            objectPositionTextBox.Text = string.Empty;
            finishPointTextBox.Text = string.Empty;
            missionEditor.Close();
        }
        private void AreVehiclesAllowedCheckBox_Click(object sender, GTA.MouseEventArgs e)
        {
            if (areVehiclesAllowedCheckBox.BackColor == Color.DarkRed) {
                areVehiclesAllowedCheckBox.BackColor = Color.DarkGreen;
            }
            else {
                areVehiclesAllowedCheckBox.BackColor = Color.DarkRed;
            }
        }
        private void IsPlayerWantedCheckBox_Click(object sender, GTA.MouseEventArgs e)
        {
            if (isPlayerWantedCheckBox.BackColor == Color.DarkRed) {
                isPlayerWantedCheckBox.BackColor = Color.DarkGreen;
            }
            else {
                isPlayerWantedCheckBox.BackColor = Color.DarkRed;
            }
        }
        private void RemoveAllWeaponsOnStartCheckBox_Click(object sender, GTA.MouseEventArgs e)
        {
            if (removeAllWeaponsOnStartCheckBox.BackColor == Color.DarkRed) {
                removeAllWeaponsOnStartCheckBox.BackColor = Color.DarkGreen;
            }
            else {
                removeAllWeaponsOnStartCheckBox.BackColor = Color.DarkRed;
            }
        }
        #endregion

        #endregion

        #region Functions
        private void EndMission(bool missionPassed, bool respondToMissionState = false, bool playSound = false)
        {
            if (isMissionActive) {
                // Reset
                if (objectLocationBlip.Exists()) { objectLocationBlip.Delete(); }
                if (endLocationBlip.Exists()) { endLocationBlip.Delete(); }
                objectLocationBlip = null;
                endLocationBlip = null;
                endLocationCheckpoint.Disable();
                endLocationCheckpoint = null;
                isMissionActive = false;
                msgTemp = false;
                hasPickedUpObject = false;
                allowDrawing = false;
                selectedDrawingMode = drawingMode.None;

                if (missionPassed) { // MissionPassed
                    ShowMessage("~s~Mission ~g~Passed~s~!", 2500); // Info
                    if (respondToMissionState) {
                        Player.Character.SayAmbientSpeech("DARTS_HAPPY");
                    }

                    if (playSound) {
                        if (isUsingCustomMusic) {
                            Bass.BASS_ChannelPlay(customMissionCompleteSound, false);
                        }
                        else {
                            Bass.BASS_ChannelPlay(defaultMissionCompleteSound, false);
                        }
                    }
                }
                else { // MissionFailed
                    ShowMessage("~s~Mission ~r~failed~s~!", 2500); // Info
                    if (respondToMissionState) {
                        Player.Character.SayAmbientSpeech("MISSION_FAIL_RAGE");
                    }
                }
            }
        }
        private Blip BlipObj(Vector3 target, BlipColor blipColor, BlipDisplay blipDisplay, BlipIcon blipIcon, bool showOnlyWhenNear, string name)
        {
            Blip blip = Blip.AddBlip(target);
            blip.Color = blipColor;
            blip.Display = blipDisplay;
            blip.Icon = blipIcon;
            blip.ShowOnlyWhenNear = showOnlyWhenNear;
            blip.Name = name;
            return blip;
        }
        private void ShowMessage(string text, int time = 1000)
        {
            Function.Call("PRINT_STRING_WITH_LITERAL_STRING_NOW", "STRING", text, time, 1);
        }
        #endregion

        public Main()
        {
            // Setup Bass.dll
            Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);

            // Load settings
            openMenuKey = Settings.GetValueKey("OpenMenuKey", "Main", Keys.F9);
            soundVolume = Settings.GetValueInteger("Volume", "Sound", 10);

            // Sounds
            defaultMissionCompleteSound = Bass.BASS_StreamCreateFile(DataFolder + "\\default.mp3", 0, 0, BASSFlag.BASS_STREAM_PRESCAN);
            Bass.BASS_ChannelSetAttribute(defaultMissionCompleteSound, Un4seen.Bass.BASSAttribute.BASS_ATTRIB_VOL, soundVolume / 100.0f);

            #region Setup forms

            #region mainForm
            mainForm = new GTA.Forms.Form();
            mainForm.Text = "GTA IV Object Transportation Main Menu";
            mainForm.Font = new GTA.Font("Microsoft Sans Serif", 0.3f, FontScaling.FontSize, false, false);
            mainForm.Size = new Size(507, 270);
            mainForm.StartPosition = GTA.FormStartPosition.CenterScreen;

            createLabel = new GTA.Forms.Label();
            createLabel.Text = "Create";
            createLabel.Location = new Point(24, 14);

            createNewMissionButton = new GTA.Forms.Button();
            createNewMissionButton.Text = "Create new mission";
            createNewMissionButton.Size = new Size(138, 23);
            createNewMissionButton.Location = new Point(30, 30);
            createNewMissionButton.Click += new GTA.MouseEventHandler(CreateNewMissionButton_Click);

            allMissionsLabel = new GTA.Forms.Label();
            allMissionsLabel.Text = "All missions";
            allMissionsLabel.Size = new Size(71, 13);
            allMissionsLabel.Location = new Point(24, 67);

            allMissionsListBox = new GTA.Forms.Listbox();
            allMissionsListBox.Size = new Size(447, 95);
            allMissionsListBox.Location = new Point(30, 83);

            startMissionButton = new GTA.Forms.Button();
            startMissionButton.Text = "Start selected mission";
            startMissionButton.Size = new Size(447, 23);
            startMissionButton.Location = new Point(30, 184);
            startMissionButton.Click += new GTA.MouseEventHandler(StartMissionButton_Click);

            abortMissionButton = new GTA.Forms.Button();
            abortMissionButton.Text = "Abort current mission";
            abortMissionButton.Size = new Size(447, 23);
            abortMissionButton.Location = new Point(30, 213);
            abortMissionButton.Click += new GTA.MouseEventHandler(AbortMissionButton_Click);

            mainForm.Controls.Add(createLabel);
            mainForm.Controls.Add(createNewMissionButton);
            mainForm.Controls.Add(allMissionsLabel);
            mainForm.Controls.Add(allMissionsListBox);
            mainForm.Controls.Add(startMissionButton);
            mainForm.Controls.Add(abortMissionButton);
            #endregion

            #region missionEditor
            missionEditor = new GTA.Forms.Form();
            missionEditor.Text = "GTA IV Object Transportation Mission Editor";
            missionEditor.Font = new GTA.Font("Microsoft Sans Serif", 0.3f, FontScaling.FontSize, false, false);
            missionEditor.Size = new Size(600, 500);

            missionNameLabel = new GTA.Forms.Label();
            missionNameLabel.Text = "Mission name";
            missionNameLabel.Size = new Size(500, 13);
            missionNameLabel.Location = new Point(23, 14);

            missionNameTextBox = new GTA.Forms.Textbox();
            missionNameTextBox.Size = new Size(458, 20);
            missionNameTextBox.Location = new Point(26, 30);

            missionSettingsLabel = new GTA.Forms.Label();
            missionSettingsLabel.Text = "Mission settings";
            missionSettingsLabel.Size = new Size(500, 13);
            missionSettingsLabel.Location = new Point(23, 60);

            areVehiclesAllowedCheckBox = new GTA.Forms.Button();
            areVehiclesAllowedCheckBox.Text = "Are vehicles allowed";
            areVehiclesAllowedCheckBox.Size = new Size(250, 18);
            areVehiclesAllowedCheckBox.Location = new Point(26, 77);
            areVehiclesAllowedCheckBox.BackColor = Color.DarkRed;
            areVehiclesAllowedCheckBox.Click += new GTA.MouseEventHandler(AreVehiclesAllowedCheckBox_Click);

            areVehiclesAllowedInfoLabel = new GTA.Forms.Label();
            areVehiclesAllowedInfoLabel.Text = "Allowed vehicles (Please seperate each vehicle with a comma like this: ADMIRAL,POLICE2.\n\rIf all vehicles should be allowed type in: ALL):";
            areVehiclesAllowedInfoLabel.Size = new Size(600, 40);
            areVehiclesAllowedInfoLabel.Location = new Point(41, 95);

            allowedVehiclesTextBox = new GTA.Forms.Textbox();
            allowedVehiclesTextBox.Size = new Size(458, 20);
            allowedVehiclesTextBox.Location = new Point(26, 128);

            isPlayerWantedCheckBox = new GTA.Forms.Button();
            isPlayerWantedCheckBox.Text = "Is player wanted after picking up object";
            isPlayerWantedCheckBox.Size = new Size(250, 18);
            isPlayerWantedCheckBox.Location = new Point(26, 159);
            isPlayerWantedCheckBox.BackColor = Color.DarkRed;
            isPlayerWantedCheckBox.Click += new GTA.MouseEventHandler(IsPlayerWantedCheckBox_Click);

            isPlayerWantedInfoLabel = new GTA.Forms.Label();
            isPlayerWantedInfoLabel.Text = "How many stars should the player get? (Please only enter numbers from 0 - 6)";
            isPlayerWantedInfoLabel.Size = new Size(500, 15);
            isPlayerWantedInfoLabel.Location = new Point(41, 177);

            wantedLevelTextBox = new GTA.Forms.Textbox();
            wantedLevelTextBox.Size = new Size(48, 20);
            wantedLevelTextBox.Location = new Point(26, 197);

            secondsUntilLoseInfoLabel = new GTA.Forms.Label();
            secondsUntilLoseInfoLabel.Text = "If player enters a vehicle that is not allowed, in how many seconds should the mission fail?";
            secondsUntilLoseInfoLabel.Size = new Size(600, 15);
            secondsUntilLoseInfoLabel.Location = new Point(41, 223);

            secondsUntilLoseTextBox = new GTA.Forms.Textbox();
            secondsUntilLoseTextBox.Size = new Size(48, 20);
            secondsUntilLoseTextBox.Location = new Point(26, 244);

            removeAllWeaponsOnStartCheckBox = new GTA.Forms.Button();
            removeAllWeaponsOnStartCheckBox.Text = "Remove all weapons on mission start";
            removeAllWeaponsOnStartCheckBox.Size = new Size(250, 18);
            removeAllWeaponsOnStartCheckBox.Location = new Point(26, 275);
            removeAllWeaponsOnStartCheckBox.BackColor = Color.DarkRed;
            removeAllWeaponsOnStartCheckBox.Click += new GTA.MouseEventHandler(RemoveAllWeaponsOnStartCheckBox_Click);

            objectNeededInfoLabel = new GTA.Forms.Label();
            objectNeededInfoLabel.Text = "Object that is needed for this mission (Pickup any object and press CNTRL + O)";
            objectNeededInfoLabel.Size = new Size(500, 15);
            objectNeededInfoLabel.Location = new Point(41, 303);

            objectNeededTextBox = new GTA.Forms.Textbox();
            objectNeededTextBox.Size = new Size(458, 20);
            objectNeededTextBox.Location = new Point(26, 323);

            objectPositionInfoLabel = new GTA.Forms.Label();
            objectPositionInfoLabel.Text = "Choose the initial location of the object (Go to the initial location and press CNTRL + I)";
            objectPositionInfoLabel.Size = new Size(500, 15);
            objectPositionInfoLabel.Location = new Point(41, 350);

            objectPositionTextBox = new GTA.Forms.Textbox();
            objectPositionTextBox.Size = new Size(458, 20);
            objectPositionTextBox.Location = new Point(26, 366);

            finishPointInfoLabel = new GTA.Forms.Label();
            finishPointInfoLabel.Text = "Finish point (Go to the spot that should be the finish point and press CNTRL + P)";
            finishPointInfoLabel.Size = new Size(500, 15);
            finishPointInfoLabel.Location = new Point(41, 393);

            finishPointTextBox = new GTA.Forms.Textbox();
            finishPointTextBox.Size = new Size(458, 20);
            finishPointTextBox.Location = new Point(26, 410);

            abortButton = new GTA.Forms.Button();
            abortButton.Text = "Discard mission";
            abortButton.Size = new Size(226, 23);
            abortButton.Location = new Point(24, 445);
            abortButton.Click += new GTA.MouseEventHandler(AbortButton_Click);
            createMissionButton = new GTA.Forms.Button();
            createMissionButton.Text = "CREATE MISSION";
            createMissionButton.Size = new Size(226, 23);
            createMissionButton.Location = new Point(256, 445);
            createMissionButton.Click += new GTA.MouseEventHandler(CreateMissionButton_Click);

            missionEditor.Controls.Add(missionNameLabel);
            missionEditor.Controls.Add(missionNameTextBox);
            missionEditor.Controls.Add(missionSettingsLabel);
            missionEditor.Controls.Add(areVehiclesAllowedCheckBox);
            missionEditor.Controls.Add(areVehiclesAllowedInfoLabel);
            missionEditor.Controls.Add(allowedVehiclesTextBox);
            missionEditor.Controls.Add(isPlayerWantedCheckBox);
            missionEditor.Controls.Add(isPlayerWantedInfoLabel);
            missionEditor.Controls.Add(wantedLevelTextBox);
            missionEditor.Controls.Add(secondsUntilLoseInfoLabel);
            missionEditor.Controls.Add(secondsUntilLoseTextBox);
            missionEditor.Controls.Add(removeAllWeaponsOnStartCheckBox);
            missionEditor.Controls.Add(objectNeededInfoLabel);
            missionEditor.Controls.Add(objectNeededTextBox);
            missionEditor.Controls.Add(objectPositionInfoLabel);
            missionEditor.Controls.Add(objectPositionTextBox);
            missionEditor.Controls.Add(finishPointInfoLabel);
            missionEditor.Controls.Add(finishPointTextBox);
            missionEditor.Controls.Add(abortButton);
            missionEditor.Controls.Add(createMissionButton);
            missionNameTextBox.Text = "";
            allowedVehiclesTextBox.Text = "";
            wantedLevelTextBox.Text = "";
            secondsUntilLoseTextBox.Text = "";
            objectPositionTextBox.Text = "";
            objectNeededTextBox.Text = "";
            finishPointTextBox.Text = "";
            #endregion

            #endregion

            // Setup timer
            countdownTimer = new GTA.Timer(1000, false);
            countdownTimer.Tick += new EventHandler(CountdownTimer_Tick);
            textDrawingTimer1 = new GTA.Timer(5000, false);
            textDrawingTimer1.Tick += new EventHandler(TextDrawingTimer1_Tick);

            this.Interval = 100;
            this.Tick += new EventHandler(Main_Tick);
            this.KeyDown += new GTA.KeyEventHandler(Main_KeyDown);
            this.PerFrameDrawing += new GraphicsEventHandler(Main_PerFrameDrawing);
        }

        private void Main_Tick(object sender, EventArgs e)
        {
            if (isMissionActive) {

                // Object
                GTA.Object objectInHand = Function.Call<GTA.Object>("GET_OBJECT_PED_IS_HOLDING", Player.Character);
                if (objectInHand != null && objectInHand.Exists()) {
                    if (objectInHand.Model.ToString() == startingObject) {
                        if (!hasPickedUpObject) {
                            if (isWantedAfterObjectPickup) { Player.WantedLevel = targetWantedLevel; }
                            hasPickedUpObject = true;

                            if (!msgTemp) { // Info message
                                ShowMessage("~s~Bring the object to the ~r~delivery point~s~.", 5000);
                                msgTemp = true;
                            }
                        }
                    }
                }
                else {
                    msgTemp = false;
                    hasPickedUpObject = false;
                }
                
                // Vehicle
                if (Player.Character.isInVehicle()) {
                    if (areVehiclesAllowedToUse) {
                        if (!allowedVehicles.Contains("ALL")) {
                            if (!allowedVehicles.Contains(Player.Character.CurrentVehicle.Name)) {
                                if (selectedDrawingMode != drawingMode.VehicleWarningText) {
                                    countdownTimer.Start();
                                    selectedDrawingMode = drawingMode.VehicleWarningText;
                                }
                            }
                        }
                    }
                    else {
                        if (selectedDrawingMode != drawingMode.VehicleWarningText) {
                            countdownTimer.Start();
                            selectedDrawingMode = drawingMode.VehicleWarningText;
                        }
                    }
                }
                else {
                    if (selectedDrawingMode == drawingMode.VehicleWarningText) {
                        countdownTimer.Stop();
                        selectedDrawingMode = drawingMode.None;
                        tempCountdownTime = remainingTimeUntilLose; // Reset time
                    }
                }

                // WantedLevel
                if (isWantedAfterObjectPickup) {
                    if (hasPickedUpObject) {
                        if (Player.WantedLevel != targetWantedLevel) {
                            Player.WantedLevel = targetWantedLevel; // Reset wantedlevel
                        }
                    }
                }

                // Delivery point / Mission passed
                if (hasPickedUpObject) {
                    if (Math.Round(endLocationCheckpoint.Position.DistanceTo(Player.Character.Position), 1) <= 1.1) { // Stands in checkpoint
                        Player.Character.Weapons.inSlot(WeaponSlot.Thrown).Remove();
                        Player.WantedLevel = 0;
                        EndMission(true, true, true);
                    }
                }

            }
        }

        private void Main_PerFrameDrawing(object sender, GraphicsEventArgs e)
        {
            if (isCreatingMission) {
                e.Graphics.DrawText(string.Format("In Mission Editor\n\rPress {0} to open the mission settings.\n\rTo exit the editor, press {0} and click on Discard mission.\n\r\n\rShortcuts\n\rControl + O : Copy current object in to textbox.\n\rControl + I : Copy object initial position in to textbox.\n\rControl + P : Copy finish position in to textbox.", openMenuKey.ToString()), (Game.Resolution.Width - Game.Resolution.Width) + 50, (Game.Resolution.Height - Game.Resolution.Height) + 60, Color.White);
            }

            if (allowDrawing) {
                switch (selectedDrawingMode) {
                    case drawingMode.StartingText:
                        e.Graphics.DrawText(string.Format("Mission: {0}\n\rGood luck!", missionName), (Game.Resolution.Width - Game.Resolution.Width) + 60, (Game.Resolution.Height - Game.Resolution.Height) + 60, Color.White);
                        break;
                    case drawingMode.VehicleWarningText:
                        e.Graphics.DrawText(string.Format("Get out of the vehicle!\n\rTime until mission fails: {0}", tempCountdownTime.ToString()), (Game.Resolution.Width / 2) - 10, (Game.Resolution.Height / 2) - 160, Color.Red);
                        break;
                }
            }
        }

        private void Main_KeyDown(object sender, GTA.KeyEventArgs e)
        {
            if (e.Key == openMenuKey) {
                if (isCreatingMission) {
                    missionEditor.Show(); // Show editor
                }
                else {
                    allMissionsListBox.Items.Clear(); // Clears list

                    // Add all missions to list
                    if (System.IO.Directory.Exists(DataFolder + "\\Missions")) {
                        foreach (string mission in System.IO.Directory.GetDirectories(DataFolder + "\\Missions")) {
                            if (System.IO.File.Exists(mission + "\\mission.ini")) {
                                allMissionsListBox.Items.Add(System.IO.Path.GetFileNameWithoutExtension(mission));
                            }
                        }
                    }

                    mainForm.Show(); // Shows main form
                }
            }
            else if (e.Key == Keys.P && e.Control) { // Copy position
                if (isCreatingMission) {
                    Vector3 playerPos = Player.Character.Position;
                    finishPointTextBox.Text = string.Format("{0}|{1}|{2}", playerPos.X.ToString(), playerPos.Y.ToString(), playerPos.Z.ToString());
                    ShowMessage("Finish position copied in to textbox!", 3000);
                }
            }
            else if (e.Key == Keys.I && e.Control) { // Copy initial position
                if (isCreatingMission) {
                    Vector3 playerPos = Player.Character.Position;
                    objectPositionTextBox.Text = string.Format("{0}|{1}|{2}", playerPos.X.ToString(), playerPos.Y.ToString(), playerPos.Z.ToString());
                    ShowMessage("Initial object position copied in to textbox!", 3000);
                }
            }
            else if (e.Key == Keys.O && e.Control) { // Copy object
                if (isCreatingMission) {
                    GTA.Object inHand = Function.Call<GTA.Object>("GET_OBJECT_PED_IS_HOLDING", Player.Character);
                    if (inHand != null && inHand.Exists()) {
                        objectNeededTextBox.Text = inHand.Model.ToString();
                        ShowMessage("Target object set to: " + inHand.Model.ToString(), 3000);
                    }
                    else {
                        ShowMessage("Please pick up any object you want to use for this mission.", 5000);
                    }
                }
            }
        }

    }
}
