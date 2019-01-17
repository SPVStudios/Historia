using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Xml.Serialization;
using System.Xml;

namespace Historia
{
    public class HealthBar
    {
        public Image Foreground;
        public Image Background;

        int AmountofPixelsInFull;

        public HealthBar()
        {
            Foreground = new Image();
            Background = new Image();
        }


        public void LoadContent()
        {
            Foreground.LoadContent();
            AmountofPixelsInFull = Foreground.SourceRect.Width;
            Background.LoadContent();
        }

        public void Draw(SpriteBatch spriteBatch, int CurrentHealth, int MaxHealth)
        {
            Background.Draw(spriteBatch);
            int PixelsWide = (AmountofPixelsInFull * CurrentHealth) / MaxHealth;
            Foreground.SourceRect.Width = PixelsWide;
            Foreground.Draw(spriteBatch);
        }




    }
}
