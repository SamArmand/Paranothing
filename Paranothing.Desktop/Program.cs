#region Using Statements

#endregion

namespace Paranothing
{
    internal static class Program
    {
        private static void Main ()
        {
            using (var game = new Game1 ()) {
                game.Run ();
            }
        }
    }
}
