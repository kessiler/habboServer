using Cyber.HabboHotel.GameClients;
using Cyber.HabboHotel.Items;
using Cyber.Messages;
using Cyber.Messages.Headers;
using Cyber.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Cyber.HabboHotel.SoundMachine.Composers
{
	internal class JukeboxComposer
	{
		internal static ServerMessage Compose(GameClient Session)
		{
			return Session.GetHabbo().GetInventoryComponent().SerializeMusicDiscs();
		}

		internal static ServerMessage Compose(int PlaylistCapacity, List<SongInstance> Playlist)
		{
			ServerMessage serverMessage = new ServerMessage(Outgoing.JukeboxPlaylistMessageComposer);
			serverMessage.AppendInt32(PlaylistCapacity);
			serverMessage.AppendInt32(Playlist.Count);

			foreach (SongInstance current in Playlist)
			{
				serverMessage.AppendUInt(current.DiskItem.itemID);
				serverMessage.AppendUInt(current.SongData.Id);
			}
			return serverMessage;
		}
		internal static ServerMessage Compose(uint SongId, int PlaylistItemNumber, int SyncTimestampMs)
		{
			ServerMessage serverMessage = new ServerMessage(Outgoing.JukeboxNowPlayingMessageComposer);

			if (SongId == 0)
			{
				serverMessage.AppendInt32(-1);
				serverMessage.AppendInt32(-1);
				serverMessage.AppendInt32(-1);
				serverMessage.AppendInt32(-1);
				serverMessage.AppendInt32(0);
			}
			else
			{
				serverMessage.AppendUInt(SongId);
				serverMessage.AppendInt32(PlaylistItemNumber);
				serverMessage.AppendUInt(SongId);
				serverMessage.AppendInt32(0);
				serverMessage.AppendInt32(SyncTimestampMs);
			}
			return serverMessage;
		}
		public static ServerMessage Compose(List<SongData> Songs)
		{
			ServerMessage serverMessage = new ServerMessage(Outgoing.SongsMessageComposer);
			serverMessage.AppendInt32(Songs.Count);

			foreach (SongData current in Songs)
			{
				serverMessage.AppendUInt(current.Id);
				serverMessage.AppendString(current.Codename);
				serverMessage.AppendString(current.Name);
				serverMessage.AppendString(current.Data);
				serverMessage.AppendInt32(current.LengthMiliseconds);
				serverMessage.AppendString(current.Artist);
			}
			return serverMessage;
		}
		public static ServerMessage ComposePlayingComposer(uint SongId, int PlaylistItemNumber, int SyncTimestampMs)
		{
			ServerMessage serverMessage = new ServerMessage(Outgoing.JukeboxNowPlayingMessageComposer);
			if (SongId == 0)
			{
				serverMessage.AppendInt32(-1);
				serverMessage.AppendInt32(-1);
				serverMessage.AppendInt32(-1);
				serverMessage.AppendInt32(-1);
				serverMessage.AppendInt32(0);
			}
			else
			{
				serverMessage.AppendUInt(SongId);
				serverMessage.AppendInt32(PlaylistItemNumber);
				serverMessage.AppendUInt(SongId);
				serverMessage.AppendInt32(0);
				serverMessage.AppendInt32(SyncTimestampMs);
			}
			return serverMessage;
		}
		internal static ServerMessage SerializeSongInventory(HybridDictionary songs)
		{
			ServerMessage serverMessage = new ServerMessage(Outgoing.SongsLibraryMessageComposer);
			serverMessage.AppendInt32(songs.Count);
			foreach (UserItem userItem in songs.Values)
			{
                uint songID = (uint)TextHandling.Parse(userItem.ExtraData);
                serverMessage.AppendUInt(userItem.Id);
                serverMessage.AppendUInt(songID);
			}
			return serverMessage;
		}
	}
}
