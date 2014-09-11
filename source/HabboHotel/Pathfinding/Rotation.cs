using System;
namespace Mercury.HabboHotel.Pathfinding
{
	internal static class Rotation
	{
		internal static int Calculate(int X1, int Y1, int X2, int Y2)
		{
			int result = 0;
			if (X1 > X2 && Y1 > Y2)
			{
				result = 7;
			}
			else
			{
				if (X1 < X2 && Y1 < Y2)
				{
					result = 3;
				}
				else
				{
					if (X1 > X2 && Y1 < Y2)
					{
						result = 5;
					}
					else
					{
						if (X1 < X2 && Y1 > Y2)
						{
							result = 1;
						}
						else
						{
							if (X1 > X2)
							{
								result = 6;
							}
							else
							{
								if (X1 < X2)
								{
									result = 2;
								}
								else
								{
									if (Y1 < Y2)
									{
										result = 4;
									}
									else
									{
										if (Y1 > Y2)
										{
											result = 0;
										}
									}
								}
							}
						}
					}
				}
			}
			return result;
		}
		internal static int Calculate(int X1, int Y1, int X2, int Y2, bool moonwalk)
		{
			int num = Rotation.Calculate(X1, Y1, X2, Y2);
			if (!moonwalk)
			{
				return num;
			}
			return Rotation.RotationIverse(num);
		}
		internal static int RotationIverse(int rot)
		{
			checked
			{
				if (rot > 3)
				{
					rot -= 4;
				}
				else
				{
					rot += 4;
				}
				return rot;
			}
		}
	}
}
