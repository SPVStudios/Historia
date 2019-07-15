using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Historia
{
    public static class DungeonCreator
    {

        private static List<Passageway> CreatePassageways(ref List<Room> Rooms, Vector2 CurrentMapDimensions)
        {

            List<Passageway> Passages = new List<Passageway>();

            Random R = new Random();

            List<Passageway> PossiblePassages = new List<Passageway>();
            List<Passageway> ExtraPassages = new List<Passageway>();
            List<List<int>> RoomsAlreadyConnected = new List<List<int>>();
            for (int I = 0; I < Rooms.Count; I++)
            {
                RoomsAlreadyConnected.Add(new List<int>());
            }


            //Part A: create a list of all possible passageways between rooms.

            for (int I = 0; I < Rooms.Count; I++)
            {
                //for every room, define all possible links ("arcs") to other rooms.
                Room This = Rooms[I];
                int I_Overlaps = 0;

                List<Rectangle> CompassChecks = new List<Rectangle>
                {
                    new Rectangle(This.Location.X, 0, This.Location.Width, This.Location.Y),
                    //north check from top of room to top of map

                    new Rectangle((This.Location.Right), This.Location.Y,
                        (int)CurrentMapDimensions.X - (This.Location.Right), This.Location.Height),
                    //east check from the right hand side of the room to the right-most reaches of the map

                    new Rectangle(This.Location.X, (This.Location.Bottom),
                        This.Location.Width, (int)CurrentMapDimensions.Y - (This.Location.Bottom)),
                    //south check from the bottom of the room to the bottom of the map

                    new Rectangle(0, This.Location.Y, This.Location.X, This.Location.Height)
                };
                //west chek from the left side of the room to the west-most reaches of the map

                List<Vector2> CompassCheckCriteria = new List<Vector2>()
                {
                    new Vector2(0,1),
                    new Vector2(-1,0),
                    new Vector2(0,-1),
                    new Vector2(1,0)
                };//this defines which side of the CompassCheck is closest to the room for use later

                int TargetIndex;


                for (int Check = 0; Check < CompassChecks.Count; Check++)
                {
                    Rectangle C_Check = CompassChecks[Check];
                    List<int> OverlapIndexes = new List<int>();
                    //any room that overlaps with this check will have its number added here

                    for (int J = 0; J < Rooms.Count; J++)
                    {
                        if (I != J)//unless they are the same room...
                        {
                            if (C_Check.Intersects(Rooms[J].Location))
                            {
                                I_Overlaps++;
                                if (!RoomsAlreadyConnected[I].Contains(J))
                                {
                                    OverlapIndexes.Add(J);
                                }


                            }
                        }
                    }



                    if (OverlapIndexes.Count > 0)
                    {
                        //narrow down the options to the closest room only for this compass direction
                        if (OverlapIndexes.Count == 1)
                        {
                            TargetIndex = OverlapIndexes[0];
                            RoomsAlreadyConnected[I].Add(TargetIndex);
                            RoomsAlreadyConnected[TargetIndex].Add(I);
                        }
                        else
                        {

                            int BestIndex = OverlapIndexes[0];
                            int Score = ScoreRoomConnection(Rooms[OverlapIndexes[0]].Location, CompassCheckCriteria[Check]);
                            for (int RoomID = 1; RoomID < OverlapIndexes.Count; RoomID++)
                            {
                                int NewChallenger = ScoreRoomConnection(
                                    Rooms[OverlapIndexes[RoomID]].Location, CompassCheckCriteria[Check]);

                                if (NewChallenger > Score)
                                {
                                    Score = NewChallenger;
                                    BestIndex = OverlapIndexes[RoomID];
                                }
                            }
                            TargetIndex = BestIndex;
                            RoomsAlreadyConnected[I].Add(TargetIndex);
                            RoomsAlreadyConnected[TargetIndex].Add(I);

                        }

                        //target index has been found; establish where the link can go.

                        if (CompassCheckCriteria[Check].X == 1)//West Check
                        {
                            //see what Y co-ords they have in common
                            int upperBound = 0;
                            int LowerBound = 0;
                            FindCommonRange(ref upperBound, ref LowerBound, Rooms[I].Location.Bottom - 1, Rooms[I].Location.Top,
                               Rooms[TargetIndex].Location.Bottom - 1, Rooms[TargetIndex].Location.Top);
                            int Y_Coord;
                            if (upperBound - LowerBound > 3)
                            {
                                Y_Coord = R.Next(LowerBound + 1, upperBound - 1);//selects it at random from the overlap,
                                // ignoring the border tiles, if > 3 wide.
                            }
                            else
                            {
                                Y_Coord = (upperBound + LowerBound) / 2;//finds the average if a small overlap.
                            }
                            int theWidth = Rooms[I].Location.Left - Rooms[TargetIndex].Location.Right;
                            Passageway.Endpoint HomeEnd = new Passageway.Endpoint(new Vector2(Rooms[I].Location.Left, Y_Coord), I);
                            Passageway.Endpoint AwayEnd = new Passageway.Endpoint(new Vector2(Rooms[TargetIndex].Location.Right - 1, Y_Coord), TargetIndex);

                            Passageway ThisP = new Passageway((new Rectangle(Rooms[TargetIndex].Location.Right, Y_Coord, theWidth, 1)), AwayEnd, HomeEnd);


                            VerifyPassage(Rooms[I].Location, Rooms[TargetIndex].Location, ThisP);

                            PossiblePassages.Add(ThisP);

                        }
                        else if (CompassCheckCriteria[Check].X == -1)// East Check
                        {
                            //see what Y co-ords they have in common
                            int upperBound = 0;
                            int LowerBound = 0;
                            int Y_Coord;
                            FindCommonRange(ref upperBound, ref LowerBound, Rooms[I].Location.Bottom - 1, Rooms[I].Location.Top,
                                Rooms[TargetIndex].Location.Bottom - 1, Rooms[TargetIndex].Location.Top);
                            if (upperBound - LowerBound > 3)
                            {
                                Y_Coord = R.Next(LowerBound + 1, upperBound - 1);//selects it at random from the overlap,
                            } // ignoring the border tiles, if > 3 wide.
                            else
                            {
                                Y_Coord = (upperBound + LowerBound) / 2;//finds the average if a small overlap.
                            }
                            int theWidth = Rooms[TargetIndex].Location.Left - Rooms[I].Location.Right;//can be just "right" here
                            Passageway.Endpoint HomeEnd = new Passageway.Endpoint(new Vector2(Rooms[I].Location.Right - 1, Y_Coord), I);
                            Passageway.Endpoint AwayEnd = new Passageway.Endpoint(new Vector2(Rooms[TargetIndex].Location.Left, Y_Coord), TargetIndex);

                            Passageway ThisP = new Passageway(
                                (new Rectangle(Rooms[I].Location.Right, Y_Coord, theWidth, 1)), HomeEnd, AwayEnd);

                            VerifyPassage(Rooms[I].Location, Rooms[TargetIndex].Location, ThisP);

                            PossiblePassages.Add(ThisP);

                        }
                        else if (CompassCheckCriteria[Check].Y == 1)// North Check
                        {
                            //see what X co-ords they have in common
                            int upperBound = 0;
                            int LowerBound = 0;
                            FindCommonRange(ref upperBound, ref LowerBound, Rooms[I].Location.Right - 1, Rooms[I].Location.Left,
                                Rooms[TargetIndex].Location.Right - 1, Rooms[TargetIndex].Location.Left);
                            int X_Coord;
                            if (upperBound - LowerBound > 3)
                            {
                                X_Coord = R.Next(LowerBound + 1, upperBound - 1);//selects it at random from the overlap,
                                // ignoring the border tiles, if > 3 wide.
                            }
                            else
                            {
                                X_Coord = (upperBound + LowerBound) / 2;//finds the average if a small overlap.
                            }

                            int theHeight = Rooms[I].Location.Top - Rooms[TargetIndex].Location.Bottom;
                            Passageway.Endpoint HomeEnd = new Passageway.Endpoint(new Vector2(X_Coord, Rooms[I].Location.Top), I);
                            Passageway.Endpoint AwayEnd = new Passageway.Endpoint(new Vector2(X_Coord, Rooms[TargetIndex].Location.Bottom - 1), TargetIndex);

                            Passageway ThisP = new Passageway(
                                (new Rectangle(X_Coord, Rooms[TargetIndex].Location.Bottom, 1, theHeight)), AwayEnd, HomeEnd);

                            VerifyPassage(Rooms[I].Location, Rooms[TargetIndex].Location, ThisP);

                            PossiblePassages.Add(ThisP)
                                ;

                        }
                        else if (CompassCheckCriteria[Check].Y == -1) //South Check
                        {
                            //see what X co-ords they have in common
                            int upperBound = 0;
                            int LowerBound = 0;
                            int X_Coord;
                            FindCommonRange(ref upperBound, ref LowerBound, Rooms[I].Location.Right - 1, Rooms[I].Location.Left,
                                Rooms[TargetIndex].Location.Right - 1, Rooms[TargetIndex].Location.Left);

                            if (upperBound - LowerBound > 3)
                            {
                                X_Coord = R.Next(LowerBound + 1, upperBound - 1);//selects it at random from the overlap,
                                // ignoring the border tiles, if > 3 wide.
                            }
                            else
                            {
                                X_Coord = (upperBound - LowerBound) / 2 + LowerBound;//finds the average if a small overlap.
                            }

                            int theHeight = Rooms[TargetIndex].Location.Top - Rooms[I].Location.Bottom;
                            Passageway.Endpoint HomeEnd = new Passageway.Endpoint(new Vector2(X_Coord, Rooms[I].Location.Bottom - 1), I);
                            Passageway.Endpoint AwayEnd = new Passageway.Endpoint(new Vector2(X_Coord, Rooms[TargetIndex].Location.Top), TargetIndex);

                            Passageway ThisP = new Passageway(
                                (new Rectangle(X_Coord, Rooms[I].Location.Bottom, 1, theHeight)), HomeEnd, AwayEnd);

                            VerifyPassage(Rooms[I].Location, Rooms[TargetIndex].Location, ThisP);

                            PossiblePassages.Add(ThisP);

                        }

                    }


                }
                if (I_Overlaps == 0)//no options, so the room(somehow) is completely isolated.
                {
                    Rooms.RemoveAt(I);
                }

            }
            //Part A complete

            //Part B: use Kruskal's algorithm to sort all passageways into size order, then use this to create an MST of rooms.

            Kruskals(ref Passages, ref ExtraPassages, PossiblePassages, Rooms.Count);

            //Part B complete

            //stage C: Add a few extra passages in 
            if (ExtraPassages.Count > 0)
            {
                for (int I = 0; I <= ExtraPassages.Count / 4; I++)
                {
                    int NextSelection = R.Next(0, ExtraPassages.Count - 1);
                    Passages.Add(ExtraPassages[NextSelection]);
                    ExtraPassages.RemoveAt(NextSelection);
                }
            }

            //Part D: Add the data of the end points to each Room.
            for (int I = 0; I < Passages.Count; I++)
            {
                Passageway P = Passages[I];
                Rooms[P.End1.EndPointID].AddEntryPoint(P.End2.EndPointID, I, P.End1.Location);

                Rooms[P.End2.EndPointID].AddEntryPoint(P.End1.EndPointID, I, P.End2.Location);
            }


            return Passages;
        }

        public static bool CreateFloorMap
            (
            Vector2 LargeSectionSize, Vector2 MinimumRoomSize,
            Vector2 HalfTheFloorMapDimensions, ref List<Room> Rooms, ref List<Passageway> Passageways, ref Vector2 MapSize, out bool[,] FloorMap
            )
        {

            Random R = new Random();

            Rooms = new List<Room>();
            Passageways = new List<Passageway>();
            MapSize = Vector2.Zero;
            List<Rectangle> Sections = new List<Rectangle>
            {
                new Rectangle(0, 0, (int)HalfTheFloorMapDimensions.X, (int)HalfTheFloorMapDimensions.Y)//the whole map as on section
            };
            bool SectionsSmallEnough = false;
            int LargeRoomXPlusY = (int)LargeSectionSize.X + (int)LargeSectionSize.Y;


            Vector2 MinimumSectionSize = new Vector2(MinimumRoomSize.X + 1, MinimumRoomSize.Y + 2);

            int Iterations = 0;//If iterations reaches 10,000 , something has gone wrong!
            while (!SectionsSmallEnough)
            {
                if (Iterations > 10000)
                {
                    throw new Exception(
                        "The Section Splitter has been unable to generate Sections to fulill the required constraints.");
                }
                List<Rectangle> NewSections = new List<Rectangle>();//empties NewSections
                for (int I = 0; I < Sections.Count; I++)
                {
                    Rectangle Section = Sections[I];
                    int splitter = -1;
                    if (Section.Width > (2 * MinimumRoomSize.X) + 1)
                    {
                        if (Section.Height > (2 * MinimumRoomSize.Y) + 2)//both
                        {
                            splitter = R.Next(1, 100);
                        }
                        else//Wide enough but not tall enough
                        {
                            splitter = R.Next(1, 59);
                        }

                    }
                    else if (Section.Height > (2 * MinimumRoomSize.Y) + 2)//tall enough but not wide enough
                    {
                        splitter = R.Next(41, 100);
                    }

                    if (0 <= splitter && splitter <= 40)//40% chance to be a horizontal split
                    {
                        int halfway = Section.Width / 2;
                        int leeway = halfway - (int)MinimumSectionSize.X;
                        if (leeway >= 0)
                        {
                            int modMax = Math.Min(leeway, Section.Width / 4);
                            int splitmodifier = R.Next(-modMax, modMax);
                            Section.Width = halfway + splitmodifier;
                            NewSections.Add(new Rectangle(Section.X + Section.Width, Section.Y, halfway - splitmodifier, Section.Height));
                        }
                    }
                    else if (splitter >= 60)//40% chance to be a vertical split - to be changed if possible to do L shapes
                    {
                        int halfway = Section.Height / 2;
                        int leeway = halfway - (int)MinimumSectionSize.Y;
                        if (leeway >= 0)
                        {
                            int modMax = Math.Min(leeway, Section.Height / 4);
                            int splitmodifier = R.Next(-modMax, modMax);
                            Section.Height = halfway + splitmodifier;
                            NewSections.Add(new Rectangle(Section.X, Section.Y + Section.Height, Section.Width, halfway - splitmodifier));
                        }

                    }
                    else
                    {//20% to do nothing for this section. Also occurs if no split was rolled as section is too small.


                    }
                    Sections[I] = Section;
                }
                foreach (Rectangle NewSection in NewSections)
                {
                    Sections.Add(NewSection);
                }
                //check if the sections are small enough

                int TotalWidths = 0;
                int TotalHeights = 0;
                foreach (Rectangle Section in Sections)
                {
                    TotalWidths += Section.Width;
                    TotalHeights += Section.Height;
                }

                Vector2 ActualAverageSectionSize = new Vector2(
                    (TotalWidths / Sections.Count),
                    (TotalHeights / Sections.Count));

                ///<summary>
                /// The program may generate quite wide rooms, where the LargeSection values given favour a sqaure,
                /// meaning that the rooms are plenty thin enough but too long, so therefore not breaking the loop.
                /// 
                /// To counteract this, the algorithm will add the two values for X and Y together, and compare the two,
                /// allowing unusually fat/tall sections to be passed by this section of the algorithm. 
                /// 
                /// This isn't a perfect fix, but it should at least help.
                /// 
                ///</summary>


                int AverageSectionXPlusY = (int)ActualAverageSectionSize.X + (int)ActualAverageSectionSize.Y;

                if (AverageSectionXPlusY <= LargeRoomXPlusY)
                {
                    SectionsSmallEnough = true;
                }
                Iterations++;
            }
            //stage 2: turn the sections into Rooms.
            foreach (Rectangle Section in Sections)
            {
                int X_Offset, Y_Offset, Shrink_X, Shrink_Y;
                Vector2 leeway = new Vector2(Section.Width, Section.Height) - MinimumRoomSize;
                if (leeway.X > 1)
                {
                    int Max_X_OffSet = Math.Min((int)leeway.X, 3);
                    X_Offset = R.Next(1, Max_X_OffSet);
                    leeway.X -= X_Offset;
                    Shrink_X = Math.Min((int)leeway.X, R.Next(0, 3));
                }
                else
                {
                    Shrink_X = 0;
                    if (leeway.X == 1)
                    {
                        X_Offset = 1;
                    }
                    else
                    {
                        X_Offset = 0;
                    }
                }

                if (leeway.Y > 2)
                {
                    int Max_Y_OffSet = Math.Min((int)leeway.Y, 3);
                    Y_Offset = R.Next(2, Max_Y_OffSet);
                    leeway.Y -= Y_Offset;
                    Shrink_Y = Math.Min((int)leeway.Y, R.Next(0, 2));
                }
                else
                {
                    Shrink_Y = 0;
                    if (leeway.Y > 0)
                    {
                        Y_Offset = (int)leeway.Y;
                    }
                    else
                    {
                        Y_Offset = 0;
                    }
                }


                Rooms.Add(new Room(
                    new Rectangle(Section.X + X_Offset, Section.Y + Y_Offset,
                    Section.Width - (X_Offset + Shrink_X), Section.Height - (Y_Offset + Shrink_Y))
                    ));
            }



            //stage 3: Double the size of the rooms

            //double the size and offset of every room,in addition adding 2 to the offset in X and 2 in Y;
            for (int Z = 0; Z < Rooms.Count; Z++)
            {
                Room ThisRoom = Rooms[Z];

                Rectangle ThisLocation = ThisRoom.Location;
                ThisLocation.X = (ThisLocation.X * 2) + 2;
                ThisLocation.Y = (ThisLocation.Y * 2) + 2;
                ThisLocation.Width = ThisLocation.Width * 2;
                ThisLocation.Height = ThisLocation.Height * 2;
                ThisRoom.ChangeLocation(ThisLocation);


                Rooms[Z] = ThisRoom;
            }

            //stage 4: Create Passageways between Rooms

            Passageways = CreatePassageways(ref Rooms, new Vector2((HalfTheFloorMapDimensions.X * 2) + 4, (HalfTheFloorMapDimensions.Y * 2) + 4));


            //4a: assign an entry room
            int Size = int.MaxValue;
            int Select = -1;
            for (int I = 0; I < Rooms.Count; I++)
            {
                int ThisSize = Rooms[I].Location.Width * Rooms[I].Location.Height;
                if (ThisSize < Size && ThisSize >= 10)
                {
                    int SpacesInARow = 0;
                    for (int X = Rooms[I].Location.X; X < Rooms[I].Location.Right; X++)
                    {
                        if (IsEntryPointHere(Rooms[I].EntryPoints, new Vector2(X, Rooms[I].Location.Y)))
                        {
                            SpacesInARow = 0;
                        }
                        else
                        {
                            SpacesInARow++;

                        }
                        if (SpacesInARow >= 5)
                        {
                            Size = ThisSize;
                            Select = I;
                            break;
                        }
                    }
                }
            }
            if (Select == -1)
            {
                FloorMap = null;
                return false;
            }
            //assign this room as the entry
            Rooms[Select].Purpose = "Entry";

            //stage 5: turn the Rooms and Passages into a floorMap
            bool[,] floorMap = new bool[(int)(HalfTheFloorMapDimensions.X * 2) + 5, (int)(HalfTheFloorMapDimensions.Y * 2) + 5];//+1 for security

            foreach (Room Room in Rooms)
            {

                for (int I = 0; I < Room.Location.Width; I++)
                {
                    for (int J = 0; J < Room.Location.Height; J++)
                    {
                        floorMap[Room.Location.X + I, Room.Location.Y + J] = true;
                    }
                }

            }

            foreach (Passageway Passage in Passageways)
            {
                for (int I = 0; I < Passage.Location.Width; I++)
                {
                    for (int J = 0; J < Passage.Location.Height; J++)
                    {
                        floorMap[Passage.Location.X + I, Passage.Location.Y + J] = true;
                    }
                }
            }
            //stage 5 complete.

            MapSize = new Vector2(floorMap.GetLength(0), floorMap.LongLength / floorMap.GetLength(0));
            FloorMap = floorMap;
            return true;
        }


        private static void VerifyPassage(Rectangle RectA, Rectangle RectB, Passageway Test)
        {
            if (Test.IsVertical)
            {
                if (Test.Location.X <= RectA.Right - 1 && Test.Location.X <= RectB.Right - 1)
                {
                    if (Test.Location.X >= RectA.Left && Test.Location.X >= RectB.Left)
                    {
                        if (Test.Location.Y == RectA.Bottom || Test.Location.Y == RectB.Bottom)
                        {
                            return;
                        }
                        else
                        {
                            throw new Exception("Passage starts too low");
                        }
                    }
                    else
                    {
                        throw new Exception("Passage too far left");
                    }
                }
                else
                {
                    throw new Exception("Passage too far right");
                }
            }
            else//Is Horizontal
            {
                if (Test.Location.Y <= RectA.Bottom - 1 && Test.Location.Y <= RectB.Bottom - 1)
                {
                    if (Test.Location.Y >= RectA.Top && Test.Location.Y >= RectB.Top)
                    {
                        if (Test.Location.X == RectA.Right || Test.Location.X == RectB.Right)
                        {
                            return;
                        }
                        else
                        {
                            throw new Exception("Passage starts too far over");
                        }
                    }
                    else
                    {
                        throw new Exception("Passage too far up");
                    }
                }
                else
                {
                    throw new Exception("Passage too far down");
                }
            }
        }

        private static int ScoreRoomConnection(Rectangle TargetRoom, Vector2 Criteria)
        {

            if (Criteria.X == 1)// a horizontal check
            {
                return TargetRoom.Right;
            }
            else if (Criteria.X == -1)
            {
                return TargetRoom.Left;
            }
            else if (Criteria.Y == 1)// a vertical check
            {
                return TargetRoom.Bottom;
            }
            else if (Criteria.Y == -1)
            {
                return TargetRoom.Top;
            }
            else
            {
                throw new Exception("You've handed this a criteria with nothing in it");
            }
        }

        private static void FindCommonRange(ref int TopofRange, ref int BottomofRange, int Top1, int Bottom1, int Top2, int Bottom2)
        {
            TopofRange = Math.Min(Top1, Top2);
            BottomofRange = Math.Max(Bottom1, Bottom2);
        }

        private static void Kruskals(ref List<Passageway> SelectedEdges, ref List<Passageway> RefusedEdges,
            List<Passageway> AllEdges, int NumberofNodes)
        {
            //sort all possible passageways into length order
            AllEdges = SortByLength(AllEdges);

            //Kruskal's!!!!

            List<List<int>> Trees = new List<List<int>>();

            foreach (Passageway Arc in AllEdges)
            {
                bool End1_Present = false;
                bool End2_Present = false;
                int End1_Tree = -1;
                int End2_Tree = -1;

                int TreeRef = -1;
                foreach (List<int> Tree in Trees)
                {
                    TreeRef++;
                    foreach (int Node in Tree)
                    {
                        if (Node == Arc.End1.EndPointID)
                        {
                            End1_Present = true;
                            End1_Tree = TreeRef;
                        }
                        else if (Node == Arc.End2.EndPointID)
                        {
                            End2_Present = true;
                            End2_Tree = TreeRef;
                        }
                    }
                }
                if ((!End1_Present))
                {
                    if (!End2_Present)//both are new
                    {
                        SelectedEdges.Add(Arc);
                        Trees.Add(new List<int>() { Arc.End1.EndPointID, Arc.End2.EndPointID });
                    }
                    else//End 1 is new..
                    {

                        //add end1 to End2's tree.
                        SelectedEdges.Add(Arc);
                        Trees[End2_Tree].Add(Arc.End1.EndPointID);
                    }
                }
                else if (!End2_Present)//only end2 is new
                {
                    SelectedEdges.Add(Arc);
                    Trees[End1_Tree].Add(Arc.End2.EndPointID);//add end2 to end1's tree
                }
                else//both are in A tree....
                {
                    if (End1_Tree != End2_Tree)
                    {//if they are in differrent trees...

                        //combine those trees together
                        SelectedEdges.Add(Arc);
                        List<int> Tree1 = Trees[End1_Tree];
                        List<int> Tree2 = Trees[End2_Tree];
                        Tree1.AddRange(Tree2);
                        if (End1_Tree < End2_Tree)
                        {
                            Trees.RemoveAt(End1_Tree);
                            Trees.RemoveAt(End2_Tree - 1);
                        }
                        else
                        {
                            Trees.RemoveAt(End2_Tree);
                            Trees.RemoveAt(End1_Tree - 1);
                        }

                        Trees.Add(Tree1);
                    }
                    else
                    {
                        //do nothing - they would create a loop
                        //add this one to the extra passages though
                        RefusedEdges.Add(Arc);
                    }
                }
                if (Trees.Count == 1 && Trees[0].Count == NumberofNodes)//if all in one tree and all nodes are present...
                {
                    //MST complete
                    return;
                }


            }
            throw new Exception("this graph cannot have an MST created - it is not fully connected.");
        }

        private static int FindDiscrepancy(bool[,] floormap, int X, int Y, int Offset)//Offset of 1 is looking 1 space right. -1 looks left by 1.
        {
            int Discrepancy = 0;
            while (true)
            {
                if (floormap[X, Y - Discrepancy] == false)
                {
                    if (floormap[X + Offset, Y - Discrepancy] == true)
                    {
                        Discrepancy++;
                    }
                    else
                    {
                        if (Discrepancy > 0)
                        {
                            return Discrepancy;

                        }
                        else
                        {
                            throw new Exception("Discrepancy of 0 - should not be L/R Class");
                        }
                    }
                }
                else
                {
                    return -1;//this void ran out before the other wall actually ran into a void- it's a vertical corridor, not a step up
                }

            }
        }

        private static List<Passageway> SortByLength(List<Passageway> Arcs)
        {
            for (int I = 0; I < Arcs.Count - 1; I++)
            {
                if (Arcs[I + 1].Length < Arcs[I].Length)
                {//switch, and continue switching
                    int MoveTo = -1;
                    for (int J = I; J >= 0; J--)
                    {
                        MoveTo = J;
                        if (Arcs[I + 1].Length < Arcs[J].Length)
                        {
                            //keep going
                        }
                        else
                        {
                            MoveTo++;//J is NOT the target as it failed to meet above requirements. It is therefore reverted.
                            break;
                        }

                    }


                    //move it to this spot
                    Passageway MiddleMan = Arcs[I + 1];
                    Arcs.RemoveAt(I + 1);
                    Arcs.Insert(MoveTo, MiddleMan);
                }
            }
            return Arcs;
        }


        public static Map CreateMap(int SizeOutof5)//creates a new map isolated from the system of GameStates
        {
            Random R = new Random();
            int Height = R.Next((SizeOutof5 - 1) * 4, SizeOutof5 * 4) + 6;
            int Width = R.Next((SizeOutof5 - 1) * 4, SizeOutof5 * 4) + 6;

            Environment e = new Environment(true, R);


            List<Room> MapRooms = new List<Room>();
            List<Passageway> Passages = new List<Passageway>();
            Vector2 MapSize = new Vector2();
            bool[,] floorMap;
            while (true)
            {
                if (DungeonCreator.CreateFloorMap(new Vector2(4, 7),
                    new Vector2(2, 2), new Vector2(Height, Width), ref MapRooms,
                    ref Passages, ref MapSize, out floorMap))
                {
                    break;
                }//else try again. This allows invalid maps to be rejected rather than starting over.
            }
            TileSet floorTileSet = new TileSet();
            TileSet wallsTileSet = new TileSet();
            TileSet entryTileSet = new TileSet();
            ObjectTileSet Objects = new ObjectTileSet();
            ObjectTileSet SmallObjectSet = new ObjectTileSet();
            ObjectTileSet LargeObjectSet = new ObjectTileSet();
            XmlManager<TileSet> tileSetLoad = new XmlManager<TileSet>();
            floorTileSet = tileSetLoad.Load("Load/Gameplay/TileSets/" + e.Name + "_Floors.xml");
            wallsTileSet = tileSetLoad.Load("Load/Gameplay/TileSets/" + e.Name + "_Walls.xml");
            entryTileSet = tileSetLoad.Load("Load/Gameplay/TileSets/" + e.Name + "_Entry.xml");

            XmlManager<ObjectTileSet> ObjLoad = new XmlManager<ObjectTileSet>();
            Objects = ObjLoad.Load("Load/Gameplay/TileSets/" + e.Name + "_Objects.xml");
            SmallObjectSet = ObjLoad.Load("Load/Gameplay/TileSets/" + e.Name + "_Objects_Small.xml");
            LargeObjectSet = ObjLoad.Load("Load/Gameplay/TileSets/" + e.Name + "_Objects_Large.xml");
            floorTileSet.LoadContent();
            wallsTileSet.LoadContent();
            entryTileSet.LoadContent();
            Objects.LoadContent();
            SmallObjectSet.LoadContent();
            LargeObjectSet.LoadContent();


            int Version = DungeonImageTracer.FindFirstVersion();
            DungeonImageTracer.CreateandSaveFloorMapImage(floorMap, MapSize, Version, MapRooms);

            Map map = DungeonCreator.CreateMap(MapSize, floorMap, wallsTileSet, floorTileSet, entryTileSet,
                new List<ObjectTileSet>() { Objects, SmallObjectSet, LargeObjectSet }, MapRooms, Passages, e.AcceptedEnemyTypes);
            DungeonImageTracer.CreateandSaveFullMap(map, Version, true);
            map.Name = "The Unknown Ruins";//generic name as not loaded from an actual GameState Location
            return map;
        }

        public static Map CreateMap(int SizeOutof5, GameState gameState)//creates a new map relying if present on the GameState
        {
            Random R = new Random();
            int Height = R.Next((SizeOutof5 - 1) * 4, SizeOutof5 * 4) + 6;
            int Width = R.Next((SizeOutof5 - 1) * 4, SizeOutof5 * 4) + 6;

            Environment e;
            string Name;
            if (gameState.EnteringDungeon)
            {
                e = gameState.WorldMap.Dungeons[gameState.WorldMapLocation].Env;
                Name = gameState.DungeonName;
            }
            else
            {
                e = new Environment(true, R);
                Name = "THe Unknown Ruins";
            }



            List<Room> MapRooms = new List<Room>();
            List<Passageway> Passages = new List<Passageway>();
            Vector2 MapSize = new Vector2();
            bool[,] floorMap;
            while (true)
            {
                if (DungeonCreator.CreateFloorMap(new Vector2(4, 7),
                    new Vector2(2, 2), new Vector2(Height, Width), ref MapRooms,
                    ref Passages, ref MapSize, out floorMap))
                {
                    break;
                }//else try again. This allows invalid maps to be rejected rather than starting over.
            }
            TileSet floorTileSet = new TileSet();
            TileSet wallsTileSet = new TileSet();
            TileSet entryTileSet = new TileSet();
            ObjectTileSet Objects = new ObjectTileSet();
            ObjectTileSet SmallObjectSet = new ObjectTileSet();
            ObjectTileSet LargeObjectSet = new ObjectTileSet();
            XmlManager<TileSet> tileSetLoad = new XmlManager<TileSet>();
            floorTileSet = tileSetLoad.Load("Load/Gameplay/TileSets/" + e.Name + "_Floors.xml");
            wallsTileSet = tileSetLoad.Load("Load/Gameplay/TileSets/" + e.Name + "_Walls.xml");
            entryTileSet = tileSetLoad.Load("Load/Gameplay/TileSets/" + e.Name + "_Entry.xml");

            XmlManager<ObjectTileSet> ObjLoad = new XmlManager<ObjectTileSet>();
            Objects = ObjLoad.Load("Load/Gameplay/TileSets/" + e.Name + "_Objects.xml");
            SmallObjectSet = ObjLoad.Load("Load/Gameplay/TileSets/" + e.Name + "_Objects_Small.xml");
            LargeObjectSet = ObjLoad.Load("Load/Gameplay/TileSets/" + e.Name + "_Objects_Large.xml");
            floorTileSet.LoadContent();
            wallsTileSet.LoadContent();
            entryTileSet.LoadContent();
            Objects.LoadContent();
            SmallObjectSet.LoadContent();
            LargeObjectSet.LoadContent();


            int Version = DungeonImageTracer.FindFirstVersion();
            DungeonImageTracer.CreateandSaveFloorMapImage(floorMap, MapSize, Version, MapRooms);

            Map map = DungeonCreator.CreateMap(MapSize, floorMap, wallsTileSet, floorTileSet, entryTileSet,
                new List<ObjectTileSet>() { Objects, SmallObjectSet, LargeObjectSet }, MapRooms, Passages, e.AcceptedEnemyTypes);
            DungeonImageTracer.CreateandSaveFullMap(map, Version, true);
            map.Name = Name;
            return map;
        }




        public static Map CreateMap(Vector2 floorMapDimensions, bool[,] floorMap,
            TileSet WallTileSet, TileSet FloorTileSet, TileSet EntryTileSet,
            List<ObjectTileSet> ObjectTileSets, List<Room> Rooms,
            List<Passageway> Passageways, List<string> EnemyTypes)
        {
            ///<Overview>
            /// 
            /// Tile Types, Key :
            /// 
            /// 0 = Null,
            /// 1 = WallLeft(of the floor)
            /// 2 = WallRight
            /// 3 = BottomLeftCorner
            /// 4 = BottomRightCorner
            /// 5 = Bottom
            /// 6 = TopAndLeftWall, Lower(R1)
            /// 7 = TopAndLeftWall, Middle(R2)
            /// 8 = TopAndLeftWall, Upper(R3)
            /// 9 = TopAndRightWall, Lower(L1)
            /// 10 = TopAndRightWall, Middle(L2)
            /// 11 = TopAndRightWall, Upper(L3)
            /// 12 = TopWall, Lower(M1)
            /// 13 = TopWall, Middle(M2)
            /// 14 = TopWall, Upper(M3)
            ///
            /// >> The descriptions above may seem at odds with their R, L and M labels.
            /// >> R1 - R3 tiles form BOTH a top wall and a Left wall to a room, despite being on the Right hand side of any "peninsula" to a room
            ///   
            /// 15 = TopLeftCorner(/)
            /// 16 = TopRightCorner(\)
            ///   
            /// 17 = BottomInverseLeftCorner
            /// 18 = BottomInverseRightCorner
            ///  
            /// 19 = Entrance Tile(which in particular to calculate later)
            ///  
            /// 20 = Base Floor Tile
            /// 21 - 28 = (BaseFloorTile)to TopRight, Top, TopLeft, etc of the next alternate floor tile type
            /// 29 - 33 = (BaseFloorTile) in circular sections on top of the next alternate floor tile type
            /// 40 = Alternate Floor Tile Type 1
            /// 41 - 43 = Transitions between base floor tile and Alternate tle type 2....
            ///
            /// 60 = alternate tile type 2...
            ///
            ///  and so on.
            /// </Overview>
            Vector2 TDimensions = WallTileSet.TileDimensions;
            Random R = new Random();
            int[,] TileTypeMap = new int[(int)floorMapDimensions.X, (int)floorMapDimensions.Y];


            for (int X = 1; X < floorMapDimensions.X - 1; X++)
            //After the above process giving a FloorMap with a built-in border around it, of 1 tile every way, that is guaranteed a value of false
            // There is no need to check these values. This 1-tile gap acts as a buffer.
            {

                int discrepancy = 0;

                for (int Y = (int)floorMapDimensions.Y - 2; Y > 0; Y--)//working UPWARDS, X by X.
                {//the - 2 is because of it being base zero (-1) AND not needing the bottom row as it is guaranteed false (-1).

                    if (floorMap[X, Y] == true)
                    {
                        TileTypeMap[X, Y] = 20;

                    }
                    else
                    {
                        if (floorMap[X, Y - 1] == true)
                        {
                            if (floorMap[X - 1, Y] == true)//Inverse Bottom Left
                            {
                                TileTypeMap[X, Y] = 17;
                            }
                            else if (floorMap[X + 1, Y] == true)//Inverse Bottom Right
                            {
                                TileTypeMap[X, Y] = 18;
                            }
                            else//Bottom
                            {
                                TileTypeMap[X, Y] = 5;
                            }
                        }
                        else if (floorMap[X, Y + 1] == true)//if there is a floor tile beneath this one...
                        {
                            if (floorMap[X + 1, Y] == true)//L series...
                            {
                                discrepancy = FindDiscrepancy(floorMap, X, Y, 1);
                                TileTypeMap[X, Y] = 6;
                                TileTypeMap[X, Y - 1] = 7;
                                TileTypeMap[X, Y - 2] = 8;
                                if (discrepancy > 0)
                                {
                                    if (discrepancy > 1)
                                    {
                                        for (int I = 1; I < discrepancy; I++)
                                        {
                                            TileTypeMap[X, Y - (2 + I)] = 1;
                                        }
                                    }
                                    TileTypeMap[X, Y - (2 + discrepancy)] = 15;//Top Left Corner
                                    Y -= (2 + discrepancy);
                                }
                                else
                                {
                                    // Isn't a step up, but a boundary between a room and a vertical corridor
                                    Y -= 2;
                                }

                                discrepancy = 0;
                            }
                            else if (floorMap[X - 1, Y] == true)//R series...
                            {
                                discrepancy = FindDiscrepancy(floorMap, X, Y, -1);//looks left
                                TileTypeMap[X, Y] = 9;
                                TileTypeMap[X, Y - 1] = 10;
                                TileTypeMap[X, Y - 2] = 11;
                                if (discrepancy > 0)
                                {
                                    if (discrepancy > 1)
                                    {
                                        for (int I = 1; I < discrepancy; I++)
                                        {
                                            TileTypeMap[X, Y - (2 + I)] = 2;
                                        }
                                    }
                                    TileTypeMap[X, Y - (2 + discrepancy)] = 16;//Top Left Corner
                                    Y = Y - (2 + discrepancy);
                                }
                                else
                                {
                                    // Isn't a step up, but a boundary between a room and a vertical corridor
                                    Y -= 2;
                                }

                                discrepancy = 0;
                            }
                            else//M series
                            {
                                TileTypeMap[X, Y] = 12;
                                TileTypeMap[X, Y - 1] = 13;
                                TileTypeMap[X, Y - 2] = 14;
                                Y -= 2;
                            }
                        }
                        else if (floorMap[X + 1, Y] == true)
                        {
                            TileTypeMap[X, Y] = 1;//left wall
                        }
                        else if (floorMap[X - 1, Y] == true)
                        {
                            TileTypeMap[X, Y] = 2;//right wall
                        }
                        else if (floorMap[X - 1, Y - 1] == true)//bottom right corner
                        {
                            TileTypeMap[X, Y] = 4;
                        }
                        else if (floorMap[X + 1, Y - 1] == true)//bottom left corner
                        {
                            TileTypeMap[X, Y] = 3;
                        }
                        else if (floorMap[X + 1, Y + 1])//top left sections
                        {
                            TileTypeMap[X, Y] = 1;
                            TileTypeMap[X, Y - 1] = 1;
                            TileTypeMap[X, Y - 2] = 15;
                            Y -= 2;
                        }
                        else if (floorMap[X - 1, Y + 1] == true)
                        {
                            TileTypeMap[X, Y] = 2;
                            TileTypeMap[X, Y - 1] = 2;
                            TileTypeMap[X, Y - 2] = 16;
                            Y -= 2;
                        }
                    }
                }
            }

            //1b: assign a tile as the transition. It must be one tile above the centre of the room with a guaranteed tile of L,M or R series next to it on either side.
            //find the entry room
            int EntryRoom = -1;
            Vector2 EntryLoc = Vector2.Zero;
            for (int I = 0; I < Rooms.Count; I++)
            {
                if (Rooms[I].Purpose == "Entry")
                {
                    EntryRoom = I;
                    break;
                }
            }
            if (EntryRoom != -1)
            {
                int SpacesInARow = 0;
                List<int> PossXs = new List<int>();//all possible centre points for the entrance will be placed here.
                Rectangle Room = Rooms[EntryRoom].Location;
                for (int X = Room.X; X < Room.Right; X++)
                {
                    if (IsEntryPointHere(Rooms[EntryRoom].EntryPoints, new Vector2(X, Rooms[EntryRoom].Location.Y)))
                    {
                        SpacesInARow = 0;
                    }
                    else
                    {
                        SpacesInARow++;

                    }

                    if (SpacesInARow == 5)
                    {
                        PossXs.Add(X - 2);
                        SpacesInARow = 4;

                    }
                }
                int Choice = PossXs[R.Next(0, PossXs.Count)];
                TileTypeMap[Choice, Room.Y - 1] = 19;//TRANSITION TILE.
                EntryLoc = new Vector2(Choice, Room.Y - 1);
            }

            //step 1 complete.

            //step 2: Place objects in valid places on the map

            //creat a new layer for Objects with the Object Tileset(s).
            ObjectLayer Objects = new ObjectLayer(ObjectTileSets, new List<MapObject>(), TDimensions, floorMapDimensions);

            int[,] OverallObjectLocations = new int[(int)floorMapDimensions.X, (int)floorMapDimensions.Y];



            //load all object fill methods
            const string FillDictPath = "ObjectFillMethods/FillMethodDirectory.xml";

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(FillDictPath);//file path

            List<ObjectFillMethod> GeneralFillMethods = new List<ObjectFillMethod>();
            List<ObjectFillMethod> PurposeFillMethods = new List<ObjectFillMethod>();
            foreach (XmlNode Method in xmlDoc.DocumentElement.ChildNodes)
            {
                string Class = Method.InnerText;
                string YesNo = Method.Attributes["IsGP"].Value;

                ObjectFillMethod This = (ObjectFillMethod)Activator.CreateInstance(Type.GetType("Historia.ObjectFillMethods." + Class));
                if (YesNo == "Yes")
                {
                    This.GeneralPurpose = true;
                    GeneralFillMethods.Add(This);
                }
                else
                {
                    This.GeneralPurpose = false;
                    This.SpecificPurpose = Method.Attributes["Purpose"].InnerText;
                    PurposeFillMethods.Add(This);
                }
            }



            //Populate the map with objects on a room-by room basis
            foreach (Room I in Rooms)
            {

                //create a set of rectangles that should NEVER be built over, as the "Guaranteed Path".
                List<Rectangle> BannedAreas = new List<Rectangle>();
                ///<overview>
                ///This can happen in 4 ways:
                ///
                /// 1: the route through the middle from point to point.
                /// 
                /// 2: the route semi-randomly carved through from all points to 1 central point
                /// 
                /// 3: the border round the outskirts of the room.
                /// 
                /// 4: BOTH number 3 and 1 of the other 2.
                /// 
                /// </overview>
                int roomSize = I.Location.Width * I.Location.Height;
                int banmethod;


                int NumObjectfillmethods;//the amount of different object fill methods that will be applied to this room

                int biggestobjectfillmethod;//the latest one in the list of those to apply,
                //and therefore the one that requires the biggest room for it to work

                if (roomSize > 70 && I.EntryPoints.Count > 1)
                {
                    //big room, so all are fine
                    banmethod = R.Next(1, 5);
                    NumObjectfillmethods = R.Next(2, 4);
                    biggestobjectfillmethod = 6;// zero index of 7
                }
                else if (roomSize > 25)
                {
                    if (I.EntryPoints.Count > 1 && TossCoin(R))
                    {//50:50 chance of being method 4 if there is 2+ entry points to the room
                        banmethod = 4;
                    }
                    else
                    {//otherwise is one of the first two.
                        banmethod = R.Next(1, 2);
                    }
                    //limited room - 1 or 2 only

                    NumObjectfillmethods = R.Next(1, 3);
                    biggestobjectfillmethod = 4;
                }
                else
                {
                    banmethod = 1;
                    //very small room, so don't run any large ones.
                    NumObjectfillmethods = R.Next(1, 2);
                    biggestobjectfillmethod = 2;
                }
                switch (banmethod)
                {
                    case 1:
                        BannedAreas.AddRange(CreateGuaranteedPath1(I, Passageways));
                        break;
                    case 2:
                        BannedAreas.AddRange(CreateGuaranteedPath2(I, Passageways));
                        break;
                    case 3:
                        BannedAreas.AddRange(CreateGuaranteedPath3(I, Passageways));
                        break;
                    case 4:
                        BannedAreas.AddRange(CreateGuaranteedPath4(I, Passageways));
                        break;
                    case 5:
                        BannedAreas.AddRange(CreateGuaranteedPath5(I, Passageways));
                        break;
                }

                if (I.Purpose == "Entry")
                {
                    BannedAreas.Add(new Rectangle((int)EntryLoc.X, (int)EntryLoc.Y, 1, 2));
                }

                List<MapObject> CreatedforI = new List<MapObject>();
                List<Rectangle> AlreadyFilledI = new List<Rectangle>();

                //assign the correct fill method(s) for a room of a given purpose
                if (I.Purpose != null && I.Purpose != string.Empty)
                {
                    foreach (ObjectFillMethod Method in PurposeFillMethods)
                    {
                        if (NumObjectfillmethods > 0 && Method.SpecificPurpose == I.Purpose)
                        {
                            Method.FillRoom(I, ref CreatedforI, ObjectTileSets, R, ref AlreadyFilledI);
                            NumObjectfillmethods--;//apply one less other method
                        }
                    }
                }
                if (NumObjectfillmethods > 0)//if any methods are left to be carried out
                {
                    //randomly the predetermined number of object-filling effects to the room, if the room has no assigned purpose,
                    // or the number of 
                    List<ObjectFillMethod> DeletingList = new List<ObjectFillMethod>();
                    biggestobjectfillmethod = Math.Min(biggestobjectfillmethod, GeneralFillMethods.Count);
                    for (int M = 0; M < biggestobjectfillmethod; M++)
                    {
                        DeletingList.Add(GeneralFillMethods[M]);
                    }

                    //select the correct number of fill methods from the list
                    // apply each one after the other in a loop
                    for (int M = 0; M < NumObjectfillmethods; M++)
                    {
                        int TheNextMethod = R.Next(0, DeletingList.Count - 1);
                        DeletingList[TheNextMethod].FillRoom(I, ref CreatedforI, ObjectTileSets, R, ref AlreadyFilledI);
                        DeletingList.RemoveAt(TheNextMethod);
                    }
                }
                ///<overview>
                /// Object Fill Methods will be CLASSES, based off of the original abstract class ObjectFillMethod
                /// 
                /// they will contain their 1 static method.
                /// 
                /// There also needs to be a database of these in an xmlfile to call them, with their Type value stored in the xml file
                /// along with whether they are repetitive, or should be taken off the list of possibilities after being rolled once
                /// 
                /// these xmlfiles will also store if they may only be used in rooms with a specific purpose too
                /// 
                /// </overview>

                ///<GeneralFillMethods>
                /// 
                /// Fill Methods, from the smallest to largest room it should be used on at minimum
                /// 
                ///     Installation (up to 3)
                ///     Border Fill
                ///     Center fill
                ///     Quartering Off
                ///     Negative Circle Fill
                /// 
                ///  Total 7 (3 installations, 4 other)
                /// 
                /// </GereralFillMethods>

                ///<SpecificFillMethods>
                ///
                /// These are invoked only if a room's purpose is that of the fill method. NO OTHER METHODS should be used in the
                /// same area as these methods - if you want very large room, give it a rectangle in that room, and keep a separate rect
                ///  to apply a general method(s) to.
                ///  
                ///     Full (Storage Only)
                ///     Sleeping Quarter Setup (Sleeping Q)
                ///     Trove (Loot Room Only)
                ///     Canteen Setup (Canteen Only)
                ///     Grand Object (Boss Room)
                /// 
                /// </SpecificFillMethods>

                //finally, remove all objects from the Room's list that overlap at all with any of the banned areas
                for (int O = 0; O < CreatedforI.Count; O++)
                {
                    Rectangle Presence = CreatedforI[O].FullTileLocation;

                    /*for (int P = 0; P < I.EntryPoints.Count; P++)
                    {
                        BannedAreas.Add(new Rectangle((int)I.EntryPoints[P].Location.X,(int)I.EntryPoints[P].Location.Y,1,1));
                    }
                    */
                    foreach (Rectangle Ban in BannedAreas)
                    {

                        if (RectMethod.TileIntersects(Presence, Ban))
                        {
                            //delete the object
                            CreatedforI.RemoveAt(O);
                            O--;
                            break;
                        }
                    }
                }
                //Add these objects to the overall ObjectLayer
                foreach (MapObject O in CreatedforI)
                {
                    Objects.AddObject(O);
                }

            }//next room



            //step 2 complete


            //step 3: alter the tiletypemap tiles with valid patterns of floor tiles of various types.

            //find spots that can have patterns

            //conjoin adjacent spots into Rectangles and/or  multiple rectangles stored as 1 variable

            //(OPTIONAL) split up large areas and ignore some of these smaller split-offs

            //ensure that all areas are separated by at least a 1-tile thick border from other areas

            //change the tiles on the TileTypeMap based on where that tile is in relation to (base tile or wall)s like with
            //how the wall's tiles were generated

            //step 3 complete

            /*step 4: Create a Layer for the tile map, loading the images from the spritesheet as in line with the TileTypeMap.
             * 
             * Based on a random (q. small) chance, change the basic tile of a given type (not a transition) to a different image
             * if one exists on the tile sheet (check if yes/no, and how many options there are, before beginning the iteration part
             * of this step)
             */

            Tile[,] LayerTiles = new Tile[(int)floorMapDimensions.X, (int)floorMapDimensions.Y];


            for (int X = 0; X < floorMapDimensions.X; X++)
            {
                for (int Y = 0; Y < floorMapDimensions.Y; Y++)
                {
                    int WhichTileSet = 0;//to be changed later

                    Vector2 SourceRect;
                    if (TileTypeMap[X, Y] < 20)//0 through 19
                    {
                        WhichTileSet = 0; // Set 0 refers to the WALLS
                        if (TileTypeMap[X, Y] < 10) // 0 through 9
                        {
                            SourceRect = new Vector2(TileTypeMap[X, Y] * (int)TDimensions.X, 0);
                        }
                        else//10 through 19
                        {
                            int PosOnRow2 = TileTypeMap[X, Y] - 10;
                            SourceRect = new Vector2(PosOnRow2 * (int)TDimensions.X, (int)TDimensions.Y);
                        }
                    }
                    else//not less than 20
                    {
                        WhichTileSet = 1;//the floor tile set
                        int FloorTileTheme = (TileTypeMap[X, Y] / 20) - 1; // the base type would be type 1, NOT TYPE 0, hence deducting 1 afterwards.

                        int FloorTileType = TileTypeMap[X, Y] % 20;
                        if (FloorTileType == 0)
                        {
                            //base tile of that tile theme. Introduce a small chace for a random tile, if there are any.
                            int Roll = R.Next(1, 100);
                            if (Roll > 70)
                            { // a random alternate form, IF such alternate forms exist 
                                if (FloorTileSet.Schema.TileThemes[FloorTileTheme].BaseTile_HasAlternate)
                                {
                                    int Variants = FloorTileSet.Schema.TileThemes[FloorTileTheme].BaseTile_NumAlternates;
                                    int ChosenVariant = R.Next(1, Variants);
                                    int TileRow = 2 * FloorTileTheme;
                                    SourceRect = new Vector2(ChosenVariant * (int)TDimensions.X,
                                        FloorTileTheme * (int)TDimensions.Y);
                                }
                                else
                                {
                                    int TileRow = 2 * (FloorTileTheme);
                                    SourceRect = new Vector2(0, TileRow * (int)TDimensions.Y);
                                }//this is the same as below, as it will just print the default tile anyway if
                                //there are no other variants
                            }
                            else
                            {
                                int TileRow = 2 * (FloorTileTheme);
                                SourceRect = new Vector2(0, TileRow * (int)TDimensions.Y);
                            }
                        }
                        else//it is a transition tile to that Tile Theme
                        {
                            int TileRow = (2 * FloorTileTheme) + 1;
                            SourceRect = new Vector2(FloorTileType * (int)TDimensions.X, TileRow * (int)TDimensions.Y);
                        }
                    }
                    Tile This = new Tile();
                    This.LoadContent(SourceRect, WhichTileSet);
                    LayerTiles[X, Y] = This;
                }
            }
            //Correct the tiles that will make up the exit
            int EntranceWidth = EntryTileSet.Image.Texture.Width / (int)EntryTileSet.TileDimensions.X;
            int EntranceHeight = EntryTileSet.Image.Texture.Height / (int)EntryTileSet.TileDimensions.Y;
            for (int X = 0; X < EntranceWidth; X++)
            {
                for (int Y = 0; Y < EntranceHeight; Y++)
                {
                    Tile Override = new Tile();
                    Override.LoadContent(new Vector2(32 * X, 32 * Y), 2);
                    LayerTiles[(int)EntryLoc.X - 1 + X, (int)EntryLoc.Y - EntranceHeight + 1 + Y] = Override;

                }
            }

            List<TileSet> BaseTileSets = new List<TileSet>()
            {
                WallTileSet,
                FloorTileSet,
                EntryTileSet
            };
            Layer BaseLayer = new Layer(BaseTileSets, LayerTiles, floorMapDimensions, TDimensions);


            ///<TileTypeOverview>
            /// 
            /// Tile Types, Key :
            /// 
            /// 0 = Null,
            /// 1 = WallLeft(of the floor)
            /// 2 = WallRight
            /// 3 = BottomLeftCorner
            /// 4 = BottomRightCorner
            /// 5 = Bottom
            /// 6 = TopAndLeftWall, Lower(R1)
            /// 7 = TopAndLeftWall, Middle(R2)
            /// 8 = TopAndLeftWall, Upper(R3)
            /// 9 = TopAndRightWall, Lower(L1)
            /// 10 = TopAndRightWall, Middle(L2)
            /// 11 = TopAndRightWall, Upper(L3)
            /// 12 = TopWall, Lower(M1)
            /// 13 = TopWall, Middle(M2)
            /// 14 = TopWall, Upper(M3)
            ///
            /// >> The descriptions above may seem at odds with their R, L and M labels.
            /// >> R1 - R3 tiles form BOTH a top wall and a Left wall to a room, despite being on the Right hand side of any "peninsula" to a room
            ///   
            /// 15 = TopLeftCorner(/)
            /// 16 = TopRightCorner(\)
            ///   
            /// 17 = BottomInverseLeftCorner
            /// 18 = BottomInverseRightCorner
            ///  
            ///  
            ///  20 = Base Floor Tile
            ///  21 - 28 = (BaseFloorTile)to TopRight, Top, TopLeft, etc of the next alternate floor tile type
            ///  29 - 33 = (BaseFloorTile) in circular sections on top of the next alternate floor tile type
            ///  
            ///  40 = Alternate Floor Tile Type 1
            ///  41 - 53 = Transitions between base floor tile and Alternate tle type 2....
            ///
            ///  60 = alternate tile type 2...
            ///
            ///  and so on.
            /// </Overview>

            //step 5: create a collision map from the floor map, then altering values for tiles that are objects.
            /* DO NOT make the collision map a bool system, but rather have different values of <obstructed> for what it
             * is obstructed BY, ie 0=wall/void, 1= object, 2= enemy, 3 = the player, 4 = bosses, 5 = NPCs, 6= VALID, and run obstruction checks on whether
             * the collsion value is < or > 6. Enemy AI can then use a more specific check to see what is in their way,
             * and act differently if it is another fellow enemy or not.
             */
            int[,] Collision_Map = new int[(int)floorMapDimensions.X, (int)floorMapDimensions.Y];

            for (int X = 0; X < floorMapDimensions.X; X++)
            {
                for (int Y = 0; Y < floorMapDimensions.Y; Y++)
                {
                    if (TileTypeMap[X, Y] == 0)
                    {
                        Collision_Map[X, Y] = 0;
                    }
                    else if (TileTypeMap[X, Y] < 19)
                    {
                        Collision_Map[X, Y] = 1;
                    }
                    else if (Objects.UNTraversables[X, Y])
                    {
                        if (Objects.CanClimbIntos[X, Y])
                        {
                            Collision_Map[X, Y] = 10;
                        }
                        else
                        {
                            Collision_Map[X, Y] = 2;
                        }
                    }
                    else if (new Vector2(X, Y) == EntryLoc)
                    {
                        Collision_Map[X, Y] = 21;
                    }
                    else
                    {
                        Collision_Map[X, Y] = 30;
                    }
                }
            }
            /// <Collision Overview>
            /// Determines the collision status of the target tile.
            /// 
            /// Key:
            /// 0 = Impassable - Void
            /// 1 = Impassable - Wall
            /// 2 = Impassable - Object
            /// 3 = Impassable - NPC
            /// 4 = Impassable - Enemy
            /// 5 = Impassable - Boss
            /// 
            /// 10 = Object You can climb into, with the relevant perk
            /// 
            /// 20 = Local Transition tile          -therefore 20 or more is passable, although they can have varying effects
            /// 21 = Global Transition tile
            /// 
            /// 30 = Free to move into
            /// 
            /// </Collision Overview>


            //step 6: collate all of the layers,collision maps, rooms, enemy preferences etc into a map instance, and export.
            //Delete this before use

            List<Layer> Layers = new List<Layer>()
            {
                BaseLayer

            };
            Map map = new Map(Layers, Objects, Collision_Map, floorMapDimensions, TDimensions, Rooms, Passageways, EnemyTypes);
            map.EntryLoc = EntryLoc;
            return map;

        }

        private static List<Rectangle> CreateGuaranteedPath1(Room R, List<Passageway> Passages)
        {
            List<Rectangle> AreasToAvoid = new List<Rectangle>();

            Vector2 middleOfRoom = new Vector2(R.Location.Left + (int)(R.Location.Width / 2),
                R.Location.Top + (int)(R.Location.Height / 2));

            foreach (Room.EntryPoint E in R.EntryPoints)
            {
                AreasToAvoid.AddRange(CarvePathAtoB(Passages[E.PassagewayUsing].IsVertical, E.Location, middleOfRoom));
            }
            return AreasToAvoid;
        }

        private static List<Rectangle> CreateGuaranteedPath2(Room R, List<Passageway> Passages)
        {
            List<Rectangle> AreasToAvoid = new List<Rectangle>();
            Random D = new Random();


            Vector2 randomSpot = new Vector2(D.Next(R.Location.Left, R.Location.Right - 1),
                D.Next(R.Location.Top, R.Location.Bottom - 1));

            foreach (Room.EntryPoint E in R.EntryPoints)
            {
                Vector2 wayToSpot = randomSpot - E.Location;
                int distance = (int)wayToSpot.X + (int)wayToSpot.Y;
                if (distance > 12)
                {
                    //2 splits

                    int FirstX = D.Next(Math.Min(0, (int)wayToSpot.X / 2), Math.Max(0, (int)wayToSpot.X / 2));
                    int FirstY = D.Next(Math.Min(0, (int)wayToSpot.Y / 2), Math.Max(0, (int)wayToSpot.Y / 2));
                    int SecondX = D.Next(Math.Min(0, (int)wayToSpot.X - FirstX), Math.Max(0, (int)wayToSpot.X - FirstX));
                    int SecondY = D.Next(Math.Min(0, (int)wayToSpot.Y - FirstY), Math.Max(0, (int)wayToSpot.Y - FirstY));
                    Vector2 checkpoint1 = E.Location + new Vector2(FirstX, FirstY);
                    Vector2 checkpoint2 = E.Location + new Vector2(SecondX, SecondY);

                    AreasToAvoid.AddRange(CarvePathAtoB(TossCoin(D), E.Location, checkpoint1));
                    AreasToAvoid.AddRange(CarvePathAtoB(TossCoin(D), checkpoint1, checkpoint2));
                    AreasToAvoid.AddRange(CarvePathAtoB(TossCoin(D), checkpoint2, randomSpot));
                }
                else if (distance > 4)
                {//1 split
                    int FirstX = D.Next(Math.Min(0, (int)wayToSpot.X / 2), Math.Max(0, (int)wayToSpot.X / 2));
                    int FirstY = D.Next(Math.Min(0, (int)wayToSpot.Y / 2), Math.Max(0, (int)wayToSpot.Y / 2));

                    Vector2 checkpoint1 = E.Location + new Vector2(FirstX, FirstY);

                    AreasToAvoid.AddRange(CarvePathAtoB(TossCoin(D), E.Location, checkpoint1));
                    AreasToAvoid.AddRange(CarvePathAtoB(TossCoin(D), checkpoint1, randomSpot));


                }
                else
                {
                    bool VFirst = TossCoin(D);

                    AreasToAvoid.AddRange(CarvePathAtoB(VFirst, E.Location, randomSpot));
                }

            }
            return AreasToAvoid;
        }

        private static List<Rectangle> CreateGuaranteedPath3(Room R, List<Passageway> Passages)
        {
            return new List<Rectangle>()
            {
                new Rectangle(R.Location.X,R.Location.Y,R.Location.Width,1),
                new Rectangle(R.Location.X,R.Location.Y,1,R.Location.Height),
                new Rectangle(R.Location.X,R.Location.Bottom-1,R.Location.Width,1),
                new Rectangle(R.Location.Right-1,R.Location.Y,1,R.Location.Height)
            };
        }

        private static List<Rectangle> CreateGuaranteedPath4(Room R, List<Passageway> Passages)
        {
            //Makes all entry points stem to one single entry point. Only works on rooms with multiple entry points.
            List<Rectangle> AreasToAvoid = new List<Rectangle>();
            Random D = new Random();
            int ChosenEntrypoint = D.Next(0, R.EntryPoints.Count - 1);
            for (int I = 0; I < R.EntryPoints.Count; I++)
            {
                if (I != ChosenEntrypoint)
                {
                    bool vertfirst = Passages[R.EntryPoints[I].PassagewayUsing].IsVertical;
                    AreasToAvoid.AddRange(CarvePathAtoB(vertfirst, R.EntryPoints[I].Location, R.EntryPoints[ChosenEntrypoint].Location));
                }
            }
            return AreasToAvoid;
        }

        private static List<Rectangle> CreateGuaranteedPath5(Room R, List<Passageway> Passages)
        {
            List<Rectangle> AreasToAvoid = CreateGuaranteedPath3(R, Passages);
            AreasToAvoid.AddRange(CreateGuaranteedPath1(R, Passages));
            return AreasToAvoid;
        }

        private static List<Rectangle> CarvePathAtoB(bool VerticalFirst, Vector2 PointA, Vector2 PointB)
        {
            Vector2 Route = PointB - PointA;
            List<Rectangle> Path = new List<Rectangle>();

            if (VerticalFirst)
            {
                //do the vertical column first
                if (Route.Y >= 0)
                {
                    //goes down or nowhere
                    Path.Add(new Rectangle((int)PointA.X, (int)PointA.Y, 1, (int)Route.Y + 1));


                }
                else//goes up
                {
                    Path.Add(new Rectangle((int)PointA.X, (int)PointB.Y, 1, (int)-Route.Y + 1));

                }
                //now do the horizontal row
                if (Route.X >= 0)//right or nowhere
                {
                    Path.Add(new Rectangle((int)PointA.X, (int)PointB.Y, (int)Route.X + 1, 1));
                }
                else//goes left
                {
                    Path.Add(new Rectangle((int)PointB.X, (int)PointB.Y, (int)-Route.X + 1, 1));
                }
            }
            else//Is Horizontal
            {
                //do the horizontal row first
                if (Route.X >= 0)//right or nowhere
                {
                    Path.Add(new Rectangle((int)PointA.X, (int)PointA.Y, (int)Route.X + 1, 1));
                }
                else//goes left
                {
                    Path.Add(new Rectangle((int)PointB.X, (int)PointA.Y, (int)-Route.X + 1, 1));
                }
                //now do the vertical column
                if (Route.Y >= 0)
                {
                    //goes down or nowhere
                    Path.Add(new Rectangle((int)PointB.X, (int)PointA.Y, 1, (int)Route.Y + 1));
                }
                else//goes up
                {
                    Path.Add(new Rectangle((int)PointB.X, (int)PointB.Y, 1, (int)-Route.Y + 1));
                }

            }
            return Path;
        }

        public static bool TossCoin(Random C)
        {


            if (C.Next() >= int.MaxValue / 2)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public static bool IsEntryPointHere(List<Room.EntryPoint> Entries, Vector2 Loc)
        {
            foreach (Room.EntryPoint E in Entries)
            {
                if (E.Location == Loc)
                {
                    return true;
                }

            }
            return false;
        }
    }
}
