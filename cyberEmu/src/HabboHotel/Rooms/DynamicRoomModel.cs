using Cyber.Messages;
using Cyber.Messages.Headers;
using System;
using System.Text;
namespace Cyber.HabboHotel.Rooms
{
	internal class DynamicRoomModel
	{
		private RoomModel staticModel;
		internal int DoorX;
		internal int DoorY;
		internal double DoorZ;
		internal int DoorOrientation;
		internal string Heightmap;
		internal SquareState[,] SqState;
		internal int[,] SqFloorHeight;
		internal byte[,] SqSeatRot;
		internal char[,] SqChar;
		internal int MapSizeX;
		internal int MapSizeY;
		internal bool ClubOnly;
		private ServerMessage SerializedHeightmap;
		internal bool HeightmapSerialized;
        private Room mRoom;

		internal DynamicRoomModel(RoomModel pModel, Room room)
		{
			this.staticModel = pModel;
			this.DoorX = this.staticModel.DoorX;
			this.DoorY = this.staticModel.DoorY;
			this.DoorZ = this.staticModel.DoorZ;
			this.DoorOrientation = this.staticModel.DoorOrientation;
			this.Heightmap = this.staticModel.Heightmap;
			this.MapSizeX = this.staticModel.MapSizeX;
			this.MapSizeY = this.staticModel.MapSizeY;
			this.ClubOnly = this.staticModel.ClubOnly;
            this.mRoom = room;
			this.Generate();
		}

		internal void Generate()
		{
			this.SqState = new SquareState[this.MapSizeX, this.MapSizeY];
			this.SqFloorHeight = new int[this.MapSizeX, this.MapSizeY];
			this.SqSeatRot = new byte[this.MapSizeX, this.MapSizeY];
			this.SqChar = new char[this.MapSizeX, this.MapSizeY];
			checked
			{
				for (int i = 0; i < this.MapSizeY; i++)
				{
					for (int j = 0; j < this.MapSizeX; j++)
					{
						if (j > this.staticModel.MapSizeX - 1 || i > this.staticModel.MapSizeY - 1)
						{
							this.SqState[j, i] = SquareState.BLOCKED;
						}
						else
						{
							this.SqState[j, i] = this.staticModel.SqState[j, i];
							this.SqFloorHeight[j, i] = this.staticModel.SqFloorHeight[j, i];
							this.SqSeatRot[j, i] = this.staticModel.SqSeatRot[j, i];
							this.SqChar[j, i] = this.staticModel.SqChar[j, i];
						}
					}
				}
				this.HeightmapSerialized = false;
			}
		}
		internal void refreshArrays()
		{
			checked
			{
				SquareState[,] array = new SquareState[this.MapSizeX + 1, this.MapSizeY + 1];
				int[,] array2 = new int[this.MapSizeX + 1, this.MapSizeY + 1];
				byte[,] array3 = new byte[this.MapSizeX + 1, this.MapSizeY + 1];
				for (int i = 0; i < this.MapSizeY; i++)
				{
					for (int j = 0; j < this.MapSizeX; j++)
					{
						if (j > this.staticModel.MapSizeX - 1 || i > this.staticModel.MapSizeY - 1)
						{
							array[j, i] = SquareState.BLOCKED;
						}
						else
						{
							array[j, i] = this.SqState[j, i];
							array2[j, i] = this.SqFloorHeight[j, i];
							array3[j, i] = this.SqSeatRot[j, i];
						}
					}
				}
				this.SqState = array;
				this.SqFloorHeight = array2;
				this.SqSeatRot = array3;
				this.HeightmapSerialized = false;
			}
		}
		internal void SetUpdateState()
		{
			this.HeightmapSerialized = false;
		}
		internal ServerMessage GetHeightmap()
		{
			if (!this.HeightmapSerialized)
			{
				this.SerializedHeightmap = this.SerializeHeightmap();
				this.HeightmapSerialized = true;
			}
			return this.SerializedHeightmap;
		}
		private ServerMessage SerializeHeightmap()
		{
			ServerMessage serverMessage = new ServerMessage(Outgoing.FloorMapMessageComposer);
			serverMessage.AppendBoolean(true);
			serverMessage.AppendInt32(mRoom.WallHeight);
			StringBuilder stringBuilder = new StringBuilder();
			checked
			{
				for (int i = 0; i < this.MapSizeY; i++)
				{
					for (int j = 0; j < this.MapSizeX; j++)
					{
                        try
                        {
                            stringBuilder.Append(this.SqChar[j, i].ToString());
                        }
                        catch (Exception)
                        {
                            stringBuilder.Append("0");
                        }
					}
					stringBuilder.Append(Convert.ToChar(13));
				}
				string s = stringBuilder.ToString();
				serverMessage.AppendString(s);
				return serverMessage;
			}
		}
		internal void AddX()
		{
			checked
			{
				this.MapSizeX++;
				this.refreshArrays();
			}
		}
		internal void OpenSquare(int x, int y, double z)
		{
			if (z > 9.0)
			{
				z = 9.0;
			}
			if (z < 0.0)
			{
				z = 0.0;
			}
			this.SqFloorHeight[x, y] = (int)checked((short)z);
			this.SqState[x, y] = SquareState.OPEN;
		}
		internal void AddY()
		{
			checked
			{
				this.MapSizeY++;
				this.refreshArrays();
			}
		}
		internal void SetMapsize(int x, int y)
		{
			this.MapSizeX = x;
			this.MapSizeY = y;
			this.refreshArrays();
		}
		internal void Destroy()
		{
			Array.Clear(this.SqState, 0, this.SqState.Length);
			Array.Clear(this.SqFloorHeight, 0, this.SqFloorHeight.Length);
			Array.Clear(this.SqSeatRot, 0, this.SqSeatRot.Length);
			this.staticModel = null;
			this.Heightmap = null;
			this.SqState = null;
			this.SqFloorHeight = null;
			this.SqSeatRot = null;
		}
	}
}
