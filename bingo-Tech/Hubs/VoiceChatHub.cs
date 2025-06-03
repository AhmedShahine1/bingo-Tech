using bingo_Tech.Models;
using bingo_Tech.IServices;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace bingo_Tech.Hubs
{
    public class VoiceChatHub : Hub
    {
        private static readonly ConcurrentDictionary<string, UserInfo> ConnectedUsers = new();
        private static readonly ConcurrentDictionary<string, string> UserRooms = new();
        private static readonly ConcurrentDictionary<string, int> ActiveCalls = new();
        private readonly ICallLogService _callLogService;

        public VoiceChatHub(ICallLogService callLogService)
        {
            _callLogService = callLogService;
        }

        public async Task JoinRoom(string username, string roomName = "General")
        {
            var userId = Context.ConnectionId;

            if (UserRooms.TryGetValue(userId, out var oldRoom))
            {
                await Groups.RemoveFromGroupAsync(userId, oldRoom);
                if (ConnectedUsers.TryGetValue(userId, out var oldUserInfo))
                {
                    await Clients.Group(oldRoom).SendAsync("UserLeft", oldUserInfo.Username, userId);
                }
            }

            ConnectedUsers[userId] = new UserInfo { Username = username, Room = roomName };
            UserRooms[userId] = roomName;
            await Groups.AddToGroupAsync(userId, roomName);

            await Clients.Group(roomName).SendAsync("UserJoined", username, userId);

            var usersInRoom = ConnectedUsers
                .Where(kvp => kvp.Value.Room == roomName && kvp.Key != userId)
                .Select(kvp => new { Username = kvp.Value.Username, ConnectionId = kvp.Key })
                .ToList();

            await Clients.Caller.SendAsync("UpdateUsersList", usersInRoom);
        }

        public async Task SendSignalingMessage(string targetUserId, string type, object data)
        {
            var senderInfo = ConnectedUsers.GetValueOrDefault(Context.ConnectionId);
            if (senderInfo != null)
            {
                await Clients.Client(targetUserId).SendAsync("ReceiveSignalingMessage",
                    Context.ConnectionId, senderInfo.Username, type, data);
            }
        }

        public async Task StartCall(string targetUserId)
        {
            var callerInfo = ConnectedUsers.GetValueOrDefault(Context.ConnectionId);
            var receiverInfo = ConnectedUsers.GetValueOrDefault(targetUserId);

            if (callerInfo != null && receiverInfo != null)
            {
                var callId = await _callLogService.StartCallAsync(callerInfo.Username, receiverInfo.Username);
                ActiveCalls[Context.ConnectionId] = callId;

                await Clients.Client(targetUserId).SendAsync("IncomingCall",
                    Context.ConnectionId, callerInfo.Username);
            }
        }

        public async Task AcceptCall(string callerId)
        {
            var receiverInfo = ConnectedUsers.GetValueOrDefault(Context.ConnectionId);
            if (receiverInfo != null)
            {
                await Clients.Client(callerId).SendAsync("CallAccepted",
                    Context.ConnectionId, receiverInfo.Username);
            }
        }

        public async Task RejectCall(string callerId)
        {
            if (ActiveCalls.TryRemove(callerId, out var callId))
            {
                await _callLogService.EndCallAsync(callId);
            }

            await Clients.Client(callerId).SendAsync("CallRejected", Context.ConnectionId);
        }

        public async Task EndCall(string otherUserId)
        {
            if (ActiveCalls.TryRemove(Context.ConnectionId, out var callId))
            {
                await _callLogService.EndCallAsync(callId);
            }
            if (ActiveCalls.TryRemove(otherUserId, out var otherCallId))
            {
                await _callLogService.EndCallAsync(otherCallId);
            }

            await Clients.Client(otherUserId).SendAsync("CallEnded", Context.ConnectionId);
        }
        public async Task LeaveRoom()
        {
            var userId = Context.ConnectionId;

            try
            {
                // End any active calls
                if (ActiveCalls.TryRemove(userId, out var callId))
                {
                    await _callLogService.EndCallAsync(callId);
                }

                // Remove from room and notify others
                if (ConnectedUsers.TryGetValue(userId, out var userInfo) &&
                    UserRooms.TryGetValue(userId, out var room))
                {
                    await Groups.RemoveFromGroupAsync(userId, room);
                    await Clients.Group(room).SendAsync("UserLeft", userInfo.Username, userId);
                }

                // Clean up user data but keep connection alive
                ConnectedUsers.TryRemove(userId, out _);
                UserRooms.TryRemove(userId, out _);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error leaving room: {ex.Message}");
                throw;
            }
        }
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.ConnectionId;

            if (ActiveCalls.TryRemove(userId, out var callId))
            {
                await _callLogService.EndCallAsync(callId);
            }

            if (ConnectedUsers.TryRemove(userId, out var userInfo))
            {
                if (UserRooms.TryRemove(userId, out var room))
                {
                    await Clients.Group(room).SendAsync("UserLeft", userInfo.Username, userId);
                }
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}