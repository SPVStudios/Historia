using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Historia
{
    public class Camera
    {

        public Vector2 Focus;
        public Vector2 Offset;
        protected Vector2 mapSize;
        public Vector2 tileSize;
        protected Vector2 tilesOnScreen;
        protected Vector2 theMiddleTile;//the tile on screen that the Camera regards as the centre tile. Won't actually be dead centre.

        public Vector2 Origin; // the top-left full tile on the screen. With no offset it would start at (0,0).


        public Rectangle DrawArea;
        public Vector2 DrawPoint;//integer referring to how many TILES from top left or into void everything should be drawn.




        //^^ the co-ordinate, likely with both parts NEGATIVE, is the point at which the map should start
        // being drawn to allign the map correctly. This should be taken as the Origin of the drawn section of the map
        // when drawing any objects on the map at all.
        //in ACTUAL drawing, the origin will be taken as the Drawpoint - Offset.


        public Camera(Vector2 MapSize, Vector2 TilesOnScreen, Vector2 TileSize, Vector2 FirstPlayerLocation)
        {
            mapSize = MapSize;

            tilesOnScreen = TilesOnScreen;
            theMiddleTile = new Vector2((int)TilesOnScreen.X / 2, (int)TilesOnScreen.Y / 2);
            tileSize = TileSize;

            Focus = FirstAllign(FirstPlayerLocation, theMiddleTile, tilesOnScreen, new Rectangle(0, 0, (int)MapSize.X, (int)mapSize.Y));

            DrawArea = new Rectangle((int)Focus.X - (int)theMiddleTile.X
       , (int)Focus.Y - ((int)theMiddleTile.Y)
       , (int)tilesOnScreen.X, (int)tilesOnScreen.Y);

            Origin = new Vector2(DrawArea.X - DrawPoint.X, DrawArea.Y - DrawPoint.Y);

        }

        public void Update(GameTime gameTime, Vector2 PlayerPosition, Vector2 PlayerOffset, Rectangle FullMap)
        {
            CheckIfFocusCentral(PlayerPosition, FullMap, PlayerOffset);

            if (Offset.X < 0 && Focus.X - theMiddleTile.X > FullMap.X)
            {//extra X tile upwards
                DrawArea.X = (int)Focus.X - ((int)theMiddleTile.X + 1);
                DrawArea.Width = (int)tilesOnScreen.X + 1;
                DrawPoint.X = -1;
            }
            else
            {
                DrawArea.X = (int)Focus.X - (int)theMiddleTile.X;
                DrawArea.Width = (int)tilesOnScreen.X + 1;
                DrawPoint.X = 0;
            }


            if (Offset.Y < 0 && Focus.Y - theMiddleTile.Y > FullMap.Y)
            {
                DrawArea.Y = (int)Focus.Y - ((int)theMiddleTile.Y + 1);
                DrawArea.Height = (int)tilesOnScreen.Y + 1;
                DrawPoint.Y = -1;
            }
            else
            {
                DrawArea.Y = (int)Focus.Y - (int)theMiddleTile.Y;
                DrawArea.Height = (int)tilesOnScreen.Y + 1;
                DrawPoint.Y = 0;
            }

            Origin.X = DrawArea.X - DrawPoint.X;
            Origin.Y = DrawArea.Y - DrawPoint.Y;
        }

        private void CheckIfFocusCentral(Vector2 PlayerPosition, Rectangle FullMap, Vector2 PlayerOffset)
        {
            //The X co-ordinate:
            if (PlayerPosition.X - theMiddleTile.X > FullMap.X + 1
                && PlayerPosition.X < FullMap.Right - ((int)tilesOnScreen.X - theMiddleTile.X))
            {
                Focus.X = PlayerPosition.X;
                Offset.X = PlayerOffset.X;
            }
            else if (PlayerPosition.X - theMiddleTile.X == FullMap.X + 1
                && PlayerPosition.X == FullMap.Right - ((int)tilesOnScreen.X - theMiddleTile.X))
            {
                Offset.X = PlayerOffset.X;
            }

            //The Y co-ordinate
            if (PlayerPosition.Y - theMiddleTile.Y > FullMap.Y + 1
                && PlayerPosition.Y < FullMap.Bottom - ((int)tilesOnScreen.Y - theMiddleTile.Y))
            {
                Focus.Y = PlayerPosition.Y;
                Offset.Y = PlayerOffset.Y;
            }
            else if (PlayerPosition.Y - theMiddleTile.Y == FullMap.Y + 1
                && PlayerPosition.Y == FullMap.Bottom - ((int)tilesOnScreen.Y - theMiddleTile.Y))
            {
                Offset.Y = PlayerOffset.Y;
            }

        }

        private static Vector2 FirstAllign(Vector2 FirstPlayerPosition, Vector2 theMiddleTile, Vector2 tilesOnScreen, Rectangle FullMap)
        {
            int X_coord;
            int Y_coord;

            //The X co-ordinate:
            if (FirstPlayerPosition.X - theMiddleTile.X > FullMap.X + 1
                && FirstPlayerPosition.X < FullMap.Right - ((int)tilesOnScreen.X - theMiddleTile.X))
            {
                X_coord = (int)FirstPlayerPosition.X;

            }
            else
            {
                if (FirstPlayerPosition.X - theMiddleTile.X <= FullMap.X + 1)//far left
                {
                    X_coord = (int)theMiddleTile.X;
                }
                else//far right
                {
                    X_coord = FullMap.Right - (((int)tilesOnScreen.X - (int)theMiddleTile.X) + 1);
                }

            }

            //The Y co-ordinate
            if (FirstPlayerPosition.Y - theMiddleTile.Y > FullMap.Y + 1
                && FirstPlayerPosition.Y < FullMap.Bottom - ((int)tilesOnScreen.Y - theMiddleTile.Y))
            {
                Y_coord = (int)FirstPlayerPosition.Y;

            }

            else
            {
                if (FirstPlayerPosition.Y - theMiddleTile.Y <= FullMap.Y + 1)//far top
                {
                    Y_coord = (int)theMiddleTile.Y;
                }
                else//far bottom
                {
                    Y_coord = FullMap.Bottom - (((int)tilesOnScreen.Y - (int)theMiddleTile.Y) + 1);
                }

            }
            return new Vector2(X_coord, Y_coord);
        }
    }
}

    

