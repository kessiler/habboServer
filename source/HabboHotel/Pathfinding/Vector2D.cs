using System;
namespace Cyber.HabboHotel.PathFinding
{
	internal class Vector2D
	{
		private int x;
		private int y;
		public static Vector2D Zero = new Vector2D(0, 0);
		public int X
		{
			get
			{
				return this.x;
			}
			set
			{
				this.x = value;
			}
		}
		public int Y
		{
			get
			{
				return this.y;
			}
			set
			{
				this.y = value;
			}
		}
		public Vector2D()
		{
		}
		public Vector2D(int x, int y)
		{
			this.x = x;
			this.y = y;
		}
		public int GetDistanceSquared(Vector2D Point)
		{
			checked
			{
				int num = this.X - Point.X;
				int num2 = this.Y - Point.Y;
				return num * num + num2 * num2;
			}
		}
		public override bool Equals(object obj)
		{
			if (obj is Vector2D)
			{
				Vector2D vector2D = (Vector2D)obj;
				return vector2D.X == this.X && vector2D.Y == this.Y;
			}
			return false;
		}
		public override int GetHashCode()
		{
			return (this.X + " " + this.Y).GetHashCode();
		}
		public override string ToString()
		{
			return this.X + ", " + this.Y;
		}
		public static Vector2D operator +(Vector2D One, Vector2D Two)
		{
			return checked(new Vector2D(One.X + Two.X, One.Y + Two.Y));
		}
		public static Vector2D operator -(Vector2D One, Vector2D Two)
		{
			return checked(new Vector2D(One.X - Two.X, One.Y - Two.Y));
		}
	}
}
