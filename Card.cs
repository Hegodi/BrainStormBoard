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
    public partial class Card : UserControl
    {
        Point grabOffset;
        bool isGrabbed = false;
        public CallbackFromCard callbackLink = null;
        public CallbackFromCard callbackUnlink = null;
        public CallbackFromCard callbackCardSelected = null;
        public CallbackFromCard callbackCardMoved = null;
        public CallbackFromCard callbackCardDeleted = null;
        public List<Link> links;
        ulong cardId = 0;
        int cardColor = -1;
        public ulong CardId { get { return cardId; } }
        public int CardColor { get { return cardColor; } set { SetBackColor(value); } }
        public bool IsExpanded { 
            get { return expanded; }
            set
            {
                expanded = value;
                if (expanded)
                {
                    Size = new Size(Width, 250);
                }
                else
                {
                    Size = new Size(Width, 65);
                }
            }
        }
        bool expanded = true;

        public string CardText { get { return textBoxNote.Text; } set { textBoxNote.Text = value; } }
        public string CardTitle { get { return textBoxTitle.Text; } set { textBoxTitle.Text = value; } }

        public Card(ulong id)
        {
            cardId = id;
            links = new List<Link>();
            InitializeComponent();

            UpdateVisualsFromSettings();
            SetBackColor(0);
            IsExpanded = true;
        }
        public Card(ulong id, int color)
        {
            cardId = id;
            links = new List<Link>();
            InitializeComponent();

            UpdateVisualsFromSettings();
            SetBackColor(color);
            IsExpanded = true;
        }

        private void Card_Click(object sender, EventArgs e)
        {
            if (callbackCardSelected != null)
            {
                callbackCardSelected(this);
            }
        }

        private void Card_MouseMove(object sender, MouseEventArgs e)
        {
            if (isGrabbed)
            {
                Point mouseLocation = Parent.PointToClient(PointToScreen(e.Location));
                if (MainForm.snapToGrid)
                {
                    int x = MainForm.GridSize * (int)((mouseLocation.X - grabOffset.X) / MainForm.GridSize);
                    int y = MainForm.GridSize * (int)((mouseLocation.Y - grabOffset.Y) / MainForm.GridSize);
                    Location = new Point(x, y);

                }
                else
                {
                    Location = new Point(mouseLocation.X - grabOffset.X, mouseLocation.Y - grabOffset.Y);
                }

                MainForm.isDirty = true;
                if (callbackCardMoved != null)
                {
                    callbackCardMoved(this);
                }
            }
        }

        private void Card_MouseUp(object sender, MouseEventArgs e)
        {
            isGrabbed = false;
        }

        private void Card_MouseLeave(object sender, EventArgs e)
        {
            isGrabbed = false;
        }

        private void Card_MouseDown(object sender, MouseEventArgs e)
        {
            grabOffset = e.Location;
            isGrabbed = true;
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            if (callbackLink != null)
            {
                callbackLink(this);
            }
        }
        private void buttonUnlink_Click(object sender, EventArgs e)
        {
            if (callbackUnlink != null)
            {
                callbackUnlink(this);
            }
        }
        public Point GetAnchorPosition(Point target)
        {
            Point p = Location;
            float dX = Location.X - target.X;
            float dY = Location.Y - target.Y;

            if (dX >= 0.0f && Math.Abs(dY) <= dX)
            {
                p.Y += Height / 2;
            }
            else if (dX <= 0.0f && Math.Abs(dY) <= -dX)
            {
                p.Y += Height / 2;
                p.X += Width;
            }
            else if (dY > 0.0f && Math.Abs(dX) < dY)
            {
                p.X += Width / 2;
            }
            else if (dY < 0.0f && Math.Abs(dX) < -dY)
            {
                p.X += Width / 2;
                p.Y += Height;
            }

            return p;
        }

        private void buttonExpandColapse_Click(object sender, EventArgs e)
        {
            IsExpanded = !expanded;
            MainForm.isDirty = true;
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            if (callbackCardDeleted != null)
            {
                callbackCardDeleted(this);
            }
        }

        private void buttonColor0_Click(object sender, EventArgs e)
        {
            SetBackColor(0);                
        }

        private void buttonColor1_Click(object sender, EventArgs e)
        {
            SetBackColor(1);
        }

        private void buttonColor2_Click(object sender, EventArgs e)
        {
            SetBackColor(2);
        }

        private void buttonColor3_Click(object sender, EventArgs e)
        {
            SetBackColor(3);
        }

        private void buttonColor4_Click(object sender, EventArgs e)
        {
            SetBackColor(4);
        }

        private void SetBackColor(int value)
        {
            if (value < 0 || value >= 5)
            {
                return;
            }
            MainForm.isDirty = true;
            cardColor = value;
            BackColor = MainForm.settings.colorsNotes[cardColor];
        }

        public void RemoveLink(Link link)
        {
            for (int i=0; i<links.Count; i++)
            {
                if (links[i].IsTheSame(link))
                {
                    links.RemoveAt(i);
                    break;
                }
            }
        }

        public void UpdateVisualsFromSettings()
        {
            textBoxTitle.BackColor = MainForm.settings.colorCardsBackground;
            textBoxTitle.ForeColor = MainForm.settings.colorTitle;
            textBoxTitle.Font = MainForm.settings.fontTitle;

            textBoxNote.BackColor = MainForm.settings.colorCardsBackground;
            textBoxNote.ForeColor = MainForm.settings.colorBody;
            textBoxNote.Font = MainForm.settings.fontBody;

            buttonColor0.BackColor = MainForm.settings.colorsNotes[0];
            buttonColor1.BackColor = MainForm.settings.colorsNotes[1];
            buttonColor2.BackColor = MainForm.settings.colorsNotes[2];
            buttonColor3.BackColor = MainForm.settings.colorsNotes[3];
            buttonColor4.BackColor = MainForm.settings.colorsNotes[4];
            SetBackColor(cardColor);
        }
    }
}

