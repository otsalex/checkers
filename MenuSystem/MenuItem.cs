namespace MenuSystem;

public class MenuItem
{
    public string Title {get; set;}
    public Func<MenuItem>? MethodToRun { get; set; }

    public MenuItem(string title, Func<MenuItem>? methodToRun)
    {
        Title = title;
        MethodToRun = methodToRun; 
    }
    
    public override string ToString() => Title;
}   