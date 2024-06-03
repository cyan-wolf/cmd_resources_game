
using util;

public class Game 
{
    // To access singleton.
    public static Game Current => _current;

    // Initialized after the game starts.
    private Window? _window;

    private Input _prevInput = new("");

    // To store singeton.
    private static Game _current;

    public Game() 
    {
        // Store singleton.
        _current = this;
    }

    // Called when a game is started.
    public void Start() 
    {
        ShowStartMenu();
        CallUpdate();
    }

    // Gets input from the user to setup the game window.
    private void ShowStartMenu()
    {
        Console.WriteLine(
@"*******************************
* Command Line Resources Game *
*******************************

*******************************
*           Setup             *
*******************************");

        while (true)
        {
            Console.WriteLine("1) What will be the width and height of your game window? (Enter the width and height seperated by a space).");
            var strInput = HandleInput("Enter the width and height: ").Keys;

            (bool, int)[] dimensions = strInput
                .Split(" ")
                .Select(s => {
                    return (int.TryParse(s, out var res), res);
                }).ToArray();

            // Error handling.
            if (dimensions.Length != 2)
            {
                ColorUtils.ColorWriteLine("Error: invalid dimensions format", ConsoleColor.Red);
                continue;
            }
            else if (!(dimensions[0].Item1 && dimensions[1].Item1))
            {
                ColorUtils.ColorWriteLine("Error: invalid dimensions", ConsoleColor.Red);
                continue;
            }

            var windowDim = new Rect(dimensions[1].Item2, dimensions[0].Item2); // swap coordinates

            Console.WriteLine("2) Enter the number of domains that will be in the game: ");
            var inputSuccessful = int.TryParse(HandleInput("Enter the domain amount: ").Keys, out var domainAmt);

            // Error handling.
            if (!inputSuccessful || domainAmt < 0)
            {
                ColorUtils.ColorWriteLine("Error: invalid domain amount", ConsoleColor.Red);
                continue;
            }

            var domainStartingPositions = new List<Point>(domainAmt);

            Console.WriteLine("3) Enter the starting positions for each domain: ");

            var errorInGettingDomainPos = false;

            for (int i = 0; i < domainAmt; i++)
            {
                strInput = HandleInput($"Enter domain #{i + 1}'s X and Y starting position: ").Keys;
                (bool, int)[] domainPos = strInput
                    .Split(" ")
                    .Select(s => {
                        return (int.TryParse(s, out var res), res);
                    })
                    .ToArray();

                // Error handling.
                if (domainPos.Length != 2) 
                {
                    ColorUtils.ColorWriteLine("Error: invalid domain position format", ConsoleColor.Red);
                    errorInGettingDomainPos = true;
                    continue;
                }

                // Error handling.
                if (!(domainPos[0].Item1 && domainPos[1].Item1))
                {
                    ColorUtils.ColorWriteLine("Error: invalid domain", ConsoleColor.Red);
                    errorInGettingDomainPos = true;
                    continue;
                }
                
                // Swap the coordinates, since in the game's coordinate system, Y is horizontal 
                // and X is vertical.
                domainStartingPositions.Add(new (domainPos[1].Item2, domainPos[0].Item2)); 
            }

            // Error handling.
            if (errorInGettingDomainPos)
            {
                ColorUtils.ColorWriteLine("Error: invalid dimensions", ConsoleColor.Red);
                continue;
            }

            // Initialize window.
            _window = new Window(windowDim, domainStartingPositions);

            _prevInput = new("");   // clear the cached input value

            break;  // break out of loop
        }
    }

    // Handles getting input and running the game loop.
    private void CallUpdate() 
    {
        while (true) {
            // Exit the game.
            if (_prevInput.Keys.StartsWith('x')) 
            {
                break;
            }

            Update();
            HandleInput("Enter input: ");
            Thread.Sleep(20);
            // Clear screen.
            Console.Clear();
        }
    }

    // Tells the window to update each frame.
    private void Update() 
    {
        _window!.Update();
    }

    // Gets input. Caches the previous input and returns it from this function.
    private Input HandleInput(string prompt) 
    {
        Console.Write(prompt);
        Console.Out.Flush();
        var input = Console.ReadLine();

        _prevInput = new(input!);

        return _prevInput;
    }
}