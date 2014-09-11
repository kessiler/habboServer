using Astar.Algorithm;
using enclosuretest.Algorithm;
using System;
using System.Collections.Generic;
using System.Drawing;
namespace enclosuretest
{
	public class GameField : IPathNode
	{
		private byte[,] currentField;
		private AStarSolver<GameField> astarSolver;
		private Queue<GametileUpdate> newEntries = new Queue<GametileUpdate>();
		private bool diagonal;
		private GametileUpdate currentlyChecking;
		public bool this[int y, int x]
		{
			get
			{
				return y >= 0 && x >= 0 && y <= this.currentField.GetUpperBound(0) && x <= this.currentField.GetUpperBound(1);
			}
		}
        public GameField(byte[,] theArray, bool diagonalAllowed)
        {
            this.currentField = theArray;
            this.diagonal = diagonalAllowed;
            this.astarSolver = new AStarSolver<GameField>(diagonalAllowed, AStarHeuristicType.EXPERIMENTAL_SEARCH, this, theArray.GetUpperBound(1) + 1, theArray.GetUpperBound(0) + 1);

        }

		public void updateLocation(int x, int y, byte value)
		{
			this.newEntries.Enqueue(new GametileUpdate(x, y, value));
		}
		public List<PointField> doUpdate(bool oneloop = false)
		{
			List<PointField> list = new List<PointField>();
			while (this.newEntries.Count > 0)
			{
				this.currentlyChecking = this.newEntries.Dequeue();
				List<Point> connectedItems = this.getConnectedItems(this.currentlyChecking);
				if (connectedItems.Count > 1)
				{
					List<LinkedList<AStarSolver<GameField>.PathNode>> list2 = this.handleListOfConnectedPoints(connectedItems, this.currentlyChecking);
					foreach (LinkedList<AStarSolver<GameField>.PathNode> current in list2)
					{
						if (current.Count >= 4)
						{
							PointField pointField = this.findClosed(current);
							if (pointField != null)
							{
								list.Add(pointField);
							}
						}
					}
				}
				this.currentField[this.currentlyChecking.y, this.currentlyChecking.x] = this.currentlyChecking.value;
			}
			return list;
		}
		private PointField findClosed(LinkedList<AStarSolver<GameField>.PathNode> nodeList)
		{
			PointField pointField = new PointField(this.currentlyChecking.value);
			int num = 2147483647;
			int num2 = -2147483648;
			int num3 = 2147483647;
			int num4 = -2147483648;
			foreach (AStarSolver<GameField>.PathNode current in nodeList)
			{
				if (current.X < num)
				{
					num = current.X;
				}
				if (current.X > num2)
				{
					num2 = current.X;
				}
				if (current.Y < num3)
				{
					num3 = current.Y;
				}
				if (current.Y > num4)
				{
                    num4 = current.Y;
				}
			}
			checked
			{
				int x = (int)Math.Ceiling((double)((float)(num2 - num) / 2f)) + num;
				int y = (int)Math.Ceiling((double)((float)(num4 - num3) / 2f)) + num3;
				List<Point> list = new List<Point>();
				List<Point> list2 = new List<Point>();
				list2.Add(new Point(this.currentlyChecking.x, this.currentlyChecking.y));
				list.Add(new Point(x, y));
				while (list.Count > 0)
				{
					Point point = list[0];
					int x2 = point.X;
					int y2 = point.Y;
					if (x2 < num)
					{
						return null;
					}
					if (x2 > num2)
					{
						return null;
					}
					if (y2 < num3)
					{
						return null;
					}
					if (y2 > num4)
					{
						return null;
					}
					if (this[y2 - 1, x2] && this.currentField[y2 - 1, x2] == 0)
					{
						Point item = new Point(x2, y2 - 1);
						if (!list.Contains(item) && !list2.Contains(item))
						{
							list.Add(item);
						}
					}
					if (this[y2 + 1, x2] && this.currentField[y2 + 1, x2] == 0)
					{
						Point item = new Point(x2, y2 + 1);
						if (!list.Contains(item) && !list2.Contains(item))
						{
							list.Add(item);
						}
					}
					if (this[y2, x2 - 1] && this.currentField[y2, x2 - 1] == 0)
					{
						Point item = new Point(x2 - 1, y2);
						if (!list.Contains(item) && !list2.Contains(item))
						{
							list.Add(item);
						}
					}
					if (this[y2, x2 + 1] && this.currentField[y2, x2 + 1] == 0)
					{
						Point item = new Point(x2 + 1, y2);
						if (!list.Contains(item) && !list2.Contains(item))
						{
							list.Add(item);
						}
					}
					if (this.getValue(point) == 0)
					{
						pointField.add(point);
					}
					list2.Add(point);
					list.RemoveAt(0);
				}
				return pointField;
			}
		}
		private List<LinkedList<AStarSolver<GameField>.PathNode>> handleListOfConnectedPoints(List<Point> pointList, GametileUpdate update)
		{
			List<LinkedList<AStarSolver<GameField>.PathNode>> list = new List<LinkedList<AStarSolver<GameField>.PathNode>>();
			int num = 0;
			checked
			{
				foreach (Point current in pointList)
				{
					num++;
					if (num == pointList.Count / 2 + 1)
					{
						return list;
					}
					foreach (Point current2 in pointList)
					{
						if (!(current == current2))
						{
							LinkedList<AStarSolver<GameField>.PathNode> linkedList = this.astarSolver.Search(current2, current);
							if (linkedList != null)
							{
								list.Add(linkedList);
							}
						}
					}
				}
				return list;
			}
		}
		private List<Point> getConnectedItems(GametileUpdate update)
		{
			List<Point> list = new List<Point>();
			int x = update.x;
			int y = update.y;
			checked
			{
				if (this.diagonal)
				{
					if (this[y - 1, x - 1] && this.currentField[y - 1, x - 1] == update.value)
					{
						list.Add(new Point(x - 1, y - 1));
					}
					if (this[y - 1, x + 1] && this.currentField[y - 1, x + 1] == update.value)
					{
						list.Add(new Point(x + 1, y - 1));
					}
					if (this[y + 1, x - 1] && this.currentField[y + 1, x - 1] == update.value)
					{
						list.Add(new Point(x - 1, y + 1));
					}
					if (this[y + 1, x + 1] && this.currentField[y + 1, x + 1] == update.value)
					{
						list.Add(new Point(x + 1, y + 1));
					}
				}
				if (this[y - 1, x] && this.currentField[y - 1, x] == update.value)
				{
					list.Add(new Point(x, y - 1));
				}
				if (this[y + 1, x] && this.currentField[y + 1, x] == update.value)
				{
					list.Add(new Point(x, y + 1));
				}
				if (this[y, x - 1] && this.currentField[y, x - 1] == update.value)
				{
					list.Add(new Point(x - 1, y));
				}
				if (this[y, x + 1] && this.currentField[y, x + 1] == update.value)
				{
					list.Add(new Point(x + 1, y));
				}
				return list;
			}
		}
		private void setValue(int x, int y, byte value)
		{
			if (this[y, x])
			{
				this.currentField[y, x] = value;
			}
		}
		public byte getValue(int x, int y)
		{
			if (this[y, x])
			{
				return this.currentField[y, x];
			}
			return 0;
		}
		public byte getValue(Point p)
		{
			if (this[p.Y, p.X])
			{
				return this.currentField[p.Y, p.X];
			}
			return 0;
		}
		public bool IsBlocked(int x, int y, bool lastTile)
		{
			return (this.currentlyChecking.x == x && this.currentlyChecking.y == y) || this.getValue(x, y) != this.currentlyChecking.value;
		}
		public void destroy()
		{
			this.currentField = null;
		}
	}
}
