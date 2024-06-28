
using util;

public class Game 
{
    // To access singleton.
    public static Game Current => _current;

    // Initialized after the game starts.
    public Window? Window { get; private set; } = null;

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

        while (Window is null)
        {
            var strInput = HandleInput("Do you have a configuration file for automatically setting up the game? (Y/N) ").Keys;

            switch (strInput)
            {
            case "Y" or "y":
                // Custom setup from file
                CustomFileWindowSetup();
                break;

            case "N" or "n":
                // Manual setup
                ManualWindowSetup();
                break;

            default:
                ColorUtils.ColorWriteLine("Error: invalid response", ConsoleColor.Red);
                continue;
            }
        }
    }

    private void ManualWindowSetup()
    {
        while (Window is null)
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
            Window = new Window(windowDim, domainStartingPositions);

            _prevInput = new("");   // clear the cached input value
        }
    }

    private void CustomFileWindowSetup()
    {
        var path = HandleInput("Enter the absolute path of the configuration file: ").Keys;

        // Ignores the label on the config file.
        static string GetField(StreamReader reader) => reader.ReadLine()!.Split(":")[1];

        try
        {
            using var sr = new StreamReader(path);
            string? ln;

            var dimensionsArray = GetField(sr)   // ignore the label
                .Split(' ')                      // split the x and y coords
                .Select(int.Parse)               // convert them to ints
                .ToArray();                      // convert to array

            var dimensions = new Rect(dimensionsArray[1], dimensionsArray[0]); // swap coords

            var domainAmt = int.Parse(GetField(sr));

            sr.ReadLine();  // skip line

            var domainPositions = new List<Point>(domainAmt);

            for (int i = 0; i < domainAmt; i++)
            {
                ln = sr.ReadLine();
                var posArray = ln!.Split(' ').Select(int.Parse).ToArray();

                var pos = new Point(posArray[1], posArray[0]);  // swap coords

                domainPositions.Add(pos);
            }

            sr.ReadLine();  // skip line

            char[,] customWorldLayout = new char[dimensions.X, dimensions.Y];

            var x = 0;

            // Read the custom map.
            while ((ln = sr.ReadLine()) != null)
            {
                for (var y = 0; y < dimensions.Y; y++)
                {
                    customWorldLayout[x, y] = ln[y];
                }
                x++;
            }

            Window = new Window(dimensions, domainPositions, customWorldLayout);
            _prevInput = new Input("");
        }
        catch (Exception e)
        {
            ColorUtils.ColorWriteLine(e.Message, ConsoleColor.Red);
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
            Thread.Sleep(5);
            // Clear screen.
            Console.Clear();
        }
    }

    // Tells the window to update each frame.
    private void Update() 
    {
        Window!.Update();
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