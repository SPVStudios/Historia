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
    public class GameplayScreen : GameScreen
    {
        public Player player;
        public Map map;
        Camera camera;

        HealthBar HPBar;
        SuspicionBar suspicionBar;
        SpriteFont font;

        Vector2 tilesOnScreen;
        const int NumOfEnemies = 2;//usually overwritten in case of having active Quests
        //USE EnemiesAtATime everywhere where NumOfEnemies is used - but equate the two in LoadContent to be overruled if necessary by a Quest
        int TotalEnemies;
        int EnemiesAtATime;

        Quest CurrentQuest; 

        bool exiting;

        public override void LoadContent()
        {


            base.LoadContent();
            XmlManager<Player> playerLoader = new XmlManager<Player>();
            player = playerLoader.Load("Load/Gameplay/Player.xml");

            //if loading from a GameState info....
            if (GameState.Instance.EnteringDungeon)
            {
                if (GameState.Instance.PlayerExists)
                {
                    player.Stats = GameState.Instance.PlayerStats;
                }//else the default value will be used as normal having not been overwritten

                map = DungeonCreator.CreateMap(3, GameState.Instance);
            }
            else
            {
                map = DungeonCreator.CreateMap(3);
            }


            AIMoveManager.Instance.StartNewFloor(map, NumOfEnemies);
            map.LoadContent(true);


            ContentManager content = new ContentManager(ScreenManager.Instance.Content.ServiceProvider, "Content");

            font = content.Load<SpriteFont>("Gameplay/Fonts/Font1");

            int tilesacross;
            if (ScreenManager.Instance.Dimensions.X % map.TileDimensions.X != 0)
            {
                tilesacross = ((int)ScreenManager.Instance.Dimensions.X / (int)map.TileDimensions.X) + 1;
            }
            else
            {
                tilesacross = ((int)ScreenManager.Instance.Dimensions.X / (int)map.TileDimensions.X);
            }

            int tilesdown;
            if (ScreenManager.Instance.Dimensions.Y % map.TileDimensions.Y != 0)
            {
                tilesdown = ((int)ScreenManager.Instance.Dimensions.Y / (int)map.TileDimensions.Y) + 1;
            }
            else
            {
                tilesdown = ((int)ScreenManager.Instance.Dimensions.Y / (int)map.TileDimensions.Y);
            }
            tilesOnScreen = new Vector2(tilesacross, tilesdown);




            player.LoadContent(map.TileDimensions, map.EntryLoc + new Vector2(0, 1), map);
            map.SpawnEnemies(NumOfEnemies, player.CurrentRoomIn);

            camera = new Camera(map.Size, tilesOnScreen, map.TileDimensions, player.TilePosition);
            StealthManager.Instance.StartNewFloor(map);

            XmlManager<HealthBar> HPBL = new XmlManager<HealthBar>();
            HPBar = HPBL.Load("Load/Gameplay/HealthBar.xml");
            HPBar.LoadContent();

            XmlManager<SuspicionBar> SBL = new XmlManager<SuspicionBar>();
            suspicionBar = SBL.Load("Load/Gameplay/SuspicionBar.xml");
            suspicionBar.LoadContent();

            TotalEnemies = map.D.Next(10, 30);//will be overruled by Quests later though if necessary
            EnemiesAtATime = NumOfEnemies;
            //	TotalEnemies = 25;
            if (GameState.Instance.IsCreated)
            {
                if (GameState.Instance.QuestForDungeon(GameState.Instance.CurrentDungeon, out Quest QuestIfAny))
                {
                    if (QuestIfAny.GetType() == typeof(DungeonQuest))
                    {

                        DungeonQuest CurrentQ = QuestIfAny as DungeonQuest;
                        TotalEnemies = CurrentQ.TotalEnemies;
                        if (CurrentQ.CurrentEnemies > 0)
                        {
                            EnemiesAtATime = CurrentQ.CurrentEnemies;
                        }
                        CurrentQuest = QuestIfAny;
                        CurrentQuest.Populate();
                    }
                }
            }


            GameState.Instance.EnteringDungeon = false;//resets in case of error elsewhere
            GameState.Instance.PlayerExists = true;

            exiting = false;
        }

        /*private Vector2 FirstValidPosition(Map.LevelCollisionMap Collisions, Vector2 MapSize)
        {
            for(int X = 0; X < MapSize.X; X++)
            {
                for(int Y = 0; Y< MapSize.Y; Y++)
                {
                    if(Collisions.CMap[X,Y] == 30)
                    {
                        return new Vector2(X, Y);
                    }
                }
            }
            throw new Exception("AllImpassable");
        }*/

        public override void UnloadContent()
        {
            base.UnloadContent();
            map.UnloadContent();
            player.UnloadContent();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            player.Update(gameTime, out bool PlayerDead);
            if (PlayerDead)
            {
                GameState.Instance.Clear();
                ScreenManager.Instance.ChangeScreens("GameOverScreen");
            }
            if (PlayerLeaving)
            {//if leaving map:
                if (!exiting)//if process yet to be called
                {
                    //call GameStateEnterWorld
                    GameStateToEnterWorld();
                    ScreenManager.Instance.ChangeScreens("WorldScreen");
                    exiting = true;

                }
                //else wait for exit

            }
            else
            {
                StealthManager.Instance.Update();
                map.Update(gameTime);
                if (map.LivingEnemies < NumOfEnemies && TotalEnemies > 0)
                 {
                    map.SpawnEnemies(1, player.CurrentRoomIn);
                    TotalEnemies--;
                }
                if (CurrentQuest != null)
                {
                    if (CurrentQuest.CompleteYet)
                    {

                    }
                }

                camera.Update(gameTime, player.TilePosition, player.Offset, map.FullMap);
                AIMoveManager.Instance.Update(gameTime);
                AttackManager.Instance.Update(gameTime);
                if ((map.LivingEnemies + 1) < map.BC.BC.Count)
                {
                    map.BC.Clean(map.CurrentEnemies.Values.ToList(), player);
                }
                CollisionCheck.Instance.CleanOverrules();
            }



        }
        //as a matter of course, it is probably wise to update your map AFTER your player, but
        //draw your map BEFORE your player.
        //some map elements are based on how the player moves, but you do not want the map drawn OVER the player.

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            map.Draw(spriteBatch, camera);
            player.Draw(spriteBatch, camera);
            if (map.LivingEnemies > 0)
            {
                //spriteBatch.DrawString(font, "Enemy HP: " + map.CurrentEnemies[0].HP + "/ " + map.CurrentEnemies[0].MaxHP, new Vector2(10, 80), Color.PaleVioletRed);
                //spriteBatch.DrawString(font, "Susp: " + map.CurrentEnemies[0].TotalofAlertPoints, new Vector2(10, 100), Color.PaleVioletRed);
                if (map.CurrentEnemies.Count > 1)
                {
                    //spriteBatch.DrawString(font, "Enemy 2 HP: " + map.CurrentEnemies[1].HP + "/ " + map.CurrentEnemies[1].MaxHP, new Vector2(10, 130), Color.MidnightBlue);
                    //spriteBatch.DrawString(font, "Susp 2 : " + map.CurrentEnemies[1].TotalofAlertPoints, new Vector2(10, 150), Color.MidnightBlue);
                }
            }

            HPBar.Draw(spriteBatch, player.Stats.HP, player.Stats.MaxHP);
            if (map.CurrentEnemies.Count > 0)
            {
                suspicionBar.Draw(spriteBatch, map.CurrentAvgSuspicion);
            }
            int EnemiesKilled = map.NextEnemyIndex - NumOfEnemies;
            spriteBatch.DrawString(font, "Total Enemies Killed: " + EnemiesKilled, new Vector2(10, 80), Color.AliceBlue);
            spriteBatch.DrawString(font, "Block State" + player.BlockStatus, new Vector2(10, 100), Color.Azure);
            if (CurrentQuest != null)
            {
                if (CurrentQuest.CompleteYet)
                {
                    spriteBatch.DrawString(font, "Quest Complete!", new Vector2(10, 450), Color.White);
                }
            }
        }

        public void GameStateToEnterWorld()
        {
            player.Stats.AgeStatChanges();
            player.Stats.LoopStats();
            GameState.Instance.UpdatePlayerStats(player.Stats);
            GameState.Instance.EnteringWorld = true;
            GameState.Instance.EnteringVillage = GameState.Instance.EnteringVillage = false;
            GameState.Instance.PlayerExists = true;
        }

        public bool PlayerLeaving
        {
            get
            {
                int CollValue = map.Collision.CMap[(int)player.TilePosition.X, (int)player.TilePosition.Y];
                if (CollValue == 20 || CollValue == 21)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}