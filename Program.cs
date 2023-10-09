using Configs;
using System.IO;
using System.Text;
using System.Text.Json;
using ValeraTheMarginal;

internal class Program
{
    private const string MenuText =
    """
    Welcome to Valera The Marginal!
    Type:
        'play' — to start game (or continue current session)
        'actions' — to get list of currently available actions
        'save' — to save valera's current status
        'load' — to load previously saved game session
        'menu' — to get back to the menu
        'exit' — to quit the game
    """;

    private static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.InputEncoding = System.Text.Encoding.UTF8;
        if (args.Length != 2)
        {
            Console.WriteLine("not enough cmd args");
            return;
        }
        if (!(args[0] == "--config" || args[0] == "-c"))
        {
            Console.WriteLine("invalid cmd arg");
            return;
        }
        try
        {
            var config = new Config(ref args[1]);
            if (config == null || config.Actions == null)
            {
                Console.WriteLine("cannot load configuration");
                return;
            }
            var valera = new Valera(config.Actions);
            Console.WriteLine(MenuText);
            while (true)
            {
                Console.Write(">");
                var cmd = Console.ReadLine();
                if (cmd == "play")
                {
                    PlayGame(ref valera);
                }
                else if (cmd == "actions")
                {
                    ShowActions(valera.Actions);
                }
                else if (cmd == "save")
                {
                    SaveGame(ref valera);
                }
                else if (cmd == "load")
                {
                    LoadGame(ref valera);
                }
                else if (cmd == "menu")
                {
                    Console.WriteLine(MenuText);
                }
                else if (cmd == "exit")
                {
                    Console.WriteLine("Bye bye...");
                    return;
                }
                else if (cmd == null || cmd.Length == 0)
                {
                    continue;
                }
                else
                {
                    Console.WriteLine("Invalid command!");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private static void PlayGame(ref Valera valera)
    {
        Console.WriteLine($"Valera status: {valera}\nEnter the action number");
        while (true)
        {
            try
            {
                string? cmd;
                Console.Write(">");
                cmd = Console.ReadLine();
                if (cmd == "menu")
                {
                    Console.WriteLine(MenuText);
                    return;
                }
                if (!int.TryParse(cmd, out int actionNumber))
                {
                    continue;
                }
                valera.DoAction(actionNumber - 1);
                if (valera.IsDead())
                {
                    Console.WriteLine($"GAME OVER\nValera is dead... *starts crying*\n" +
                        $"Valera's status at the time of death: {valera}");
                    return;
                }
                if (valera.IsBroke())
                {
                    Console.WriteLine($"GAME OVER\nValera is broke boy now (he can't pay his rent " +
                        $"and he'll die of cold outside in a couple days)\n" +
                        $"Valera's status at the time he was kicked out of the house: {valera}");
                    return;
                }
                Console.WriteLine($"Valera status: {valera}");
            }
            catch (ActionNotFoundException)
            {
                Console.WriteLine("Unexpected action number. Try again or get back to the menu" +
                    "and type 'actions' to get list of currently available actions");
            }
            catch (ActionException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    private static void ShowActions(List<ValeraTheMarginal.Action> actions)
    {
        Console.WriteLine("Available actions:");
        for (int i = 0; i < actions.Count; ++i)
        {
            Console.WriteLine($"{i + 1} {actions[i]}");
        }
    }

    private static void SaveGame(ref Valera valera)
    {
        Console.WriteLine("Enter the path where to save");
        while (true)
        {
            string? filePath;
            try
            {
                Console.Write(">");
                filePath = Console.ReadLine();
                if (filePath == "menu")
                {
                    Console.WriteLine(MenuText);
                    return;
                }
                if (File.Exists(filePath))
                {
                    Console.WriteLine($"File with name {filePath} already exists");
                    continue;
                }
                using var writer = File.CreateText(filePath);
                writer.WriteLine(valera);
                Console.WriteLine($"Valera is successfully saved in file {filePath}");
                return;
            }
            catch (ArgumentException)
            {
                continue;
            }
            catch (IOException)
            {
                Console.WriteLine("Invalid filepath. Try again.");
            }
        }
    }

    private static void LoadGame(ref Valera valera)
    {
        Console.WriteLine("Enter the path where to load");
        while (true)
        {
            string? filePath;
            try
            {
                Console.Write(">");
                filePath = Console.ReadLine();
                if (filePath == "menu")
                {
                    Console.WriteLine(MenuText);
                    return;
                }
                var status = JsonSerializer.Deserialize<ValeraStatus>(File.ReadAllText(filePath));
                if (status == null)
                {
                    Console.WriteLine("Failed to deserialize status info");
                    continue;
                }
                valera.LoadStatus(ref status);
                Console.WriteLine($"Valera is successfully loaded from file {filePath}\nHis status now: {valera}");
                return;
            }
            catch (ArgumentException)
            {
                continue;
            }
            catch (StatusException e)
            {
                Console.WriteLine($"Invalid JSON loading game file: {e.Message}\nFix it and try again or choose another file");
            }
            catch (System.Text.Json.JsonException)
            {
                Console.WriteLine("Invalid JSON loading game file. Fix it and try again or choose another file");
            }
            catch (IOException)
            {
                Console.WriteLine("Invalid filepath. Try again.");
            }
        }
    }
}