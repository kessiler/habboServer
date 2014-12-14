using System;
using System.Collections.Generic;
using System.Drawing;
namespace enclosuretest
{
	public class PointField
	{
		private List<Point> PointList;
		private static readonly Point badPoint = new Point(-1, -1);
		private Point mostLeft = PointField.badPoint;
		private Point mostTop = PointField.badPoint;
		private Point mostRight = PointField.badPoint;
		private Point mostDown = PointField.badPoint;
		public byte forValue
		{
			get;
			private set;
		}
		public PointField(byte forValue)
		{
			this.PointList = new List<Point>();
			this.forValue = forValue;
		}
		public List<Point> getPoints()
		{
			return this.PointList;
		}
		public void add(Point p)
		{
			if (this.mostLeft == PointField.badPoint)
			{
				this.mostLeft = p;
			}
			if (this.mostRight == PointField.badPoint)
			{
				this.mostRight = p;
			}
			if (this.mostTop == PointField.badPoint)
			{
				this.mostTop = p;
			}
			if (this.mostDown == PointField.badPoint)
			{
				this.mostDown = p;
			}
			if (p.X < this.mostLeft.X)
			{
				this.mostLeft = p;
			}
			if (p.X > this.mostRight.X)
			{
				this.mostRight = p;
			}
			if (p.Y > this.mostTop.Y)
			{
				this.mostTop = p;
			}
			if (p.Y < this.mostDown.Y)
			{
				this.mostDown = p;
			}
			this.PointList.Add(p);
		}
	}
}
