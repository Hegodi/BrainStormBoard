using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BrainStormBoard
{
    public partial class SettingsForm : Form
    {
        public Settings settings;
        public SettingsForm(Settings _settings)
        {
            settings = new Settings(_settings);
            InitializeComponent();
            SetFormFromSettings();
            this.DialogResult = DialogResult.Cancel;
        }

        private void buttonBoardColor_Click(object sender, EventArgs e)
        {
            SelectColor(ref settings.colorBoardBackground);
        }

        private void buttonLinksColor_Click(object sender, EventArgs e)
        {
            SelectColor(ref settings.colorLinks);
        }
        private void buttonCardBackgroundColor_Click(object sender, EventArgs e)
        {
            SelectColor(ref settings.colorCardsBackground);
        }

        private void buttonCardColor0_Click(object sender, EventArgs e)
        {
            SelectColor(ref settings.colorsNotes[0]);
        }

        private void buttonCardColor1_Click(object sender, EventArgs e)
        {
            SelectColor(ref settings.colorsNotes[1]);
        }

        private void buttonCardColor2_Click(object sender, EventArgs e)
        {
            SelectColor(ref settings.colorsNotes[2]);
        }

        private void buttonCardColor3_Click(object sender, EventArgs e)
        {
            SelectColor(ref settings.colorsNotes[3]);
        }

        private void buttonCardColor4_Click(object sender, EventArgs e)
        {
            SelectColor(ref settings.colorsNotes[4]);
        }

        private void buttonFontColorTitle_Click(object sender, EventArgs e)
        {
            SelectColor(ref settings.colorTitle);
        }

        private void buttonFontColorBody_Click(object sender, EventArgs e)
        {
            SelectColor(ref settings.colorBody);
        }

        private void SelectColor(ref Color color)
        {
            colorDialog1.Color = color;
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                color = colorDialog1.Color;
                SetFormFromSettings();
            }
        }
        private void SelectFont(ref Font font)
        {
            fontDialog1.Font = font;
            if (fontDialog1.ShowDialog() == DialogResult.OK)
            {
                font = fontDialog1.Font;
                SetFormFromSettings();
            }
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void SetFormFromSettings()
        {
            buttonBoardColor.BackColor = settings.colorBoardBackground;
            buttonLinksColor.BackColor = settings.colorLinks;
            buttonCardColor0.BackColor = settings.colorsNotes[0];
            buttonCardColor1.BackColor = settings.colorsNotes[1];
            buttonCardColor2.BackColor = settings.colorsNotes[2];
            buttonCardColor3.BackColor = settings.colorsNotes[3];
            buttonCardColor4.BackColor = settings.colorsNotes[4];
            buttonFontColorBody.BackColor = settings.colorBody;
            buttonFontColorTitle.BackColor = settings.colorTitle;
            buttonCardBackgroundColor.BackColor = settings.colorCardsBackground;
            labelFontTitle.Font = settings.fontTitle;
            labelFontBody.Font = settings.fontBody;

            Debug.WriteLine("Font Name: " + settings.fontTitle.Name);
            Debug.WriteLine("Font Family Name: " + settings.fontTitle.FontFamily.Name);
            Debug.WriteLine("Font Style: " + settings.fontTitle.Style);
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void labelFontTitle_Click(object sender, EventArgs e)
        {
            SelectFont(ref settings.fontTitle);
        }

        private void labelFontBody_Click(object sender, EventArgs e)
        {
            SelectFont(ref settings.fontBody);
        }

        private void buttonDefault_Click(object sender, EventArgs e)
        {
            settings = new Settings();
            SetFormFromSettings();
        }
    }
}
