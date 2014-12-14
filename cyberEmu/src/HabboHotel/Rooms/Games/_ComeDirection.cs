using System;
using System.Drawing;
namespace Cyber.HabboHotel.Rooms.Games
{
	public class _ComeDirection
	{
		internal static ComeDirection GetComeDirection(Point user, Point ball)
		{
			checked
			{
				ComeDirection result;
				try
				{
					if (user.X == ball.X && user.Y - 1 == ball.Y)
					{
						result = ComeDirection.DOWN;
					}
					else
					{
						if (user.X + 1 == ball.X && user.Y - 1 == ball.Y)
						{
							result = ComeDirection.DOWN_LEFT;
						}
						else
						{
							if (user.X + 1 == ball.X && user.Y == ball.Y)
							{
								result = ComeDirection.LEFT;
							}
							else
							{
								if (user.X + 1 == ball.X && user.Y + 1 == ball.Y)
								{
									result = ComeDirection.UP_LEFT;
								}
								else
								{
									if (user.X == ball.X && user.Y + 1 == ball.Y)
									{
										result = ComeDirection.UP;
									}
									else
									{
										if (user.X - 1 == ball.X && user.Y + 1 == ball.Y)
										{
											result = ComeDirection.UP_RIGHT;
										}
										else
										{
											if (user.X - 1 == ball.X && user.Y == ball.Y)
											{
												result = ComeDirection.RIGHT;
											}
											else
											{
												if (user.X - 1 == ball.X && user.Y - 1 == ball.Y)
												{
													result = ComeDirection.DOWN_RIGHT;
												}
												else
												{
													result = ComeDirection.NULL;
												}
											}
										}
									}
								}
							}
						}
					}
				}
				catch
				{
					result = ComeDirection.NULL;
				}
				return result;
			}
		}
		internal static ComeDirection GetInverseDirectionEasy(ComeDirection comeWith)
		{
			ComeDirection result;
			try
			{
				if (comeWith == ComeDirection.UP)
				{
					result = ComeDirection.DOWN;
				}
				else
				{
					if (comeWith == ComeDirection.UP_RIGHT)
					{
						result = ComeDirection.DOWN_LEFT;
					}
					else
					{
						if (comeWith == ComeDirection.RIGHT)
						{
							result = ComeDirection.LEFT;
						}
						else
						{
							if (comeWith == ComeDirection.DOWN_RIGHT)
							{
								result = ComeDirection.UP_LEFT;
							}
							else
							{
								if (comeWith == ComeDirection.DOWN)
								{
									result = ComeDirection.UP;
								}
								else
								{
									if (comeWith == ComeDirection.DOWN_LEFT)
									{
										result = ComeDirection.UP_RIGHT;
									}
									else
									{
										if (comeWith == ComeDirection.LEFT)
										{
											result = ComeDirection.RIGHT;
										}
										else
										{
											if (comeWith == ComeDirection.UP_LEFT)
											{
												result = ComeDirection.DOWN_RIGHT;
											}
											else
											{
												result = ComeDirection.NULL;
											}
										}
									}
								}
							}
						}
					}
				}
			}
			catch
			{
				result = ComeDirection.NULL;
			}
			return result;
		}
		internal static void GetNewCoords(ComeDirection comeWith, ref int newX, ref int newY)
		{
			checked
			{
				try
				{
					if (comeWith == ComeDirection.UP)
					{
						newY++;
					}
					else
					{
						if (comeWith == ComeDirection.UP_RIGHT)
						{
							newX--;
							newY++;
						}
						else
						{
							if (comeWith == ComeDirection.RIGHT)
							{
								newX--;
							}
							else
							{
								if (comeWith == ComeDirection.DOWN_RIGHT)
								{
									newX--;
									newY--;
								}
								else
								{
									if (comeWith == ComeDirection.DOWN)
									{
										newY--;
									}
									else
									{
										if (comeWith == ComeDirection.DOWN_LEFT)
										{
											newX++;
											newY--;
										}
										else
										{
											if (comeWith == ComeDirection.LEFT)
											{
												newX++;
											}
											else
											{
												if (comeWith == ComeDirection.UP_LEFT)
												{
													newX++;
													newY++;
												}
											}
										}
									}
								}
							}
						}
					}
				}
				catch
				{
				}
			}
		}
		internal static ComeDirection InverseDirections(Room room, ComeDirection comeWith, int x, int y)
		{
			checked
			{
				ComeDirection result;
				try
				{
					if (comeWith == ComeDirection.UP)
					{
						result = ComeDirection.DOWN;
					}
					else
					{
						if (comeWith == ComeDirection.UP_RIGHT)
						{
							if (room.GetGameMap().StaticModel.SqState[x, y] == SquareState.BLOCKED)
							{
								if (room.GetGameMap().StaticModel.SqState[x + 1, y] == SquareState.BLOCKED)
								{
									result = ComeDirection.DOWN_RIGHT;
								}
								else
								{
									result = ComeDirection.UP_LEFT;
								}
							}
							else
							{
								result = ComeDirection.DOWN_RIGHT;
							}
						}
						else
						{
							if (comeWith == ComeDirection.RIGHT)
							{
								result = ComeDirection.LEFT;
							}
							else
							{
								if (comeWith == ComeDirection.DOWN_RIGHT)
								{
									if (room.GetGameMap().StaticModel.SqState[x, y] == SquareState.BLOCKED)
									{
										if (room.GetGameMap().StaticModel.SqState[x + 1, y] == SquareState.BLOCKED)
										{
											result = ComeDirection.UP_RIGHT;
										}
										else
										{
											result = ComeDirection.DOWN_LEFT;
										}
									}
									else
									{
										result = ComeDirection.UP_RIGHT;
									}
								}
								else
								{
									if (comeWith == ComeDirection.DOWN)
									{
										result = ComeDirection.UP;
									}
									else
									{
										if (comeWith == ComeDirection.DOWN_LEFT)
										{
											if (room.GetGameMap().Model.MapSizeX - 1 <= x)
											{
												result = ComeDirection.DOWN_RIGHT;
											}
											else
											{
												result = ComeDirection.UP_LEFT;
											}
										}
										else
										{
											if (comeWith == ComeDirection.LEFT)
											{
												result = ComeDirection.RIGHT;
											}
											else
											{
												if (comeWith == ComeDirection.UP_LEFT)
												{
													if (room.GetGameMap().Model.MapSizeX - 1 <= x)
													{
														result = ComeDirection.UP_RIGHT;
													}
													else
													{
														result = ComeDirection.DOWN_LEFT;
													}
												}
												else
												{
													result = ComeDirection.NULL;
												}
											}
										}
									}
								}
							}
						}
					}
				}
				catch
				{
					result = ComeDirection.NULL;
				}
				return result;
			}
		}
	}
}
