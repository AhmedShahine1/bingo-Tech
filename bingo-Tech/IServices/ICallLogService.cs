using bingo_Tech.Models;

namespace bingo_Tech.IServices
{
    public interface ICallLogService
    {
        Task<int> StartCallAsync(string callerName, string receiverName);
        Task EndCallAsync(int callId);
        Task<List<CallLog>> GetCallHistoryAsync();
    }
}
