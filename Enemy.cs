using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Xml.Serialization;
using System.Xml;
using Historia.Items;
using Historia.AI_Behaviour;

namespace Historia
{
    public class Enemy : Being
    {
        [XmlIgnore]
        public AI_Behaviour.AIBehaviour[] BehaviourSets;

        [XmlIgnore]
        public bool HaveConfirmedTile
        {
            get;private set;
        }
        [XmlIgnore]
        public Vector2 ConfirmedTile
        { get; private set; }
        


        [XmlElement("BehaviourSet")]
        public List<int> BehaviourSetIndexesforLoad;

        public int AlertLevel;

        public int AlertPoints;

        public const int AlertPointsPerLevel = 10000;
        public const int MaximumAlertPoints = AlertPointsPerLevel * 8;
        public const decimal AlertDecayHalfLife = 10M;//in seconds.

        public int TotalofAlertPoints
        {
            get
            {
                return (AlertLevel * AlertPointsPerLevel) + AlertPoints;
            }
        }
        /// <summary>
        /// ONLY TO BE USED IN the CurrentBS property. Nothing else! 
        /// </summary>
        private int lastSelectedBehaviourSet;
        public AI_Behaviour.AIBehaviour CurrentBS
        {
            get
            {
                int toSelect;
                if (HaveConfirmedTile)
                {
                    toSelect = 3;//always AlertBehaviour for Confirmation
                }
                else
                {
                    toSelect = AlertLevel;
                }
                if(toSelect != lastSelectedBehaviourSet)
                {
                    BehaviourSets[lastSelectedBehaviourSet].CancelPlan();
                    lastSelectedBehaviourSet = toSelect;
                }
                return BehaviourSets[toSelect];

            }
        }


        [XmlIgnore]
        public Vector2 CurrentTarget
        { get; protected set; }
        [XmlIgnore]
        public Vector2 MovingToTilePosition
        { get; protected set; }

        //protected Vector2 NextTarget;//next tile to walk to, if any; adjusted by BehaviourSets.
        //protected int NextActionType;//next action to carry out, adjusted by BehaviourSets.
        //protected bool NAIsMovementBased;//whether or not the above is movement-based.
        [XmlIgnore]
        private PlanStep stepToComplete;
        [XmlIgnore]
        protected int WaitTime;
        /// <summary>
        /// Whether or not this enemy is currently standing still for a pre-specified time as insstructed by a PlanStep. 
        /// </summary>
         [XmlIgnore]
        protected bool IsWaiting;

        /// <summary ActionFrameSet>
        /// The multiple of 4 referrring to the animation of whichever current action should be ocurring.
        /// Key:
        /// 0 = idle
        /// 1 = movement
        /// 2 = running
        /// 3 = regular attacking
        /// 4 = heavy attacking
        /// 5 = dodging
        /// 6 = blocking
        /// 7 = parrying
        /// 8 = sneaking
        /// 9 = Interact
        /// 
        /// 10-15 = Special Moves 0-5 (DO NOT MOVE THESE, THE ATTACKMANAGER RELIES ON THEM BEING HERE) (Defined separately)
        /// 
        /// Special Moves, However, provide their own spritesheets to draw from, so after the first 10 animations (0-9), the 11th is Death, 1 for each facing.
        /// 
        /// 
        /// 
        /// These numbers are used both by animations and to determine by update logic what action is currently being taken.
        /// 
        /// </summary>


        public string Name;

        [XmlIgnore]
        public int MyIndex
        {
            get; private set;
        }

        


        [XmlIgnore]
        public Rectangle ImmediatelyAhead
        {
            get
            {
                switch (FacingAddup)
                {
                    case 0:
                        return new Rectangle((int)TilePosition.X - 1, (int)TilePosition.Y, 3, 2);
                       
                    case 1:
                        return new Rectangle((int)TilePosition.X - 2, (int)TilePosition.Y - 1, 2, 3);

                    case 2:
                        return new Rectangle((int)TilePosition.X, (int)TilePosition.Y - 1, 2, 3);
                        
                    case 3:
                        return new Rectangle((int)TilePosition.X - 1, (int)TilePosition.Y - 2, 3, 2);
                    default:
                        throw new Exception("Not a number 0-3");
                }
               
            }
        }

        

        //100 AlertPoints = 1 AlertLevel. the AlertLevel is used for many functions to calculate stealth etc.
        /// <AlertLevels>
        /// Specific Behaviour Sets are carried out by the AI depending on their Alert Level. Their Alert Points are equal to the various
        /// suspicions they have (in points) of specific map tiles added together. As Alert Level goes up, the AI stops walking on its normal
        /// OBLIVIOUS route and starts looking more closely for where you might be. 
        /// 
        /// Certain Behaviours of SUSPICIOUS+ rank will allow enemies to confer their most suspicious co-ordinates to nearby allies, making
        /// being found much more problematic. Some will take this to the extreme, and run off to get backup rather than face you.
        /// 
        /// For each Update that a given suspicious location is not bolstered in any way, it decreases by (some) points, and disappears again at 0.
        /// 
        /// Some Enemies like harmless woodland creatures will likely just move away with higher alert levels.
        /// 
        /// **ALERT LEVELS**
        /// 0: OBLIVIOUS: Behaviours that are enacted with a Detection Level of 0. Normal patrol routes etc.
        /// 1: UNSETTLED: Behaviour for detection Level 1, with a Room as the vague suspicion of location.
        /// 2: SUSPICIOUS: Detection Level 2 - is looking for you, with a rectangular area of reasonable confidence in your location (could be wrong though - think distractions)
        /// 3+: ALERTED: /Detection Level 3+ - Has quite concrete belief in a particular Co-ordinate.
        ///      would normally pathfind to your location and attack you.
        ///      Alerted_Behaviours consist of a response behaviour to the threat level, usually just trying to get to you to attack you - 
        ///      and also a specific attack strategy behaviour, be it zealous and use-up-stamina-quick, or sneak-and-dodge-y, or Parry intensive etc.
        /// 
        /// Things that increase the suspicion of a given tile:
        /// 
        ///> Seeing you/your companion/a distraction you made
        ///> Hearing you/your companion/a distraction you made
        ///> Seeing a dead ally
        ///> Seeing a destroyed object outside of a Boss Room
        ///> Being Informed by an ally
        /// 
        /// 
        /// </AlertLevels>

        [XmlIgnore]
        public Dictionary<Vector2, int> CurrentSuspicions;
        private Dictionary<Vector2, int> RecentSuspicions;
        //Whether or not the enemy saw the human in the last update loop. Not used in calculation of suspicion but to track.
        public bool SawPlayerRecently(Vector2 PlayerLoc)
        {
            if (RecentSuspicions != null)
            {
                return RecentSuspicions.ContainsKey(PlayerLoc);
            }
            return false;
        }


        public int MinGoldDrop;
        public int MaxGoldDrop;

        [XmlElement("PossItemDrop")]
        public List<LoadItemStack> PossDrops;

        int GoldDrop;
        [XmlIgnore]
        List<Item> DropItems;

        public override void LoadContent( Vector2 TileDimensions, Vector2 SpawnLocation, Map map)
        {
            base.LoadContent( TileDimensions, SpawnLocation, map);
            LoadBehaviourSets(map);
            FacingAddup = map.D.Next(0, 4);
            CollisionType = 4;
            MyCollFactFile = new CollFactFile(CollisionType, true, MyIndex);
            GoldDrop = map.D.Next(MinGoldDrop, MaxGoldDrop);
            stepToComplete = new EmptyStep(TilePosition);
//while (true)
//            {
//                foreach (LoadItemStack IS in PossDrops)
//                {
//                    if (map.D.Next(0, 100) < IS.I)
//                    {
//                        //add that item to the drops
//                        Item New = new Item(IS.ID);
//                        New.Load();
//                        DropItems.Add(New);
//                    }
//                }
//            }
        }

        public void AssignIndex(int index)
        {
            MyIndex = index;
        }

        protected void LoadBehaviourSets(Map map)//use the directory to turn numbers into a behaviour for each alert level.
        {
            BehaviourSets = new AI_Behaviour.AIBehaviour[BehaviourSetIndexesforLoad.Count];
            for (int B = 0; B < BehaviourSetIndexesforLoad.Count; B++)
            {
                //load all Behaviours for this AlertLevel
                string FillDictPath = "AI Behaviour/s"+B+"/s"+B+"BehaviourDirectory.xml";

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(FillDictPath);//file path
                var obj = this;

                foreach (XmlNode Behaviour in xmlDoc.DocumentElement.ChildNodes)
                {
                    if (Behaviour.Attributes["ID"].Value == BehaviourSetIndexesforLoad[B].ToString())
                    {//if the reference ID for this behaviour is equal to that of the required behaviour
                        string Class = Behaviour.InnerText;
                        AI_Behaviour.AIBehaviour This = (AI_Behaviour.AIBehaviour)Activator.CreateInstance(Type.GetType("Historia.AI_Behaviour.s" + B + "." + Class));

                        This.LoadContent(ref obj,map);
                        //^^^^Calculate normal walking route under the ObliviousBehaviour, ect
                        BehaviourSets[B] = This;
                        break;
                    }
                }
            }
        }

        //unload content as a Being would
        
        public Enemy()//for use with the Xml Loader
        {
            CollisionType = 4;
            BehaviourSetIndexesforLoad = new List<int>();
            CurrentSuspicions = new Dictionary<Vector2, int>();
            MovingToTilePosition = new Vector2();
            UnsetConfirmedTile();
        }

        public override void Update(GameTime gameTime, out bool Died)
        {
            UpdateSuspicions(gameTime);
            
            base.Update(gameTime, out Died);//invokes DecideWhatToDo if idle

            
        }

        public void UpdateSuspicions(GameTime gameTime)
        {
            Dictionary<Vector2, int> NewSuspicions = new Dictionary<Vector2, int>();
            
            if (IsInRoom)
            {
                foreach(KeyValuePair<Vector2,int> I in StealthManager.Instance.RoomSuspiciousPointsThisPass[CurrentRoomIn])
                {
                    NewSuspicions.Add(I.Key, I.Value);
                }
                foreach(Room.EntryPoint E in mapRef.Rooms[CurrentRoomIn].EntryPoints)
                {
                    foreach(KeyValuePair<Vector2,int> S in StealthManager.Instance.PassageSuspiciousPointsThisPass[E.PassagewayUsing])
                    {
                        NewSuspicions.Add(S.Key, S.Value/2);
                    }
                }
            }
            else
            {
                foreach (KeyValuePair<Vector2, int> I in StealthManager.Instance.PassageSuspiciousPointsThisPass[CurrentPassagewayIn])
                {
                    NewSuspicions.Add(I.Key, I.Value);
                }
                //the room at one end of a passage
                foreach (KeyValuePair<Vector2, int> S in StealthManager.Instance.RoomSuspiciousPointsThisPass[mapRef.Passages[CurrentPassagewayIn].End1.EndPointID])
                {
                    NewSuspicions.Add(S.Key, (3* S.Value)/4);
                }
                foreach (KeyValuePair<Vector2, int> S in StealthManager.Instance.RoomSuspiciousPointsThisPass[mapRef.Passages[CurrentPassagewayIn].End2.EndPointID])
                {
                    NewSuspicions.Add(S.Key, (3* S.Value) / 4);
                }
            }


            //Apply all Suspicious Activities and Noises in the room you are currently in to the suspicious points.

            //run L.O.S checks on these suspicious things to "thin down the herd"
            NewSuspicions = StealthMethod.MassCheckLineOfSight(mapRef, TilePosition,this.Direction, NewSuspicions);
            //now add the Noises by adding a few random tiles in the noise range to the suspicious points.
            NewSuspicions = StealthMethod.TemperSuspicionsByDistance(NewSuspicions, TilePosition);
            RecentSuspicions = NewSuspicions;
            /*foreach (KeyValuePair<Rectangle, int> Noise in StealthManager.Instance.Noises)
            {
                int NoiseX = mapRef.D.Next(Noise.Key.X, Noise.Key.Right);
                int NoiseY = mapRef.D.Next(Noise.Key.Y, Noise.Key.Bottom);
                StealthMethod.AddNewSuspiciousness(ref NewSuspicions, new Vector2(NoiseX, NoiseY), Noise.Value);
            }
            */
            if(CurrentSuspicions.Count>0 || NewSuspicions.Count > 0)
            {
                double elapsed_time_seconds = gameTime.ElapsedGameTime.TotalSeconds;
                Dictionary<Vector2, int> CommonSuspicions =StealthMethod.TallyAndDecaySuspicionInLists
                    (NewSuspicions, CurrentSuspicions,
                    out Dictionary<Vector2, int> OnlyOldSuspicions, AlertDecayHalfLife,elapsed_time_seconds);

                //Decrement points that haven't been topped up since last Update - carried out in above process.
                CurrentSuspicions = StealthMethod.CombineSuspicionLists(OnlyOldSuspicions, CommonSuspicions);
                //Calculate new Suspicion Level.
                AlertPoints = StealthMethod.TallyTotalSuspicion(CurrentSuspicions);
                AlertLevel = AlertPoints / AlertPointsPerLevel;
                AlertPoints = AlertPoints % AlertPointsPerLevel;

                if (AlertLevel > 3)
                {
                    AlertPoints = AlertPointsPerLevel - 1;
                    AlertLevel = 3;
                }
            }
            
        }

        /// <summary>
        /// Updates suspicions without applying any effect of time passing, like decaying anything. Just uses currentsuspicions again to recalc alertpoints.
        /// </summary>
        private void UpdateSuspicionsLite()
        {
            AlertPoints = StealthMethod.TallyTotalSuspicion(CurrentSuspicions);
            AlertLevel = AlertPoints / AlertPointsPerLevel;
            AlertPoints = AlertPoints % AlertPointsPerLevel;

            if (AlertLevel > 3)
            {
                AlertPoints = AlertPointsPerLevel - 1;
                AlertLevel = 3;
            }
        }

        public void MakeSuspiciousOfTile(Vector2 Loc, int HowSuspicious)
        {
            Dictionary<Vector2, int> New = new Dictionary<Vector2, int>
            {
                { Loc, HowSuspicious }
            };
            CurrentSuspicions = StealthMethod.CombineSuspicionLists(CurrentSuspicions, New);
        }

        protected override bool DecideWhattoDo(GameTime gameTime, out int Action)
        {
            if (IsWaiting)
            {
                WaitTime -= (int)gameTime.ElapsedGameTime.TotalMilliseconds;
                if (WaitTime > 0)
                {
                    Action = 0;
                    return false;//note this is a return and so terminates the method
                }
                //else
                IsWaiting = false;
                WaitTime = 0;
            }
            //if was waiting and is no longer, or was never waiting...
            //CurrentTarget = NextTarget;
            if (stepToComplete.Completed)
            {//so get a new one!
                stepToComplete.End();//just before binning the old one, call End in case it needs to signal back to AIBehaviour (like the results of a search)
                stepToComplete = CurrentBS.DecideWhatToDo();// now fetch a new one.
            }
            stepToComplete.UsePlanStep(gameTime, this, out Action, out Vector2 NextTarget);
            // was an instruction to wait? otherwise, do the action outputted
            if (!IsWaiting)
            {
                //turn to face the next target
                Vector2 NextFacing = new Vector2(Math.Sign(NextTarget.X - TilePosition.X), Math.Sign(NextTarget.Y - TilePosition.Y));

                if (NextFacing != Vector2.Zero)
                {
                    FacingAddup = GetFacing(NextFacing);
                }

                if (Action > 0)
                {
                    if (ActionIsMovementBased(Action))
                    {
                        if (NextTarget == TilePosition)
                        {
                            Action = 0;
                            return false;
                        }
                        else
                        {//target elsewhere, so a valid 'moving action'
                            MovingToTilePosition = NextTarget;
                            return true;
                        }
                    }
                    else
                    {//target must be this tile or elsewhere, but we aren't moving!, since we are on the tile already!
                        MovingToTilePosition = TilePosition;
                        return true;
                    }

                }

                return false;

            }
            //else IS waiting
            return false;

        }

        protected override void StartMove()
        {
            if (CheckWalkValid())
            {
                IsBusy = true;
                
            }
            else
            {
                if(CollisionCheck.Instance.CheckTarget(mapRef.Collision.CMap, mapRef.BC.BC, TilePosition + Direction) == 6)//is the Player...
                {
                    MakeSuspiciousOfTile(TilePosition + Direction, AlertPointsPerLevel);
                }
            }

        }
        /// <summary>
        /// Make the Enemy wait for a given number of seconds. Ideally to be used by WaitStep instructions and nothing else.
        /// </summary>
        /// <param name="Milliseconds">the number of millisecinds to wait for.</param>
        public void MakeWait(int Milliseconds)
        {
            IsWaiting = true;
            WaitTime = Milliseconds;
        }

        public static Enemy LoadEnemyFromTemplate(string Name)
        {
            Enemy Beastie = new Enemy();
            XmlManager<Enemy> BeastLoader = new XmlManager<Enemy>();
            Beastie = BeastLoader.Load("Load/Gameplay/Bestiary/" + Name + ".xml");
            return Beastie;
        }
 
        public override void Die()
        {
            base.Die();
            //mapRef.BC.RemoveAllClaims(MyCollFactFile);
            mapRef.BC.RemovePreviousLocation(MyCollFactFile, Vector2.Zero);
        }

        public void SetConfirmedTile(Vector2 NewLoc)
        {
            ConfirmedTile = NewLoc;
            HaveConfirmedTile = true;
            UpdateSuspicionsLite();
        }

        public void UnsetConfirmedTile()
        {
            ConfirmedTile = Vector2.Zero;
            HaveConfirmedTile = false;
        }

        public static bool ActionIsMovementBased(int Action)
        {
            switch (Action)
            {
                case 0:
                    return false;
                case 1: return true;//walking
                case 2: return true;//running
                case 3: return false;//attack
                case 4: return false;//heavy attack
                case 5: return true;//dodge
                case 6: return false;//block
                case 7: return false;//parry
                case 8: return true ;//sneak
                case 9: return false;//interact
                case 10: return false;//ability 1
                case 11: return false;//2
                case 12: return false;//3
                case 13: return false;//4
                case 14: return false;//5
                case 15: return false;//6
                default: throw new NotImplementedException();

            }

            
        }
    }


}
