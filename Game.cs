
using util;

public class Game 
{
    // To access singleton.
    public static Game Current => _current;

    public Window Window { get; } = new(new Rect(10, 10));

    private Input _prevInput = new("");

    // To store singeton.
    private static Game _current;

    public Game() {
        // Store singleton.
        _current = this;
    }

    public void Start() 
    {
        CallUpdate();
    }

    private void CallUpdate() 
    {
        while (true) {
            // Exit the game.
            if (_prevInput.Keys.StartsWith('x')) 
            {
                break;
            }

            Update();
            HandleInput();
            Thread.Sleep(1000);
            // Clear screen.
            Console.Clear();
        }
    }

    private void Update() 
    {
        Window.Update();
    }

    private void HandleInput() 
    {
        Console.Write("Enter input: ");
        Console.Out.Flush();
        var input = Console.ReadLine();

        _prevInput = new(input!);

        switch (input)
        {
            case "x":
                break;
            
            default:
                break;
        }
    }
}