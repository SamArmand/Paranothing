using Microsoft.Xna.Framework;

namespace Paranothing
{
    internal interface ICollideable
    {
        Rectangle GetBounds();
        bool IsSolid();
    }
}
