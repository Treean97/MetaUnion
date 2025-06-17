using System.Collections.Generic;
using Photon.Realtime;

public static class CachedRoomList
{
    private static List<RoomInfo> _roomList = new List<RoomInfo>();

    public static void SetRoomList(List<RoomInfo> roomList)
    {
        _roomList = roomList;
    }

    public static List<RoomInfo> GetRoomList()
    {
        return _roomList;
    }
}
