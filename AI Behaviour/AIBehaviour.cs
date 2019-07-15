using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;


namespace Historia.AI_Behaviour
{
    public abstract class AIBehaviour
    {
        protected Enemy Me;
        protected Map map;

        protected bool HaveConfirmedTile
        {
            get
            {
                return Me.HaveConfirmedTile;
            }
        }

        protected Vector2 ConfirmedTile
        {
            get
            {
                return Me.ConfirmedTile;
            }
        }

        protected PlanStep finalStep
        {
            get
            {
                if (plannedActions.Count > 0)
                {
                    return plannedActions[plannedActions.Count - 1];
                }
                return new EmptyStep(Me.TilePosition);
            }
        }


        public int MaxConfirmDistance
        {
            get; protected set;
        }

        private int RethinkCount;//the amount of times a new scheme() will be called again before triggering a Rethink().
        const int TimesTillRethink = 5;
        const int MaxRethinkScope = 5;

        protected String behaviourName;//the name we give to describe this particular AIbehaviour,
        //to be filled in by the end level behaviour.

        protected bool HasAPlan
        {
            get
            {
                if (plannedActions.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        protected List<PlanStep> plannedActions; //includes locations, and action types

        public List<Vector2> myPlannedMoves
        {
            get
            {
                List<Vector2> build = new List<Vector2>();
                foreach (PlanStep P in plannedActions)
                {
                    if (P.GetType() != typeof(EmptyStep))
                    {
                        build.Add(P.Location);
                    }

                }
                return build;
            }
        }

        //public List<Vector2> PlannedTargets { get { return plannedTargets; } }
        //protected List<Vector2> plannedTargets;
        //public List<int> PlannedActions { get { return plannedActions; } }
        //protected List<int> plannedActions;
        //public List<bool> PlannedActionsMB { get { return plannedActionsMB; } }
        //protected List<bool> plannedActionsMB;
        //when the end of this list is reached, the AI should carry out the process the Plan's Aim was.


        protected List<PlanStep> backupActions;
        //public List<Vector2> BackupTargets { get { return backupTargets; } }
        //protected List<Vector2> backupTargets;
        //public List<int> BackupActions { get { return backupActions; } }
        //protected List<int> backupActions;
        //public List<bool> BackupActionsMB { get { return backupActionsMB; } }
        //protected List<bool> backupActionsMB;



        public bool WaitingforGoAhead { get; private set; }
        bool GoAhead;




        //protected int PlanAimID;
        /// <summary>
        /// Key for what the plans meant
        /// 0 = Walk to Location - no end action
        /// 1 = Check Suspicious Location - Confirm Enemy Location or Dissipate Suspicion of tile. Look at 2 facings as well.
        /// 2 = Attack Final Location
        /// 3 = Pull into Layby - have moved here to get out of way of another. switch WaitingOnGoAhead to true GoAhead to false, and transfer the backupPlan to this one, on this plan being completed.
        /// 
        /// 
        /// 
        /// 
        /// </summary>
        /// <param name="NextTarget"></param>
        /// <param name="NextAction"></param>
        /// <param name="HasAPlan"></param>
        /// <param name="PlanAimID"></param>

        //the Lists above work in cooperation with each other. If the AI deems itself to "HaveAPlan",
        //then it just returns the next values and deletes those from the plan, until ther isn't any left.

        //Certain conditions may be monitored to check that the Plan still makes sense.
        //One interrupt mechanic already exists - suspicion rising or falling to the extent that this Behaviour isn't active any more!

        public virtual void LoadContent(ref Enemy Me, Map map)
        {
            this.Me = Me;
            this.map = map;
            plannedActions = new List<PlanStep>();

        }

        //public virtual void DecideWhatToDo(out Vector2 NextTarget, out int NextAction, out bool MovementBased)
        //{
        //    if (!WaitingforGoAhead)
        //    {
        //        if (!HasAPlan)
        //        {
        //            Scheme();
        //        }

        //        ExecutePlan(out NextTarget, out NextAction, out MovementBased);
        //    }
        //    else
        //    {

        //        if (GoAhead || AIMoveManager.Instance.NumPassCalls == 0)
        //        {
        //            WaitingforGoAhead = false;
        //            GoAhead = false;
        //            if (!HasAPlan)
        //            {
        //                Scheme();
        //            }

        //            ExecutePlan(out NextTarget, out NextAction, out MovementBased);
        //        }
        //        else
        //        {
        //            NextAction = 0;
        //            NextTarget = Me.TilePosition;
        //            MovementBased = false;
        //        }
        //    }

        //}

        public virtual PlanStep DecideWhatToDo()
        {
            if (!WaitingforGoAhead)
            {
                if (!Me.HaveConfirmedTile && InterruptedByFoe())
                {
                    CancelPlan();
                    AddSearchStep();
                }

                else if (!HasAPlan)
                {
                    Scheme();
                }

                return ExecutePlan();
            }
            else
            {

                if (GoAhead || AIMoveManager.Instance.NumPassCalls == 0)
                {
                    WaitingforGoAhead = false;
                    GoAhead = false;
                    if (!HasAPlan)
                    {
                        Scheme();
                    }

                    return ExecutePlan();
                }
                else
                {
                    return CreateEmptyPlanStep(); // A waiting plan step that is instantly fulfilled, repeated while you wait in a layby
                    //NextAction = 0;
                    //NextTarget = Me.TilePosition;
                    //MovementBased = false;
                }
            }
        }

        public PlanStep ExecutePlan()
        {
            if (plannedActions.Count > 0)
            {
                //if the next planstep is valid...
                if (plannedActions[0].IsValid(Me.TilePosition))

                //RectMethod.TwoTilesAreAdjacentOrSame(plannedActions[0].Location, Me.TilePosition)
                //&& 
                //(plannedActions[0].Location == Me.TilePosition 
                //|| CollisionCheck.Instance.CheckTarget(map.Collision.CMap, map.BC.BC, plannedActions[0].Location, true) != 4))//if next location is on or adjacent to this tile and it's not occupied by a fellow enemy....
                {//enact plan
                    RethinkCount = TimesTillRethink;//reset rethink count on valid plan
                    PlanStep NextStep = plannedActions[0];
                    plannedActions.RemoveAt(0);
                    Type plantype = NextStep.GetType();
                    if (plantype == typeof(MoveStep))
                    {
                        return NextStep;
                    }
                    //this bit's unneeded now since the searchstep manages it.
                    //if ( plantype== typeof(SearchStep))
                    //{
                    //    //check this suspicious Location
                    //    AttemptPlayerConfirmation();
                    //}
                    else if (plantype == typeof(AttackStep))
                    {
                        //Attack this confirmed Player location, if that makes sense (all handled bt the PlanStep itself...
                        return NextStep;
                    }
                    else if (plantype == typeof(NowInLaybyStep))
                    {
                        CollisionCheck.Instance.AddOverrule(Me.TilePosition, 4);
                        WaitingforGoAhead = true;
                        //PlanAimID = 0;
                        GoAhead = false;

                        plannedActions.AddRange(backupActions);
                        backupActions = null;
                    }
                    else if (plantype == typeof(RestoreBackupPlanStep))
                    {
                        plannedActions.AddRange(backupActions);
                        backupActions = null;
                        if (plannedActions.Count > 0)
                        {//now replace this specialist fake step with the first of the backup steps it introduced.
                            NextStep = plannedActions[0];
                            plannedActions.RemoveAt(0);
                        }
                    }
                    //other specialist behaviours should go here through having their own PlanStep.
                    return NextStep;

                }
                else
                {//plan invalid, back to the drawing board
                    CancelPlan();
                    RethinkCount--;
                    if (RethinkCount > 0)
                    {
                        Scheme();
                        //ExecutePlan(out NextTarget, out NextAction, out MovementBased);
                        return CreateEmptyPlanStep();
                    }
                    else
                    {
                        Rethink();
                        //ExecutePlan(out NextTarget, out NextAction, out MovementBased);
                        RethinkCount++;
                        return CreateEmptyPlanStep();

                    }

                }
            }
            else
            {
                //miss-call? or error from strange RestoreBackupPlanCall that has no BackupPlan stored? or ...
                CancelPlan();
                Scheme();
                //ExecutePlan(out NextTarget, out NextAction, out MovementBased);
                RethinkCount--;
                return CreateEmptyPlanStep();

            }

        }

        /// <summary>
        /// Scheming occurs when a plan does not already exist, so creates a new one.
        /// </summary>
        public abstract void Scheme();

        /// <summary>
        /// Called to determine if an foe has been seen ahead of them. Low suspicion levels default-return false so they don't see enemies (since it's unexpected!) but high levels have increasingly lax criteria.
        /// </summary>
        /// <returns>if the AI behviour is enabled to identify enemies </returns>
        public bool InterruptedByFoe()
        {
            Vector2 playerLoc = StealthManager.Instance.PlayerLoc;
            int DistanceFromMe = RectMethod.DistanceBetweenLocations(playerLoc, Me.TilePosition);
            if (StealthMethod.CheckLOS(map, Me.TilePosition, playerLoc)
                && StealthMethod.CheckFOV(Me.TilePosition, playerLoc, Me.Direction)
                && DistanceFromMe <= MaxConfirmDistance)
            {
                return true;
            }
            return false;
        }





        /// <summary>
        /// Rethink occurs when your current plan has been attempted a few times with
        /// no positive result. it attempts to just 'get out of the way' 
        /// and move to a few tiles away in the hope it eases the apparent congestion.
        /// </summary>
        public void Rethink()
        {
            int Scope = 1;
            int TriesTillBiggerScope = 5;

            while (true)
            {
                int WanderToX = map.D.Next((int)Me.TilePosition.X - Scope, (int)Me.TilePosition.X + Scope);
                int WanderToY = map.D.Next((int)Me.TilePosition.Y - Scope, (int)Me.TilePosition.Y + Scope);
                if (CollisionCheck.Instance.CheckTargetBool(map.Collision.CMap, new Vector2(WanderToX, WanderToY)) && Me.TilePosition != new Vector2(WanderToX, WanderToY))
                {
                    if (PlanWalkToLocation(new Vector2(WanderToX, WanderToY), true))
                    {
                        break;
                    }
                    else
                    {
                        TriesTillBiggerScope--;//if keeps failing, cast the net wider
                    }
                }

                if (TriesTillBiggerScope <= 0 && Scope < MaxRethinkScope)
                {
                    Scope++;
                    TriesTillBiggerScope = 5 + Scope;
                }
            }
        }
        //A rethink takes place when a plan is invalid and has been declared invalid a few times.




        protected Rectangle CalculateCurrentRoomIn(Vector2 Location)
        {
            for (int R = 0; R < map.Rooms.Count; R++)
            {
                if (RectMethod.LocationIsInRectangle(Location, map.Rooms[R].Location))
                {

                    return map.Rooms[R].Location;
                }
            }
            //now checks passageways
            for (int P = 0; P < map.Passages.Count; P++)
            {
                if (RectMethod.LocationIsInRectangle(Location, map.Passages[P].Location))
                {


                    return map.Passages[P].Location;
                }
            }
            throw new Exception("Not in a Room or a Passageway..");
        }

        protected Rectangle CalculateCurrentRoomIn(Vector2 Location, out int RoomOrPassageNum, out bool IsInRoom)
        {
            for (int R = 0; R < map.Rooms.Count; R++)
            {
                if (RectMethod.LocationIsInRectangle(Location, map.Rooms[R].Location))
                {
                    RoomOrPassageNum = R;
                    IsInRoom = true;
                    return map.Rooms[R].Location;
                }
            }
            //now checks passageways
            for (int P = 0; P < map.Passages.Count; P++)
            {
                if (RectMethod.LocationIsInRectangle(Location, map.Passages[P].Location))
                {

                    IsInRoom = false;
                    RoomOrPassageNum = P;
                    return map.Passages[P].Location;
                }
            }
            throw new Exception("Not in a Room or a Passageway..");
        }

        //protected void AddToPlan(Vector2 Target, int Action, bool IsMovementBased)
        //{
        //    plannedTargets.Add(Target);
        //    plannedActions.Add(Action);
        //    plannedActionsMB.Add(IsMovementBased);
        //}


        //end of Pathfinding algorithm(s)


        public void ConfirmEnemyLocation(Vector2 OpponentsTile)
        /// when a location is confirmed to contain the opponent, redistribute suspicion to halve all others,
        /// and set the confirmed tile to MAX suspicion.
        /// Add A LOT of Suspicion. Confirmation shouldn't happen in s0 and s1 unless the enemy is literally in front of them.

        /// NOTE ON WHEN TO CONFIRM
        /// Oblivious: NEVER. taking damage will atou-take you to Suspicious though, as coded in AttackManager.
        /// Unsettled: 1 tile and in LOS/FOV
        /// Suspicious: 4 tiles and in LOS/FOV
        /// Alerted: 6 tiles and in LOS/FOV
        /// 
        {

            if (HaveConfirmedTile)
            {
                MoveConfirmedPosition(OpponentsTile);
            }
            else
            {
                if (StealthManager.Instance.PlayerLoc != OpponentsTile)
                {
                    throw new IncorrectConfirmException();
                }

                Dictionary<Vector2, int> NewSuspicions = new Dictionary<Vector2, int>();
                foreach (KeyValuePair<Vector2, int> I in Me.CurrentSuspicions)
                {
                    NewSuspicions.Add(I.Key, Math.Min(I.Value / 2, Enemy.AlertPointsPerLevel));

                }
                //and finally update this change in overall Enemy state
                NewSuspicions[OpponentsTile] = Enemy.MaximumAlertPoints;
                Me.CurrentSuspicions = NewSuspicions;
                Me.SetConfirmedTile(OpponentsTile);
            }
        }

        protected void MoveConfirmedPosition(Vector2 NewLocation)
        //When you see the opponent move, just move the confirmed tile and its suspicion onto the new tile.

        {
            if (!HaveConfirmedTile)
            {
                ConfirmEnemyLocation(NewLocation);
            }
            else
            {
                //before moving, halve all suspicions to make the other suspicious areas much less relevant
                List<Vector2> SuspLocs = ListMethod.DictToKeysList(Me.CurrentSuspicions);
                foreach (Vector2 Loc in SuspLocs)
                {
                    Me.CurrentSuspicions[Loc] = Me.CurrentSuspicions[Loc] / 2;
                }


                //now move or refresh the confimed location.
                if (Me.CurrentSuspicions.ContainsKey(NewLocation))
                {
                    Me.CurrentSuspicions[NewLocation] = Enemy.MaximumAlertPoints;
                }
                else
                {
                    //Me.CurrentSuspicions.Add(NewLocation, Me.CurrentSuspicions[ConfirmedTile]);
                    Me.CurrentSuspicions.Add(NewLocation, Enemy.MaximumAlertPoints);
                }
                if (ConfirmedTile != NewLocation)
                {
                    Me.CurrentSuspicions.Remove(ConfirmedTile);
                }
                Me.SetConfirmedTile(NewLocation);
            }
        }

        /// <summary>
        /// when the player disappears from their once confirmed location to somewhere unknown,
        /// spread the suspicion out amongst different sensible tiles, like blindspots
        /// close to the previous confirmed position.
        /// </summary>
        protected virtual void CancelConfirmedPosition()

        {
            if (HaveConfirmedTile)
            {

                //Rectangle RoomOfTarget = CalculateCurrentRoomIn(ConfirmedTile);

                //take a 7x7 box centred on the previous confirmed location of the enemy. 

                Rectangle Close_By = RectMethod.RadiusRectangle(ConfirmedTile, 3); // a 7x7 square
                List<Vector2> poss_hiding_spots = RectMethod.RectToLocationList(Close_By);
                poss_hiding_spots = ListMethod.FilterBOutOfA(poss_hiding_spots, map.AllImpassable);
                poss_hiding_spots = StealthMethod.MassPrecludeLineOfSight
                    (map, Me.TilePosition, Me.Direction, poss_hiding_spots);//remove those spots we can already see from contention

                //we should now have a hopefully short list of possbilities. Now, divvy up the suspicion from the confirmed tile across these options, with closer to original as better.
                int portions_to_split_across = 0;
                foreach (Vector2 Loc in poss_hiding_spots)
                {
                    int portions_here = 7 - RectMethod.DistanceBetweenLocations(ConfirmedTile, Loc);
                    portions_to_split_across += portions_here;
                }
                Dictionary<Vector2, int> SpreadSuspicions = new Dictionary<Vector2, int>();
                int points_per_portion = Me.CurrentSuspicions[ConfirmedTile] / portions_to_split_across;
                foreach (Vector2 Loc in poss_hiding_spots)
                {
                    int portions_here = 7 - RectMethod.DistanceBetweenLocations(ConfirmedTile, Loc);
                    SpreadSuspicions.Add(Loc, points_per_portion * portions_here);
                }


                Me.CurrentSuspicions.Remove(ConfirmedTile);
                Me.CurrentSuspicions = StealthMethod.CombineSuspicionLists(Me.CurrentSuspicions, SpreadSuspicions);


                Me.UnsetConfirmedTile();
            }
        }

        public void DissipateSuspicion(Vector2 Target)
        //similar to the above, but doesn't need to be the confirmed position. It also decays the values a bit to promote looking at other original ideas.
        {
            Rectangle Close_By = RectMethod.RadiusRectangle(Target, 3); // a 7x7 square
            List<Vector2> poss_hiding_spots = RectMethod.RectToLocationList(Close_By);

            poss_hiding_spots = StealthMethod.MassPrecludeLineOfSight
                (map, Me.TilePosition, Me.Direction, poss_hiding_spots);//remove those spots we can already see from contention
            poss_hiding_spots = ListMethod.FilterBOutOfA(poss_hiding_spots, map.AllImpassable);
            //we should now have a hopefully short list of possbilities. Now, divvy up the suspicion from the confirmed tile across these options, with closer to original as better.
            int portions_to_split_across = 0;
            foreach (Vector2 Loc in poss_hiding_spots)
            {
                int portions_here = 7 - RectMethod.DistanceBetweenLocations(Loc, Target);
                portions_to_split_across += portions_here;
            }
            if (portions_to_split_across > 0)
            {


                Dictionary<Vector2, int> SpreadSuspicions = new Dictionary<Vector2, int>();
                int points_per_portion = Me.CurrentSuspicions[Target] / (portions_to_split_across * 2);//here we see the decay - our values are halved
                foreach (Vector2 Loc in poss_hiding_spots)
                {
                    int portions_here = 7 - RectMethod.DistanceBetweenLocations(Loc, Target);
                    SpreadSuspicions.Add(Loc, points_per_portion * portions_here);
                }
                Me.CurrentSuspicions = StealthMethod.CombineSuspicionLists(Me.CurrentSuspicions, SpreadSuspicions);
            }
            Me.CurrentSuspicions.Remove(Target);//this always happens. In the case of there seeming to be no options, we just bin the lot!

        }


        /// <summary>
        /// This method is called to let each level of AI doe their own test to confirm the player, 
        /// with increasingly lax requirements as the enemy is more suspicious.
        /// </summary>
        public bool AttemptPlayerConfirmation(out Vector2 playerLoc)
        {
            Vector2 trueLoc = StealthManager.Instance.PlayerLoc;
            int DistanceFromMe = RectMethod.DistanceBetweenLocations(trueLoc, Me.TilePosition);
            if (StealthMethod.CheckLOS(map, Me.TilePosition, trueLoc)
                && StealthMethod.CheckFOV(Me.TilePosition, trueLoc, Me.Direction)
                && DistanceFromMe <= MaxConfirmDistance)
            {
                playerLoc = trueLoc;
                return true;
            }
            playerLoc = Vector2.Zero;
            return false;

        }

        //Routines to be called by Scheme sections to put plans together


        /// <summary>
        /// Default endstep: SearchStep
        /// </summary>
        /// <param name="Target"></param>
        /// <returns></returns>


        protected bool PlanWalkToLocation(Vector2 Target, bool SearchAtEnd)
        {
            List<Vector2> NearbyObstacles = map.BC.OccupiedLocations(RectMethod.RadiusRectangle(Me.TilePosition, 2), Me.MyIndex);
            if (NearbyObstacles.Contains(Me.TilePosition))
            {
                NearbyObstacles.Remove(Me.TilePosition);
            }
            if (NearbyObstacles.Contains(Target))
            {
                if (map.BC.BC[Target].CollisionType == 6)
                {//if our target is the player themselves, ignore the fact their tile seems to be impassable
                    NearbyObstacles.Remove(Target);
                }
            }

            if (Pathfinding.AStar(Target, Me.TilePosition, out List<Vector2> WayToGetThere, map, NearbyObstacles))
            {
                //this.PlanAimID = PlanAimID;
                WayToGetThere = ListMethod.RemoveConsecDuplicates(WayToGetThere);
                if (WayToGetThere[0] == Me.TilePosition)
                {
                    WayToGetThere.RemoveAt(0);
                }
                for (int I = 0; I < WayToGetThere.Count; I++)
                {
                    plannedActions.Add(new MoveStep(WayToGetThere[I]));
                    //AddToPlan(WayToGetThere[I], 1, true);

                }
                if (SearchAtEnd)
                {
                    AddSearchStep();
                }
                return true;
            }
            else
           {
                return false;
            }
        }

        /// <summary>
        /// For a rectangle, the default is NOT to insert a SearchStep at the end.
        /// </summary>
        /// <param name="AreaOfTarget"></param>
        /// <returns></returns>
        protected bool PlanWalkToLocation(Rectangle AreaOfTarget)//picks random valid tile in the rectangle.
                                                                 //bool is whether Plan was successfully added.
        {
            for (int Try = 0; Try < 5; Try++)
            {
                int X = map.D.Next(AreaOfTarget.X, AreaOfTarget.Right - 1);
                int Y = map.D.Next(AreaOfTarget.Y, AreaOfTarget.Bottom - 1);
                if (Pathfinding.AStar(new Vector2(X, Y), Me.TilePosition, out List<Vector2> WayToGetThere, map))
                {
                    WayToGetThere = ListMethod.RemoveConsecDuplicates(WayToGetThere);
                    if (WayToGetThere[0] == Me.TilePosition)
                    {
                        WayToGetThere.RemoveAt(0);
                    }
                    //this.PlanAimID = PlanAimID;
                    for (int I = 0; I < WayToGetThere.Count; I++)
                    {
                        plannedActions.Add(new MoveStep(WayToGetThere[I]));
                    }
                    return true;
                }
                //else
                //{

                //}
            }
            return false;
        }

        protected bool CreatePlanToAvoid_B(Vector2 CollLoc, List<Vector2> B_Moves, int AHitsBhere, int B_index)
        {
            int WhenPlanShouldBeInserted;
            int WhenPlanShouldEndBefore;
            List<Vector2> Diversion;

            //find out whether there is an oppurtunity to just go "the other side of a square"
            Vector2 MovementToColl = CollLoc - Me.MovingToTilePosition;

            int ChangePhase = -1;

            if (MovementToColl.X != 0 && MovementToColl.Y != 0)
            {
                List<Vector4> Phases = new List<Vector4>();//Each "Phase" details the X-stint OR Y-stint next, e.g. zigzaggy diagonals will have as many stints as moves.
                                                           //the first two values refer to its movement(X and Y). The second 2 coordinates refer to the X and Y of the first Tile in it.



                if (0 == AHitsBhere)//in case the collision is near-immediate
                {
                    ChangePhase = 0;
                }

                for (int I = 1; I < plannedActions.Count; I++)
                {
                    if ((plannedActions[I].Location - plannedActions[I - 1].Location).X == 1 || (plannedActions[I].Location - plannedActions[I - 1].Location).X == -1)
                    {
                        if (Phases.Count > 0)
                        {
                            if (Phases[Phases.Count - 1].X != 0)
                            {
                                Phases[Phases.Count - 1] = Phases[Phases.Count - 1] + new Vector4((plannedActions[I].Location - plannedActions[I - 1].Location), 0, 0);
                            }
                            else
                            {
                                Phases.Add(new Vector4((plannedActions[I].Location - plannedActions[I - 1].Location), plannedActions[I - 1].Location.X, plannedActions[I - 1].Location.Y));
                            }
                        }
                        else
                        {
                            Phases.Add(new Vector4((plannedActions[I].Location - plannedActions[I - 1].Location), plannedActions[I - 1].Location.X, plannedActions[I - 1].Location.Y));
                        }
                    }
                    else if ((plannedActions[I].Location - plannedActions[I - 1].Location).Y == 1 || (plannedActions[I].Location - plannedActions[I - 1].Location).Y == -1)
                    {
                        if (Phases.Count > 0)
                        {
                            if (Phases[Phases.Count - 1].Y != 0)
                            {
                                Phases[Phases.Count - 1] = Phases[Phases.Count - 1] + new Vector4((plannedActions[I].Location - plannedActions[I - 1].Location), 0, 0);
                            }
                            else
                            {
                                Phases.Add(new Vector4((plannedActions[I].Location - plannedActions[I - 1].Location), plannedActions[I - 1].Location.X, plannedActions[I - 1].Location.Y));
                            }
                        }

                        else
                        {
                            Phases.Add(new Vector4((plannedActions[I].Location - plannedActions[I - 1].Location), plannedActions[I - 1].Location.X, plannedActions[I - 1].Location.Y));
                        }
                    }
                    else//No Movement
                    {
                        //nothing
                    }


                    if (I == AHitsBhere)//find the phase where the collision takes place
                    {
                        ChangePhase = Phases.Count - 1;
                    }
                }
                bool ChangePhaseIsVertical = false;
                if (Phases[ChangePhase].Y != 0)
                {
                    ChangePhaseIsVertical = true;
                }

                if (ChangePhase < Phases.Count - 1)
                {//try to shift the ChangePhase's section one into the next section by extending out the previous stint in that direction
                    List<Vector4> TryPhases = new List<Vector4>();
                    int TryChangePhase = ChangePhase;
                    TryPhases.AddRange(Phases);
                    if (ChangePhaseIsVertical)
                    {
                        TryPhases[TryChangePhase + 1] -= new Vector4(Math.Sign(MovementToColl.X), 0, -Math.Sign(MovementToColl.X), 0);
                        if (TryChangePhase > 0)
                        {
                            TryPhases[TryChangePhase - 1] += new Vector4(Math.Sign(MovementToColl.X), 0, 0, 0);
                        }
                        else
                        {
                            TryPhases.Add(new Vector4(Math.Sign(MovementToColl.X), 0, finalStep.Location.X, finalStep.Location.Y - Math.Sign(MovementToColl.Y)));
                        }
                        TryPhases[TryChangePhase] += new Vector4(0, 0, Math.Sign(MovementToColl.X), 0);
                    }
                    else
                    {
                        TryPhases[TryChangePhase + 1] -= new Vector4(0, Math.Sign(MovementToColl.Y), 0, -Math.Sign(MovementToColl.Y));
                        if (ChangePhase > 0)
                        {
                            TryPhases[TryChangePhase - 1] += new Vector4(0, Math.Sign(MovementToColl.Y), 0, 0);
                        }
                        else
                        {
                            List<Vector4> Middleman = new List<Vector4>();
                            Middleman.AddRange(TryPhases);
                            TryPhases.Clear();
                            TryPhases.Add(new Vector4(0, Math.Sign(MovementToColl.Y), plannedActions[0].Location.X, plannedActions[0].Location.Y));
                            TryPhases.AddRange(Middleman);
                            TryChangePhase++;
                        }
                        TryPhases[TryChangePhase] += new Vector4(0, 0, 0, Math.Sign(MovementToColl.Y));
                    }
                    TryPhases = CleanPhases(TryPhases);
                    bool Valid = true;
                    for (int I = 0; I < TryPhases.Count; I++)
                    {
                        if (!Pathfinding.KnightPath_Check_Path(
                            new Vector2(TryPhases[I].Z, TryPhases[I].W)
                            , new Vector2(TryPhases[I].X, TryPhases[I].Y),
                            true, map, out Vector2 q, out bool Q))
                        {
                            Valid = false;
                            break;
                        }
                    }
                    if (Valid)
                    {
                        //if this patch works
                        List<Vector2> NewPlan = PhasesToTargetList(TryPhases);
                        //Diversion = CalculateAvoidSolution(TryPhases, ChangePhase, ChangePhaseIsVertical, out WhenPlanShouldBeInserted, out WhenPlanShouldEndBefore);
                        //Insert......
                        //InsertPlan(Diversion, 1, true, WhenPlanShouldBeInserted, WhenPlanShouldEndBefore);
                        CancelPlan();
                        for (int I = 0; I < NewPlan.Count; I++)
                        {
                            AddPlanStep(new MoveStep(NewPlan[I]));
                            //AddToPlan(NewPlan[I], 1, true);
                        }

                        return true;
                    }
                }

                if (ChangePhase > 0)
                {// (now) try to shift the ChangePhase's section one into the previous section by shortening it by 1, and extending the next stint in that direction
                    List<Vector4> TryPhases = new List<Vector4>();
                    TryPhases.AddRange(Phases);
                    if (ChangePhaseIsVertical)
                    {
                        TryPhases[ChangePhase - 1] -= new Vector4(Math.Sign(MovementToColl.X), 0, 0, 0);
                        if (TryPhases.Count - 1 > ChangePhase)
                        {
                            TryPhases[ChangePhase + 1] += new Vector4(Math.Sign(MovementToColl.X), 0, -Math.Sign(MovementToColl.X), 0);
                        }
                        else
                        {
                            TryPhases.Add(new Vector4(Math.Sign(MovementToColl.X), 0, finalStep.Location.X - Math.Sign(MovementToColl.X), finalStep.Location.Y));
                        }
                        TryPhases[ChangePhase] -= new Vector4(0, 0, Math.Sign(MovementToColl.X), 0);
                    }
                    else
                    {
                        TryPhases[ChangePhase - 1] -= new Vector4(0, Math.Sign(MovementToColl.Y), 0, 0);
                        if (TryPhases.Count - 1 > ChangePhase)
                        {
                            TryPhases[ChangePhase + 1] += new Vector4(0, Math.Sign(MovementToColl.Y), 0, -Math.Sign(MovementToColl.Y));
                        }
                        else
                        {
                            TryPhases.Add(new Vector4(0, Math.Sign(MovementToColl.Y), finalStep.Location.X, finalStep.Location.Y - Math.Sign(MovementToColl.Y)));
                        }
                        TryPhases[ChangePhase] -= new Vector4(0, 0, 0, Math.Sign(MovementToColl.Y));
                    }
                    TryPhases = CleanPhases(TryPhases);
                    bool Valid = true;
                    for (int I = 0; I < TryPhases.Count; I++)
                    {
                        if (!Pathfinding.KnightPath_Check_Path(
                            new Vector2(TryPhases[I].Z, TryPhases[I].W)
                            , new Vector2(TryPhases[I].X, TryPhases[I].Y),
                            true, map, out Vector2 q, out bool Q))
                        {
                            Valid = false;
                            break;
                        }
                    }
                    if (Valid)
                    {
                        //if this patch works
                        List<Vector2> NewPlan = PhasesToTargetList(TryPhases);
                        //Diversion = CalculateAvoidSolution(TryPhases, ChangePhase, ChangePhaseIsVertical, out WhenPlanShouldBeInserted, out WhenPlanShouldEndBefore);
                        //Insert......
                        //InsertPlan(Diversion, 1, true, WhenPlanShouldBeInserted, WhenPlanShouldEndBefore);
                        CancelPlan();
                        for (int I = 0; I < NewPlan.Count; I++)
                        {
                            AddPlanStep(new MoveStep(NewPlan[I]));
                            //AddToPlan(NewPlan[I], 1, true);
                        }

                        return true;
                    }
                }
            }

            //if not, check to see whether it is 2 wide or more at the site of the collision and a bit before and after it, so you can move unhindered
            bool CollIsVertical = false;
            if (AHitsBhere > 0)
            {
                if ((plannedActions[AHitsBhere].Location - plannedActions[AHitsBhere - 1].Location).Y != 0)
                {
                    CollIsVertical = true;
                }
            }
            else
            {
                if ((Me.MovingToTilePosition - plannedActions[0].Location).Y != 0)
                {
                    CollIsVertical = true;
                }
            }

            List<Vector2> Poss1 = new List<Vector2>();
            List<Vector2> Poss2 = new List<Vector2>();

            if (IsInStraightLine(plannedActions, Math.Max(AHitsBhere - 2, 0), Math.Min(AHitsBhere + 2, plannedActions.Count - 1)))
            {
                try
                {
                    if (CollIsVertical)
                    {

                        for (int I = -2; I <= 2; I++)
                        {
                            Poss1.Add((plannedActions[AHitsBhere + I]).Location + new Vector2(1, 0));
                            Poss2.Add((plannedActions[AHitsBhere + I]).Location + new Vector2(-1, 0));
                        }

                    }
                    else
                    {

                        for (int I = -2; I <= 2; I++)
                        {
                            Poss1.Add((plannedActions[AHitsBhere + I]).Location + new Vector2(0, 1));
                            Poss2.Add((plannedActions[AHitsBhere + I]).Location + new Vector2(0, -1));
                        }
                    }
                    if (CollisionCheck.Instance.CheckListBool(map.Collision.CMap, Poss1))//if the first option is valid...
                    {
                        Diversion = CalculateAvoidSolution(Poss1, AHitsBhere, 2, out WhenPlanShouldBeInserted, out WhenPlanShouldEndBefore);
                        InsertPlan(VectorsAsMovePlan(Diversion), WhenPlanShouldBeInserted, WhenPlanShouldEndBefore);

                        return true;



                    }
                    else if (CollisionCheck.Instance.CheckListBool(map.Collision.CMap, Poss2))//if the second option is valid
                    {
                        Diversion = CalculateAvoidSolution(Poss2, AHitsBhere, 2, out WhenPlanShouldBeInserted, out WhenPlanShouldEndBefore);
                        InsertPlan(VectorsAsMovePlan(Diversion), WhenPlanShouldBeInserted, WhenPlanShouldEndBefore);
                        return true;
                    }
                }
                catch (ArgumentOutOfRangeException)
                {

                }
            }


            //Doesn't seem to be a solution without "pulling in somewhere". Fail.
            WhenPlanShouldBeInserted = WhenPlanShouldEndBefore = 0;
            Diversion = new List<Vector2>();
            return false;
        }



        protected List<Vector4> CleanPhases(List<Vector4> Prev)
        {
            for (int I = 0; I < Prev.Count; I++)
            {
                if (Prev[I].X == 0 && Prev[I].Y == 0)
                {
                    Prev.RemoveAt(I);
                    I--;
                }
            }
            return Prev;
        }

        protected bool GetOutOfTheWayOf_B(Vector2 CollLoc, List<Vector2> B_Moves, Vector2 B_Loc, int AhitsBHere, int BhitsAhere, int B_index, int A_index)
        {
            Vector2 LatestDiversion = CalculateLatestDiversion(myPlannedMoves, B_Moves, B_Loc, AhitsBHere,
                BhitsAhere, out List<Vector2> B_LocationsNearCrash, out int LTime);
            B_LocationsNearCrash = ListMethod.RemoveDuplicates(B_LocationsNearCrash);
            List<Vector2> Possible_LayBys = new List<Vector2>();
            List<Vector2> Nextpossible = new List<Vector2>
            {
                LatestDiversion + new Vector2(1, 0),
                LatestDiversion + new Vector2(-1, 0),
                LatestDiversion + new Vector2(0, 1),
                LatestDiversion + new Vector2(0, -1)
            };

            List<Vector2> OldPossible = new List<Vector2>
            {
                LatestDiversion
            };

            int DistanceCount = 0;//if it gets really far away it just gives up.
            while (Possible_LayBys.Count < 1 && DistanceCount < 11)
            {
                Possible_LayBys = new List<Vector2>();
                Possible_LayBys.AddRange(Nextpossible);
                Possible_LayBys = CollisionCheck.Instance.FilterList(map.Collision.CMap, Possible_LayBys);
                Possible_LayBys = ListMethod.FilterBOutOfA(Possible_LayBys, B_Moves);

                if (Possible_LayBys.Count > 0)
                {
                    if (Possible_LayBys.Count == 1)
                    {

                        if (Pathfinding.AStar(Possible_LayBys[0], LatestDiversion, out List<Vector2> PlanBeforeStop, map, B_LocationsNearCrash))
                        {
                            return CalculateLaybyLogistics(PlanBeforeStop, Me.MovingToTilePosition, LatestDiversion, LTime, B_Moves, B_index, A_index);
                        }
                        else
                        {
                            Possible_LayBys = new List<Vector2>();//increments and tries again
                        }
                    }
                    else
                    {//multiple possibilities
                        List<Vector2> BestPlan = new List<Vector2>();
                        int LengthofBest = int.MaxValue;

                        for (int I = 0; I < Possible_LayBys.Count; I++)
                        {
                            if (Pathfinding.AStar(Possible_LayBys[I], LatestDiversion, out List<Vector2> CurrentPlan, map, B_LocationsNearCrash))
                            {
                                if (CurrentPlan.Count < LengthofBest)
                                {
                                    LengthofBest = CurrentPlan.Count;
                                    BestPlan = CurrentPlan;
                                }
                            }
                        }
                        if (LengthofBest < int.MaxValue)//if a valid route was found...
                        {
                            return CalculateLaybyLogistics(BestPlan, Me.MovingToTilePosition, LatestDiversion, LTime, B_Moves, B_index, A_index);
                        }
                        else
                        {//none of the supposedly valid layby routes were actually reachable
                            Possible_LayBys = new List<Vector2>();
                        }
                    }

                }

                if (Nextpossible.Count == 0)
                {
                    return false;
                }
                DistanceCount++;
                OldPossible.AddRange(Nextpossible);
                List<Vector2> LastPossible = new List<Vector2>();
                LastPossible.AddRange(Nextpossible);
                Nextpossible = new List<Vector2>();
                foreach (Vector2 Loc in LastPossible)
                {
                    if (!Nextpossible.Contains(Loc + new Vector2(1, 0)) && !OldPossible.Contains(Loc + new Vector2(1, 0)) && Loc.X >= 0 && Loc.Y >= 0 && Loc.X < map.Size.X && Loc.Y < map.Size.Y)
                    {
                        Nextpossible.Add(Loc + new Vector2(1, 0));
                    }
                    if (!Nextpossible.Contains(Loc + new Vector2(-1, 0)) && !OldPossible.Contains(Loc + new Vector2(-1, 0)) && Loc.X >= 0 && Loc.Y >= 0 && Loc.X < map.Size.X && Loc.Y < map.Size.Y)
                    {
                        Nextpossible.Add(Loc + new Vector2(-1, 0));
                    }
                    if (!Nextpossible.Contains(Loc + new Vector2(0, 1)) && !OldPossible.Contains(Loc + new Vector2(0, 1)) && Loc.X >= 0 && Loc.Y >= 0 && Loc.X < map.Size.X && Loc.Y < map.Size.Y)
                    {
                        Nextpossible.Add(Loc + new Vector2(0, 1));
                    }
                    if (!Nextpossible.Contains(Loc + new Vector2(0, -1)) && !OldPossible.Contains(Loc + new Vector2(0, -1)) && Loc.X >= 0 && Loc.Y >= 0 && Loc.X < map.Size.X && Loc.Y < map.Size.Y)
                    {
                        Nextpossible.Add(Loc + new Vector2(0, -1));
                    }
                }
            }
            return false;

        }

        private static Vector2 CalculateLatestDiversion(List<Vector2> MyMoves, List<Vector2> TheirMoves, Vector2 TheirLoc, int I_Hit_Them, int They_Hit_Me, out List<Vector2> LocationsNearCrash, out int LatestDiversionTime)
        {
            LocationsNearCrash = new List<Vector2>();
            //Add the locations before the crash

            //if very close to occuring, add their actual location as well as their planned targets
            if (They_Hit_Me < 2)
            {
                LocationsNearCrash.Add(TheirLoc);
            }
            for (int I = Math.Min(2, They_Hit_Me); I > 0; I--)//TheyHitMe also refers to the number of moves the enemy will make before reaching us, as its zero index
            {
                LocationsNearCrash.Add(TheirMoves[They_Hit_Me - I]);
            }

            if (I_Hit_Them - 1 < 0)
            {
                LatestDiversionTime = 0;
                return MyMoves[0];
            }
            else if (I_Hit_Them - 1 == 0)//if its in 2 tiles

            {
                LocationsNearCrash.Add(MyMoves[I_Hit_Them]);//add the collision as it won't needed to be the latest diversion
                LatestDiversionTime = 0;
                return MyMoves[0];
            }
            else
            {//it's further away
                LocationsNearCrash.Add(MyMoves[I_Hit_Them]);//add the collision as it won't needed to be the latest diversion
                LocationsNearCrash.Add(MyMoves[I_Hit_Them - 1]);//add the one before for safety and fluidity as you van spare the tile
                LatestDiversionTime = Math.Max(I_Hit_Them - 2, 0);
                return MyMoves[LatestDiversionTime];
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="PlanBeforeStop">The list of locations making up the journey from the LatestDiversion to the Layby location.</param>
        /// <param name="CurrentLoc"></param>
        /// <param name="SplitOffPoint"></param>
        /// <param name="SplitOff_time"></param>
        /// <param name="B_Moves"></param>
        /// <param name="B_index"></param>
        /// <param name="A_index"></param>
        /// <returns></returns>

        private bool CalculateLaybyLogistics(List<Vector2> PlanBeforeStop, Vector2 CurrentLoc, Vector2 SplitOffPoint, int SplitOff_time, List<Vector2> B_Moves, int B_index, int A_index)
        {
            //add the location of the split-off to the diversion so it's a complete route when flipped

            List<Vector2> PlanBeforeStopPlusStartLocation = new List<Vector2>() { SplitOffPoint };
            PlanBeforeStopPlusStartLocation.AddRange(PlanBeforeStop);

            List<Vector2> ReturnToRoutePlan = new List<Vector2>();
            ReturnToRoutePlan.AddRange(PlanBeforeStopPlusStartLocation);
            ReturnToRoutePlan.Reverse();

            List<PlanStep> Plan_Until_SplitOff = new List<PlanStep>();
            Plan_Until_SplitOff.AddRange(plannedActions);
            Plan_Until_SplitOff.RemoveRange(SplitOff_time, (plannedActions.Count - (SplitOff_time)));

            List<PlanStep> Plan_After_Resume = new List<PlanStep>();
            Plan_After_Resume.AddRange(plannedActions);
            Plan_After_Resume.RemoveRange(0, SplitOff_time);

            //find where the latest point of the original planned route that can be rejoined as part of the journey back from the layby
            if (ListMethod.ReturnIfListsShareTerm(ReturnToRoutePlan, Plan_After_Resume, out int FirstInstanceA, out int LastInstancePlanned))
            {//if this is possible...
                ReturnToRoutePlan.RemoveRange(FirstInstanceA, ReturnToRoutePlan.Count - FirstInstanceA);//cut off the rejoin plan after it connects to the original route.
                if (LastInstancePlanned > 0)//if it's cutting out part of the route doing this...
                {
                    Plan_After_Resume.RemoveRange(0, LastInstancePlanned);
                }

                List<PlanStep> Overall_Plan_After = new List<PlanStep>();//combine the journey to the route with the remaining route.
                foreach (Vector2 V in ReturnToRoutePlan)
                {
                    Overall_Plan_After.Add(new MoveStep(V));
                }
                Overall_Plan_After.AddRange(Plan_After_Resume);
                backupActions = Overall_Plan_After;//set this plan as 'backup', ie kicks in when Layby no longer needed

                //now finally add the bit in at the front
                plannedActions = Plan_Until_SplitOff;
                foreach (Vector2 V in PlanBeforeStop)
                {
                    plannedActions.Add(new MoveStep(V));
                }


                //Add the Passcall for where the B character is last in our way after resuming from the layby
                if (ListMethod.ReturnIfListsShareTerm(Overall_Plan_After, B_Moves, out int FA, out int LB))
                {
                    AddEmptyStep(new NowInLaybyStep(finalStep.Location));
                    AIMoveManager.Instance.AddNewPassCall(B_index, A_index, Overall_Plan_After[FA].Location, finalStep.Location);
                    return true;
                }
                else
                {//how is there now no collision??? 
                    AddEmptyStep(new RestoreBackupPlanStep(finalStep.Location));
                    return true;
                }
            }
            else
            {//the original route and the route to the layby don't overlap
                return false;
            }
        }

        private static bool IsInStraightLine(List<Vector2> Path, int Start, int Stop)//includes both the start and stop values given
        {
            Vector2 PrimaryMove = Path[Start + 1] - Path[Start];

            for (int I = 1; I <= Stop - Start; I++)
            {
                Vector2 ThisMove = Path[Start + I] - Path[Start + I - 1];

                if (ThisMove != PrimaryMove && ThisMove != Vector2.Zero)
                {
                    return false;
                }
            }
            return true;
        }

        private static bool IsInStraightLine(List<PlanStep> Path, int Start, int Stop)
        {
            Vector2 PrimaryMove = Path[Start + 1].Location - Path[Start].Location;

            for (int I = 1; I <= Stop - Start; I++)
            {
                Vector2 ThisMove = Path[Start + I].Location - Path[Start + I - 1].Location;

                if (ThisMove != PrimaryMove && ThisMove != Vector2.Zero)
                {
                    return false;
                }
            }
            return true;
        }

        protected List<Vector2> PhasesToTargetList(List<Vector4> Phases)
        {
            List<Vector2> Targets = new List<Vector2>() { };
            foreach (Vector4 Phase in Phases)
            {
                if (Phase.X != 0)
                {
                    for (int X = 0; MathMethod.Modulus(X) <= MathMethod.Modulus(Phase.X); X += Math.Sign(Phase.X))
                    {
                        Targets.Add(new Vector2(Phase.Z + X, Phase.W));
                    }
                }
                else if (Phase.Y != 0)
                {
                    for (int Y = 0; MathMethod.Modulus(Y) <= MathMethod.Modulus(Phase.Y); Y += Math.Sign(Phase.Y))
                    {
                        Targets.Add(new Vector2(Phase.Z, Phase.W + Y));
                    }
                }
            }
            Targets = ListMethod.RemoveConsecDuplicates(Targets);
            return Targets;
        }

        private List<Vector2> CalculateAvoidSolution(List<Vector2> Insert, int AHitsBHere, int BeforeAfterCollOffset, out int WhenPlanShouldBeInserted, out int WhenPlanShouldEndBefore)
        {
            WhenPlanShouldBeInserted = AHitsBHere - BeforeAfterCollOffset;
            WhenPlanShouldEndBefore = AHitsBHere + BeforeAfterCollOffset;

            return Insert;
        }

        public bool WarnOfFriendlyCollision(int SuperiorInd, int YourInd, Vector2 Location, List<Vector2> OthersMovement, Vector2 OthersLocation, int YourMovesTillOccurence, int TheirMovesTillOccurence)
        {

            if (!CreatePlanToAvoid_B(Location, OthersMovement, YourMovesTillOccurence, SuperiorInd))//first, try insering a plan to divert you
            {
                //if even this is impossible, find somewhere to "pull into" to let the other past, and inform the AIMoveManager that you are waiting for a signal that the other has passed you to continue
                if (!GetOutOfTheWayOf_B(Location, OthersMovement, OthersLocation, YourMovesTillOccurence, TheirMovesTillOccurence, SuperiorInd, YourInd))
                {
                    //nonw of the solutions worked, avoidance failed
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return true;
            }


            //you may even have to double back to find somewhere.
        }

        public void InsertPlan(List<PlanStep> Insert, int PlaceStartAfter, int PlaceEndBefore)
        {
            plannedActions.RemoveRange(PlaceStartAfter + 1, PlaceEndBefore - (PlaceStartAfter + 1));
            plannedActions.InsertRange(PlaceStartAfter + 1, Insert);

        }

        /// <summary>
        /// Translates a list of vector tile positions into the set of plansteps of that movement.
        /// </summary>
        /// <param name="Locations_of_Movement"></param>
        /// <returns></returns>
        private List<PlanStep> VectorsAsMovePlan(List<Vector2> Locations_of_Movement)
        {
            List<PlanStep> output = new List<PlanStep>();
            foreach (Vector2 V in Locations_of_Movement)
            {
                output.Add(new MoveStep(V));
            }
            return output;
        }

        private void RemovePartOfPlan(int FromAndIncluding, int ToAndIncluding)
        {
            plannedActions.RemoveRange(FromAndIncluding, ToAndIncluding);
        }

        private void RemoveEndOfPlan(int DeletePositions_FromAndIncluding)
        {
            plannedActions.RemoveRange(DeletePositions_FromAndIncluding, plannedActions.Count - 1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="FromStart__UpToAndIncluding"> REMEMBER: 0-INDEX</param>
        private void RemoveStartOfPlan(int FromStart__UpToAndIncluding)
        {
            plannedActions.RemoveRange(0, FromStart__UpToAndIncluding);
        }

        /// Here begins the start of the methods that append plansteps to the current plan.
        /// The methods below are structured like this to make sure I don't have the ability to call a generic
        /// 'AddPlanStep' for a WaitStep, when they need to be prepped slightly first in their own method,
        /// to match the last step's location. Hence a strange lack of one method with lots instead!


        protected virtual void AddSearchStep()
        {
            SearchStep searchStep = new SearchStep(finalStep.Location, this, map);
            plannedActions.Add(searchStep);
        }
        //protected void AddSearchStep(int Time)
        //{
        //    SearchStep searchStep = new SearchStep(finalStep.Location, Time, this);
        //}
        protected void AddWaitStep(WaitStep waitStep)
        {
            waitStep.Location = finalStep.Location;
            plannedActions.Add(waitStep);
        }
        protected void AddEmptyStep()
        {
            EmptyStep emptyStep = new EmptyStep(finalStep.Location);
            plannedActions.Add(emptyStep);
        }
        protected void AddEmptyStep(EmptyStep emptyStep)
        {
            emptyStep.Location = finalStep.Location;
            plannedActions.Add(emptyStep);
        }
        protected void AddPlanStep(AttackStep planStep)
        {
            plannedActions.Add(planStep);
        }
        protected void AddPlanStep(MoveStep planStep)
        {
            plannedActions.Add(planStep);
        }
        protected void AddPlanStep(FacingStep planStep)
        {
            if (RectMethod.DistanceBetweenLocations(finalStep.Location, planStep.Target) == 1
                && finalStep.Location == planStep.Location)
            {
                plannedActions.Add(planStep);
            }
            else
            {
                //some form of error. Handle by implementing the Facing change, through changing the Target
                planStep.Target = finalStep.Location + Being.GetDirection(planStep.Facing);
                planStep.Location = finalStep.Location;
            }
        }


        public bool GiveGoAhead()//indicates whether it was successful
        {
            if (WaitingforGoAhead)
            {
                GoAhead = true;
                WaitingforGoAhead = false;
                return true;
            }
            else
            {
                return false;
            }


        }

        public void CancelPlan()
        {
            plannedActions.Clear();
        }

        public PlanStep CreateEmptyPlanStep()
        {
            return new EmptyStep(Me.TilePosition);
        }

        public string getName()
        {
            return behaviourName;
        }
    }

    /// <summary>
    /// to throw when calling ConfirmLocation() with an incorrect location passed.
    /// </summary>
    public class IncorrectConfirmException : Exception
    {

    }
}
