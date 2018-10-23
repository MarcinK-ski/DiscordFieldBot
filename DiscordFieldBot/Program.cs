namespace DiscordFieldBot
{
    class Program
    {
        static void Main(string[] args)
            => new DiscordConnection().MainAsync().GetAwaiter().GetResult();
    }
}
