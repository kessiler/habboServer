using System;
namespace Cyber.HabboHotel.Rooms
{
	internal class RoomModel
	{
		internal int DoorX;
		internal int DoorY;
		internal double DoorZ;
		internal int DoorOrientation;
		internal string Heightmap;
		internal SquareState[,] SqState;
		internal int[,] SqFloorHeight;
		internal byte[,] SqSeatRot;
		internal char[,] SqChar;
		internal byte[,] mRoomModelfx;
		internal int MapSizeX;
		internal int MapSizeY;
		internal string StaticFurniMap;
		internal bool ClubOnly;
		internal bool gotPublicPool;
		internal RoomModel(int DoorX, int DoorY, double DoorZ, int DoorOrientation, string Heightmap, string StaticFurniMap, bool ClubOnly, string Poolmap)
		{
			checked
			{
				try
				{
					this.DoorX = DoorX;
					this.DoorY = DoorY;
					this.DoorZ = DoorZ;
					this.DoorOrientation = DoorOrientation;
					this.Heightmap = Heightmap.ToLower();
					this.StaticFurniMap = StaticFurniMap;
					this.gotPublicPool = !string.IsNullOrEmpty(Poolmap);
					Poolmap.Split(new char[]
					{
						Convert.ToChar(13)
					});
                    Heightmap = Heightmap.Replace(Convert.ToChar(10) + "", "");
                    string[] array = Heightmap.Split(Convert.ToChar(13));
					this.MapSizeX = array[0].Length;
					this.MapSizeY = array.Length;
					this.ClubOnly = ClubOnly;
					string text = "abcdefghijklmnopqrstuvw";
					this.SqState = new SquareState[this.MapSizeX, this.MapSizeY];
					this.SqFloorHeight = new int[this.MapSizeX, this.MapSizeY];
					this.SqSeatRot = new byte[this.MapSizeX, this.MapSizeY];
					this.SqChar = new char[this.MapSizeX, this.MapSizeY];
					if (this.gotPublicPool)
					{
						this.mRoomModelfx = new byte[this.MapSizeX, this.MapSizeY];
					}
					for (int Y = 0; Y < this.MapSizeY; Y++)
					{
						string text2 = array[Y].Replace(Convert.ToChar(13) + "", "").Replace(Convert.ToChar(10) + "", "");

						for (int X = 0; X < this.MapSizeX; X++)
						{
                            char c = 'x';
                            try
                            {
                                c = text2[X];
                            }
                            catch (Exception) { }
							if (X == DoorX && Y == DoorY)
							{
								this.SqFloorHeight[X, Y] = (int)this.DoorZ;
								this.SqState[X, Y] = SquareState.OPEN;
								if (this.SqFloorHeight[X, Y] > 9)
								{
									this.SqChar[X, Y] = text[(this.SqFloorHeight[X, Y] - 10)];
								}
								else
								{
									this.SqChar[X, Y] = char.Parse(this.DoorZ.ToString());
								}
							}
							else
							{
								if (c.Equals('x'))
								{
									this.SqFloorHeight[X, Y] = -1;
									this.SqState[X, Y] = SquareState.BLOCKED;
									this.SqChar[X, Y] = c;
								}
								else
								{
									if (char.IsDigit(c))
									{
										this.SqFloorHeight[X, Y] = int.Parse(c.ToString());
										this.SqState[X, Y] = SquareState.OPEN;
										this.SqChar[X, Y] = c;
									}
									else
									{
										if (char.IsLetter(c))
										{
											this.SqFloorHeight[X, Y] = text.IndexOf(char.ToLower(c)) + 10;
											this.SqState[X, Y] = SquareState.OPEN;
											this.SqChar[X, Y] = c;
										}
									}
								}
							}
						}
					}
				}
				catch (Exception)
				{
				}
			}
		}
	}
}
