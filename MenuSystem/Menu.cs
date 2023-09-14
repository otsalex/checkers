
namespace MenuSystem;

public class Menu
{
    private readonly EMenuLevel _level;
    private string Title { get; set; }
    
    private readonly List<MenuItem> _menuItems = new();

    public Menu(EMenuLevel level, string title, List<MenuItem> menuItems)
    {
        _level = level;
        
        Title = title;
        foreach (var menuItem in menuItems)
        {
            _menuItems.Add(menuItem);
        }
        _menuItems.Add(new MenuItem("Exit", methodToRun:null));
        if (level != EMenuLevel.Main)
        {
            _menuItems.Add(new MenuItem("Back", methodToRun:null));
            _menuItems.Add(new MenuItem("Main menu", methodToRun:null));
        }
    }
    
    public MenuItem RunMenu()
    {
        MenuItem? userChoice;
        MenuItem? lastChoiceReturnValue = null;
        var menuDone = false;
        do
        {
            userChoice = UserChoice();
            if (userChoice.Title == "Exit")
            {
                menuDone = true;
            }
            
            if (userChoice.MethodToRun != null)
            {
                lastChoiceReturnValue = userChoice.MethodToRun!();
            }
            
            // used for closing the menus correctly when starting the game from the load menu
            if (userChoice.Title == "Play Game" || lastChoiceReturnValue?.Title == "Play Game")
            {
                userChoice = lastChoiceReturnValue ?? userChoice;
                if (_level == EMenuLevel.Main)
                {
                    userChoice = _menuItems[1].MethodToRun!();
                }
                else
                {
                    menuDone = true;
                }
            }

            if (lastChoiceReturnValue?.Title == "Exit" ||
                userChoice.Title == "Main menu" ||
                lastChoiceReturnValue?.Title == "Main menu")
            {
                userChoice = lastChoiceReturnValue ?? userChoice;
                if (!(userChoice.Title == "Main menu" && _level == EMenuLevel.Main))
                    menuDone = true;
            }
            
            if (userChoice.Title == "Back" || (userChoice.Title == "Continue game" && _level == EMenuLevel.Main))
                menuDone = true;
            
        } while (!menuDone);
        
        return userChoice;
    }

    private MenuItem UserChoice()
    {
        var activeLine = 0;
        var done = false;
        do
        {
            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(Title);

            for (int i = 0; i < _menuItems.Count; i++)
            {
                Console.BackgroundColor = i == activeLine ? ConsoleColor.White : ConsoleColor.Black;

                Console.WriteLine(_menuItems[i]);
            }

            var key = Console.ReadKey(true);

            switch (key.Key)
            {
                case ConsoleKey.UpArrow:
                {
                    activeLine--;
                    if (activeLine < 0) activeLine = _menuItems.Count - 1;
                    break;
                }
                case ConsoleKey.DownArrow:
                {
                    activeLine++;
                    if (activeLine >= _menuItems.Count) activeLine = 0;
                    break;
                }
                case ConsoleKey.Enter:
                    Console.Clear();
                    return _menuItems[activeLine];
            }
            Console.Clear();

        } while (!done);

        return _menuItems[0];
    }

}