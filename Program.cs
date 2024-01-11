using System;
using System.Threading.Tasks;
using Argali.Tasks;

namespace Argali;

public class Program
{
    public static async Task Main()
    {
        do
        {
            while (!Console.KeyAvailable)
            {
                await FetchProfile.GetProfileConnections();
                break;
            }
        } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
    }
}