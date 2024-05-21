
public class Game {
    class Input(string keys)
    {
        public string Keys { get; } = keys;
    }

    public readonly (int, int) WINDOW_DIM = (400, 400);

    private Input _prevInput = new("");

    public Game() {}

    public void Start() {
        CallUpdate();
    }

    private void CallUpdate() {
        while (true) {
            HandleInput();

            // Exit the game.
            if (_prevInput.Keys.StartsWith('x')) {
                break;
            }

            Update();
            Thread.Sleep(1000);
        }
    }

    private void Update() {
        Console.WriteLine("hi");

        // TODO: Print out the game window.
    }

    private void HandleInput() {
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