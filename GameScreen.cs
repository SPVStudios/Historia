using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Xml.Serialization;



namespace Historia
{
    public class GameScreen
    {
        protected ContentManager Content;
        [XmlIgnore]
        public Type Type;//this variable is ignored by the xmlserializer as we don't want it to do anything with this!

        public string XmlPath;

        public GameScreen()
        {
            Type = this.GetType();//retrieves the type of this instance to be called elsewhere

            XmlPath = "Load/"+Type.ToString().Replace("Historia.", "") +".xml";//this makes the XMLManager look to the path
            //SHARING A NAME with the class name of the game screen, so long as it is in the LOAD folder of the solution.
        }

        public virtual void LoadContent()
        {
            Content = new ContentManager(ScreenManager.Instance.Content.ServiceProvider, "Content");
            //calls the content from the ScreenManager instance currently running and places it in a
            //variable "content" that has been defined above as protected, meaning all methods can use
            //this data easily.
        }

        public virtual void UnloadContent()
        {
            Content.Unload();//this will remove all content called into the GameScreen once done with it.
        }

        public virtual void Update(GameTime gameTime)
        {
            InputManager.Instance.Update(gameTime);//updates the (only) instance of InputManager by default - all game screens need it.
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {

        }
    }
}
