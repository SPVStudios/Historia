using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Xml.Serialization;
using System.Xml;

namespace Historia.AI_Behaviour.s0
{
    public abstract class Oblivious_Behaviour : AIBehaviour//Behaviours that are enacted with a Detection Level of 0.
    {
        public Oblivious_Behaviour()
        {
            MaxConfirmDistance = 0;
        }

        List<Vector2> Default_Route_Waypoints;
        List<int> Default_Route_Waypoint_Rooms;
        //There is no 'current' waypoint - we approach the 'next' waypoint at all times,
        //and track the one we last reached, the 'previous' waypoint.

        int NextWaypointNum;
        
        int PreviousWaypointNum
        { get { return (NextWaypointNum + Default_Route_Waypoints.Count - 1) % Default_Route_Waypoints.Count; } }

        Vector2 NextWaypoint
        {get { return Default_Route_Waypoints[NextWaypointNum]; } }

        Vector2 PreviousWaypoint
        { get { return Default_Route_Waypoints[PreviousWaypointNum]; }}

        

        protected List<Vector2> Create_New_Waypoint_Set(List<int> RoomIDsInOrder)//for circular routes.
        //NOTE: These rooms should all be connected. Call Dijkstra's Algorithm to  organise the list first.
        {
            List<Vector2> Waypoints = new List<Vector2>();
            Vector2 Next;
            for (int I = 0; I < RoomIDsInOrder.Count; I++)
            {

                Rectangle RoomArea = map.Rooms[RoomIDsInOrder[I]].Location;
                if(map.D.Next(0,100) > 50)
                {
                    Rectangle CentralArea = new Rectangle(RoomArea.X + (RoomArea.Width / 4), RoomArea.Y + (RoomArea.Height / 4), RoomArea.Width / 2, RoomArea.Height / 2);
                    Next = RectMethod.ReturnRandomEmptyTile(CentralArea, map.Collision.CMap, map.D);
                    if(Next != new Vector2(-1, -1))
                    {
                        Waypoints.Add(Next);
                    }
                    else
                    {
                        Next = RectMethod.ReturnRandomEmptyTile(RoomArea, map.Collision.CMap, map.D);
                        if (Next != new Vector2(-1, -1))
                        {
                            Waypoints.Add(Next);
                        }
                        else
                        {
                            //skip it
                        }
                    }
                    
                }
                else
                {
                    Next = RectMethod.ReturnRandomEmptyTile(RoomArea, map.Collision.CMap, map.D);
                    if (Next != new Vector2(-1, -1))
                    {
                        Waypoints.Add(Next);
                    }
                    else
                    {
                        //skip
                    }
                }
            }
            
            
            return Waypoints;
        }

        protected void Create_Standard_Patrol(int End_A, int End_B)
        {
            List<int> Route = NetworkMethod.Dijkstrafy_Routes(map, new List<int>() { End_A, End_B });
            if(Route.Count > 2)
            {
                List<int> Way_Back = new List<int>();
                for(int I = Route.Count-2; I >= 1; I--)//cuts the first and last point - these would just be repeats.
                {
                    Way_Back.Add(Route[I]);
                }
                Route.AddRange(Way_Back);
            }
            
            AIMoveManager.Instance.RemoveFromRR(Route);
            Default_Route_Waypoints = Create_New_Waypoint_Set(Route);
            
            if(Default_Route_Waypoints.Count == Route.Count)
            {
                Default_Route_Waypoint_Rooms = Route;
            }
            else
            {
                Default_Route_Waypoint_Rooms = new List<int>();
                for(int I = 0; I < Default_Route_Waypoints.Count; I++)
                {
                    Default_Route_Waypoint_Rooms.Add(RectMethod.FindWhatRoomLocationIsIn(Default_Route_Waypoints[I], map, out bool VOID));
                }
            }
            NextWaypointNum = AssignStartingWaypoint();
        }

        protected void Create_Standard_Patrol(bool IsDuringLoad)
        {
            if (!IsDuringLoad || ListMethod.NumberUnique(AIMoveManager.Instance.RoomsForRoutes) < 2)
            {
                AIMoveManager.Instance.TopUpRoomsForRoutes();
            }
            int A = AIMoveManager.Instance.RoomsForRoutes[map.D.Next(0, AIMoveManager.Instance.RoomsForRoutes.Count)];
            int B;
            while (true)
            {
                 B = AIMoveManager.Instance.RoomsForRoutes[map.D.Next(0, AIMoveManager.Instance.RoomsForRoutes.Count)];
                if (B != A)
                {
                    break;
                }
            }
            Create_Standard_Patrol(A, B);
        }

        protected void Create_Long_Circuit_Patrol(List<int> Visits)//doesn't have to be sorted
        {
            List<int> Route = NetworkMethod.Dijkstrafy_Routes(map, Visits);
             AIMoveManager.Instance.RemoveFromRR(Route);
            Default_Route_Waypoints = Create_New_Waypoint_Set(Route);

            if (Default_Route_Waypoints.Count == Route.Count)
            {
                Default_Route_Waypoint_Rooms = Route;
            }
            else
            {
                Default_Route_Waypoint_Rooms = new List<int>();
                for (int I = 0; I < Default_Route_Waypoints.Count; I++)
                {
                    Default_Route_Waypoint_Rooms.Add(RectMethod.FindWhatRoomLocationIsIn(Default_Route_Waypoints[I], map, out bool VOID));
                }
            }
            NextWaypointNum = AssignStartingWaypoint();
        }

        protected void Create_Long_Circuit_Patrol(bool IsDuringLoad)//generates suggested visits to run the above
        {
            int NumofVisitsLB = Math.Min(3, map.Rooms.Count / 3);
            int NumofVisits = map.D.Next(NumofVisitsLB, map.Rooms.Count / 2);
            if (!IsDuringLoad || ListMethod.NumberUnique(AIMoveManager.Instance.RoomsForRoutes) < NumofVisits)
            {
                AIMoveManager.Instance.TopUpRoomsForRoutes();
            }
            
            if (NumofVisitsLB < 2)
            {// tiny map means that a AtoB is the only option, really
                Create_Standard_Patrol(false);//run the A to B instead. Don't say its running after startup as you've already added enough RoomsForRoutes above
            }
            else
            {
                

                List<int> Visits = new List<int>
                {
                    AIMoveManager.Instance.RoomsForRoutes[map.D.Next(0, AIMoveManager.Instance.RoomsForRoutes.Count-1)]
                };
                for (int I = 1; I < NumofVisits; I++)
                {
                    while (true)
                    {
                        int Next = AIMoveManager.Instance.RoomsForRoutes[map.D.Next(0, AIMoveManager.Instance.RoomsForRoutes.Count - 1)];
                        if (Visits[Visits.Count - 1] != Next)
                        {
                            Visits.Add(Next);
                            break;
                        }
                    }
                }
                Create_Long_Circuit_Patrol(Visits);
            }
        }

        //assigns a starting waypoint of either the first waypoint in the list found to be in the same room as this 
        protected int AssignStartingWaypoint()
        {
            int FirstWaypoint = 0;
            for (int I = 0; I < Default_Route_Waypoints.Count; I++)
            {
                if (Me.CurrentRoomIn == Default_Route_Waypoint_Rooms[I])
                {
                    FirstWaypoint = I;
                    break;
                }
            }
            
            return FirstWaypoint;
        }


        /// <summary>
        /// if we can navigate to the next through A*, do so! otherwise find an alternative waypoint to navigate to.
        /// </summary>
        protected void Plan_Journey_ToNextWaypoint()
        {
            if(Me.TilePosition == NextWaypoint)
            {
                IncrementWaypointNumber();
            }
            
            if(Pathfinding.TraverseMapToLocation(NextWaypoint,Me.TilePosition,map,out List<Vector2> PathTaken))
            {
                MakeWalkPlanFromDestinations(PathTaken);
                IncrementWaypointNumber();
            }
            else
            {
                //a big ol' error here! We can't actually get to the next waypoint.
                //first check to see if you're 'locked in' and can't move at all to save effort

                // in which case,
                //skip this waypoint and try to navigate to the other points. CYcle through all of them ,then if there's still an issue, rethink.
            }

        }

        //protected void Plan_Journey_ToNextWaypoint()
        //{
        //    if (Me.IsInRoom)
        //    {
        //        if (Me.CurrentRoomIn == CurrentWaypointRoom)
        //        {
        //            if (PlanWalkToLocation(Default_Route_Waypoints[NextWaypoint], 0))
        //            {//all is well; plan route and continue
        //                CurrentWaypoint = NextWaypoint;
        //                return;
        //            }
        //            else
        //            {//something is in the way
        //                Create_Long_Circuit_Patrol(false);
        //                Plan_Journey_ToNextWaypoint();

        //            }
        //        }
        //        else//not in the right room, due to being distracted or on spawn
        //        {
        //            for (int I = 0; I < Default_Route_Waypoints.Count; I++)
        //            {
        //                if (Default_Route_Waypoint_Rooms[I] == Me.CurrentRoomIn)
        //                {
        //                    CurrentWaypoint = I;
        //                    Plan_Journey_ToNextWaypoint();
        //                    return;
        //                }
        //            }
        //            //this room isn't in the patrol
        //            List<int> ReturnRoute = NetworkMethod.Dijkstras(map, Me.CurrentRoomIn, Default_Route_Waypoint_Rooms, out int RejoinWaypoint);
        //            // plot a route from where we are to the closest waypoint in terms of minimising the number of passages used.
        //            // the target waypoint chosen is the useful info there.
        //            // EDIT:: now to use A* and simplify, it'll just navigate in one step to the closest waypoint.
        //            if (Pathfinding.TraverseMapToLocation(Default_Route_Waypoints[RejoinWaypoint], Me.TilePosition, map, out List<Vector2> Path))
        //            {
        //                MakeWalkPlanFromDestinations(Path);
        //                //Use the pathfinding mechanism for traversing maps to go to the room on the patrol deemed closest,
        //                //and start the patrol up again from there
        //                CurrentWaypoint = RejoinWaypoint;
        //            }
        //            else
        //            {
        //                Rethink();
        //                //throw new Exception("Location Unreachable");
        //            }
        //        }
        //    }
        //    else//current location is in a passageway
        //    {
        //        int RoomA = map.Passages[Me.CurrentPassagewayIn].End1.EndPointID;
        //        int RoomB = map.Passages[Me.CurrentPassagewayIn].End2.EndPointID;
        //        bool Successful = false;

        //        for (int I = 0; I < Default_Route_Waypoints.Count - 1; I++)
        //        {
        //            if (Default_Route_Waypoint_Rooms[I] == RoomA)
        //            {
        //                CurrentWaypoint = I;
        //                CurrentWaypoint = PreviousWaypoint;//above step must be enacted first to set correct value for what will be the next waypoint
        //                Successful = true;
        //                break;
        //            }
        //            else if (Default_Route_Waypoint_Rooms[I] == RoomB)
        //            {
        //                CurrentWaypoint = I;
        //                CurrentWaypoint = PreviousWaypoint;//above step must be enacted first to set correct value for what will be the next waypoint
        //                Successful = true;
        //                break;
        //            }
        //        }
        //        if (Successful)
        //        {   //now plan a traversal to the next waypoint

        //            List<Vector2> Path = new List<Vector2>();

        //            if (Pathfinding.TraverseMapToLocation(Default_Route_Waypoints[NextWaypoint], Default_Route_Waypoint_Rooms[NextWaypoint],
        //                true, Me.TilePosition, Me.CurrentPassagewayIn, Me.IsInRoom, out Path, map))
        //            {
        //                MakeWalkPlanFromDestinations(Path);
        //                //Use the pathfinding mechanism for traversing maps to go to the room on the patrol deemed closest,
        //                //and start the patrol up again from there
        //                CurrentWaypoint = NextWaypoint;

        //            }
        //            else
        //            {//location unreachable, create new set of waypoints and try again
        //                Create_Long_Circuit_Patrol(false);
        //                Plan_Journey_ToNextWaypoint();
        //            }

        //        }

        //        else
        //        {//neither of the  rooms connected to this passageway are anywhere in the waypoint route...
        //            //run Dijkstras to find the nearest waypoint route, from the perspective of the room you are nearest to.

        //            int DistanceFromEnd1, DistanceFromEnd2, End1RoomIndex, End2RoomIndex;
        //            //find which end of the 
        //            End1RoomIndex = map.Passages[Me.CurrentPassagewayIn].End1.EndPointID;
        //            DistanceFromEnd1 = RectMethod.DistanceBetweenLocations(Me.TilePosition, map.Passages[Me.CurrentPassagewayIn].End1.Location);
        //            NetworkMethod.Dijkstras(map, End1RoomIndex, Default_Route_Waypoint_Rooms, out int BestRoomFromEnd1, out int DistanceEnd1ToBest);
        //            BestRoomFromEnd1 = Default_Route_Waypoint_Rooms[BestRoomFromEnd1];
        //            int TotalForEnd1 = DistanceFromEnd1 + DistanceEnd1ToBest;

        //            End2RoomIndex = map.Passages[Me.CurrentPassagewayIn].End2.EndPointID;
        //            DistanceFromEnd2 = RectMethod.DistanceBetweenLocations(Me.TilePosition, map.Passages[Me.CurrentPassagewayIn].End2.Location);
        //            NetworkMethod.Dijkstras(map, End2RoomIndex, Default_Route_Waypoint_Rooms, out int BestRoomFromEnd2, out int DistanceEnd2ToBest);
        //            BestRoomFromEnd2 = Default_Route_Waypoint_Rooms[BestRoomFromEnd2];
        //            int TotalForEnd2 = DistanceFromEnd2 + DistanceEnd2ToBest;

        //            if (TotalForEnd1 <= TotalForEnd2)
        //            {//get to End1, then the best room as decided for the list of waypoints.
        //                CurrentWaypoint = Default_Route_Waypoint_Rooms.IndexOf(BestRoomFromEnd1);
        //                CurrentWaypoint = PreviousWaypoint;//above step must be enacted first to set correct value for what will be the next waypoint
        //                List<Vector2> Path = new List<Vector2>();

        //                if (Pathfinding.TraverseMapToLocation(Default_Route_Waypoints[NextWaypoint], Default_Route_Waypoint_Rooms[NextWaypoint],
        //                    true, Me.TilePosition, Me.CurrentPassagewayIn, false, out Path, map))
        //                {
        //                    MakeWalkPlanFromDestinations(Path);
        //                    //Use the pathfinding mechanism for traversing maps to go to the room on the patrol deemed closest,
        //                    //and start the patrol up again from there
        //                    CurrentWaypoint = NextWaypoint;

        //                }
        //                else
        //                {

        //                }
        //            }
        //            else if (TotalForEnd2 < TotalForEnd1)
        //            {//end2's route is faster
        //                CurrentWaypoint = Default_Route_Waypoint_Rooms.IndexOf(BestRoomFromEnd2);
        //                CurrentWaypoint = PreviousWaypoint;//above step must be enacted first to set correct value for what will be the next waypoint
        //                List<Vector2> Path = new List<Vector2>();

        //                if (Pathfinding.TraverseMapToLocation(Default_Route_Waypoints[NextWaypoint], Default_Route_Waypoint_Rooms[NextWaypoint],
        //                    true, Me.TilePosition, Me.CurrentPassagewayIn, false, out Path, map))
        //                {
        //                    MakeWalkPlanFromDestinations(Path);
        //                    //Use the pathfinding mechanism for traversing maps to go to the room on the patrol deemed closest,
        //                    //and start the patrol up again from there
        //                    CurrentWaypoint = NextWaypoint;

        //                }
        //                else
        //                {

        //                }
        //            }
        //            //This is impossible........
        //            throw new NotImplementedException();
        //        }
        //    }
        //}

        protected void MakeWalkPlanFromDestinations(List<Vector2> Locations)
        {
            Vector2 prev = finalStep.Location ;
            foreach (Vector2 Location in Locations)
            {
                if (Location != prev)
                {
                    AddPlanStep(new MoveStep(Location));
                    prev = Location;
                }
            }
        }

        /// <summary>
        /// Increment the waypoint number to the next location after laying out the plan to get to it, so when the pla is returned to, it will increment.
        /// </summary>
        protected void IncrementWaypointNumber()
        {
            NextWaypointNum = (NextWaypointNum + 1) % Default_Route_Waypoints.Count;
        }

        
       


       

    }
}
