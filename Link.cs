using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrainStormBoard
{
    public class Link
    {
        Point[] points;
        Card m_cardOne;
        Card m_cardTwo;

        public Card CardOne { get { return m_cardOne; } }
        public Card CardTwo { get { return m_cardTwo; } }

        public Link(Card c1, Card c2)
        {
            if (c1.CardId < c2.CardId)
            {
                m_cardOne = c1;
                m_cardTwo = c2;

            }
            else 
            {
                m_cardOne = c2;
                m_cardTwo = c1;

            }
            points = new Point[2];
            UpdatePositions();
        }

        public void UpdatePositions()
        {
            Point p1 = m_cardOne.GetAnchorPosition(m_cardTwo.Location);
            Point p2 = m_cardTwo.GetAnchorPosition(m_cardOne.Location);
            points[0] = p1;
            points[1] = p2;
        }

        public void Draw(Graphics graphics, Pen pen)
        {
            //graphics.DrawCurve(pen, points);
            graphics.DrawLine(pen, points[0], points[1]);
        }

        public bool IsTheSame(Link link)
        {
            // Cards are sorted by Id
            return link.m_cardTwo.CardId == m_cardTwo.CardId && link.m_cardOne.CardId == m_cardOne.CardId;
        }

        public Card GetOtherCard(Card card)
        {
            if (m_cardOne.CardId == card.CardId)
            {
                return m_cardTwo;
            }
            else if (m_cardTwo.CardId == card.CardId)
            {
                return m_cardOne;
            }

            return null;
        }
    }
}
