
using util;

public class Game 
{
    // To access singleton.
    public static Game Current => _current;

    public Window Window { get; } = new(new Rect(20, 20));

    private Input _prevInput = new("");

    // To store singeton.
    private static Game _current;

    public Game() {
        // Store singleton.
        _current = this;
    }

    // Called when a game is started.
    public void Start() 
    {
        ShowStartMenu();
        CallUpdate();
    }

    private void ShowStartMenu()
    {
        HandleInput("Enter the value: ");

        _prevInput = new("");   // clear the cached input value
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
            Thread.Sleep(1000);
            // Clear screen.
            Console.Clear();
        }
    }

    // Tells the window to update each frame.
    private void Update() 
    {
        Window.Update();
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