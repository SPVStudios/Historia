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

namespace Historia
{
    public class Enemy : Being
    {
        [XmlIgnore]
        public AI_Behaviour.AIBehaviour[] BehaviourSets;

        [XmlElement("BehaviourSet")]
        public List<int> BehaviourSetIndexesforLoad;

        [XmlIgnore]
        public Vector2 CurrentTarget
        { get; protected set; }
        [XmlIgnore]
        public Vector2 MovingToTilePosition
        { get; protected set; }

        protected Vector2 NextTarget;//next tile to walk to, if any; adjusted by BehaviourSets.
        protected int NextActionType;//next action to carry out, adjusted by BehaviourSets.
        protected bool NAIsMovementBased;//whether or not the above is movement-based.

        public string Name;

        [XmlIgnore]
        public int MyIndex
        {
            get; private set;
        }

        public int AlertLevel;

        public int AlertPoints;

        public int TotalofAlertPoints
        {
            get
            {
                return (AlertLevel * AlertPointsPerLevel) + AlertPoints;
            }
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


        public const int AlertPointsPerLevel = 2000;

        public AI_Behaviour.AIBehaviour CurrentBS
        {
            get
            {
                return BehaviourSets[AlertLevel];
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
        
        public override void LoadContent(Vector2 TileDimensions, Vector2 SpawnLocation, Map map)
        {
            base.LoadContent(TileDimensions, SpawnLocation, map);
            LoadBehaviourSets(map);
            FacingAddup = map.D.Next(0, 4);
            CollisionType = 4;
            MyCollFactFile = new CollFactFile(CollisionType, true, MyIndex);
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
        }

        public override void Update(GameTime gameTime, out bool Died)
        {
            UpdateSuspicions();
            
            base.Update(gameTime, out Died);//invokes DecideWhatToDo if idle

            if(NextActionType == 1 && NextTarget != TilePosition)
            {

            }
        }

        public void UpdateSuspicions()
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

            /*foreach (KeyValuePair<Rectangle, int> Noise in StealthManager.Instance.Noises)
            {
                int NoiseX = mapRef.D.Next(Noise.Key.X, Noise.Key.Right);
                int NoiseY = mapRef.D.Next(Noise.Key.Y, Noise.Key.Bottom);
                StealthMethod.AddNewSuspiciousness(ref NewSuspicions, new Vector2(NoiseX, NoiseY), Noise.Value);
            }
            */
            if(CurrentSuspicions.Count>0 || NewSuspicions.Count > 0)
            {
                Dictionary<Vector2, int> OnlyOldSuspicions = new Dictionary<Vector2, int>();
                Dictionary<Vector2, int> CommonSuspicions = StealthMethod.TallySuspicionInLists(NewSuspicions, CurrentSuspicions, out OnlyOldSuspicions, true);
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
            CurrentTarget = NextTarget;
            
            BehaviourSets[AlertLevel].DecideWhatToDo(out NextTarget, out NextActionType, out NAIsMovementBased);
            Action = NextActionType;

            if (CurrentTarget == NextTarget)
            {

            }
            //turn to face the next target
            Vector2 NextFacing = new Vector2(Math.Sign(NextTarget.X - TilePosition.X), Math.Sign(NextTarget.Y - TilePosition.Y));

            if (NextFacing != Vector2.Zero)
            {
                FacingAddup = GetFacing(NextFacing);
            }

            if (Action != 0)
            {
                if (NAIsMovementBased)
                {
                    if(NextTarget == TilePosition)
                    {
                        Action = 0;
                        return false;
                    }
                    else
                    {
                        MovingToTilePosition = NextTarget;
                        return true;
                    }
                }
                else
                {
                    MovingToTilePosition = TilePosition;
                    return true;
                }
                
            }

            return false;
        }

        protected override void StartMove()
        {
            if (CheckWalkValid())
            {
                IsBusy = true;
                //mapRef.BC.Claim(MyCollFactFile,TilePosition);
            }
            else
            {
                if(CollisionCheck.Instance.CheckTarget(mapRef.Collision.CMap, mapRef.BC.BC, TilePosition + Direction) == 6)//is the Player...
                {
                    MakeSuspiciousOfTile(TilePosition + Direction, AlertPointsPerLevel);
                }
            }

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

    }


}
