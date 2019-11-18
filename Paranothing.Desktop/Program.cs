#region Using Statements
#endregion

namespace Paranothing
{
    public static class Program
	{
		static void Main() {
			using var game1 = new Game1();
			game1.Run();
		}
	}
}
