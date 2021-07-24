using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Security.Permissions;
using System.Windows.Forms;
using System.Xml;

namespace BrainStormBoard
{
    public class Settings
    {

        public Color[] colorsNotes;
        public Font fontTitle;
        public Font fontBody;
        public Color colorTitle;
        public Color colorBody;
        public Color colorBoardBackground;
        public Color colorCardsBackground;
        public Color colorLinks;

        static private readonly string filename = "settings.xml";

        public Settings()
        {
            colorsNotes = new Color[5];
            colorsNotes[0] =  Color.NavajoWhite;
            colorsNotes[1] =  Color.Salmon;
            colorsNotes[2] =  Color.LightGreen;
            colorsNotes[3] =  Color.LightSkyBlue;
            colorsNotes[4] =  Color.Violet;
            fontTitle = new Font("Microsoft Sans Serif", 12, FontStyle.Bold);
            fontBody = new Font("Microsoft Sans Serif", 8, FontStyle.Regular);
            colorBoardBackground = Color.LightGray;
            colorCardsBackground = Color.White;
            colorLinks = Color.Black;
            colorBody = Color.Black;
            colorTitle = Color.Black;
        }

        public Settings(Settings settings)
        {
            colorsNotes = new Color[5];
            colorsNotes[0] = settings.colorsNotes[0];
            colorsNotes[1] = settings.colorsNotes[1];
            colorsNotes[2] = settings.colorsNotes[2];
            colorsNotes[3] = settings.colorsNotes[3];
            colorsNotes[4] = settings.colorsNotes[4];
            fontTitle = settings.fontTitle;
            fontBody = settings.fontBody;
            colorBoardBackground = settings.colorBoardBackground;
            colorCardsBackground = settings.colorCardsBackground;
            colorLinks = settings.colorLinks;
            colorBody = settings.colorBody;
            colorTitle = settings.colorTitle;
        }

        public void Save()
        {
            XmlWriterSettings xmlSettings = new XmlWriterSettings()
            {
                Indent = true,
                IndentChars = "    "
            };


            XmlWriter xmlWriter = null;
            try
            {
                if (File.Exists(filename))
                {
                    File.Delete(filename);
                }
                FileStream fs = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write);
                xmlWriter = XmlWriter.Create(fs, xmlSettings);
                xmlWriter.WriteStartDocument();
                xmlWriter.WriteStartElement("Settings");

                for (int i = 0; i < 5; i++)
                {
                    WriteColor(xmlWriter, "colorNote" + i.ToString(), colorsNotes[i]);
                }

                WriteColor(xmlWriter, "colorBoardBackground", colorBoardBackground);
                WriteColor(xmlWriter, "colorCardsBackground", colorCardsBackground);
                WriteColor(xmlWriter, "colorLinks", colorLinks);
                WriteColor(xmlWriter, "colorBody", colorBody);
                WriteColor(xmlWriter, "colorTitle", colorTitle);
                WriteFont(xmlWriter, "fontTitle", fontTitle);
                WriteFont(xmlWriter, "fontBody", fontBody);


                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndDocument();
                xmlWriter.Close();
                fs.Close();
            }
            catch 
            {
                if (xmlWriter != null)
                {
                    xmlWriter.Close();
                }
                MessageBox.Show("Settings cannot be saved in " + filename + "\nMake sure the executable is not inside a zip file, the file " + filename + " is not open and that you can create files inside that folder", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        static private void WriteColor(XmlWriter xmlWriter, string name, Color color)
        {
            xmlWriter.WriteStartElement(name);
            xmlWriter.WriteAttributeString("r", color.R.ToString());
            xmlWriter.WriteAttributeString("g", color.G.ToString());
            xmlWriter.WriteAttributeString("b", color.B.ToString());
            xmlWriter.WriteEndElement();
        }

        static private bool TryLoadColor(XmlReader xmlReader, string name, ref Color color)
        {
            if (xmlReader.Name == name)
            {
                int r = int.Parse(xmlReader.GetAttribute("r"));
                int g = int.Parse(xmlReader.GetAttribute("g"));
                int b = int.Parse(xmlReader.GetAttribute("b"));
                color = Color.FromArgb(255, r, g, b);
                return true;
            }
            return false;
        }

        static private void WriteFont(XmlWriter xmlWriter, string name, Font font)
        {
            xmlWriter.WriteStartElement(name);
            xmlWriter.WriteAttributeString("fontFamily", font.FontFamily.Name);
            xmlWriter.WriteAttributeString("fontStyle", ((int)font.Style).ToString());
            xmlWriter.WriteAttributeString("fontSize", font.Size.ToString());
            xmlWriter.WriteEndElement();
        }
        static private bool TryLoadFont(XmlReader xmlReader, string name, ref Font font)
        {
            if (xmlReader.Name == name)
            {
                string fontFamily = xmlReader.GetAttribute("fontFamily");
                FontStyle fontStyle = (FontStyle) int.Parse(xmlReader.GetAttribute("fontStyle"));
                float fontSize = float.Parse(xmlReader.GetAttribute("fontSize"));
                font = new Font(fontFamily, fontSize, fontStyle);
                return true;
            }
            return false;
        }

        public void Load()
        {
            XmlReader xmlReader = null;
            try
            {
                xmlReader = XmlReader.Create(filename);
                while (xmlReader.Read())
                {
                    if (xmlReader.NodeType == XmlNodeType.Element)
                    {
                        bool found = false;
                        for (int i = 0; i < 5; i++)
                        {
                            if (TryLoadColor(xmlReader, "colorNote" + i.ToString(), ref colorsNotes[i]))
                            {
                                found = true;
                                break;
                            }
                        }
                        if (found)
                        {
                            continue;
                        }

                        if (TryLoadColor(xmlReader, "colorBoardBackground", ref colorBoardBackground)) { }
                        else if (TryLoadColor(xmlReader, "colorCardsBackground", ref colorCardsBackground)) { }
                        else if (TryLoadColor(xmlReader, "colorLinks", ref colorLinks)) { }
                        else if (TryLoadColor(xmlReader, "colorBody", ref colorBody)) {}
                        else if (TryLoadColor(xmlReader, "colorTitle", ref colorTitle)) {}
                        else if (TryLoadFont(xmlReader, "fontTitle", ref fontTitle)) {}
                        else if (TryLoadFont(xmlReader, "fontBody", ref fontBody)) {}
                    }
                }
                xmlReader.Close();
            }
            catch
            {
                if (xmlReader != null)
                {
                    xmlReader.Close();
                }
                Debug.WriteLine("Error while loading settings");
            }


        }
    }
}
