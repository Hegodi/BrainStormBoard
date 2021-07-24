using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace BrainStormBoard
{
    public delegate void CallbackFromCard(Card card);

    enum EditMode
    {
        EditCards,
        AddCard,
        CreateLink,
        RemoveLink,
        COUNT
    }


    public partial class MainForm : Form
    {
        float scale = 1.0f;
        List<Card> m_cards;
        List<Link> m_links;
        ulong lastCardId = 0;
        EditMode editMode = EditMode.EditCards;
        Card cardSelected = null;
        Point mousePosition;
        bool allExpanded = true;

        string currentDocumentName = "";
        static public bool isDirty = false;

        static public Settings settings = new Settings();
        static bool m_snapToGrid = false;
        static public bool snapToGrid { get { return m_snapToGrid; } }
        static public readonly int GridSize = 50;

        public MainForm()
        {
            InitializeComponent();
            labelInfo.Text = "";
            openFileDialog1.InitialDirectory = Application.StartupPath;
            m_cards = new List<Card>();
            m_links = new List<Link>();
            panelMainBoard.Size = new Size(5000, 5000);
            settings.Load();
            OnSettingsUpdated();
            labelInfo.Text = "New document created (not saved)";

        }

        private void panelMainBoard_MouseClick(object sender, MouseEventArgs e)
        {
            switch (editMode)
            {
                case EditMode.EditCards:
                case EditMode.AddCard:
                    if (e.Button == MouseButtons.Right)
                    {
                        AddCard(e.Location);
                    }
                    break;
                case EditMode.CreateLink:
                case EditMode.RemoveLink:
                    editMode = EditMode.AddCard;
                    cardSelected = null;
                    panelMainBoard.Refresh();
                    break;
                case EditMode.COUNT:
                    break;
            }
        }
        private void panelMainBoard_MouseWheel(object sender, MouseEventArgs e)
        {
            // TODO: add zoom
        }

        private void panelMainBoard_Paint(object sender, PaintEventArgs e)
        {
            Pen pen = new Pen(settings.colorLinks);
            pen.Width = 3;
            foreach(Link link in m_links)
            {
                link.Draw(e.Graphics, pen);
            }

            if (editMode == EditMode.CreateLink && cardSelected != null)
            {
                e.Graphics.DrawLine(pen, cardSelected.GetAnchorPosition(mousePosition), mousePosition);
            }
        }

        private void OnLinkCard(Card card)
        {
            Debug.WriteLine("Start Link");
            editMode = EditMode.CreateLink;
            cardSelected = card;
        }
        private void OnUnlinkCard(Card card)
        {
            Debug.WriteLine("Remove Link");
            editMode = EditMode.RemoveLink;
            cardSelected = card;
        }

        private void OnCardSelected(Card card)
        {
            if (cardSelected != null)
            {
                if (editMode == EditMode.CreateLink)
                {
                    AddLink(cardSelected, card);
                    cardSelected = null;
                }
                else if (editMode == EditMode.RemoveLink)
                {
                    RemoveLink(cardSelected, card);
                    cardSelected = null;
                }
            }
        }

        private void OnCardMoved(Card card)
        {
            foreach (Link link in card.links)
            {
                link.UpdatePositions();
            }
            panelMainBoard.Refresh();
            isDirty = true;
        }

        private void OnCardDeleted(Card card)
        {
            Debug.WriteLine("Remove Card");
            foreach (Link link in card.links)
            {
                Card other = link.GetOtherCard(card);
                Debug.Assert(other != null);
                other.RemoveLink(link);
                RemoveLinkFromList(link);
            }

            RemoveCardFromList(card);
            panelMainBoard.Refresh();
            isDirty = true;
        }

        private void AddCard(Point location)
        {
            lastCardId++;
            if (lastCardId > 4294967000)
            {
                MessageBox.Show("You have created/deleted TOO many boxes! I didn't expect someone would use this so much. If you keep creating boxes you migth experience unexpected behaviors", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            Card card = new Card(lastCardId);
            card.Location = location;
            AddCardToPanel(card);
            panelMainBoard.ResumeLayout(true);
            m_cards.Add(card);
            isDirty = true;
        }

        private void AddCardToPanel(Card card)
        {
            card.callbackLink = OnLinkCard;
            card.callbackUnlink = OnUnlinkCard;
            card.callbackCardSelected = OnCardSelected;
            card.callbackCardMoved = OnCardMoved;
            card.callbackCardDeleted = OnCardDeleted;
            card.Scale(new SizeF(this.scale, this.scale));
            panelMainBoard.Controls.Add(card);
        }

        private void AddLink(Card c1, Card c2, bool refresh = true)
        {
            Link link = new Link(c1, c2);
            c1.links.Add(link);
            c2.links.Add(link);
            m_links.Add(link);
            editMode = EditMode.AddCard;
            if (refresh)
            {
                panelMainBoard.Refresh();
            }
            isDirty = true;
        }

        private void RemoveLink(Card c1, Card c2)
        {
            Link linkToRemove = new Link(c1, c2);
            RemoveLinkFromList(linkToRemove);
            c1.RemoveLink(linkToRemove);
            c2.RemoveLink(linkToRemove);
            panelMainBoard.Refresh();
            isDirty = true;
        }

        private void RemoveLinkFromList(Link link)
        {
            for (int i=0; i<m_links.Count; i++)
            {
                if (m_links[i].IsTheSame(link))
                {
                    m_links.RemoveAt(i);
                    break;
                }
            }
        }

        private void RemoveCardFromList(Card card)
        {
            for (int i=0; i<m_cards.Count; i++)
            {
                if (card.CardId == m_cards[i].CardId)
                {
                    m_cards.RemoveAt(i);
                    break;
                }
            }

            panelMainBoard.Controls.Remove(card);
        }

        private void panelMainBoard_MouseMove(object sender, MouseEventArgs e)
        {
            if (editMode == EditMode.CreateLink)
            {
                mousePosition = e.Location;
                panelMainBoard.Refresh();
            }
        }

        private void SaveDocument(string filename)
        {
            XmlWriterSettings xmlSettings = new XmlWriterSettings()
            {
                Indent = true,
                IndentChars = "    "
            };

            XmlWriter xmlWriter = XmlWriter.Create(filename, xmlSettings);

            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("Document");

            xmlWriter.WriteStartElement("Cards");
            foreach (Card card in m_cards)
            {
                xmlWriter.WriteStartElement("card");
                xmlWriter.WriteAttributeString("id", card.CardId.ToString());
                xmlWriter.WriteAttributeString("x", card.Location.X.ToString());
                xmlWriter.WriteAttributeString("y", card.Location.Y.ToString());
                xmlWriter.WriteAttributeString("color", card.CardColor.ToString());
                xmlWriter.WriteAttributeString("expanded", card.IsExpanded.ToString());

                xmlWriter.WriteStartElement("title");
                xmlWriter.WriteString(card.CardTitle);
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("text");
                xmlWriter.WriteString(card.CardText);
                xmlWriter.WriteEndElement();


                xmlWriter.WriteEndElement();
            }
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("Links");
            foreach (Link link in m_links)
            {
                xmlWriter.WriteStartElement("link");
                xmlWriter.WriteAttributeString("id1", link.CardOne.CardId.ToString());
                xmlWriter.WriteAttributeString("id2", link.CardTwo.CardId.ToString());
                xmlWriter.WriteEndElement();
            }
            xmlWriter.WriteEndElement();

            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();
            xmlWriter.Close();
            labelInfo.Text = filename + " (SAVED) ";
            isDirty = false;
            currentDocumentName = filename;
        }

        private void LoadDocument(string filename)
        {
            CleanUpDocument();

            XmlReader xmlReader = XmlReader.Create(filename);

            Card card = null;
            List<Tuple<ulong, ulong>> linksIds = new List<Tuple<ulong, ulong>>();
            Dictionary<ulong, Card> dictionaryCards = new Dictionary<ulong, Card>();

            while (xmlReader.Read())
            {
                if (xmlReader.NodeType == XmlNodeType.Element)
                {
                    if (xmlReader.Name == "card")
                    {
                        ulong id = ulong.Parse(xmlReader.GetAttribute("id"));
                        int x = int.Parse(xmlReader.GetAttribute("x"));
                        int y = int.Parse(xmlReader.GetAttribute("y"));
                        int color = int.Parse(xmlReader.GetAttribute("color"));
                        bool expanded = bool.Parse(xmlReader.GetAttribute("expanded"));
                        card = new Card(id, color);
                        card.Location = new Point(x, y);
                        card.IsExpanded = expanded;
                        m_cards.Add(card);
                        dictionaryCards.Add(id, card);
                    }
                    else if (xmlReader.Name == "title")
                    {
                        if (card != null)
                        {
                            card.CardTitle = xmlReader.ReadElementContentAsString();
                        }
                    }
                    else if (xmlReader.Name == "text")
                    {
                        if (card != null)
                        {
                            card.CardText = xmlReader.ReadElementContentAsString();
                        }
                    }
                    else if (xmlReader.Name == "link")
                    {
                        ulong id1 = ulong.Parse(xmlReader.GetAttribute("id1"));
                        ulong id2 = ulong.Parse(xmlReader.GetAttribute("id2"));
                        linksIds.Add(new Tuple<ulong, ulong>(id1, id2));
                    }
                }
                else if (xmlReader.NodeType == XmlNodeType.EndElement)
                {
                    card = null;
                }
            }

            xmlReader.Close();

            foreach (Card c in m_cards)
            {
                AddCardToPanel(c);
            }

            foreach(var ids in linksIds)
            {
                Card c1 = dictionaryCards[ids.Item1];
                Card c2 = dictionaryCards[ids.Item2];
                AddLink(c1, c2, false);
            }
            panelMainBoard.ResumeLayout(true);
            panelMainBoard.Refresh();
            isDirty = false;
            currentDocumentName = filename;
            labelInfo.Text = filename;
        }

        private void CollapsExpandAll()
        {
            allExpanded = !allExpanded;
            foreach (Card card in m_cards)
            {
                card.IsExpanded = allExpanded;
            }
        }

        private void CleanUpDocument()
        {
            m_cards.Clear();
            m_links.Clear();
            panelMainBoard.Controls.Clear();
            panelMainBoard.Refresh();
            currentDocumentName = "";
            isDirty = false;
        }

        private void toolStripButtonSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(currentDocumentName))
            {
                SaveDocumentAs();
            }
            else
            {
                SaveDocument(currentDocumentName);
            }
        }
        private void toolStripButtonSaveAs_Click(object sender, EventArgs e)
        {
            SaveDocumentAs();
        }

        private void SaveDocumentAs()
        {
            openFileDialog1.CheckFileExists = false;
            openFileDialog1.FileName = "NewDocument.bsb";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                SaveDocument(openFileDialog1.FileName);
            }
        }

        private void toolStripButtonCollapseExpandAll_Click(object sender, EventArgs e)
        {
            CollapsExpandAll();
        }

        private void toolStripButtonOpen_Click(object sender, EventArgs e)
        {
            bool canOpen = true;
            if (isDirty)
            {
                canOpen = MessageBox.Show("All unsaved changes will be lost. Do you want to continue and load another Document?", "Open Document", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes;
            }
            if (canOpen)
            {
                openFileDialog1.CheckFileExists = true;
                openFileDialog1.FileName = "";
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    LoadDocument(openFileDialog1.FileName);
                }
            }
        }

        private void toolStripButtonNew_Click(object sender, EventArgs e)
        {
            bool canClear = true;
            if (isDirty)
            {
                canClear = MessageBox.Show("All unsaved changes will be lost. Do you want to continue and create a new Document?", "New Document", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes;
            }

            if (canClear)
            {
                CleanUpDocument();
                labelInfo.Text = "New document created (not saved)";
            }
        }

        private void OnSettingsUpdated()
        {
            foreach (Card card in m_cards)
            {
                card.UpdateVisualsFromSettings();
            }
            panelMainBoard.BackColor = settings.colorBoardBackground;
            panelMainBoard.Refresh();
        }

        private void toolStripButtonSettings_Click(object sender, EventArgs e)
        {
            SettingsForm settingsForm = new SettingsForm(settings);
            if (settingsForm.ShowDialog() == DialogResult.OK)
            {
                settings = new Settings(settingsForm.settings);
                settings.Save();
                OnSettingsUpdated();
            }
        }

        private void toolStripButtonSnap_Click(object sender, EventArgs e)
        {
            m_snapToGrid = !m_snapToGrid;
        }

        private void toolStripButtonAbout_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Brain Strom Board - v 1.0\n" +
                "\n" +
                "Add cards using the right mouse button. Move cards around holding the left button\n\n" +
                "Developed by Diego Gonzalez Herrero \n" + 
                "diegonher@gmail.com", "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
