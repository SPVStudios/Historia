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
    public static class DungeonMethod
    {
        /// <summary>
        /// For when an enemy is spawning - its BC profile is automatically that of an enemy.
        /// </summary>
        /// <param name="mapRef"></param>
        /// <param name="EnemyIndex">The Index of the Enemy spawning</param>
        /// <returns>The location that they will spawn in.</returns>
        public static Vector2 SpawnInValidLocation(ref Map mapRef, int EnemyIndex, int RoomOfPlayer)
        {
            int[,] CollisionMap = mapRef.Collision.CMap;
            Dictionary<Vector2, CollFactFile> BeingColl = mapRef.BC.BC;
            List<Room> Rooms = mapRef.Rooms;
            List<Passageway> Passages = mapRef.Passages;
            List<int> PossRooms = new List<int>();
            for (int R = 0; R < Rooms.Count; R++)
            {
                PossRooms.Add(R);
            }
            PossRooms.Remove(RoomOfPlayer);
            foreach (KeyValuePair<int, Enemy> E in mapRef.CurrentEnemies)
            {
                PossRooms.Remove(E.Value.CurrentRoomIn);
            }
            int WhichR_Room = PossRooms[mapRef.D.Next(0, PossRooms.Count)];
               
            Rectangle RoomLoc = Rooms[WhichR_Room].Location[0];
            while (true)
            {
                int RoomX = mapRef.D.Next(RoomLoc.X, RoomLoc.Right - 1);
                int RoomY = mapRef.D.Next(RoomLoc.Y, RoomLoc.Bottom - 1);
                if(CollisionCheck.Instance.CheckTarget(CollisionMap,BeingColl, new Vector2(RoomX, RoomY)) == 30)//if is an empty tile
                {
                    BeingColl.Add(new Vector2(RoomX, RoomY), new CollFactFile(4, true, EnemyIndex));
                    return new Vector2(RoomX, RoomY);
                }
            }
            
        }




        /// <summary>
        /// For beings not definitely an enemy.
        /// </summary>
        /// <param name="mapRef"></param>
        /// <param name="BC_Value">THe value it wil have in a BC array</param>
        /// <param name="Irrelevant">Ignore this, to differentiate</param>
        /// <returns></returns>
        public static Vector2 SpawnInValidLocation(ref Map mapRef, int BC_Value, int RoomOfPlayer, bool Irrelevant)
        {
            int[,] CollisionMap = mapRef.Collision.CMap;
            Dictionary<Vector2, CollFactFile> BeingColl = mapRef.BC.BC;
            List<Room> Rooms = mapRef.Rooms;
            List<Passageway> Passages = mapRef.Passages;

            List<int> PossRooms = new List<int>();
            for (int R = 0; R < Rooms.Count; R++)
            {
                PossRooms.Add(R);
            }
            PossRooms.Remove(RoomOfPlayer);
            foreach (KeyValuePair<int, Enemy> E in mapRef.CurrentEnemies)
            {
                PossRooms.Remove(E.Value.CurrentRoomIn);
            }
            int WhichR_Room = PossRooms[mapRef.D.Next(0, PossRooms.Count)];

            Rectangle RoomLoc = Rooms[WhichR_Room].Location[0];
            while (true)
            {
                int RoomX = mapRef.D.Next(RoomLoc.X, RoomLoc.Right - 1);
                int RoomY = mapRef.D.Next(RoomLoc.Y, RoomLoc.Bottom - 1);
                if (CollisionCheck.Instance.CheckTarget(CollisionMap, BeingColl, new Vector2(RoomX, RoomY)) == 30)//if is an empty tile
                {
                    BeingColl.Add(new Vector2(RoomX, RoomY), new CollFactFile(BC_Value, true));
                    return new Vector2(RoomX, RoomY);
                }
            }

        }



    }
}
