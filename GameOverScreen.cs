using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Historia
{
    public class GameOverScreen : GameScreen
    {

        MenuManager menuManager;

        public GameOverScreen()
        {
            menuManager = new MenuManager();
        }

        public override void LoadContent()
        {
            base.LoadContent();
            menuManager.LoadContent("Load/Menus/GameOver.xml");
        }

        public override void UnloadContent()
        {
            base.UnloadContent();
            menuManager.UnloadContent();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            menuManager.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            menuManager.Draw(spriteBatch);
        }


    }
}
