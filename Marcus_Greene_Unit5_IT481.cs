using System.Diagnostics;

namespace Classwork
{
    public class DressingRooms(int rooms = 3)
    {
        private SemaphoreSlim semaphore = new(rooms, rooms);
        private object lockObj = new object();
        private int completeWaitTime = 0;
        private int completeUsageTime = 0;
        private int amountOfCustomers = 0;

        public async Task RequestRoom(int customerId, int itemCount, int maxItemsPerCustomer = 6)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            
            Console.WriteLine($"Customer {customerId} is waiting to try on {itemCount} items...");

            await semaphore.WaitAsync();
            try
            {
                stopwatch.Stop();
                var waitTime = (int)stopwatch.ElapsedMilliseconds / 1000;
                
                lock (lockObj)
                {
                    completeWaitTime += waitTime;
                    amountOfCustomers++;
                }

                Console.WriteLine($"Customer {customerId} entered a dressing room after waiting {waitTime} seconds");
                
                var usageTime = 0;
                var rand = new Random();
                for (var i = 0; i < itemCount; i++)
                {
                    usageTime += rand.Next(1, 4);
                }

                lock (lockObj)
                {
                    completeUsageTime += usageTime;
                }

                Console.WriteLine($"Customer {customerId} will use the room for {usageTime} minutes");
                await Task.Delay(usageTime * 1000);
            }
            finally
            {
                semaphore.Release();
                Console.WriteLine($"Customer {customerId} has left the dressing room");
            }
        }

        public void PrintStats()
        {
            lock (lockObj)
            {
                Console.WriteLine("\n=== Simulation Statistics ===");
                Console.WriteLine($"Rooms available: {rooms}");
                Console.WriteLine($"Customers processed: {amountOfCustomers}");
                if (amountOfCustomers <= 0) return;
                Console.WriteLine($"Average wait time: {completeWaitTime / amountOfCustomers} seconds");
                Console.WriteLine($"Average usage time: {completeUsageTime / amountOfCustomers} minutes");
            }
        }
    }

    public class Scenario(int rooms, int customers, int maxItems = 6)
    {
        public async Task GenerateScenario()
        {
            var dressingRooms = new DressingRooms(rooms);
            var tasks = new List<Task>();
            var rand = new Random();
            var stopwatch = new Stopwatch();

            Console.WriteLine($"\nStarting Scenario: {rooms} rooms, {customers} customers");
            stopwatch.Start();

            for (var i = 0; i < customers; i++)
            {
                var customerId = i + 1;
                var itemCount = maxItems == 0 ? rand.Next(1, 7) : 
                    Math.Min(maxItems, rand.Next(1, 21));
                
                tasks.Add(Task.Run(async () => 
                {
                    await dressingRooms.RequestRoom(customerId, itemCount, maxItems);
                }));
                
                await Task.Delay(rand.Next(500, 2000));
            }

            await Task.WhenAll(tasks);
            stopwatch.Stop();

            Console.WriteLine($"\nScenario completed in {stopwatch.Elapsed.TotalMinutes:0.00} minutes");
            dressingRooms.PrintStats();
        }
    }

    internal static class Program
    {
        private static async Task Main()
        {
            Console.WriteLine("Dressing Room Simulation\n");
            
            Console.WriteLine("Scenario 1: Minimal Traffic");
            var scenario1 = new Scenario(rooms: 3, customers: 10);
            await scenario1.GenerateScenario();
            
            Console.WriteLine("\nScenario 2: Moderate Traffic");
            var scenario2 = new Scenario(rooms: 3, customers: 30);
            await scenario2.GenerateScenario();
            
            Console.WriteLine("\nScenario 3: Increased Capacity");
            var scenario3 = new Scenario(rooms: 5, customers: 30);
            await scenario3.GenerateScenario();

            Console.WriteLine("\nSimulation complete.");
            Console.ReadKey();
        }
    }
}