using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chip8Unity
{
    public class Chip8
    {
		private readonly byte[] _fontSet = new byte[] {
			0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
			0x20, 0x60, 0x20, 0x20, 0x70, // 1
			0xF0, 0x10, 0xF0, 0x80, 0xF0, // 2
			0xF0, 0x10, 0xF0, 0x10, 0xF0, // 3
			0x90, 0x90, 0xF0, 0x10, 0x10, // 4
			0xF0, 0x80, 0xF0, 0x10, 0xF0, // 5
			0xF0, 0x80, 0xF0, 0x90, 0xF0, // 6
			0xF0, 0x10, 0x20, 0x40, 0x40, // 7
			0xF0, 0x90, 0xF0, 0x90, 0xF0, // 8
			0xF0, 0x90, 0xF0, 0x10, 0xF0, // 9
			0xF0, 0x90, 0xF0, 0x90, 0x90, // A
			0xE0, 0x90, 0xE0, 0x90, 0xE0, // B
			0xF0, 0x80, 0x80, 0x80, 0xF0, // C
			0xE0, 0x90, 0x90, 0x90, 0xE0, // D
			0xF0, 0x80, 0xF0, 0x80, 0xF0, // E
			0xF0, 0x80, 0xF0, 0x80, 0x80  // F
		};

        private byte[] _memory = new byte[4096];
        private ushort _opcode;
        private byte[] _v = new byte[16];

		// Register to store memory addresses
		private ushort _i;

		private byte _delayTimer;
		private byte _soundTimer;

		// Program Counter
		private ushort _pc;

		private ushort[] _stack = new ushort[16];
		// Stack Pointer
		private ushort _sp;

		private bool[] _keys = new bool[16];

		private BitArray _screen = new BitArray(64*32);

		private bool _needRedraw;

		public bool NeedRedraw 
		{
			get { return _needRedraw; }
			set { _needRedraw = value; }
		}

		public Chip8()
		{

		}

		public void Init()
		{
			// Initialize registers and Memory once
			_pc = 0x200;	// Program Counter starts at 0x200
			_opcode = 0;	// Reset current opcode
			_i = 0;			// Reset index register
			_sp = 0;		// Reset stack pointer

			// Clear display
			for (var i = 0; i < _screen.Length; ++i)
			{
				_screen[i] = false;
			}

			// Clear stack
			for (var i = 0; i < _stack.Length; ++i)
			{
				_stack[i] = 0;
			}

			// Clear registers V0-VF
			for (var i = 0; i < 16; ++i)
			{
				_keys[i] = false;
				_v[i] = 0;
			}

			// Clear memory
			for (var i = 0; i < _memory.Length; ++i)
			{
				_memory[i] = 0;
			}

			// Load memory
			for (var i = 0; i < 80; ++i)
			{
				_memory[i] = _fontSet[i];
			}

			// Reset timers
			_delayTimer = 0;
			_soundTimer = 0;

			_needRedraw = true;
		}

		public void LoadGame(byte[] gameBuffer)
		{
			for (var i = 0; i < gameBuffer.Length; ++i)
			{
				_memory[i + 512] = gameBuffer[i];
				Debug.LogFormat("{0:X}", gameBuffer[i]);
			}
		}

		public void Cycle()
		{
			// Fetch opcode
			_opcode = (ushort)(_memory[_pc] << 8 | _memory[_pc + 1]);

			// Decode opcode
			switch (_opcode & 0xF000)
			{
				// Some opcodes
				case 0x0000:
					switch (_opcode & 0x000F)
					{
						case 0x0000: // 0x00E0: Clears the screen
							// Execute opcode
							_needRedraw = true;
							_pc += 2;
							break;
						case 0x000E: // 0x00EE: Returns from subroutine
							// Execute opcode
							--_sp;
							_pc = _stack[_sp];
							_pc += 2;
							break;
						default:
							Debug.LogFormat("Unknown opcode: {0:X}", _opcode);
							break;	
					}
					break;
				case 0x1000: // 0x1NNN 
					// jump to address NNN
					// move _pc to address NNN
					_pc = (ushort)(_opcode & 0x0FFF);
					break;

				case 0x2000:
					_stack[_sp] = _pc;
					++_sp;
					_pc = (ushort)(_opcode & 0x0FFF);
					break;
				
				case 0x3000:
					// 0x3XNN
					// skip next instruction if V[X] == NN
					if (_v[(_opcode & 0x0F00) >> 8] == (_opcode & 0x00FF))
					{
						_pc += 4;
					}
					else
					{
						_pc += 2;
					}
					break;

				case 0x4000:
					// 0x4XNN
					// skip next instruction if V[X] != NN
					if (_v[(_opcode & 0x0F00) >> 8] != (_opcode & 0x00FF))
					{
						_pc += 4;
					}
					else
					{
						_pc += 2;
					}
					break;

				case 0x5000:
					// 0x5XY0
					// skip next instruction if V[X] == V[Y]
					if (_v[(_opcode & 0x0F00) >> 8] == _v[(_opcode & 0x00F0) >> 4])
					{
						_pc += 4;
					}
					else
					{
						_pc += 2;
					}
					break;

				case 0x6000:
					// 0x6XNN
					// set VX to NN
					_v[(_opcode & 0x0F00) >> 8] = (byte)(_opcode & 0x00FF);
					_pc += 2;
					break;

				case 0x7000:
					// 0x7XNN
					// adds NN to VX
					_v[(_opcode & 0x0F00) >> 8] += (byte)(_opcode & 0x00FF);
					_pc += 2;
					break;

				case 0x8000:
					// 0x8XY0
					switch (_opcode & 0x000F)
					{
						case 0x0000: // set VX to VY
							_v[(_opcode & 0x0F00) >> 8] = _v[(_opcode & 0x00F0) >> 4];
							_pc += 2;
							break;
						case 0x0001: // OR
							_v[(_opcode & 0x0F00) >> 8] |= _v[(_opcode & 0x00F0) >> 4];
							_pc += 2;
							break;
						case 0x0002: // AND
							_v[(_opcode & 0x0F00) >> 8] &= _v[(_opcode & 0x00F0) >> 4];
							_pc += 2;
							break;
						case 0x0003:
							_v[(_opcode & 0x0F00) >> 8] ^= _v[(_opcode & 0x00F0) >> 4];
							_pc += 2;
							break;
						case 0x0004:
							if (_v[(_opcode & 0x00F0) >> 4] > (0xFF - _v[(_opcode & 0x0F00) >> 8]))
							{
								_v[0xF] = 1; // carry
							}
							else
							{
								_v[0xF] = 0;
							}
							_v[(_opcode & 0x0F00) >> 8] += _v[(_opcode & 0x00F0) >> 4];
							_pc += 2;
							break;
						case 0x0005:
							if (_v[(_opcode & 0x00F0) >> 4] > _v[(_opcode & 0x0F00) >> 8])
							{
								_v[0xF] = 0; // borrow
							}
							else
							{
								_v[0xF] = 1;
							}
							_v[(_opcode & 0x0F00) >> 8] -= _v[(_opcode & 0x00F0) >> 4];
							_pc += 2;
							break;
						case 0x0006:
							_v[0xF] = (byte)(_v[(_opcode & 0x0F00) >> 8] & 0x0001);
							_v[(_opcode & 0x0F00) >> 8] >>= 1;
							_pc += 2;
							break;
						case 0x0007:
							if (_v[(_opcode & 0x0F00) >> 8] > _v[(_opcode & 0x00F0) >> 4])
							{
								_v[0xF] = 0; // borrow
							}
							else
							{
								_v[0xF] = 1;
							}
							_v[(_opcode & 0x0F00) >> 8] = (byte)(_v[(_opcode & 0x00F0) >> 4] - _v[(_opcode & 0x0F00) >> 8]);
							_pc += 2;
							break;
						case 0x000E:
							_v[0xF] = (byte)(_v[(_opcode & 0x0F00) >> 8] >> 7);
							_v[(_opcode & 0x0F00) >> 8] <<= 1;
							_pc += 2;
							break;
					}
					break;

				case 0x9000:
					// 0x9XY0
					// skip next instruction if V[X] != V[Y]
					if (_v[(_opcode & 0x0F00) >> 8] != _v[(_opcode & 0x00F0) >> 4])
					{
						_pc += 4;
					}
					else
					{
						_pc += 2;
					}
					break;

				case 0xA000:	// ANNN: set I to the address NNN
					// Execute opcode
					_i = (ushort)(_opcode & 0x0FFF);
					_pc += 2;
					break;

				case 0xB000:
					_pc = (ushort)(_v[0x0] + (_opcode & 0x0FFF));
					break;

				case 0xC000:
					_v[(_opcode & 0x0F00) >> 8] = (byte)((Random.Range(0, 256) % 0xFF) & (_opcode & 0x00FF));
					_pc += 2;
					break;

				case 0xD000:
					// display
					// 0xDXYN
					ushort x = _v[(_opcode & 0x0F00) >> 8];
					ushort y = _v[(_opcode & 0x00F0) >> 4];
					ushort width = 8;
					ushort height = (ushort)(_opcode & 0x000F);
					ushort pixel;
					_v[0xF] = 0;

					for ( var yline = 0; yline < height; ++yline)
					{
						pixel = _memory[_i + yline];
						for (var xline = 0; xline < 8; ++xline)
						{
							if ((pixel & (0x80 >> xline)) != 0)
							{
								if (_screen[(x + xline) + ((y + yline) * 64)] == true)
								{
									_v[0xF] = 1;
								}
								_screen[(x + xline) + ((y + yline) * 64)] ^= true;
							}
						}
					}
					_needRedraw = true;
					_pc += 2;
					break;
				
				case 0xE000:
					switch (_opcode & 0x00FF)
					{
						case 0x009E:
							if (_keys[_v[(_opcode & 0x0F00) >> 8]] != false)
							{
								_pc += 4;
							}
							else
							{
								_pc += 2;
							}
							break;
						case 0x00A1:
							if (_keys[_v[(_opcode & 0x0F00) >> 8]] == false)
							{
								_pc += 4;
							}
							else
							{
								_pc += 2;
							}
							break;
					}
					break;

				case 0xF000:
					switch (_opcode & 0x00FF)
					{
						case 0x0007:
							_v[(_opcode & 0x0F00) >> 8] = _delayTimer;
							_pc += 2;
							break;
						case 0x000A:
							var keyPressed = false;
							for (uint i = 0; i < 16; ++i)
							{
								if (_keys[i])
								{
									keyPressed = true;
									_v[(_opcode & 0x0F00) >> 8] = (byte)i;
								}
							}

							if (!keyPressed)
							{
								return;
							}
							_pc += 2;
							break;
						case 0x0015:
							_delayTimer = _v[(_opcode & 0x0F00) >> 8];
							_pc += 2;
							break;
						case 0x0018:
							_soundTimer = _v[(_opcode & 0x0F00) >> 8];
							_pc += 2;
							break;
						case 0x001E:
							_i += _v[(_opcode & 0x0F00) >> 8];
							_pc += 2;
							break;
						case 0x0029:
							_i = (ushort)(_v[(_opcode & 0x0F00) >> 8] * 0x5);
							_pc += 2;
							break;
						case 0x0033:
							// 0xFX33
							_memory[_i] = (byte)(_v[(_opcode & 0x0F00) >> 8] / 100);
							_memory[_i + 1] = (byte)((_v[(_opcode & 0x0F00) >> 8] / 10) % 10);
							_memory[_i + 2] = (byte)((_v[(_opcode & 0x0F00) >> 8] % 100) % 10);
							_pc += 2;
							break;
						case 0x0055:
							for (var i = 0; i < ((_opcode & 0x0F00) >> 8); ++i)
							{
								_memory[_i + i] = _v[i];
							}

							_i += (ushort)(((_opcode & 0x0F00) >> 8) + 1);
							_pc += 2;
							break;
						case 0x0065:
							for (var i = 0; i < ((_opcode & 0x0F00) >> 8); ++i)
							{
								_v[i] = _memory[_i + i];
							}

							_i += (ushort)(((_opcode & 0x0F00) >> 8) + 1);
							_pc += 2;
							break;
					}
					break;
				// More opcodes

				default:
					Debug.LogFormat("Unknown opcode: {0:X}", _opcode);
					break;
			}

			// Update timers
			if (_delayTimer > 0)
			{
				--_delayTimer;
			}

			if (_soundTimer > 0)
			{
				if (_soundTimer == 1)
				{
					Debug.Log("BEEP!");
				}
				--_soundTimer;
			}
		}

		public void SetKeyState(uint key, bool state)
		{
			_keys[key] = state;
		}

		public BitArray GetGraphics()
		{
			return _screen;
		}
    }
}
