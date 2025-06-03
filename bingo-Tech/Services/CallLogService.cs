using bingo_Tech.IServices;
using bingo_Tech.Models;
using Microsoft.EntityFrameworkCore;

namespace bingo_Tech.Services
{
    public class CallLogService : ICallLogService
    {
        private readonly CallLogContext _context;

        public CallLogService(CallLogContext context)
        {
            _context = context;
        }

        public async Task<int> StartCallAsync(string callerName, string receiverName)
        {
            var callLog = new CallLog
            {
                CallerName = callerName,
                ReceiverName = receiverName,
                StartTime = DateTime.UtcNow
            };

            _context.CallLogs.Add(callLog);
            await _context.SaveChangesAsync();
            return callLog.Id;
        }

        public async Task EndCallAsync(int callId)
        {
            var callLog = await _context.CallLogs.FindAsync(callId);
            if (callLog != null && callLog.EndTime == null)
            {
                callLog.EndTime = DateTime.UtcNow;
                callLog.Duration = (int)(callLog.EndTime.Value - callLog.StartTime).TotalSeconds;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<CallLog>> GetCallHistoryAsync()
        {
            return await _context.CallLogs
                .OrderByDescending(c => c.StartTime)
                .Take(50)
                .ToListAsync();
        }
    }
}
