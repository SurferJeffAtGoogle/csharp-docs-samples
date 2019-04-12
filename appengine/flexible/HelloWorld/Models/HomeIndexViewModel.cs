using Microsoft.AspNetCore.Mvc.ModelBinding;

public class HomeIndexViewModel 
{
    // The greeting for the person.
    [BindNever]
    public string Greeting { get; set; }
    
    // The name of the person to be greeted.
    public string Name { get; set; }
}