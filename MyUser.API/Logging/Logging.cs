namespace MyUser.API.Logging
{
    public class Logging : ILogging
    {
        public void Log(string message, string Type)
        {
            if (Type == "error")
            {
                Console.WriteLine("Error - " + message);
            }
            else
            {
                { Console.WriteLine(message); }
            }
        }
    }
}
