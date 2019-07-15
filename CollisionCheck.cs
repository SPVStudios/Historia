using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Historia
{
    public class CollisionCheck// this is a Static class: it doesn't need instantiation, it's just for methods
    {
        /// <Collision Overview>
        /// Determines the collision status of the target tile.
        /// 
        /// Key:
        /// 0 = Impassable - Void
        /// 1 = Impassable - Wall
        /// 2 = Impassable - Object
        /// (3 = Impassable - NPC
        /// 4 = Impassable - Enemy
        /// 5 = Impassable - Boss
        /// 6 = Impassable - Player) - these are in the BC directory.
        /// 
        /// 10 = Object You can climb into, with the relevant perk
        /// 
        /// 20 = Local Transition tile          -therefore 20 or more is passable, although they can have varying effects
        /// 21 = Global Transition tile (out of this dungeon)
        /// 
        /// 30 = Free to move into
        /// 
        /// </Collision Overview>

        private static CollisionCheck instance;//All the functions of the class are called through this "instance". The
                                               //Following get loop means the instance is created if none are already present but otherwise uses the current
                                               //one, meaning there will only ever be one instance of the AIMoveManager running.

        public static CollisionCheck Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new CollisionCheck();
                }
                return instance;
            }
        }


        Dictionary<Vector2, int> OverruleValues;

        public CollisionCheck()
        {
            OverruleValues = new Dictionary<Vector2, int>();
        }

        public void AddOverrule(Vector2 Loc, int NewCollisionValue)
        {
            OverruleValues.Add(Loc, NewCollisionValue);

        }

        public void RemoveOverrule(Vector2 Loc)
        {
            if (OverruleValues.ContainsKey(Loc))
            {
                OverruleValues.Remove(Loc);
            }
        }

        public void CleanOverrules()
        {
            if (ScreenManager.Instance.CurrentScreen.GetType() == typeof(GameplayScreen))
            {
                GameplayScreen copy = ScreenManager.Instance.CurrentScreen as GameplayScreen;
                List<Vector2> Os = OverruleValues.Keys.ToList();
                for (int I = 0; I < Os.Count; I++)
                {
                    bool Valid = false;
                    foreach (KeyValuePair<int,Enemy> E in copy.map.CurrentEnemies)
                    {
                        if(E.Value.TilePosition == Os[I])
                        {
                            Valid = true;
                            break;
                        }
                        else if (E.Value.CurrentBS.myPlannedMoves.Count > 0)
                        {

                        
                        if (E.Value.CurrentBS.myPlannedMoves[E.Value.CurrentBS.myPlannedMoves.Count - 1] == Os[I])
                            {
                                Valid = true;
                                break;
                            }
                        }
                    }
                    if(Valid = false)
                    {
                        RemoveOverrule(Os[I]);
                    }

                }
            }
        }

        public int CheckTarget(int[,] CollisionMap, Dictionary<Vector2, CollFactFile> BeingColl, Vector2 Location)//when moving to a tile, assume its occupied even if the BC CollFactFile claims it is empty
        {
            try
            {
                if (CollisionMap[(int)Location.X, (int)Location.Y] >= 20)
                {
                    if (!BeingColl.ContainsKey(Location))//if the tile is free 
                    {
                        return AdjustIntForOverrides(Location, CollisionMap[(int)Location.X, (int)Location.Y]);
                    }
                    else
                    {
                        return BeingColl[Location].CollisionType;//overrules the OverrideValues
                    }
                }
                else//level collision check failed
                {
                    return AdjustIntForOverrides(Location, CollisionMap[(int)Location.X, (int)Location.Y]);
                }
            }
            catch (IndexOutOfRangeException)
            {
                return 0;
            }
        }

        public int CheckTarget(int[,] CollisionMap, Dictionary<Vector2, CollFactFile> BeingColl, Vector2 Location, bool DoesntMindEmptyBCs)//For when not moving to that tile but just checking it is free
        {
            try
            {
                if (CollisionMap[(int)Location.X, (int)Location.Y] >= 20)
                {
                    if (!BeingColl.ContainsKey(Location))//if the tile is free 
                    {
                        return AdjustIntForOverrides(Location, CollisionMap[(int)Location.X, (int)Location.Y]);
                    }
                    else
                    {
                        if (BeingColl[Location].IsPresent)//if the tile is claimed AND occupied
                        {
                            return BeingColl[Location].CollisionType;
                        }
                        else
                        {
                            return AdjustIntForOverrides(Location, CollisionMap[(int)Location.X, (int)Location.Y]);
                        }
                    }
                }
                else//level collision check failed
                {
                    return AdjustIntForOverrides(Location, CollisionMap[(int)Location.X, (int)Location.Y]);
                }
            }
            catch (IndexOutOfRangeException)
            {
                return 0;
            }
        }

        public int CheckTarget(int[,] CollisionMap, Vector2 Location)
        {
            try
            {
                return AdjustIntForOverrides(Location, CollisionMap[(int)Location.X, (int)Location.Y]);
            }
            catch (IndexOutOfRangeException)
            {
                return 0;
            }
        }

        public int CheckTarget(int[,] CollisionMap, List<Vector2> ExtraObstacles, Vector2 Location)
        {
            if (!ExtraObstacles.Contains(Location))
            {
                try
                {

                    return AdjustIntForOverrides(Location, CollisionMap[(int)Location.X, (int)Location.Y]);
                }
                catch (IndexOutOfRangeException)
                {
                    return 0;
                }
            }
            else
            {
                return 4;
            }

        }

        // A numerical CheckTarget for a BeingColl only cannot exist, as a pass collision has no set value

        public bool CheckTargetBool(int[,] CollisionMap, Dictionary<Vector2, CollFactFile> BeingColl, Vector2 Location)
        {
            try
            {
                if (CollisionMap[(int)Location.X, (int)Location.Y] >= 20 && !BeingColl.ContainsKey(Location))
                {
                    return AdjustBoolForOverrides(Location, true);
                }
                else
                {
                    return AdjustBoolForOverrides(Location, false);
                }
            }
            catch (IndexOutOfRangeException)
            {
                return false;
            }
        }

        public bool CheckTargetBool(int[,] CollisionMap, Vector2 Location)
        {
            try
            {
                if (CollisionMap[(int)Location.X, (int)Location.Y] >= 20)
                {
                    return AdjustBoolForOverrides(Location, true);
                }
                else
                {
                    return AdjustBoolForOverrides(Location, false);
                }
            }
            catch (IndexOutOfRangeException)
            {
                return false;
            }
        }

        public bool CheckTargetBool(Map.BeingCollisions BeingCollide, Vector2 Location)
        {
            if (BeingCollide.BC.ContainsKey(Location))
            {
                return false;
            }
            return true;
        }

        public bool CheckTargetBool(int[,] CollisionMap, List<Vector2> ExtraObstacles, Vector2 Location)
        {
            try
            {
                if (CollisionMap[(int)Location.X, (int)Location.Y] >= 20)
                {
                    if (!ExtraObstacles.Contains(Location))
                    {
                        return AdjustBoolForOverrides(Location, true);
                    }
                    else
                    {
                        return false;//definitely false, given as Extra Obstacle
                    }
                }
                else
                {
                    return AdjustBoolForOverrides(Location, false);
                }
            }
            catch (IndexOutOfRangeException)
            {
                return false;
            }
        }

        public bool CheckListBool(int[,] ColllisionMap, List<Vector2> Locations)
        {
            for (int I = 0; I < Locations.Count; I++)
            {
                if (!CheckTargetBool(ColllisionMap, Locations[I]))
                {//checked on the individual basis for overrides

                    return false;


                }
            }
            return true;
        }

        public List<Vector2> FilterList(int[,] CollisionMap, List<Vector2> Filtering)
        {
            for (int I = 0; I < Filtering.Count; I++)
            {
                if (!CheckTargetBool(CollisionMap, Filtering[I]))//checked for overrules on the individual basis
                {
                    Filtering.RemoveAt(I);
                    I--;
                }

            }
            return Filtering;
        }

        private bool AdjustBoolForOverrides(Vector2 Loc, bool Original)
        {
            if (OverruleValues.ContainsKey(Loc))
            {
                if (OverruleValues[Loc] >= 20)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return Original;
            }
        }

        private int AdjustIntForOverrides(Vector2 Loc, int Original)
        {
            if (OverruleValues.ContainsKey(Loc))
            {
                return OverruleValues[Loc];
            }
            else
            {
                return Original;
            }
        }

        public void ClearAllOverridesForGivenNewValue(int OverrideValue)
        {
            for (int I = 0; I < OverruleValues.Count; I++)
            {
                if (OverruleValues.ElementAt(I).Value == OverrideValue)
                {
                    OverruleValues.Remove(OverruleValues.ElementAt(I).Key);
                    I--;
                }
            }
        }
    }
}
