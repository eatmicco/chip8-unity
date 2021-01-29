using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chip8Unity
{
    public class RendererAdapter : MonoBehaviour
    {
        [SerializeField] private GameLoop _gameLoop;
        [SerializeField] private RendererComponent _renderer;

        // Start is called before the first frame update
        void Start()
        {
            if (_gameLoop != null && _renderer != null)
            {
                _gameLoop.Renderer = _renderer;
            }
        }
    }
}
