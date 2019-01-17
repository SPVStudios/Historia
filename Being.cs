using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Historia
{
    public class LoadStats//Contain all of the stats for aa being - these are placed onto the being itself after load from the XmlFile.
    {
        public const int GoldCap = 99999;

        private int baseATK;
        private int baseDEF;
        private int baseACC;
        private int baseEVA;
        private int baseSPD;
        private int currentHP;
        private int baseMaxHP;
        private int currentGold;

        public int ATK
        {
            get
            {
                int Total = baseATK;
                foreach(StatChange S in Changes)
                {
                    if(S.StatToChange == "ATK")
                    {
                        Total += S.Impact;
                    }
                }
                return Total;
            }
        }

        public int DEF
        {
            get
            {
                int Total = baseDEF;
                foreach (StatChange S in Changes)
                {
                    if (S.StatToChange == "DEF")
                    {
                        Total += S.Impact;
                    }
                }
                return Total;
            }
        }

        public int ACC
        {
            get
            {
                int Total = baseACC;
                foreach (StatChange S in Changes)
                {
                    if (S.StatToChange == "ACC")
                    {
                        Total += S.Impact;
                    }
                }
                Total = Math.Max(Total, 0);
                Total = Math.Min(Total, 100);
                return Total;
            }
        }

        public int EVA
        {
            get
            {
                int Total = baseEVA;
                foreach (StatChange S in Changes)
                {
                    if (S.StatToChange == "EVA")
                    {
                        Total += S.Impact;
                    }
                }
                Total = Math.Max(0, Total);
                Total = Math.Min(Total, 100);
                return Total;
            }
        }

        public int SPD
        {
            get
            {
                int Total = baseSPD;
                foreach (StatChange S in Changes)
                {
                    if (S.StatToChange == "SPD")
                    {
                        Total += S.Impact;
                    }
                }
                Total = Math.Max(Total, 1);
                return Total;
            }
        }

        public int HP
        {
            get
            {
                return currentHP;
            }
        }

        public int MaxHP
        {
            get
            {
                int Total = baseMaxHP;
                foreach (StatChange S in Changes)
                {
                    if (S.StatToChange == "MaxHP")
                    {
                        Total += S.Impact;
                    }
                }
                return Total;
            }
        }

        public int GOLD
        {
            get
            {
                return currentGold;
            }
        }


        [XmlElement("Health")]
        public int InputHP;
        [XmlElement("MaxHealth")]
        public int InputMaxHP;
        [XmlElement("ATK")]
        public int InputATK;//Attack Damage
        [XmlElement("DEF")]
        public int InputDEF;//Defense. A value of 100 means that attack dealt to this being is equal to the attack values of what attacked it. A larger value begins to reduce this: 200 halves it, 300 deals 1/3, etc.
        [XmlElement("ACC")]
        public int InputACC;//accuracy
        [XmlElement("EVA")]
        public int InputEVA;//Evasion. % chance of avoiding any given attack, irrelevant of whether dodging or not. Usually very low, <10.
        [XmlElement("MoveSpeed")]
        public int InputSPD;//Speed, as a % of base movement speed, where a standard action takes .5 seconds to complete.
        [XmlElement("Gold")]
        public int InputGold;

        //Any additional stats need to be added to the LoadStats,
        // as a protected value and a public get thing in the main body, and added to the TransferStats function in load procedures.
        [XmlElement("StatChange")]
        public List<StatChange> InputChanges;/// <summary>
        /// For Xml Only - call  ApplyStatChange to add a stat change rather than adding it to the list.
        /// </summary>

        private List<StatChange> Changes;

        public LoadStats()
        {
            Changes = new List<StatChange>();
        }

        public void LoadContent()
        {
            baseATK = InputATK;
            baseDEF = Math.Max(InputDEF,1);
            baseACC = Math.Max(InputACC,1);
            currentHP = InputHP;
            baseMaxHP = InputMaxHP;
            baseEVA = InputEVA;
            baseSPD = Math.Max(InputSPD,1);
            currentGold = InputGold;
            foreach(StatChange S in InputChanges)
            {
                ApplyStatChange(S);
            }
        }

        /// <summary>
        /// Looping the stats changes all of the input values to the current ones, so Loading Content will return the values to how they were when you last called this.
        /// </summary>
        public void LoopStats()
        {
            InputACC = baseACC;
            InputATK = baseATK;
            InputDEF = baseDEF;
            InputEVA = baseEVA;
            InputGold = currentGold;
            InputHP = currentHP;
            InputSPD = baseSPD;
            InputMaxHP = baseMaxHP;
        }
        public void AgeStatChanges()
        {
            for(int I = 0; I < Changes.Count; I++)
            {
                if (!Changes[I].QuickFix)
                {
                    Changes[I].Longevity--;
                    if(Changes[I].Longevity <= 0)
                    {
                        Changes.RemoveAt(I);
                        I--;
                    }
                }
            }
        }

        public void ApplyStatChange(StatChange S)
        {
            if (S.QuickFix)
            {
                
                switch (S.StatToChange)
                {
                    case "SPD":
                        baseSPD += S.Impact;
                        baseSPD = Math.Max(baseSPD, 1);
                        break;
                    case "ATK":
                        baseATK += S.Impact;
                        break;
                    case "DEF":
                        baseDEF += S.Impact;
                        baseDEF = Math.Max(baseDEF, 1);
                        break;
                    case "ACC":
                        baseACC += S.Impact;
                        baseACC = Math.Max(0, baseACC);
                        baseACC = Math.Min(100, baseACC);
                        break;
                    case "EVA":
                        baseEVA += S.Impact;
                        baseEVA = Math.Max(0, baseEVA);
                        baseEVA = Math.Min(100, baseEVA);
                        break;
                    case "Health":
                        currentHP += S.Impact;
                        currentHP = Math.Min(currentHP, MaxHP);
                        break;
                    case "HP":
                        currentHP += S.Impact;
                        currentHP = Math.Min(currentHP, MaxHP);
                        break;
                    case "MaxHealth":
                        baseMaxHP += S.Impact;
                        currentHP = Math.Min(currentHP, MaxHP);
                        break;
                    case "MaxHP":
                        baseMaxHP += S.Impact;
                        currentHP = Math.Min(currentHP, MaxHP);
                        break;
                    case "Gold":
                        currentGold += S.Impact;
                        currentGold = Math.Max(currentGold, 0);
                        currentGold = Math.Min(currentGold, GoldCap);
                        break;
                    default:
                        throw new Exception("No Such Stat");
                }

            }
            else
            {
                Changes.Add(S);
            }
        }

        public void Heal(int Gain)
        {
            currentHP += Gain;
            currentHP = Math.Min(MaxHP, currentHP);
        }

        public void GiveGold(int Gain)
        {
            if(Gain > 0)
            {
                currentGold += Gain;
            }
        }

        public void Damage(int Loss)
        {
            currentHP -= Loss;
            currentHP = Math.Max(currentHP, 0);
        }
    }

    public class StatChange
    {
        public string StatToChange;//name of the stat to change.
        public int Impact;//+ or - by whatever number. Will not exceed or fal below given natural boundaries.

        public bool QuickFix;//indicates whether the stat change was a one-time permanent correction. If false, apply the following as a time-limited buff.
        public int Longevity;
    }

    public abstract class Being
    {
        public Being()
        {

            Abilities = new Historia.Abilities.Special_Move[MaxAbilities];
            ActiveAbilities = new string[MaxAbilities];//gives the names of the abilities it has. Should all be present in UnlockedAbilities.
            IsAlive = true;
            CurrentNoiseOutput = BaseNoise;
            AttackPhase = 0;

        }

        public bool IsAlive;

        public Image Image;

        [XmlIgnore]
        protected Map mapRef;

        [XmlIgnore]
        public Vector2 Direction
        {
            get
            {
                switch (FacingAddup)
                {
                    case 0:
                        return new Vector2(0, 1);
                    case 1:
                        return new Vector2(-1, 0);
                    case 2:
                        return new Vector2(1, 0);
                    case 3:
                        return new Vector2(0, -1);
                    default:
                        throw new Exception("given invalid FacingAddup");
                }
            }
        }

        public Vector2 TileDimensions;
        [XmlIgnore]


        public int CollisionType;

        public LoadStats Stats;//contains all values such as Attack, Defense, Speed, Evasion, and Accuracy, as well as values like Gold totals.

        protected int FacingAddup;
        protected int ActionFrameSet;

        public int CurrentAction { get { return ActionFrameSet; } }

        protected Vector2 offset;
        public Vector2 Offset { get { return offset; } }
        public Vector2 TilePosition;
        protected Vector2 TileAllign;

        private bool HasBegunAction;
        protected bool IsBusy;
        int ActionTimeOverflow;//the number of milliseconds of passed time that didn't get used for anything in the last update pass.

        const int StunnedTime = 4000;//the time a being spends stunned, when stunned. in ms. This will be doubled when StunnedAndKnockedBack.
        int StunTimeRemaining;
        bool IsStunned;
        bool IsStunnedAndKnockedBack;

        int moveTime;// in Milliseconds. ensure that this is divisible by 2.
        protected int walkOffsetPixelTime;// in milliseconds
        protected int runOffsetPixelTime;//in ms. Half of the above.
        protected int sneakOffsetPixelTime;//in ms. 3 times the above(<--run time. 1.5x slower than normal walking; 2/3 speed)
        protected int dodgeOffsetPixelTime;
        int pixelsMoved;

        protected const int BaseMoveTime = 50000;//the amount of time, times 100 (the base speed stat). A MoveSpeed greater than
                                                 //100 will therefore cut the time it takes to move.
                                                 //A base speed of 100 means that moving tiles will take 500ms, or .5 seconds.

        int KnockBackOffsetTime;

        int AttackPhase;//which part of the attack 
        const int BaseNoise = 1;
        protected int CurrentNoiseOutput;

        [XmlIgnore]
        public CollFactFile MyCollFactFile { get; protected set; }
        

        [XmlIgnore]
        public int CurrentRoomIn { get; protected set; }
        [XmlIgnore]
        public int CurrentPassagewayIn { get; protected set; }
        [XmlIgnore]
        public bool IsInRoom { get; protected set; }//false denotes being in a passageway instead.
        protected Rectangle CurrentLocaleBounds;


        [XmlIgnore]
        public Abilities.Special_Move[] Abilities { get; protected set; }

        [XmlElement("HasAbility")]
        public List<string> UnlockedAbilities;

        [XmlIgnore]
        public string[] ActiveAbilities { get; protected set; }//gives the names of the abilities it has. Should all be present in UnlockedAbilities.

        protected string activeabilites;//For Load from Xml only - to be placed straight into the above ARRAY for use.

        const int MaxAbilities = 6;

        public bool IsInContainer;
        /// <summary>
        /// 
        /// 0 = not blocking
        /// 
        /// 1 = blocking
        /// 2 = perfect blocking
        /// 3 = past-perfect blocking (same as 1, close to finishing)
        /// 
        /// 4 = parrying (fail)
        /// 5 = good parry
        /// 6 = perfect parrying
        /// 7 = good parry
        /// 8 = parry end (fail)
        /// 
        /// 9 = Early Dodge (chance of avoiding damage, WHILST MOVING)
        /// 10 = perfect dodge (avoid all damage)
        /// 11 = late dodge (chance of avoiding damage, whilst ARRIVING AT NEW TILE)
        /// 12 = recovering from dodge (slightly more vulnerable)
        /// 
        /// </summary>
        public int BlockStatus;
        

        public virtual void LoadContent(Vector2 TileDimensions, Vector2 SpawnLocation, Map map)
        {
            this.mapRef = map;
            Image.LoadContent();
            Image.IsActive = false;//whilst the being is placed
            TilePosition = SpawnLocation;
            Image.Position = TilePosition * TileDimensions;
            this.TileDimensions = TileDimensions;
            Stats.LoadContent();
            AssignNewMoveTime(Stats.SPD);
            TileAllign = new Vector2((TileDimensions.X - Image.SpriteSheetEffect.FrameWidth) / 2, TileDimensions.Y - Image.SpriteSheetEffect.FrameHeight);
            CalculateCurrentRoomIn();
            IsStunned = false;
            IsStunnedAndKnockedBack = false;
            MyCollFactFile = new CollFactFile(CollisionType, true, -1);
            var obj = this;
            foreach (Abilities.Special_Move Ability in Abilities)
            {
                if (Ability != null)
                {
                    Ability.LoadContent(ref obj);
                }

            }
            Image.IsActive = true;//made visible now it has an actual location
        }

        public virtual void LoadContent(Vector2 TileDimensions)//AS ABOVE - but no specified spawn, as either has one (Xml) or doesn't need one(blueprint)
        {
            Image.LoadContent();
            Image.IsActive = true;
            Image.Position = TilePosition * TileDimensions;
            this.TileDimensions = TileDimensions;
            Stats.LoadContent();
            IsStunned = false;
            IsStunnedAndKnockedBack = false;
            AssignNewMoveTime(Stats.SPD);
            TileAllign = new Vector2((TileDimensions.X - Image.SpriteSheetEffect.FrameWidth) / 2, TileDimensions.Y - Image.SpriteSheetEffect.FrameHeight);
            MyCollFactFile = new CollFactFile(0, true, -1);
        }

        public virtual void UnloadContent()
        {
            Image.UnloadContent();
        }

        public virtual void Update(GameTime gameTime, out bool Died)
        {
            if (IsAlive)
            {
                if (IsStunned)
                {
                    StunTimeRemaining -= gameTime.ElapsedGameTime.Milliseconds;
                    if (StunTimeRemaining <= 0)
                    {
                        StunTimeRemaining = 0;
                        IsStunned = false;
                        IsStunnedAndKnockedBack = false;
                        ReturnToIdle();
                    }
                }
                else if (IsStunnedAndKnockedBack)
                {
                    //monitor knockback
                    if (pixelsMoved < TileDimensions.X)
                    {
                        int runningtotal = ActionTimeOverflow + gameTime.ElapsedGameTime.Milliseconds;//pun intended
                        if (runningtotal >= KnockBackOffsetTime)
                        {
                            int pixelsNow = runningtotal / KnockBackOffsetTime;
                            ActionTimeOverflow = runningtotal % KnockBackOffsetTime;
                            pixelsMoved += pixelsNow;

                            if (pixelsMoved >= TileDimensions.X)
                            {
                                if (pixelsMoved > TileDimensions.X)
                                {
                                    int Reduction = pixelsMoved - (int)TileDimensions.X;
                                    pixelsNow -= Reduction;
                                }
                                pixelsMoved = (int)TileDimensions.X;
                                ActionTimeOverflow = 0;
                                CurrentNoiseOutput = BaseNoise;//reverts to normal near-silence. A perk could change this to be 0. (smhw)
                            }
                            offset += -Direction * pixelsNow;
                        }
                        else
                        {
                            ActionTimeOverflow += runningtotal;
                        }
                    }

                    //monitor overall stun
                    StunTimeRemaining -= gameTime.ElapsedGameTime.Milliseconds;
                    if (StunTimeRemaining <= 0)
                    {
                        StunTimeRemaining = 0;
                        IsStunned = false;
                        IsStunnedAndKnockedBack = false;
                        ReturnToIdle();
                    }
                }
                else
                {
                    if (!IsBusy)
                    {
                        HasBegunAction = DecideWhattoDo(gameTime, out ActionFrameSet);//see summary below

                    }
                    if (HasBegunAction)
                    {
                        StartAction(gameTime);
                    }
                    if (IsBusy)//if the character is moving (either since previously, or having only just started)
                    {
                        Act(gameTime);
                        if (!IsBusy)
                        {
                            ReturnToIdle();
                        }
                    }
                }
                Died = false;
            }
            else
            {
                Died = true;
                Image.IsActive = false;
            }
            AdjustOffsetsX(gameTime);
            AdjustOffsetsY(gameTime);
            UpdateCurrentRoomIn();
            Image.Update(gameTime);
        }

        public virtual void Act(GameTime gameTime)//decides what actions are to be taken. Override to add or change actions, but remember that some int codes are taken.
        {
            switch (ActionFrameSet)
            {
                case 0:
                    throw new Exception("This Should be Idle, therefore not doing anything");
                case 1:
                    Move(gameTime);
                    break;
                case 2:
                    Run(gameTime);
                    break;
                case 3:
                    Attack(gameTime);
                    break;
                case 4:
                    HeavyAttack(gameTime);
                    break;
                case 5:
                    Dodge(gameTime);
                    break;
                case 6:
                    Block(gameTime);
                    break;
                case 7:
                    Parry(gameTime);
                    break;
                case 8:
                    Sneak(gameTime);
                    break;
                case 9:
                    throw new NotImplementedException();
                case 10:
                    Abilities[0].ActOut();
                    break;
                case 11:
                    Abilities[1].ActOut();
                    break;
                case 12:
                    Abilities[2].ActOut();
                    break;
                case 13:
                    Abilities[3].ActOut();
                    break;
                case 14:
                    Abilities[4].ActOut();
                    break;
                case 15:
                    Abilities[5].ActOut();
                    break;
                default:
                    throw new Exception("This Move Shouldn't Exist!");
            }
        }
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

        protected void AdjustOffsetsX(GameTime gameTime)//adjusts tile positions and offsets if offset is such that it is onto the next tile.
        {
            if ((TileDimensions.X / 2) <= offset.X)
            {
                if (mapRef.BC.MoveAlong(MyCollFactFile, TilePosition + new Vector2(1, 0)))
                {
                    // has vacated the old square and inhabit new square on BeingColl as was successful
                    TilePosition.X++;
                    offset.X -= TileDimensions.X;
                }
                else
                {
                    offset.X -= TileDimensions.X;//puts at the start of this tile to negate the offset that would be caused any other way.
                }

            }
            else if (offset.X < -(TileDimensions.X / 2))
            {
                if (mapRef.BC.MoveAlong(MyCollFactFile, TilePosition + new Vector2(-1, 0)))
                {
                    // has vacated the old square and inhabit new square on BeingColl as was successful
                    offset.X += TileDimensions.X;
                    TilePosition.X--;
                }
                else
                {
                    offset.X += TileDimensions.X;
                    //puts at the start of this tile to negate the offset that would be caused any other way.
                }
            }

        }

        protected void AdjustOffsetsY(GameTime gameTime)//as above, for Y.
        {
            if ((TileDimensions.Y / 2) <= offset.Y)
            {
                if (mapRef.BC.MoveAlong(MyCollFactFile, TilePosition + new Vector2(0, 1)))
                {
                    // has vacated the old square and inhabit new square on BeingColl as was successful
                    TilePosition.Y++;
                    offset.Y -= TileDimensions.Y;
                }
                else
                {
                    offset.Y -= TileDimensions.Y;
                    //puts at the start of this tile to negate the offset that would be caused any other way.
                }
            }
            else if (offset.Y < -(TileDimensions.Y / 2))
            {
                if (mapRef.BC.MoveAlong(MyCollFactFile, TilePosition + new Vector2(0, -1)))
                {
                    // has vacated the old square and inhabit new square on BeingColl as was successful
                    offset.Y += TileDimensions.Y;
                    TilePosition.Y--;
                }
                else
                {
                    offset.Y += TileDimensions.Y;
                    //puts at the start of this tile to negate the offset that would be caused any other way.
                }
            }

        }

        //Start Action Methods

        protected virtual void StartAction(GameTime gameTime)
        {
            HasBegunAction = false;
            Image.SpriteSheetEffect.CurrentFrame.Y = (ActionFrameSet * 4) + FacingAddup;
            if (ActionFrameSet > 0)
            {

                switch (ActionFrameSet)
                {

                    case 1:
                        StartMove();
                        break;
                    case 2://or running
                        StartMove();
                        break;
                    case 3:
                        StartAttack(false);
                        break;
                    case 4:
                        StartAttack(true);
                        break;
                    case 5:
                        StartDodge();
                        break;
                    case 6:
                        StartBlock();
                        break;
                    case 7:
                        StartParry();
                        break;
                    case 8://for sneaking
                        StartMove();
                        break;
                    case 9:
                        Interact(gameTime);//is an instant thing atm, so deosn't reach process of interact at ACT stage, but at START stage.
                        break;
                    case 10:
                        IsBusy = Abilities[0].StartAction();
                        break;
                    case 11:
                        IsBusy = Abilities[0].StartAction();
                        break;
                    case 12:
                        IsBusy = Abilities[2].StartAction();
                        break;
                    case 13:
                        IsBusy = Abilities[3].StartAction();
                        break;
                    case 14:
                        IsBusy = Abilities[4].StartAction();
                        break;
                    case 15:
                        IsBusy = Abilities[5].StartAction();
                        break;
                }
            }
        }

        protected void StartAttack(bool IsHeavy)
        {
            IsBusy = true;
            if (IsHeavy)
            {
                Image.SpriteSheetEffect.StartLimitedLoop((ActionFrameSet * 4) + FacingAddup, 1, FacingAddup,
                    (Image.SpriteSheetEffect.SwitchFrame * 3) / 2);
            }
            else
            {
                Image.SpriteSheetEffect.StartLimitedLoop((ActionFrameSet * 4) + FacingAddup, 1, FacingAddup);
            }

            ActionTimeOverflow = 0;
            AttackPhase = 1;
        }

        protected void StartBlock()
        {
            IsBusy = true;
            Image.SpriteSheetEffect.StartLimitedLoop((ActionFrameSet * 4) + FacingAddup, 1, FacingAddup);
            ActionTimeOverflow = 0;
            BlockStatus = 1;
        }

        protected void StartParry()
        {
            IsBusy = true;
            Image.SpriteSheetEffect.StartLimitedLoop((ActionFrameSet * 4) + FacingAddup, 1, FacingAddup);
            ActionTimeOverflow = 0;
            BlockStatus = 4;
        }

        protected void StartDodge()
        {
            if (CheckWalkValid())
            {
                IsBusy = true;
                ActionTimeOverflow = 0;
                BlockStatus = 9;
                pixelsMoved = 0;
            }
        }

        protected abstract void StartMove();
        

        //Action Set Methods
        /// <summary>
        /// Below are the methods that are run for each action a being can complete.
        /// 
        /// These are run when the update logic has called for them to do so, but this occurs mainly due to the AI and due to input for the player.
        /// </summary>
        /// <param name="gameTime"></param>

        protected virtual void Move(GameTime gameTime)
        {
            int runningtotal = ActionTimeOverflow + gameTime.ElapsedGameTime.Milliseconds;//pun intended
            CurrentNoiseOutput = BaseNoise * 3;
            if (runningtotal >= walkOffsetPixelTime)
            {
                int pixelsNow = runningtotal / walkOffsetPixelTime;
                ActionTimeOverflow = runningtotal % walkOffsetPixelTime;
                pixelsMoved += pixelsNow;

                if (pixelsMoved >= TileDimensions.X)
                {
                    if (pixelsMoved > TileDimensions.X)
                    {
                        int Reduction = pixelsMoved - (int)TileDimensions.X;
                        pixelsNow -= Reduction;
                    }
                    IsBusy = false;
                    pixelsMoved = 0;
                    ActionTimeOverflow = 0;
                    CurrentNoiseOutput = BaseNoise;//reverts to normal near-silence. A perk could change this to be 0. (smhw)
                }
                offset += Direction * pixelsNow;
            }
            else
            {
                ActionTimeOverflow += runningtotal;
            }

        }

        protected virtual void Run(GameTime gameTime)//like walking, but with a diff animation, faster, and noisier
        {
            int runningtotal = ActionTimeOverflow + gameTime.ElapsedGameTime.Milliseconds;//pun intended
            CurrentNoiseOutput = 6 * BaseNoise;//twice as loud
            if (runningtotal >= runOffsetPixelTime)
            {
                int pixelsNow = runningtotal / runOffsetPixelTime;
                ActionTimeOverflow = runningtotal % walkOffsetPixelTime;
                pixelsMoved += pixelsNow;
                if (pixelsMoved >= TileDimensions.X)
                {

                    if (pixelsMoved > TileDimensions.X)
                    {
                        int Reduction = pixelsMoved - (int)TileDimensions.X;
                        pixelsNow -= Reduction;
                    }
                    pixelsMoved = 0;
                    IsBusy = false;
                    ActionTimeOverflow = 0;
                    CurrentNoiseOutput = BaseNoise;//reverts to normal near-silence. A perk could change this to be 0. (smhw)
                }
                offset += Direction * pixelsNow;
            }

        }

        protected virtual void Attack(GameTime gameTime)//see blocks and stuff for idea on Attack Manager singleton class to handle attacks in levels
        {
            ActionTimeOverflow += gameTime.ElapsedGameTime.Milliseconds;
            if (ActionTimeOverflow >= Image.SpriteSheetEffect.TimeforOneLoop)
            {
                //process attack with AttackManager
                AttackManager.Instance.AddNewAttack(this, TilePosition + Direction);

                IsBusy = false;
                ActionTimeOverflow = 0;
            }
        }

        protected virtual void HeavyAttack(GameTime gameTime)//as above, but takes 2x as long to set-up, and deals 2.5x the damage.
        {
            ActionTimeOverflow += gameTime.ElapsedGameTime.Milliseconds;
            if (ActionTimeOverflow >= Image.SpriteSheetEffect.TimeforOneLoop)
            {
                //process attack with AttackManager
                AttackManager.Instance.AddNewAttack(this, TilePosition + Direction);
                IsBusy = false;
                ActionTimeOverflow = 0;
            }
            //will return to 0+FacingAddup (idle) once completed animation.
        }

        protected virtual void Dodge(GameTime gameTime)//quickly change tile (perhaps up to 2 in one go) but be extra-vulnerable (and slow down) during the end-stage of the move to delay the next action. 
        {
            int runningtotal = ActionTimeOverflow + gameTime.ElapsedGameTime.Milliseconds;
            int pixelsNow = 0;

            if (pixelsMoved < (TileDimensions.X * 2) / 3)
            {
                if (runningtotal >= dodgeOffsetPixelTime)
                {
                    pixelsNow = runningtotal / dodgeOffsetPixelTime;
                    ActionTimeOverflow = runningtotal % dodgeOffsetPixelTime;
                    pixelsMoved += pixelsNow;
                }
                else
                {
                    ActionTimeOverflow += runningtotal;
                }
            }
            else//last third
            {
                if (runningtotal >= sneakOffsetPixelTime)
                {
                    pixelsNow = runningtotal / sneakOffsetPixelTime;
                    ActionTimeOverflow = runningtotal % sneakOffsetPixelTime;
                    pixelsMoved += pixelsNow;
                }
                else
                {
                    ActionTimeOverflow += runningtotal;
                }
            }
            switch ((pixelsMoved * 4 / (int)TileDimensions.X))
            {
                case 0:
                    BlockStatus = 9;
                    break;
                case 1:
                    BlockStatus = 10;
                    break;
                case 2:
                    BlockStatus = 11;
                    break;
                case 3:
                    BlockStatus = 12;
                    break;
                case 4://complete
                    if (pixelsMoved > TileDimensions.X)
                    {
                        int Reduction = pixelsMoved - (int)TileDimensions.X;
                        pixelsNow -= Reduction;
                    }
                    IsBusy = false;
                    ActionTimeOverflow = 0;
                    pixelsMoved = 0;
                    BlockStatus = 0;
                    break;
            }
            offset += Direction * pixelsNow;
        }

        protected virtual void Parry(GameTime gameTime)
        /// <summary>
        ///  stand still, and if well-timed for when an enemy strikes from the facing direction, they will stumble.
        ///  
        /// If "perfectly timed", enemies from the target direction are stumbled for more, and attacks from OTHER directions are also knocked back.
        /// 
        /// This should be achieved by the Parry move affecting an integer value for BlockStatus, with one number for 'Parrying' and one for 'PerfectParrying' over the course of the action
        /// (which will take a set amount of time like a block)
        /// 
        /// When an "AttackManager" instance calls for the attack to be carried out on this being, it will check BlockStatus to decide what will happen.
        /// </summary>
        {
            ActionTimeOverflow += gameTime.ElapsedGameTime.Milliseconds;
            if (ActionTimeOverflow >= Image.SpriteSheetEffect.TimeforOneLoop / 5)
            {
                if (BlockStatus < 8)
                {
                    BlockStatus++;
                    ActionTimeOverflow -= (Image.SpriteSheetEffect.TimeforOneLoop / 3);
                }
                else
                {
                    IsBusy = false;
                    ActionTimeOverflow = 0;
                    BlockStatus = 0;
                }
            }

        }

        protected virtual void Block(GameTime gameTime)
        /// <summary>
        ///  stand still, and suffer very little damage from the attack approaching from the tile you are facing.
        ///  
        /// If "perfectly timed", enemies from the target direction are completely blocked damage-wise, and attacks from the left or right are also blocked as normal.
        /// 
        /// This should be achieved by the Block move affecting an integer value for BlockStatus, with one number for 'Blocking' and one for 'PerfectBlocking' over the course of the action
        /// (which will take a set amount of time like a parry)
        /// 
        /// When an "AttackManager" instance calls for the attack to be carried out on this being, it will check BlockStatus to decide what will happen.
        /// </summary>
        {
            ActionTimeOverflow += gameTime.ElapsedGameTime.Milliseconds;
            if (ActionTimeOverflow >= Image.SpriteSheetEffect.TimeforOneLoop / 3)
            {
                if (BlockStatus < 3)
                {
                    BlockStatus++;
                    ActionTimeOverflow -= (Image.SpriteSheetEffect.TimeforOneLoop / 3);
                }
                else
                {
                    IsBusy = false;
                    ActionTimeOverflow = 0;
                    BlockStatus = 0;
                }
            }
        }

        protected virtual void Sneak(GameTime gameTime)//like walking, but 2/3 the speed, making little noise.
        {
            int runningtotal = ActionTimeOverflow + gameTime.ElapsedGameTime.Milliseconds;//pun intended
            CurrentNoiseOutput = BaseNoise;
            if (runningtotal >= sneakOffsetPixelTime)
            {
                int pixelsNow = runningtotal / sneakOffsetPixelTime;
                ActionTimeOverflow = runningtotal % sneakOffsetPixelTime;
                pixelsMoved += pixelsNow;
                if (pixelsMoved >= TileDimensions.X)
                {
                    if (pixelsMoved > TileDimensions.X)
                    {
                        int Reduction = pixelsMoved - (int)TileDimensions.X;
                        pixelsNow -= Reduction;
                    }
                    pixelsMoved = 0;
                    IsBusy = false;
                    ActionTimeOverflow = 0;
                }
                offset += Direction * pixelsNow;
            }
            else
            {
                ActionTimeOverflow += runningtotal;
            }
            CurrentNoiseOutput = BaseNoise;//reverts to normal near-silence. A perk could change this to be 0. (smhw)
        }

        protected virtual void Interact(GameTime gameTime)
        {

        }

        //end Action Methods

        protected virtual void ReturnToIdle()
        {
            IsBusy = false;
            ActionFrameSet = 0;
            Image.SpriteSheetEffect.SwitchFrame = Image.SpriteSheetEffect.BaseAnimationSpeed;
            Image.SpriteSheetEffect.CurrentFrame.Y = FacingAddup;

        }

        protected abstract bool DecideWhattoDo(GameTime gameTime, out int Action);//the bool is returned true if anything is decided and an action is started.
                                                                                  /// <summary Decide What to Do...>
                                                                                  /// This Method will contain all logic defining what the chracter will do next, based on AI for the enemies and input for the hero.
                                                                                  /// 
                                                                                  /// It should only really be allowed to change the values for what action is being comitted.
                                                                                  /// 
                                                                                  /// If an action is already occuring, this doesn't run.
                                                                                  /// </summary>

        public virtual void Draw(SpriteBatch spriteBatch, Camera camera)
        {
            Image.Position = (TilePosition - camera.Origin) * TileDimensions;
            Image.Position -= camera.Offset;
            Image.Position += offset;
            Image.Position += TileAllign;

            Image.Draw(spriteBatch);
        }

        public virtual void Draw(SpriteBatch spriteBatch, Vector2 CameraOffset, Vector2 Origin)
        {
            Image.Position = (TilePosition - Origin) * TileDimensions;
            Image.Position -= CameraOffset;
            Image.Position += offset;
            Image.Position += TileAllign;

            Image.Draw(spriteBatch);
        }

        public void AssignNewMoveTime(int NewMoveSpeed)
        {
            
            moveTime = BaseMoveTime / NewMoveSpeed;//how long it takes to walk 1 tile.
            walkOffsetPixelTime = moveTime / (int)TileDimensions.X;
            runOffsetPixelTime = walkOffsetPixelTime / 2;
            sneakOffsetPixelTime = runOffsetPixelTime * 3;
            dodgeOffsetPixelTime = walkOffsetPixelTime / 3;
        }

        protected bool CheckWalkValid()
        {
            if (CollisionCheck.Instance.CheckTargetBool(mapRef.Collision.CMap, mapRef.BC.BC, TilePosition + Direction))
            {//if Is Passable (collision check)
                return true;
            }
            else
            {
                //Play "thud" sound effect [Unimplemented!!]
                return false;
            }
        }

        protected void CalculateCurrentRoomIn()
        {
            for (int R = 0; R < mapRef.Rooms.Count; R++)
            {
                if (RectMethod.LocationIsInRectangle(TilePosition, mapRef.Rooms[R].Location[0]))
                {
                    CurrentLocaleBounds = mapRef.Rooms[R].Location[0];
                    IsInRoom = true;
                    CurrentRoomIn = R;
                    return;
                }
            }
            //now checks passageways
            for (int P = 0; P < mapRef.Passages.Count; P++)
            {
                if (RectMethod.LocationIsInRectangle(TilePosition, mapRef.Passages[P].Location))
                {
                    CurrentLocaleBounds = mapRef.Passages[P].Location;
                    IsInRoom = false;
                    CurrentPassagewayIn = P;
                    return;
                }
            }
            if(TilePosition == mapRef.EntryLoc)
            {
                IsInRoom = true;
                CurrentRoomIn = RectMethod.FindWhatRoomLocationIsIn(TilePosition + new Vector2(0, 1), mapRef, out bool Useless);
                return;
            }
            throw new Exception("Not in a Room or a Passageway..");//NOTE : THIS CANNOT HANDLE ENTRY TILES YET!!!!!!!!!!!
        }

        protected void UpdateCurrentRoomIn()//faster system, assuming previous location is correct.
        {
            if (!RectMethod.LocationIsInRectangle(TilePosition, CurrentLocaleBounds))//if no longer in previous location...
            {
                if (IsInRoom)//if it was in a room, check the surrounding passages
                {
                    for (int P = 0; P < mapRef.Rooms[CurrentRoomIn].EntryPoints.Count; P++)//check the passages that the previous room connected to
                    {
                        int ThisPassage = mapRef.Rooms[CurrentRoomIn].EntryPoints[P].PassagewayUsing;
                        if (RectMethod.LocationIsInRectangle(TilePosition, mapRef.Passages[ThisPassage].Location))
                        {
                            CurrentLocaleBounds = mapRef.Passages[ThisPassage].Location;
                            IsInRoom = false;
                            CurrentPassagewayIn = ThisPassage;
                            return;
                        }
                    }
                    //Something's Gone Wrong... but run CalculateCurrentRoom as a backup
                    CalculateCurrentRoomIn();
                    return;
                }
                else//was in a passageway
                {//Check the two rooms that that the passageway connected to
                    int Room1Ind = mapRef.Passages[CurrentPassagewayIn].End1.EndPointID;
                    //check the 2 ends, End1 first, End2 second.
                    if (RectMethod.LocationIsInRectangle(TilePosition, mapRef.Rooms[Room1Ind].Location[0]))
                    {
                        CurrentLocaleBounds = mapRef.Rooms[Room1Ind].Location[0];
                        IsInRoom = true;
                        CurrentRoomIn = Room1Ind;
                        return;
                    }
                    int Room2Ind = mapRef.Passages[CurrentPassagewayIn].End2.EndPointID;
                    //now try the 2nd end
                    if (RectMethod.LocationIsInRectangle(TilePosition, mapRef.Rooms[Room2Ind].Location[0]))
                    {
                        CurrentLocaleBounds = mapRef.Rooms[Room2Ind].Location[0];
                        IsInRoom = true;
                        CurrentRoomIn = Room2Ind;
                        return;
                    }

                }
                throw new Exception("New Location Not Found");
            }
        }

        protected static int GetFacing(Vector2 Direction)//reverses the process that gives the Bing's Direction value to fin facing based on a vector direction.
        {
            if (Direction == new Vector2(0, 1))
            {
                return 0;
            }
            if (Direction == new Vector2(-1, 0))
            {
                return 1;
            }
            if (Direction == new Vector2(1, 0))
            {
                return 2;
            }
            if (Direction == new Vector2(0, -1))
            {
                return 3;
            }
            else
            {
                throw new Exception("given invalid Direction - must only have a value for X OR Y");
            }
        }

        public static Vector2 GetDirection(int Facing)//Mimics the process to give the Being's Direction for other uses.
        {

            {
                switch (Facing)
                {
                    case 0:
                        return new Vector2(0, 1);
                    case 1:
                        return new Vector2(-1, 0);
                    case 2:
                        return new Vector2(1, 0);
                    case 3:
                        return new Vector2(0, -1);
                    default:
                        throw new Exception("given invalid FacingAddup");
                }

            }

        }

        //Methods to handle the loading, activation, switching and so on of abilities.

        public void ActivateAbility<AbilityType>(string NewAbility, int Slot)//(re)places the ability in a given slot.
        {
            if (UnlockedAbilities.Contains(NewAbility))
            {
                if (Abilities[Slot] != null)
                {
                    RemoveAbility(Slot);//remove the old ability
                }
                ActiveAbilities[Slot] = NewAbility;

                AbilityType NewAB = (AbilityType)Activator.CreateInstance(typeof(AbilityType));
                XmlManager<AbilityType> PowerLoader = new XmlManager<AbilityType>();
                NewAB = PowerLoader.Load("Load/Gameplay/Abilities/" + NewAbility + ".xml");
                var obj = this;
                (NewAB as Abilities.Special_Move).LoadContent(ref obj);
            }
        }

        public void StoreActiveAbilities()//stores currently equipped abilities as the one string for activatedabilities.
        {
            activeabilites = String.Empty;
            for (int A = 0; A < MaxAbilities; A++)
            {
                activeabilites += ActiveAbilities[A] + ":";
            }

            activeabilites.Remove(activeabilites.Length - 1);//removes the last colon which is unnecessary
        }

        public void RemoveAbility(int Slot)
        {
            ActiveAbilities[Slot] = string.Empty;
            Abilities[Slot].UnloadContent();
            Abilities[Slot] = null;
        }

        public void LoadAbilities()//loads the being's abilities after being compiled from an Xml file
        {
            if (activeabilites != string.Empty && activeabilites != null)
            {
                ActiveAbilities = activeabilites.Split(':');
                for (int A = 0; A < MaxAbilities; A++)
                {
                    //ActivateAbility(ActiveAbilities[A],A);
                }
            }
        }

        //Combat Procedures
        public void TakeDamage(int Damage)
        {
            Stats.Damage(Damage);
            if (Stats.HP <= 0)
            {
                Die();
            }
        }

        public void Stun()
        {
            IsStunned = true;
            StunTimeRemaining = StunnedTime;
            ActionFrameSet = 0;
        }

        public void StunAndKnockBack()
        {
            IsStunnedAndKnockedBack = true;
            StunTimeRemaining = StunnedTime * 2;
            int KnockbackTime = StunnedTime * 2 / 3;
            ActionTimeOverflow = 0;
            KnockBackOffsetTime = KnockbackTime / (int)TileDimensions.X;
            pixelsMoved = 0;
            ActionFrameSet = 0;
        }

        public virtual void Die()
        {
            IsAlive = false;
            ActionFrameSet = 10;
            IsBusy = false;
            mapRef.BC.RemovePreviousLocation(MyCollFactFile, Vector2.Zero);
            Image.SpriteSheetEffect.CurrentFrame.Y = 40 + FacingAddup;//dead (10*4, as the 11th action, 10 being interact.)
            Image.SpriteSheetEffect.IsActive = false;
        }

        //Extra Loading Procedures

       

        
    }
}
