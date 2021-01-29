using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chip8Unity
{
    public interface IRenderer
    {
        void Draw(BitArray screen);
    }
}