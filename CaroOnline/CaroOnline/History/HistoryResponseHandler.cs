namespace CaroOnline.History
{
    public class HistoryResponseHandler
    {
        private readonly HistoryManagerClient historyManager;

        public HistoryResponseHandler(
            HistoryManagerClient historyManager)
        {
            this.historyManager = historyManager;
        }

        public void Handle(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            if (!message.StartsWith("HISTORY"))
                return;

            historyManager.HandleServerMessage(message);
        }
    }
}