using Microsoft.Xna.Framework;

namespace Paranothing
{
    interface ICollideable
    {
        Rectangle GetBounds();
        bool IsSolid();
    }
}
