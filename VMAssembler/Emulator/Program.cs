using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Emulator
{
    class Program
    {
        static void Add(ref uint operationResultValue1Value2)
        {
            uint value1 = (byte)(operationResultValue1Value2 >> 8);
            uint value2 = (byte)operationResultValue1Value2;

            uint result = value1 + value2;
            operationResultValue1Value2 = operationResultValue1Value2 & 0xFF00FFFF;
            operationResultValue1Value2 = operationResultValue1Value2 | result << 16;
        }

        static void Subtract(ref uint operationResultValue1Value2)
        {
            uint value2 = (byte)operationResultValue1Value2;

            operationResultValue1Value2 = operationResultValue1Value2 & 0xFFFFFF00;
            operationResultValue1Value2 = operationResultValue1Value2 | (uint)((byte)(~value2) + 1);
            Add(ref operationResultValue1Value2);
            operationResultValue1Value2 = operationResultValue1Value2 & 0xFFFFFF00;
            operationResultValue1Value2 = operationResultValue1Value2 | value2;
        }
        static void Multiplication(ref uint operationResultValue1Value2)
        {
            uint value2 = (byte)operationResultValue1Value2;
            uint result = operationResultValue1Value2 >> 16;

            uint i = (byte)(operationResultValue1Value2 >> 8);

            uint instruction = 0x12000000;
            instruction = instruction | result << 8;
            instruction = instruction | value2;
            Add(ref instruction);
            result = (byte)(instruction >> 16);
            i--;

            operationResultValue1Value2 = operationResultValue1Value2 & 0xFF00FFFF;
            operationResultValue1Value2 = operationResultValue1Value2 | result << 16;

            operationResultValue1Value2 = operationResultValue1Value2 & 0xFFFF00FF;
            operationResultValue1Value2 = operationResultValue1Value2 | i << 8;

            if (i > 0)
            {
                Multiplication(ref operationResultValue1Value2);
                operationResultValue1Value2 = operationResultValue1Value2 & 0x00FFFFFF;
                operationResultValue1Value2 = operationResultValue1Value2 | 0x12 << 24;
            }
        }
        static void Divide(ref uint operationResultValue1Value2)
        {
            uint value1 = (byte)(operationResultValue1Value2 >> 8);
            uint value2 = (byte)operationResultValue1Value2;
            uint result = (byte)(operationResultValue1Value2 >> 16);

            int remainingNumber = (int)value1;

            uint instruction = 0x12000000;
            instruction = instruction | (uint)(remainingNumber << 8);
            instruction = instruction | value2;
            Subtract(ref instruction);
            result++;
            remainingNumber = (byte)(instruction >> 16);

            operationResultValue1Value2 = operationResultValue1Value2 & 0xFF00FFFF;
            operationResultValue1Value2 = operationResultValue1Value2 | result << 16;

            operationResultValue1Value2 = operationResultValue1Value2 & 0xFFFF00FF;
            operationResultValue1Value2 = operationResultValue1Value2 | (uint)remainingNumber << 8;
            if (remainingNumber > 0)
            {
                Divide(ref operationResultValue1Value2);
            }
            else
            {
                operationResultValue1Value2 = operationResultValue1Value2 & 0xFF00FFFF;
                operationResultValue1Value2 = operationResultValue1Value2 | (result) << 16;
            }
        }
        static void MOD(ref uint operationResultValue1Value2)
        {
            uint value1 = (byte)(operationResultValue1Value2 >> 8);
            uint value2 = (byte)operationResultValue1Value2;
            uint result = (byte)(operationResultValue1Value2 >> 16);

            int remainingNumber = (int)value1;

            uint instruction = 0x12000000;
            instruction = instruction | (uint)(remainingNumber << 8);
            instruction = instruction | value2;
            Subtract(ref instruction);
            result++;
            remainingNumber = (byte)(instruction >> 16);

            operationResultValue1Value2 = operationResultValue1Value2 & 0xFF00FFFF;
            operationResultValue1Value2 = operationResultValue1Value2 | result << 16;

            operationResultValue1Value2 = operationResultValue1Value2 & 0xFFFF00FF;
            operationResultValue1Value2 = operationResultValue1Value2 | (uint)remainingNumber << 8;
            if (remainingNumber > value2)
            {
                MOD(ref operationResultValue1Value2);
            }
            else
            {

                operationResultValue1Value2 = operationResultValue1Value2 & 0xFF00FFFF;
                operationResultValue1Value2 = operationResultValue1Value2 | (uint)((remainingNumber) << 16);

            }
        }
        static Dictionary<int, ConsoleColor> Colors = new Dictionary<int, ConsoleColor>()
        {
            {0, ConsoleColor.Black },
            {1, ConsoleColor.Red },
            {2, ConsoleColor.Blue },
            {3, ConsoleColor.Green },
            {4, ConsoleColor.Yellow },
            {5, ConsoleColor.DarkMagenta },
            {6, ConsoleColor.Cyan },
        };
        static Dictionary<ConsoleKey, ushort> Keys = new Dictionary<ConsoleKey, ushort>()
        {
            {ConsoleKey.D,1 },
            {ConsoleKey.A,2 },
            {ConsoleKey.W,3 },
            {ConsoleKey.S,4 },
            {ConsoleKey.Spacebar,5 }
        };

        static RAM ram = new RAM(ushort.MaxValue);
        static ushort[] registers = new ushort[32];
        const int instructionCounterIndex = 31;
        const int StackPointer = 30;
        static ushort[] pastScreen = new ushort[ram.data.Length/8];
        static void draw(ushort[] screen)
        {
            for(int y = 0; y < 16; y++)
            {
                for(int x = 0; x < 16; x++)
                {
                    Console.BackgroundColor = Colors[screen[x * 16 + y]];
                    Console.Write("  ");
                }
                Console.BackgroundColor = ConsoleColor.Black;
                Console.WriteLine("");
            }
            pastScreen = screen;
        }
        static bool areEqual(ushort[] array1, ushort[] array2)
        {
            for(int i = 0; i < array1.Length; i++)
            {
                if(array1[i] != array2[i])
                {
                    return false;
                }
            }
            return true;
        }
        static void emulate(List<byte[]> instructions)
        {
            while(true)
            {
                registers[26] = 16;
                if(registers[instructionCounterIndex] >= instructions.Count)
                {
                    break;
                }
                byte opCode = instructions[registers[instructionCounterIndex]][0];
                if (registers[29] == 1)
                {
                    if (Console.KeyAvailable)
                    {
                        Random random = new Random();
                        registers[28] = (ushort)random.Next(1, ushort.MaxValue);
                        registers[29] = 0;
                    }
                }
                if(registers[27] == 1)
                {
                    Span<ushort> span = ram.data.AsSpan().Slice(0, ram.data.Length / 8);
                    ushort[] screen = span.ToArray();
                    if (!areEqual(screen, pastScreen))
                    {
                        Console.Clear();
                        draw(screen);
                    }
                    registers[27] = 0;
                }
                if(registers[25] == 1)
                {
                    if(Console.KeyAvailable)
                    {
                        registers[24] = Keys[Console.ReadKey().Key];
                        registers[25] = 0;
                    }
                }
                

                switch (opCode)
                {
                    case 0x00:
                        registers[instructionCounterIndex]++;
                        break;
                    case 0x01:
                        uint thing = instructions[registers[instructionCounterIndex]][1];
                        uint value1 = registers[instructions[registers[instructionCounterIndex]][2]];
                        uint value2 = registers[instructions[registers[instructionCounterIndex]][3]];
                        uint instruction = instructions[registers[instructionCounterIndex]][0];
                        instruction = instruction & 0xFFFF0000;
                        instruction = instruction | value1 << 8;
                        instruction = instruction | value2;
                        Add(ref instruction);
                        registers[thing] = (ushort)((instruction << 8) >> 24);
                        registers[instructionCounterIndex]++;
                        break;
                    case 0x02:
                        thing = instructions[registers[instructionCounterIndex]][1];
                        value1 = registers[instructions[registers[instructionCounterIndex]][2]];
                        value2 = registers[instructions[registers[instructionCounterIndex]][3]];
                        instruction = instructions[registers[instructionCounterIndex]][0];
                        instruction = instruction & 0xFFFF0000;
                        instruction = instruction | value1 << 8;
                        instruction = instruction | value2;
                        Subtract(ref instruction);
                        registers[thing] = (ushort)((instruction << 8) >> 24);
                        registers[instructionCounterIndex]++;
                        break;
                    case 0x03:
                        thing = instructions[registers[instructionCounterIndex]][1];
                        value1 = registers[instructions[registers[instructionCounterIndex]][2]];
                        value2 = registers[instructions[registers[instructionCounterIndex]][3]];
                        instruction = instructions[registers[instructionCounterIndex]][0];
                        instruction = instruction & 0xFFFF0000;
                        instruction = instruction | value1 << 8;
                        instruction = instruction | value2;
                        Multiplication(ref instruction);
                        registers[thing] = (ushort)((instruction << 8) >> 24);
                        registers[instructionCounterIndex]++;
                        break;
                    case 0x04:
                        thing = instructions[registers[instructionCounterIndex]][1];
                        value1 = registers[instructions[registers[instructionCounterIndex]][2]];
                        value2 = registers[instructions[registers[instructionCounterIndex]][3]];
                        instruction = instructions[registers[instructionCounterIndex]][0];
                        instruction = instruction & 0xFFFF0000;
                        instruction = instruction | value1 << 8;
                        instruction = instruction | value2;
                        Divide(ref instruction);
                        registers[thing] = (ushort)((instruction << 8) >> 24);
                        registers[instructionCounterIndex]++;
                        break;
                    case 0x05:
                        thing = instructions[registers[instructionCounterIndex]][1];
                        value1 = registers[instructions[registers[instructionCounterIndex]][2]];
                        value2 = registers[instructions[registers[instructionCounterIndex]][3]];
                        instruction = instructions[registers[instructionCounterIndex]][0];
                        instruction = instruction & 0xFFFF0000;
                        instruction = instruction | value1 << 8;
                        instruction = instruction | value2;
                        MOD(ref instruction);
                        registers[thing] = (ushort)((instruction << 8) >> 24);
                        registers[instructionCounterIndex]++;
                        break;
                    case 0x06:
                        registers[instructions[registers[instructionCounterIndex]][1]] = (byte)(instructions[registers[instructionCounterIndex]][2] & instructions[registers[instructionCounterIndex]][3]);
                        registers[instructionCounterIndex]++;
                        break;
                    case 0x07:
                        registers[instructions[registers[instructionCounterIndex]][1]] = (byte)(instructions[registers[instructionCounterIndex]][2] | instructions[registers[instructionCounterIndex]][3]);
                        registers[instructionCounterIndex]++;
                        break;
                    case 0x08:
                        registers[instructions[registers[instructionCounterIndex]][1]] = (byte)(instructions[registers[instructionCounterIndex]][2] ^ instructions[registers[instructionCounterIndex]][3]);
                        registers[instructionCounterIndex]++;
                        break;
                    case 0x09:
                        registers[instructions[registers[instructionCounterIndex]][1]] = (byte)(~(uint)instructions[registers[instructionCounterIndex]][2]);
                        registers[instructionCounterIndex]++;
                        break;
                    case 0x10:
                        registers[instructions[registers[instructionCounterIndex]][1]] = (byte)((uint)instructions[registers[instructionCounterIndex]][2] >> 1);
                        registers[instructionCounterIndex]++;
                        break;
                    case 0x11:
                        registers[instructions[registers[instructionCounterIndex]][1]] = (byte)((uint)instructions[registers[instructionCounterIndex]][2] << 1);
                        registers[instructionCounterIndex]++;
                        break;
                    case 0x12:
                        if (registers[instructions[registers[instructionCounterIndex]][2]] > registers[instructions[registers[instructionCounterIndex]][3]])
                        {
                            registers[instructions[registers[instructionCounterIndex]][1]] = 1;
                        }
                        else
                        {
                            registers[instructions[registers[instructionCounterIndex]][1]] = 0;
                        }
                        registers[instructionCounterIndex]++;
                        break;
                    case 0x13:
                        if (registers[instructions[registers[instructionCounterIndex]][2]] < registers[instructions[registers[instructionCounterIndex]][3]])
                        {
                            registers[instructions[registers[instructionCounterIndex]][1]] = 1;
                        }
                        else
                        {
                            registers[instructions[registers[instructionCounterIndex]][1]] = 0;
                        }
                        registers[instructionCounterIndex]++;
                        break;
                    case 0x14:
                        if (registers[instructions[registers[instructionCounterIndex]][2]] == registers[instructions[registers[instructionCounterIndex]][3]])
                        {
                            registers[instructions[registers[instructionCounterIndex]][1]] = 1;
                        }
                        else
                         {
                            registers[instructions[registers[instructionCounterIndex]][1]] = 0;
                        }
                        registers[instructionCounterIndex]++;
                        break;
                    case 0x15:
                        if (registers[instructions[registers[instructionCounterIndex]][2]] >= registers[instructions[(int)registers[instructionCounterIndex]][3]])
                        {
                            registers[instructions[registers[instructionCounterIndex]][1]] = 1;
                        }
                        else
                        {
                            registers[instructions[registers[instructionCounterIndex]][1]] = 0;
                        }
                        registers[instructionCounterIndex]++;
                        break;
                    case 0x16:
                        if (registers[instructions[registers[instructionCounterIndex]][2]] <= registers[instructions[registers[instructionCounterIndex]][3]])
                        {
                            registers[instructions[registers[instructionCounterIndex]][1]] = 1;
                        }
                        else
                        {
                            registers[instructions[registers[instructionCounterIndex]][1]] = 0;
                        }
                        registers[instructionCounterIndex]++;
                        break;
                    case 0x17:
                        registers[instructionCounterIndex] = ushort.Parse(instructions[registers[instructionCounterIndex]][1].ToString() + instructions[registers[instructionCounterIndex]][2].ToString());
                        break;
                    case 0x18:
                        if (registers[instructions[registers[instructionCounterIndex]][1]] == 1)
                        {
                            registers[instructionCounterIndex] = ushort.Parse(instructions[registers[instructionCounterIndex]][2].ToString() + instructions[registers[instructionCounterIndex]][3].ToString());
                        }
                        else
                        {
                            registers[instructionCounterIndex]++;
                        }
                        break;
                    case 0x19:
                        registers[instructionCounterIndex] = ushort.Parse(instructions[registers[instructionCounterIndex]][1].ToString() + instructions[registers[instructionCounterIndex]][2].ToString());
                        break;
                    case 0x20:
                        if (registers[instructions[registers[instructionCounterIndex]][1]] == 1)
                        {
                            registers[instructionCounterIndex] = ushort.Parse(instructions[registers[instructionCounterIndex]][2].ToString() + instructions[registers[instructionCounterIndex]][3].ToString());
                        }
                        else
                        {
                            registers[instructionCounterIndex]++;
                        }
                        break;
                    case 0x21:
                        registers[instructions[registers[instructionCounterIndex]][1]] = ushort.Parse(instructions[registers[instructionCounterIndex]][2].ToString() + instructions[registers[instructionCounterIndex]][3].ToString());
                        registers[instructionCounterIndex]++;
                        break;
                    case 0x22:
                        ram.data[ushort.Parse(instructions[registers[instructionCounterIndex]][2].ToString() + instructions[registers[instructionCounterIndex]][3].ToString())] = registers[instructions[registers[instructionCounterIndex]][1]];
                        registers[instructionCounterIndex]++;
                        break;
                    case 0x23:
                        registers[StackPointer]++;
                        Span<ushort> stack = ram.data.AsSpan().Slice(ram.data.Length / 2, ram.data.Length / 2);
                        stack[registers[StackPointer]] = registers[instructions[registers[instructionCounterIndex]][1]];
                        break;
                    case 0x24:
                        stack = ram.data.AsSpan().Slice(ram.data.Length / 2, ram.data.Length / 2);
                        registers[instructions[instructionCounterIndex][1]] = stack[StackPointer];
                        registers[StackPointer]--;
                        registers[instructionCounterIndex]++;
                        break;
                    case 0x25:
                        registers[instructions[registers[instructionCounterIndex]][1]] = registers[instructions[registers[instructionCounterIndex]][2]];
                        registers[instructionCounterIndex]++;
                        break;
                    case 0x26:
                        ram.data[registers[instructions[registers[instructionCounterIndex]][2]]] = registers[instructions[registers[instructionCounterIndex]][1]];
                        registers[instructionCounterIndex]++;
                        break;

                }

            }
        }
        static void Main(string[] args)
        {
            Console.CursorVisible = false;
            Console.ForegroundColor = ConsoleColor.Black;
            byte[] bytes = File.ReadAllBytes("C:/Users/mason.lee/source/repos/VMAssembler/VMAssembler/MachineCode.txt");
            Span<byte> machineCode = bytes.AsSpan<byte>();

            List<byte[]> instructions = new List<byte[]>();
            for (int i = 0; i < machineCode.Length / 4; i++)
            {
                instructions.Add(machineCode.Slice(registers[instructionCounterIndex] + i * 4, 4).ToArray());
            }
            emulate(instructions);
            ;

        }
    }
}
