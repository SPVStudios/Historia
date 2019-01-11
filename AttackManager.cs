using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Historia
{
    public class ATK_Packet //All the data required to process the results of an attack.
    {

        public bool IsValidAttack; //if this is false, the attack manager will simply delete it. This happens if no target is found, for example.
        public Vector2 Location;

        //ATK = Attacker, or the one attacking the TAR, or Target.
        public int ATK_Attack;
        public int ATK_Accuracy;
        public string ATK_DamageType;
        public bool ATK_IsHidden;//the attacker is hidden from the target
        public Being Attacker;//reference to the attacker

        public bool IsRegularMove;//if false, a special move is being used, and its instructions should be followed.
        public bool IsHeavyAttk;//if true, it is a heavy (regular) attack and deals 1.5x damage. else, deals 1x damage.

        public int TAR_Defense;
        public double TAR_DamageTypeModifier;//the modifier, if any, the target has against the damage type of the attack. Default is 1, less than 1 is worse.
                                             //UNIMPLEMENTED

       

        public bool InvolvesPlayer;

        public bool PlayerIsAttackingEnemy;

        public Abilities.Special_Move SpecialMoveIfAny;

        public int TAR_BlockStatus;
        public int TAR_Evasion;
        public Being Target;//reference to the attacker

        public ATK_Packet(ref Being Attacker, ref Being Target, Vector2 TargetLocation)
        {
            this.Attacker = Attacker;
            this.Target = Target;
            IsValidAttack = VerifyTarget(TargetLocation, Target);
            this.Location = TargetLocation;
            if (IsValidAttack)
            {
                PopulatePacket();
            }
        }

        public ATK_Packet(ref Being Attacker, Vector2 TargetLocation)
        {
            this.Attacker = Attacker;
            this.Target = FindTarget(TargetLocation, out IsValidAttack);
            this.Location = TargetLocation;
            if (IsValidAttack)
            {
                PopulatePacket();
            }
        }

        private void PopulatePacket()//uses the references to beings aquired in startup to fill in the necessary details
        {
            ATK_Attack = Attacker.Stats.ATK;
            ATK_Accuracy = Attacker.Stats.ACC;
            if (Attacker.CurrentAction == 3)//is a regular attack...
            {
                ATK_DamageType = string.Empty;
                IsRegularMove = true;
                IsHeavyAttk = false;
            }
            else if(Attacker.CurrentAction == 4)//is a regular (heavy) attack...
            {
                ATK_DamageType = string.Empty;
                IsRegularMove = true;
                IsHeavyAttk = true;
            }
            else//is a special move...
            {
                IsRegularMove = false;
               SpecialMoveIfAny = Attacker.Abilities[Attacker.CurrentAction - 10];
                ATK_DamageType = SpecialMoveIfAny.DamageType;
                ATK_Attack = (ATK_Attack * SpecialMoveIfAny.AttackModifier) / 100;
                ATK_Attack += SpecialMoveIfAny.ExtraAttack;
            }

            ATK_IsHidden = CalculateStealth(Attacker, Target);

            TAR_Defense = Target.Stats.DEF;
            TAR_BlockStatus = Target.BlockStatus;
            TAR_Evasion = Target.Stats.EVA;

            InvolvesPlayer = PlayerIsAttackingEnemy = false;
            if(Attacker.GetType() == typeof(Player) || Target.GetType() == typeof(Player))
            {
                if (!ATK_IsHidden)//unless it is a stealth attack, it is classed as a loud attack against the player, which will create a suspicious noise.
                {
                    InvolvesPlayer = true;
                }
                if(Attacker.GetType() == typeof(Player) && Target.GetType() != typeof(Player))// a player is attacking non-player
                {
                    PlayerIsAttackingEnemy = true;
                }

            }


            /*
             **Still Missing**
            double TAR_DamageTypeModifier;//the modifier, if any, the target has against the damage type of the attack. Default is 1, less than 1 is worse.
            */
        }

        public Being FindTarget(Vector2 TargetLocation, out bool IsValid)
        {
            GameplayScreen GameplayRef = (GameplayScreen)ScreenManager.Instance.CurrentScreen;
            if(GameplayRef.player.TilePosition == TargetLocation)
            {
                IsValid = true;
                return GameplayRef.player;
            }
            else
            {
                foreach(KeyValuePair<int,Enemy> E in GameplayRef.map.CurrentEnemies)
                {
                    Enemy Enemy = E.Value;
                    if(Enemy.TilePosition == TargetLocation)
                    {
                        IsValid = true;
                        return Enemy;
                    }
                }
                IsValid = false;
                return new Enemy();
            }
        }

        public bool VerifyTarget(Vector2 AttackLocation, Being ActualTarget)
        {
            if(AttackLocation == ActualTarget.TilePosition)
            {
                return true;
            }
            return false;
        }

        public bool CalculateStealth(Being Atacker, Being Target)
        {
            if(Attacker.GetType() == typeof(Player))//if the user is the attacker
            {
                if(Target.GetType() == typeof(Enemy))
                {
                    if((Target as Enemy).AlertLevel < 2)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }

    public class AttackManager
    {

        private static AttackManager instance;//All the functions of the class are called through this "instance". The
                                              //Following get loop means the instance is created if none are already present but otherwise uses the current
                                              //one, meaning there will only ever be one instance of the AttackManager running.
        
        public static AttackManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AttackManager();
                }
                return instance;
            }
        }

        private Random D;
        private List<ATK_Packet> Attacks_ToProcess;

        public AttackManager()
        {
            Attacks_ToProcess = new List<ATK_Packet>();
            D = new Random();
        }

        public void AddNewAttack(Being Attacker, ref Being Target, Vector2 AttackLocation)
        {
            Attacks_ToProcess.Add(new ATK_Packet(ref Attacker, ref Target, AttackLocation));
        }

        public void AddNewAttack(Being Attacker,Vector2 AttackLocation)
         {
            Attacks_ToProcess.Add(new ATK_Packet(ref Attacker, AttackLocation));
        }//works out who the target is, if any.

        public void Update(GameTime gameTime)
        {
            foreach(ATK_Packet Attack in Attacks_ToProcess)
            {
                if (Attack.IsValidAttack)
                {
                    Process_Attack(Attack);
                }
            }
            Attacks_ToProcess = new List<ATK_Packet>();//empties old attacks.
        }

        private void Process_Attack(ATK_Packet A)
        {
            if (A.IsRegularMove)
            {
                if (AccuracyCheck(A.ATK_Accuracy) == true)
                {//attack not failed

                    if (EvasionCheck(A.TAR_Evasion, A.TAR_BlockStatus) == false)
                    {//Attack lands; process damage dealt to target

                        if (ParryCheck(A.TAR_BlockStatus, out int ParryType))
                        {
                            switch (ParryType)
                            {
                                case 1://the attack's damage is mitigated, like a perfect block

                                    //sword clash stuff, SFX, Texture effect, etc
                                    break;

                                case 2://the atacker's momentum is used against them - they are STUNNED.

                                    A.Attacker.Stun();
                                    //sword clash stuff, SFX, Texture effect, etc
                                    break;

                                case 3://the attacker's momentum is deflected and a follow-up paryy force them backwards - they are STUNNED AND KNOCKED BACK

                                    A.Attacker.StunAndKnockBack();
                                    //sword clash stuff, SFX, Texture effect, etc
                                    break;
                            }
                        }
                        else
                        {
                            int Damage = 0;
                            Damage += A.ATK_Attack;
                            Damage = (Damage * 100) / A.TAR_Defense;
                            if (Damage > 10)
                            {
                                //add mild variance in figures.
                                Damage = D.Next(Damage - (Damage / 10), Damage + (Damage / 10));
                            }
                            if (A.IsHeavyAttk)//if a heavy attack, multiply by 1.5x
                            {
                                Damage = Damage * 2;
                            }
                            if (D.Next(0, 100) > 95)//critical hit (base 5% chance) doubles damage
                            {
                                Damage = Damage * 2;
                            }
                            if (A.ATK_IsHidden)//if the attacker is hidden from the opponent, they deal triple damage
                            {
                                Damage = Damage * 3;
                            }
                            //Multipliers multiply each other, so a Critical Stealth attack is a 2*3=6x bonus, not a 2+3=5x bonus.

                            //Account for blocking
                            int PercentTaken = BlockCheck(A.TAR_BlockStatus);//BlockCheck deals with damage impacts based on blocks or recovering from dodges
                            Damage = (Damage * PercentTaken) / 100;


                            A.Target.TakeDamage(Damage);//the target creature takes the calculated damage.

                        }
                        //noises and suspicions are updated regardless of outcomes to a parry

                        if (A.InvolvesPlayer)//combat noises added to stealth management system.
                        {
                            StealthManager.Instance.MakeNewNoise(A.Location, 8);
                        }

                        if (A.PlayerIsAttackingEnemy)
                        {
                            Enemy Foe = A.Target as Enemy;
                            Foe.MakeSuspiciousOfTile(A.Attacker.TilePosition, 2 * Enemy.AlertPointsPerLevel);
                        }
                    }
                    else//dodged
                    {
                        //dodged stuff, SFX, Texture effect, etc
                    }
                }
                else//attack failed
                {
                    //failed move stuff, SFX, Texture effect, etc
                }
            }
            else//Is a weird ability thing. Run its attack 
            {
                throw new NotImplementedException();
            }
        }

        private bool AccuracyCheck(int HitChance)//as a % 
        { 
            int Roll = D.Next(0, 101);
            if(Roll < HitChance)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool EvasionCheck(int EvadeChance, int Target_BlockStatus)// as a % 
        {
            switch (Target_BlockStatus)//if the target is trying to dodge...
            {
                case 9://early dodge
                    EvadeChance += 25;
                    break;
                case 10://perfect dodge
                    EvadeChance += 100;
                    break;
                case 11://later dodge
                    EvadeChance += 25;
                    break;
                case 12://recovery
                    EvadeChance -= 5;//damage taken already increased in the BlockCheck system.
                    break;
            }

            int Roll = D.Next(0, 101);
            if (Roll < EvadeChance)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Target_BlockStatus"></param>
        /// <param name="ParryType">The type of parry that occurred: 0 = nothing, 1= Damage Mitigated, 2= Attacker Stumbles, 3= Attacker Knocked Back (where possible) </param>
        /// <returns></returns>
        private bool ParryCheck(int Target_BlockStatus, out int ParryType)
        {
            switch (Target_BlockStatus)
            {
                case 4://early parry (fail)
                    ParryType = 0;
                    return false;
                    
                case 5://good parry
                    int Action5 = D.Next(1, 101);
                    if(Action5 < 10)
                    {
                        ParryType = 0;
                        return false;
                    }
                    else if(Action5 < 55)
                    {
                        ParryType = 1;
                        return true;
                    }
                    else
                    {
                        ParryType = 2;
                        return true;
                    }
                    
                case 6://perfect parry
                    int Action6 = D.Next(1, 101);
                    if(Action6 < 20)
                    {
                        ParryType = 1;//damage cancelled
                    }
                    else if(Action6 < 60)
                    {
                        ParryType = 2;//stunned
                    }
                    else
                    {
                        ParryType = 3;//knocked back and stunned
                    }
                    return true;
                case 7://good parry
                    int Action7 = D.Next(1, 101);
                    if (Action7 < 10)
                    {
                        ParryType = 0;
                        return false;
                    }
                    else if (Action7 < 55)
                    {
                        ParryType = 1;
                        return true;
                    }
                    else
                    {
                        ParryType = 2;
                        return true;
                    }
                    
                case 8://late parry (fail)
                    ParryType = 0;
                    return false;
                   
                default://not parrying
                    ParryType = 0;
                    return false;
            }


        }
        
        private int BlockCheck(int BlockStatus)//What % of damage is actually taken - based on how the target is blocking
        {
            switch (BlockStatus)
            {
                case 1:
                    return 50;
                case 2:
                    return 0;
                case 3:
                    return 50;
                case 12://recovering state of dodge
                    return 120;
                default:
                    return 100;
            }

        }
    }
}
