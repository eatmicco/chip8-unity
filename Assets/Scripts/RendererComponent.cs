using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chip8Unity
{
    public class RendererComponent : MonoBehaviour, IRenderer
    {
        public virtual void Draw(BitArray screen)
        {
            
        }
    }
}
