#region Using Statements
#endregion

namespace Paranothing
{
    public static class Program
	{
		static void Main (string [] args)
		{
            using (var game = new Game1()) game.Run();
        }
    }
}
