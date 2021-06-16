using GTA;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IVObjectTransportationChallenge {
    public class GTAMessageBox : GTA.Forms.Form {

        private GTA.Forms.Label descLabel;
        private GTA.Forms.Button multiButton1;
        private GTA.Forms.Button multiButton2;

        private string _desc;
        private string _title;
        private MessageBoxButtons _messageBoxButtons;

        public GTAMessageBox(string Description, string Title, MessageBoxButtons messageBoxButtons)
        {
            _desc = Description;
            _title = Title;
            _messageBoxButtons = messageBoxButtons;

            this.Size = new Size(462, 145);
            this.StartPosition = GTA.FormStartPosition.CenterScreen;

            this.Closed += new EventHandler(GTAMessageBox_Closed);
            this.Opened += new EventHandler(GTAMessageBox_Opened);

            descLabel = new GTA.Forms.Label();
            descLabel.Size = new Size(460, 80);
            descLabel.Location = new Point(2, 2);

            multiButton1 = new GTA.Forms.Button();
            multiButton1.Size = new Size(110, 25);
            multiButton1.Location = new Point(345, 85);
            multiButton1.Click += new GTA.MouseEventHandler(MultiButton1_Click);

            multiButton2 = new GTA.Forms.Button();
            multiButton2.Size = new Size(110, 25);
            multiButton2.Location = new Point(230, 85);
            multiButton2.Click += new GTA.MouseEventHandler(MultiButton2_Click);
            multiButton2.Visible = false;

            this.Controls.Add(descLabel);
            this.Controls.Add(multiButton1);
            this.Controls.Add(multiButton2);
        }

        private void GTAMessageBox_Closed(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Dispose();
        }

        private void GTAMessageBox_Opened(object sender, EventArgs e)
        {
            this.Text = _title;
            descLabel.Text = _desc;

            switch (_messageBoxButtons) {
                case MessageBoxButtons.OK:
                    multiButton1.Text = "OK";
                    break;
                case MessageBoxButtons.YesNo:
                    multiButton1.Text = "Yes";
                    multiButton2.Text = "No";
                    multiButton2.Visible = true;
                    break;
                case MessageBoxButtons.OKCancel:
                    multiButton1.Text = "OK";
                    multiButton2.Text = "Cancel";
                    multiButton2.Visible = true;
                    break;
            }
        }

        private void MultiButton1_Click(object sender, GTA.MouseEventArgs e)
        {
            switch (_messageBoxButtons) {
                case MessageBoxButtons.OK:
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                    break;
                case MessageBoxButtons.YesNo:
                    this.DialogResult = DialogResult.Yes;
                    this.Close();
                    break;
                case MessageBoxButtons.OKCancel:
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                    break;
            }
        }

        private void MultiButton2_Click(object sender, GTA.MouseEventArgs e)
        {
            switch (_messageBoxButtons) {
                case MessageBoxButtons.YesNo:
                    this.DialogResult = DialogResult.No;
                    this.Close();
                    break;
                case MessageBoxButtons.OKCancel:
                    this.DialogResult = DialogResult.Cancel;
                    this.Close();
                    break;
            }
        }

    }
}
