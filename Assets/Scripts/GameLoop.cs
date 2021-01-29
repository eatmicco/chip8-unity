using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Chip8Unity
{
    public class GameLoop : MonoBehaviour
    {
        [SerializeField] private string _url;
        private Chip8 _chip8;
        private bool _gameLoaded;

        public IRenderer Renderer { get; set; }

        // Start is called before the first frame update
        void Start()
        {
            // Create and Initialize Chip8
            _chip8 = new Chip8Unity.Chip8();
            _chip8.Init();
            DownloadGame(_url);

            // InvokeRepeating("CycleUpdate", 1, 0.01f);
        }

        // Update is called once per frame
        void Update()
        {
            if (!_gameLoaded)
            {
                return;
            }

            // Emulate one cycle
            _chip8.Cycle();

            // If drawflag is set, update the screen
            if (_chip8.NeedRedraw && Renderer != null)
            {
                // Draw
                var bitData = _chip8.GetGraphics();
                Renderer.Draw(bitData);
            }

            // Store key press state
            HandleInput();
        }

        private async void DownloadGame(string url)
        {
            var uwr = UnityWebRequest.Get(url);
            uwr.downloadHandler = new DownloadHandlerBuffer();
            await uwr.SendWebRequest();
            
            if (!uwr.isNetworkError)
            {
                _chip8.LoadGame(uwr.downloadHandler.data);
                _gameLoaded = true;
                Debug.Log("Game Loaded");
            }
        }

        private void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) _chip8.SetKeyState(0x1, true);
            if (Input.GetKeyDown(KeyCode.Alpha2)) _chip8.SetKeyState(0x2, true);
            if (Input.GetKeyDown(KeyCode.Alpha3)) _chip8.SetKeyState(0x3, true);
            if (Input.GetKeyDown(KeyCode.Alpha4)) _chip8.SetKeyState(0xC, true);
            if (Input.GetKeyDown(KeyCode.Q)) _chip8.SetKeyState(0x4, true);
            if (Input.GetKeyDown(KeyCode.W)) _chip8.SetKeyState(0x5, true);
            if (Input.GetKeyDown(KeyCode.E)) _chip8.SetKeyState(0x6, true);
            if (Input.GetKeyDown(KeyCode.R)) _chip8.SetKeyState(0xD, true);
            if (Input.GetKeyDown(KeyCode.A)) _chip8.SetKeyState(0x7, true);
            if (Input.GetKeyDown(KeyCode.S)) _chip8.SetKeyState(0x8, true);
            if (Input.GetKeyDown(KeyCode.D)) _chip8.SetKeyState(0x9, true);
            if (Input.GetKeyDown(KeyCode.F)) _chip8.SetKeyState(0xE, true);
            if (Input.GetKeyDown(KeyCode.Z)) _chip8.SetKeyState(0xA, true);
            if (Input.GetKeyDown(KeyCode.X)) _chip8.SetKeyState(0x0, true);
            if (Input.GetKeyDown(KeyCode.C)) _chip8.SetKeyState(0xB, true);
            if (Input.GetKeyDown(KeyCode.V)) _chip8.SetKeyState(0xF, true);

            if (Input.GetKeyUp(KeyCode.Alpha1)) _chip8.SetKeyState(0x1, false);
            if (Input.GetKeyUp(KeyCode.Alpha2)) _chip8.SetKeyState(0x2, false);
            if (Input.GetKeyUp(KeyCode.Alpha3)) _chip8.SetKeyState(0x3, false);
            if (Input.GetKeyUp(KeyCode.Alpha4)) _chip8.SetKeyState(0xC, false);
            if (Input.GetKeyUp(KeyCode.Q)) _chip8.SetKeyState(0x4, false);
            if (Input.GetKeyUp(KeyCode.W)) _chip8.SetKeyState(0x5, false);
            if (Input.GetKeyUp(KeyCode.E)) _chip8.SetKeyState(0x6, false);
            if (Input.GetKeyUp(KeyCode.R)) _chip8.SetKeyState(0xD, false);
            if (Input.GetKeyUp(KeyCode.A)) _chip8.SetKeyState(0x7, false);
            if (Input.GetKeyUp(KeyCode.S)) _chip8.SetKeyState(0x8, false);
            if (Input.GetKeyUp(KeyCode.D)) _chip8.SetKeyState(0x9, false);
            if (Input.GetKeyUp(KeyCode.F)) _chip8.SetKeyState(0xE, false);
            if (Input.GetKeyUp(KeyCode.Z)) _chip8.SetKeyState(0xA, false);
            if (Input.GetKeyUp(KeyCode.X)) _chip8.SetKeyState(0x0, false);
            if (Input.GetKeyUp(KeyCode.C)) _chip8.SetKeyState(0xB, false);
            if (Input.GetKeyUp(KeyCode.V)) _chip8.SetKeyState(0xF, false);
        }
    }
}
