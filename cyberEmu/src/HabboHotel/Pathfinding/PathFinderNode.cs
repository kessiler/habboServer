using System;
using System.Drawing;

namespace Cyber.HabboHotel.PathFinding
{
	internal class PathFinderNode : IComparable<PathFinderNode>
	{
        public Vector2D Position;
		public PathFinderNode Next;
		public int Cost = 2147483647;
		public bool InOpen;
		public bool InClosed;

        public PathFinderNode(Vector2D Position)
		{
			this.Position = Position;
		}
		public override bool Equals(object obj)
		{
			return obj is PathFinderNode && ((PathFinderNode)obj).Position.Equals(this.Position);
		}
		public bool Equals(PathFinderNode Breadcrumb)
		{
			return Breadcrumb.Position.Equals(this.Position);
		}
		public override int GetHashCode()
		{
			return this.Position.GetHashCode();
		}
		public int CompareTo(PathFinderNode Other)
		{
			return this.Cost.CompareTo(Other.Cost);
		}
	}
}
