using Microsoft.Xna.Framework;

namespace Paranothing
{
    interface Collideable
    {
        Rectangle getBounds();
        bool isSolid();
    }
}
