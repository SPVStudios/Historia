using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.Drawing;
using Microsoft.Xna.Framework.Content;


namespace Historia
{
    public class DungeonMassProductionScreen : GameScreen
    {
        int I;
        const int Max = 10;
        Image Image;
        int FirstVersion;


        public override void LoadContent()
        {
            base.LoadContent();
            Image = new Image();
            Image.Text = "Creating" + Max + "Dungeons, then exiting";
            Image.LoadContent();
            I = 0;
           
        }

        public override void UnloadContent()
        {
            base.UnloadContent();
            if (Image != null)
            {
                Image.UnloadContent();
            }
           
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (Image == null)
            {
                Image = new Image();
                Image.Text = "Creating" + Max + "Dungeons, then exiting";
                Image.LoadContent();
            }
            Image.Update(gameTime);
            if (I < Max)
            {
                FirstVersion = DungeonImageTracer.FindFirstVersion();

                List<Room> MapRooms = new List<Room>();
                List<Passageway> Passages = new List<Passageway>();
                Vector2 MapSize = new Vector2();

                bool[,] floorMap;
                while (true)
                {
                    if (DungeonCreator.CreateFloorMap(new Vector2(4, 7),
                        new Vector2(2, 2), new Vector2(30,45), ref MapRooms,
                        ref Passages, ref MapSize, out floorMap))
                    {
                        break;
                    }//else try again. This allows invalid maps to be rejected rather than starting over.
                }

                TileSet floorTileSet = new TileSet();
                TileSet wallsTileSet = new TileSet();
                TileSet entryTileSet = new TileSet();
                ObjectTileSet ObjectSet = new ObjectTileSet();
                ObjectTileSet SmallObjectSet = new ObjectTileSet();
                ObjectTileSet LargeObjectSet = new ObjectTileSet();
                XmlManager<TileSet> tileSetLoad = new XmlManager<TileSet>();
                XmlManager<ObjectTileSet> ObjLoad = new XmlManager<ObjectTileSet>();
                floorTileSet = tileSetLoad.Load("Load/Gameplay/TileSets/Grass_Floors.xml");
                wallsTileSet = tileSetLoad.Load("Load/Gameplay/TileSets/Grass_Walls.xml");
                ObjectSet = ObjLoad.Load("Load/Gameplay/TileSets/Grass_Objects.xml");
                SmallObjectSet = ObjLoad.Load("Load/Gameplay/TileSets/Grass_Objects_Small.xml");
                LargeObjectSet = ObjLoad.Load("Load/Gameplay/TileSets/Grass_Objects_Large.xml");

                floorTileSet.LoadContent();
                wallsTileSet.LoadContent();
                entryTileSet.LoadContent();
                ObjectSet.LoadContent();
                SmallObjectSet.LoadContent();
                LargeObjectSet.LoadContent();

                List<ObjectTileSet> OSets = new List<ObjectTileSet>() {SmallObjectSet, ObjectSet, LargeObjectSet};

                DungeonImageTracer.CreateandSaveFloorMapImage(floorMap, MapSize, FirstVersion+I ,MapRooms);

                Map FullMap = DungeonCreator.CreateMap(MapSize, floorMap, wallsTileSet, floorTileSet,entryTileSet, OSets, MapRooms, Passages, new List<string>());

                DungeonImageTracer.CreateandSaveFullMap(FullMap, FirstVersion + I,true);

                I++;
            }
            else
            {
                if (!ScreenManager.Instance.IsTransitioning)
                {
                    ScreenManager.Instance.ChangeScreens("TitleScreen");
                }
                
            }

        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            if (Image != null)
            {
                Image.Draw(spriteBatch);
            }
        }



    }
}
