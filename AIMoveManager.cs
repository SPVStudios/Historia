using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Xml.Serialization;

namespace Historia
{
   

    public class TimedLocation
    {
        public Vector2 Loc;
        public int Time;
        public TimedLocation(Vector2 Loc, int Time)
        {
            this.Loc = Loc;
            this.Time = Time;
        }

        public bool Equals (TimedLocation Other)
        {
            if(this.Loc == Other.Loc && this.Time == Other.Time)
            {
                return true;
            }
            return false;
        }
        public bool MatchInList (List<TimedLocation> Others)
        {
            foreach(TimedLocation B in Others)
            {
                if (this.Equals(B))
                {
                    return true;
                }
            }
            return false;

        }

        public bool MatchInDict (Dictionary<TimedLocation,int> Others, int MyInd)//MyInd means that repeated consecutive targets for the same enemy don't trigger the mechanism
        {
            foreach (KeyValuePair<TimedLocation,int> B in Others)
            {
                if (this.Equals(B.Key) && B.Value != MyInd)
                {
                    return true;
                }
            }
            return false;
        }

        public TimedLocation DictPos (Dictionary<TimedLocation,int> Collection)
        {
            foreach (KeyValuePair<TimedLocation, int> B in Collection)
            {
                if (this.Equals(B.Key))
                {
                    return B.Key;
                }
            }
            throw new Exception("Not present in dictionary");
        }

        public TimedLocation OneLater
        {
            get
            {
                return new TimedLocation(Loc, Time + 1);
            }
        }

        public TimedLocation OneEarlier
        {
            get
            {
                return new TimedLocation(Loc, Time - 1);
                    

                
            }
        }
    }

    public class PassCall
    {
        public int EnemyWatching;
        public int EnemyWaiting;
        public Vector2 TileToPass;
        public Vector2 LaybyLocation;
        public bool BroadcastuntilReceived;

        public PassCall(int EWatch, int EWait, Vector2 TileTP, Vector2 LaybyLoc)
        {
            EnemyWatching = EWatch;
            EnemyWaiting = EWait;
            TileToPass = TileTP;
            LaybyLocation = LaybyLoc;
            BroadcastuntilReceived = false;
        }

        public static bool ContainsWaiter(List<PassCall> Pos, int Waiter, out int Which)
        {
            for(int I = 0; I < Pos.Count; I++) 
            {

                if(Pos[I].EnemyWaiting == Waiter)
                {
                    Which = I;
                    return true;
                }
            }
            Which = -1;
            return false;
        }
    }


    public class AIMoveManager
    {
        private static AIMoveManager instance;//All the functions of the class are called through this "instance". The
                                              //Following get loop means the instance is created if none are already present but otherwise uses the current
                                              //one, meaning there will only ever be one instance of the AIMoveManager running.

        public static AIMoveManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AIMoveManager();
                }
                return instance;
            }
        }

        List<PassCall> PassCalls;

        [XmlIgnore]
        public int NumPassCalls
        {
            get
            {
                return PassCalls.Count;
            }
        }

        public List<int> RoomsForRoutes;


        Map mapRef;

        public void StartNewFloor(Map mapRef, int NewEnemiesToPlace)
        {
            this.mapRef = mapRef;
            AllFutureMoves = new Dictionary<TimedLocation, int>();
            PassCalls = new List<PassCall>();
            RoomsForRoutes = new List<int>();
            for(int I = 0; I < NewEnemiesToPlace; I++)
            {
                for(int R = 0; R < mapRef.Rooms.Count; R++)
                {
                    RoomsForRoutes.Add(R);
                }
            }
        }

        public void TopUpRoomsForRoutes()
        {
            for (int R = 0; R < mapRef.Rooms.Count; R++)
            {
                RoomsForRoutes.Add(R);
            }
        }

        public void RemoveFromRR(List<int>UsedUp)
        {
            RoomsForRoutes = ListMethod.FilterBOutOfA(RoomsForRoutes, UsedUp, 1);
        }

        Dictionary<TimedLocation, int> AllFutureMoves;

        public void Update(GameTime gameTime)
        {
            AllFutureMoves = new Dictionary<TimedLocation, int>();
            //the first enemy can't crash into themselves
            if (mapRef.CurrentEnemies.Count > 0)
            {


                foreach (KeyValuePair<int, Enemy> T in mapRef.CurrentEnemies)
                {
                    int I = T.Key;
                    for (int J = 0; J < Math.Min(mapRef.CurrentEnemies[I].CurrentBS.myPlannedMoves.Count, 16); J++)//only looks 15 moves into the future at most
                    {
                        TimedLocation Move = new TimedLocation(mapRef.CurrentEnemies[I].CurrentBS.myPlannedMoves[J], J);
                        if (!Move.MatchInDict(AllFutureMoves, I) && !Move.OneEarlier.MatchInDict(AllFutureMoves, I) && !Move.OneLater.MatchInDict(AllFutureMoves, I))
                        {
                            AllFutureMoves.Add(Move, I);
                        }
                        else
                        {//collision chance here
                            int I_Priority = I;
                            int Other_Priority;
                            Vector2 LocOfColl = Move.Loc;
                            int Other_Priority_MovesToColl;
                            if (Move.MatchInDict(AllFutureMoves, I))
                            {
                                Other_Priority = AllFutureMoves[Move.DictPos(AllFutureMoves)];
                                Other_Priority_MovesToColl = Move.Time;

                            }
                            else if (Move.OneEarlier.MatchInDict(AllFutureMoves, I))
                            {
                                Other_Priority = AllFutureMoves[Move.OneEarlier.DictPos(AllFutureMoves)];
                                Other_Priority_MovesToColl = Move.OneEarlier.Time;
                            }
                            else if (Move.OneLater.MatchInDict(AllFutureMoves, I))
                            {
                                Other_Priority = AllFutureMoves[Move.OneLater.DictPos(AllFutureMoves)];
                                Other_Priority_MovesToColl = Move.OneLater.Time;
                            }
                            else
                            {
                                throw new Exception("Triggered despite fulfilling none of the categories");
                            }

                            //Check that they're not just near each other and going in the same direction

                            if (!AreTheyJustFollowingEachOther(I_Priority, J, Other_Priority, Other_Priority_MovesToColl))
                            {

                                if (I < Other_Priority)
                                {//The Target of index Other_Priority yields
                                    if (!mapRef.CurrentEnemies[Other_Priority].CurrentBS.WarnOfFriendlyCollision
                                        (I, Other_Priority, LocOfColl, mapRef.CurrentEnemies[I].CurrentBS.myPlannedMoves, mapRef.CurrentEnemies[I].MovingToTilePosition,
                                        Other_Priority_MovesToColl, J))
                                    {//if is found to be impossible for that individual, the other tries
                                        if (!mapRef.CurrentEnemies[I_Priority].CurrentBS.WarnOfFriendlyCollision
                                        (Other_Priority, I, LocOfColl, mapRef.CurrentEnemies[Other_Priority].CurrentBS.myPlannedMoves, mapRef.CurrentEnemies[Other_Priority].MovingToTilePosition,
                                        J, Other_Priority_MovesToColl))
                                        {
                                            //throw new Exception("Collision could not be averted");
                                        }
                                    }

                                }
                                else if (Other_Priority < I)
                                {// Enemy of Index I yields
                                    if (!mapRef.CurrentEnemies[I_Priority].CurrentBS.WarnOfFriendlyCollision
                                        (Other_Priority, I, LocOfColl, mapRef.CurrentEnemies[Other_Priority].CurrentBS.myPlannedMoves, mapRef.CurrentEnemies[Other_Priority].MovingToTilePosition,
                                        J, Other_Priority_MovesToColl))
                                    {
                                        if (!mapRef.CurrentEnemies[Other_Priority].CurrentBS.WarnOfFriendlyCollision
                                        (I, Other_Priority, LocOfColl, mapRef.CurrentEnemies[I].CurrentBS.myPlannedMoves, mapRef.CurrentEnemies[I].MovingToTilePosition,
                                        Other_Priority_MovesToColl, J))
                                        {
                                            //throw new Exception("Collision could not be averted");
                                        }
                                    }
                                }
                                else
                                {
                                    throw new Exception("Somehow called the same enemy against itself");
                                }
                                break;//as the path has been changed somehow, any more of this data is useless anyway.
                            }

                        }
                    }
                }
            }
            //now monitor PassCalls
            for (int I = 0; I < PassCalls.Count; I++)
            {
                if (!mapRef.CurrentEnemies.ContainsKey(PassCalls[I].EnemyWaiting))
                {//the enemy that should be waiting no longer exists - delete the passcall
                    PassCalls.RemoveAt(I);
                    I--;
                    continue;
                }
                else if (!mapRef.CurrentEnemies.ContainsKey(PassCalls[I].EnemyWatching))
                {//the enemy that was being waited for no longer exists - begin broadcast until reception as the way is therefore clear by default
                    PassCalls[I].BroadcastuntilReceived = true;//begin broadcast until reception
                }
                else
                {
                    if (!mapRef.CurrentEnemies[PassCalls[I].EnemyWatching].CurrentBS.myPlannedMoves.Contains(PassCalls[I].TileToPass)
                    && mapRef.CurrentEnemies[PassCalls[I].EnemyWatching].CurrentBS.myPlannedMoves.Count > 0
                    && mapRef.CurrentEnemies[PassCalls[I].EnemyWatching].TilePosition != PassCalls[I].TileToPass
                    && mapRef.CurrentEnemies[PassCalls[I].EnemyWatching].MovingToTilePosition != PassCalls[I].TileToPass)
                    {

                        PassCalls[I].BroadcastuntilReceived = true;//begin broadcast until reception
                    }
                }
                if (PassCalls[I].BroadcastuntilReceived)
                {

                    //The other has passed; the Enemy may continue

                    if (mapRef.CurrentEnemies[PassCalls[I].EnemyWaiting].CurrentBS.GiveGoAhead())
                    {//if the enemy has successfully been given the go ahead
                        CollisionCheck.Instance.RemoveOverrule(PassCalls[I].LaybyLocation);
                        PassCalls.RemoveAt(I);

                        I--;
                        if (PassCalls.Count == 0)
                        {
                            CollisionCheck.Instance.ClearAllOverridesForGivenNewValue(4);
                        }
                    }
                }
            }
        }
                

        public void AddNewPassCall(int EnemyWatching, int EnemyWaiting, Vector2 LocationToBePassed, Vector2 LocationOfLayby)
        {
            if (PassCall.ContainsWaiter(PassCalls, EnemyWaiting, out int Which))
            {
                CollisionCheck.Instance.RemoveOverrule(PassCalls[Which].LaybyLocation);
                PassCalls.RemoveAt(Which);
            }
            PassCalls.Add(new PassCall(EnemyWatching, EnemyWaiting, LocationToBePassed,LocationOfLayby));
            
        }
        
        public  bool AreTheyJustFollowingEachOther(int A, int AHitsBHere, int B, int BHitsAHere)
        {
            int AMovesLeft = mapRef.CurrentEnemies[A].CurrentBS.myPlannedMoves.Count - (AHitsBHere + 1);
            int BMovesLeft = mapRef.CurrentEnemies[B].CurrentBS.myPlannedMoves.Count - (AHitsBHere + 1);
            int Cap = Math.Min(3, Math.Min(AMovesLeft, BMovesLeft));
            for (int I = 1; I < Cap; I++)
            {
                if (mapRef.CurrentEnemies[A].CurrentBS.myPlannedMoves[AHitsBHere + I] != mapRef.CurrentEnemies[B].CurrentBS.myPlannedMoves[BHitsAHere + I])
                {
                    return false;
                }
                
            }
            return true;
               
        }




    }
}
