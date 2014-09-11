using Cyber.HabboHotel.Rooms;
using System;
using System.Collections.Generic;
namespace Cyber.HabboHotel.PathFinding
{
	internal class PathFinder
	{
		public static Vector2D[] DiagMovePoints = new Vector2D[]
		{
			new Vector2D(-1, -1),
			new Vector2D(0, -1),
			new Vector2D(1, -1),
			new Vector2D(1, 0),
			new Vector2D(1, 1),
			new Vector2D(0, 1),
			new Vector2D(-1, 1),
			new Vector2D(-1, 0)
		};
		public static Vector2D[] NoDiagMovePoints = new Vector2D[]
		{
			new Vector2D(0, -1),
			new Vector2D(1, 0),
			new Vector2D(0, 1),
			new Vector2D(-1, 0)
		};

        internal static int CalculateRotation(int X1, int Y1, int X2, int Y2)
        {
            int dX = X2 - X1;
            int dY = Y2 - Y1;

            double d = Math.Atan2(dY, dX) * 180 / Math.PI;
            return ((int)d + 90) / 45;
        }

		public static List<Vector2D> FindPath(RoomUser User, bool Diag, Gamemap Map, Vector2D Start, Vector2D End)
		{
			List<Vector2D> list = new List<Vector2D>();
			PathFinderNode pathFinderNode = PathFinder.FindPathReversed(User, Diag, Map, Start, End);
			if (pathFinderNode != null)
			{
				list.Add(End);
				while (pathFinderNode.Next != null)
				{
					list.Add(pathFinderNode.Next.Position);
					pathFinderNode = pathFinderNode.Next;
				}
			}
			return list;
		}
		public static PathFinderNode FindPathReversed(RoomUser User, bool Diag, Gamemap Map, Vector2D Start, Vector2D End)
		{
			MinHeap<PathFinderNode> minHeap = new MinHeap<PathFinderNode>(256);
			PathFinderNode[,] array = new PathFinderNode[Map.Model.MapSizeX, Map.Model.MapSizeY];
			PathFinderNode pathFinderNode = new PathFinderNode(Start);
			pathFinderNode.Cost = 0;
			PathFinderNode breadcrumb = new PathFinderNode(End);
			array[pathFinderNode.Position.X, pathFinderNode.Position.Y] = pathFinderNode;
			minHeap.Add(pathFinderNode);
			checked
			{
				while (minHeap.Count > 0)
				{
					pathFinderNode = minHeap.ExtractFirst();
					pathFinderNode.InClosed = true;
					int num = 0;
					while (Diag ? (num < PathFinder.DiagMovePoints.Length) : (num < PathFinder.NoDiagMovePoints.Length))
					{
						Vector2D vector2D = pathFinderNode.Position + (Diag ? PathFinder.DiagMovePoints[num] : PathFinder.NoDiagMovePoints[num]);
						bool endOfPath = vector2D.X == End.X && vector2D.Y == End.Y;
						if (Map.IsValidStep(User, new Vector2D(pathFinderNode.Position.X, pathFinderNode.Position.Y), vector2D, endOfPath, User.AllowOverride))
						{
							PathFinderNode pathFinderNode2;
							if (array[vector2D.X, vector2D.Y] == null)
							{
								pathFinderNode2 = new PathFinderNode(vector2D);
								array[vector2D.X, vector2D.Y] = pathFinderNode2;
							}
							else
							{
								pathFinderNode2 = array[vector2D.X, vector2D.Y];
							}
							if (!pathFinderNode2.InClosed)
							{
								int num2 = 0;
								if (pathFinderNode.Position.X != pathFinderNode2.Position.X)
								{
									num2++;
								}
								if (pathFinderNode.Position.Y != pathFinderNode2.Position.Y)
								{
									num2++;
								}
								int num3 = pathFinderNode.Cost + num2 + pathFinderNode2.Position.GetDistanceSquared(End);
								if (num3 < pathFinderNode2.Cost)
								{
									pathFinderNode2.Cost = num3;
									pathFinderNode2.Next = pathFinderNode;
								}
								if (!pathFinderNode2.InOpen)
								{
									if (pathFinderNode2.Equals(breadcrumb))
									{
										pathFinderNode2.Next = pathFinderNode;
										return pathFinderNode2;
									}
									pathFinderNode2.InOpen = true;
									minHeap.Add(pathFinderNode2);
								}
							}
						}
						num++;
					}
				}
				return null;
			}
		}
	}
}
