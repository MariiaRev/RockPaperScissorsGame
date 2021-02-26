using System;
using System.Threading.Tasks;

namespace RockPaperScissorsGame.Client.Platforms.Base
{
    public abstract class BasePlatform : IBasePlatform
    {
        protected bool KeepProgramActive = true;
        protected bool ShouldSkipNextInstruction = false;
        protected string PlayerId;
        
        public virtual async Task StartAsync(string playerId)
        {
            KeepProgramActive = true;
            PlayerId = playerId;
            do
            {
                await PrintUserMenu();
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.Write("Enter command number: ");
                Console.ResetColor();
                string userInput = Console.ReadLine();

                if (ShouldSkipNextInstruction)
                {
                    ShouldSkipNextInstruction = false;
                    continue;
                }
                
                if (!KeepProgramActive)
                {
                    return;
                }
                
                if (string.IsNullOrEmpty(userInput))
                {
                    Console.WriteLine("Empty input. Try again\n");
                    continue;
                }

                if (int.TryParse(userInput, out int commandId) == false)
                {
                    Console.WriteLine("Unknown command. Try again\n");
                    continue;
                }

                bool isCommandValid = await ChooseCommandAsync(commandId);
                if (isCommandValid == false)
                {
                    Console.WriteLine("Unknown command. Try again\n");
                }
                
            } while (KeepProgramActive);  
        }

        protected abstract Task PrintUserMenu();
        protected abstract Task<bool> ChooseCommandAsync(int commandNumber);

    }
}