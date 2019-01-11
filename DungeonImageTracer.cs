using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using System.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;


namespace Historia
{
    public class DungeonImageTracer
    {
        const string FloorPath = "Dungeon Imaging/FloorMap_Floor";
        const string VoidPath = "Dungeon Imaging/FloorMap_Void";
        const string CollOverlayPath = "Dungeon Imaging/Collision Overlays";
        const string EntryPath = "Dungeon Imaging/FloorMap_WorldTransition";
        const string BossPath = "Dungeon Imaging/FloorMap_BossRoom";
        const string StairsPath = "Dungeon Imaging/FloorMap_InternalTransition";

        public static int FindFirstVersion()
        {
            int FirstVersion = 0;
            while (File.Exists("Content/Dungeon Imaging/Output/Map_" + FirstVersion + "_FloorPlan.png"))
            {
                FirstVersion++;
            }
            return FirstVersion;
        }

        public static void CreateandSaveFloorMapImage(bool[,] FloorMap, Vector2 MapSize, int Version, List<Room> Rooms)
        {
            RenderTarget2D Target = new RenderTarget2D(ScreenManager.Instance.GraphicsDevice,(int)MapSize.X * 8, (int) MapSize.Y * 8);
            ContentManager content = new ContentManager(ScreenManager.Instance.Content.ServiceProvider, "Content");


            ScreenManager.Instance.GraphicsDevice.SetRenderTarget(Target);
            ScreenManager.Instance.GraphicsDevice.Clear(Color.Transparent);
            ScreenManager.Instance.SpriteBatch.Begin();

            Texture2D Floor = content.Load<Texture2D>(FloorPath);
            Texture2D Void = content.Load<Texture2D>(VoidPath);
          

            for (int X = 0; X < MapSize.X; X++)
            {
                for(int Y = 0; Y < MapSize.Y; Y++)
                {
                    if (FloorMap[X, Y])
                    {
                        ScreenManager.Instance.SpriteBatch.Draw(Floor, new Rectangle(X * 8, Y * 8, 8, 8), Color.White);
                    }
                    else
                    {
                        ScreenManager.Instance.SpriteBatch.Draw(Void, new Rectangle(X * 8, Y * 8, 8, 8), Color.White);
                    }
                }
            }

            int EntryRoom = -1;
            int BossRoom = -1;
            int StairsRoom = -1;
            for(int I = 0; I < Rooms.Count; I++)
            {
                if(Rooms[I].Purpose == "Entry")
                {
                    EntryRoom = I;
                }
                else if(Rooms[I].Purpose == "Stairs")
                {
                    StairsRoom = I;
                }
                else if(Rooms[I].Purpose == "Boss")
                {
                    BossRoom = I;
                }
            }

            if(EntryRoom >= 0)
            {
                Texture2D Entry = content.Load<Texture2D>(EntryPath);
                for (int X = Rooms[EntryRoom].Location[0].X; X < Rooms[EntryRoom].Location[0].Right; X++)
                {
                    for(int Y = Rooms[EntryRoom].Location[0].Y; Y < Rooms[EntryRoom].Location[0].Bottom; Y++)
                    {
                        ScreenManager.Instance.SpriteBatch.Draw(Entry, new Rectangle(X * 8, Y * 8, 8, 8), Color.White);
                    }
                }
            }
            if(BossRoom >= 0)
            {
                Texture2D Boss = content.Load<Texture2D>(BossPath);
                for (int X = Rooms[BossRoom].Location[0].X; X < Rooms[BossRoom].Location[0].Right; X++)
                {
                    for (int Y = Rooms[BossRoom].Location[0].Y; Y < Rooms[BossRoom].Location[0].Bottom; Y++)
                    {
                        ScreenManager.Instance.SpriteBatch.Draw(Boss, new Rectangle(X * 8, Y * 8, 8, 8), Color.White);
                    }
                }
            }
            if (StairsRoom >= 0)
            {
                Texture2D Stairs = content.Load<Texture2D>(StairsPath);
                for (int X = Rooms[StairsRoom].Location[0].X; X < Rooms[StairsRoom].Location[0].Right; X++)
                {
                    for (int Y = Rooms[StairsRoom].Location[0].Y; Y < Rooms[StairsRoom].Location[0].Bottom; Y++)
                    {
                        ScreenManager.Instance.SpriteBatch.Draw(Stairs, new Rectangle(X * 8, Y * 8, 8, 8), Color.White);
                    }
                }
            }




            ScreenManager.Instance.SpriteBatch.End();

            

            var ImageStream = new FileStream("Content/Dungeon Imaging/Output/Map_" + Version+ "_FloorPlan.png", FileMode.Create);

            
            Target.SaveAsPng(ImageStream, Target.Width, Target.Height);

            ImageStream.Close();

 
            ScreenManager.Instance.GraphicsDevice.SetRenderTarget(null);
            content.Unload(); 
        }

        public static void CreateandSaveFullMap(Map Map, int Version, bool CollisionOverlay)
        {
            RenderTarget2D Target = new RenderTarget2D(ScreenManager.Instance.GraphicsDevice, (int)Map.Size.X * (int)Map.TileDimensions.X, (int)Map.Size.Y * (int)Map.TileDimensions.Y);
            ContentManager content = new ContentManager(ScreenManager.Instance.Content.ServiceProvider, "Content");


            ScreenManager.Instance.GraphicsDevice.SetRenderTarget(Target);
            ScreenManager.Instance.GraphicsDevice.Clear(Color.Transparent);
            ScreenManager.Instance.SpriteBatch.Begin();

            Rectangle TheWholeMap = new Rectangle(0, 0, (int)Map.Size.X - 1, (int)Map.Size.Y - 1);

            Map.Draw(ScreenManager.Instance.SpriteBatch, RectMethod.ShrinkHeight(RectMethod.ShrinkWidth(Map.FullMap)) , Vector2.Zero, Vector2.Zero);
            if (CollisionOverlay)
            {
                Image Overlay = new Image();
                Overlay.Path = CollOverlayPath;
                Overlay.LoadContent();
                Overlay.Alpha = 0.4f;

                for(int X = 0; X < Map.Size.X; X++)
                {
                    for(int Y = 0; Y < Map.Size.Y; Y++)
                    {
                        int Collision = Map.Collision.CMap[X, Y];
                        int CollisionY = (Collision / 10)*32;
                        int CollisionX = (Collision % 10)*32;
                        ScreenManager.Instance.SpriteBatch.Draw(Overlay.Texture, new Rectangle(X * 32, Y * 32, 32, 32), new Rectangle(CollisionX, CollisionY, 32, 32), Color.White*Overlay.Alpha);
                    }
                }
            }


            ScreenManager.Instance.SpriteBatch.End();
            var ImageStream = new FileStream("Content/Dungeon Imaging/Output/Map_" + Version + "_Final.png", FileMode.Create);


            Target.SaveAsPng(ImageStream, Target.Width, Target.Height);

            ImageStream.Close();


            ScreenManager.Instance.GraphicsDevice.SetRenderTarget(null);
            content.Unload();
        }

    }
}
