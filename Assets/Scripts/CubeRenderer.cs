using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chip8Unity
{
    public class CubeRenderer : RendererComponent
    {
        [SerializeField] private GameObject _cubePrefab;
        [SerializeField] private Vector2 _screenSize;
        [SerializeField] private Vector2 _distance;

        private List<GameObject> _pool;

        private void Start()
        {
            _pool = new List<GameObject>();
            var xPos = 0.0f;
            var yPos = 0.0f;
            for (var y = 0; y < _screenSize.y; ++y)
            {
                xPos = 0.0f;
                for (var x = 0; x < _screenSize.x; ++x)
                {
                    var cube = Instantiate(_cubePrefab, new Vector3(xPos, yPos, 0), Quaternion.identity);
                    cube.transform.parent = transform;
                    cube.SetActive(false);
                    _pool.Add(cube);
                    xPos += _distance.x;
                }
                yPos -= _distance.y;
            }
        }

        public override void Draw(BitArray screen)
        {
            for (var i = 0; i < screen.Length; ++i)
            {
                if (screen[i] == false)
                {
                    _pool[i].SetActive(false);
                }
                else
                {
                    _pool[i].SetActive(true);
                }
            }
        }
    }
}
