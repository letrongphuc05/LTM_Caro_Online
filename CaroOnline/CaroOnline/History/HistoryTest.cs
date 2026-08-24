using System;

namespace CaroOnline.History
{
    public class HistoryTest
    {
        public void TestGetAll()
        {
            HistoryNetworkBridge bridge =
                new HistoryNetworkBridge();

            bridge.GetAllHistory();

            Console.WriteLine(
                "Da gui yeu cau HISTORY_ALL toi Server."
            );
        }
    }
}