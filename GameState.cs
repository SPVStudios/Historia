using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Xml.Serialization;
using System.Xml;


namespace Historia
{
    public class GameState
    {

        private static GameState instance;//this creates a private instance of the class to allow GameState to be used
        //as a "singleton" class - a class that can only have one version of itself in existence.
        public static GameState Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameState();

                }
                return instance;//SINGLETON - creates a new instance of the class to be called by other classes ONLY IF one does not yet exist.
            }
        }

        public List<Quest> CurrentQuests
        {
            get
            {
                List<Quest> Quests = new List<Quest>();
                foreach (Village V in GameState.Instance.WorldMap.Villages.Values)
                {
                    foreach (Quest Q in V.ActiveQuests)
                    {
                        Quests.Add(Q);
                    }
                }
                return Quests;
            }
        }

        public bool IsCreated
        {
            get
            {
                if(WorldMap != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
           
        }

        public GameState()
        {
            R = new Random();
        }


        public Random R;

        public WorldMap WorldMap;

        public Dungeon CurrentDungeon;

        public bool PlayerExists;

        public LoadStats PlayerStats
        {
            get
            {
                playerStats.LoopStats();
                return playerStats;
            }
        }

        private LoadStats playerStats;

        public bool EnteringDungeon;
        public string DungeonName;
       //environment found from tracking WorldMap's Dungeons at given location
        public Vector2 WorldMapLocation;

        public bool EnteringVillage;
        public string VillageName;

        public bool EnteringWorld;

        /// <summary>
        /// DO NOT USE LIGHTLY!!! WARNING!!! Clears literally all gamestate data.
        /// </summary>
        public void Clear()
        {
            if (WorldMap != null)
            {
                WorldMap.UnloadContent();
                WorldMap = null;
            }
            playerStats = null;
            EnteringDungeon = EnteringVillage = EnteringWorld = false;
            PlayerExists = false;
            DungeonName = null;
            WorldMapLocation = Vector2.Zero;
        }

        public bool QuestForDungeon(Dungeon reference, out Quest output)
        {
            List<Quest> AllQuests = CurrentQuests;
            foreach(Quest Q in AllQuests)
            {
                if(ContainsQuestOfThatDungeon(Q,reference,out Quest thatQuest))
                {
                    output = thatQuest;
                    return true;
                }
            }
            output = null;
            return false;
        }

        private bool ContainsQuestOfThatDungeon(Quest ToCheck, Dungeon reference, out Quest questinvolved)
        {
            if(ToCheck.GetType() == typeof(CompoundQuest))
            {
                CompoundQuest Q = ToCheck as CompoundQuest;
                foreach(Quest Sub in Q.SubQuests)
                {
                    if(ContainsQuestOfThatDungeon(Sub,reference,out questinvolved))
                    {
                        return true;
                    }
                }
                questinvolved = null;
                return false;
            }
            else if(ToCheck.GetType() == typeof(DungeonQuest))
            {
                DungeonQuest Qu = ToCheck as DungeonQuest;
                if(Qu.Target == reference)
                {
                    questinvolved = Qu;
                    return true;
                }
                else
                {
                    questinvolved = null;
                    return false;
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public void UpdatePlayerStats(LoadStats New)
        {
            playerStats = New;
        }
        

        
    }
}
