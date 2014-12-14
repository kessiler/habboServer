using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Mercury.HabboHotel.Rooms;
using Mercury.HabboHotel.PathFinding;

namespace Mercury.HabboHotel.Pathfinding
{
    class PathfinderNode
    {
        internal readonly int Distance;
        internal readonly Point Point;

        internal PathfinderNode(int Distance, Point Point)
        {
            this.Distance = Distance;
            this.Point = Point;
        }
    }

    class Pathfinder
    {
        private static int TileDistance(Point Start, Point End)
        {
            return Math.Abs(Start.X - End.X) + Math.Abs(Start.Y - End.Y);
        }

        internal static int CalculateRotation(int X1, int Y1, int X2, int Y2)
        {
            int dX = X2 - X1;
            int dY = Y2 - Y1;

            double d = Math.Atan2(dY, dX) * 180 / Math.PI;
            return ((int)d + 90) / 45;
        }

        internal static List<Point> GeneratePath(RoomUser User, Point Start, Point End, Gamemap Map)
        {
            List<Point> List = new List<Point>();
            if (!Map.validTile(End.X, End.Y))
            {
                return List;
            }

            Point WorkingCoord = Start;
            short[,] Around = new short[8, 2] { { 0, -1 }, { 1, 0 }, { 0, 1 }, { -1, 0 }, { -1, -1 }, { 1, -1 }, { 1, 1 }, { -1, 1 } };

            while (true)
            {
                var TilesAround = new List<PathfinderNode>();

                for (short i = 0; i < 8; i++)
                {
                    Point NexTile = new Point(WorkingCoord.X + Around[i, 0], WorkingCoord.Y + Around[i, 1]);

                    if (Map.IsValidStep2(User, Start, NexTile, NexTile.X == End.X && NexTile.Y == End.Y, false))
                    {
                        TilesAround.Add(new PathfinderNode(TileDistance(NexTile, End), NexTile));
                    }
                }

                if (TilesAround.Count > 0)
                {
                    var Sorted = (from node in TilesAround orderby node.Distance ascending select node).ToList();

                    WorkingCoord = Sorted[0].Point;

                    List.Add(WorkingCoord);
                    Console.WriteLine(WorkingCoord.X + " - " + WorkingCoord.Y);

                    if (End.X == WorkingCoord.X && End.Y == WorkingCoord.Y)
                    {
                        List.Add(WorkingCoord);
                        return List;
                    }
                }
                else
                {
                    return List;
                }
            }
        }
    }
}