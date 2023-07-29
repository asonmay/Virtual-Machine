using System;
using System.Collections.Generic;
using System.IO;

namespace Disassembler
{
    class Program
    {
        static Dictionary<uint, string> OpCodes = new Dictionary<uint, string>()
        {
            {0x00, "DN"},
            {0x01, "ADD"},
            {0x02, "SUB"},
            {0x03, "MUL"},
            {0x04, "DIV"},
            {0x05, "MOD"},
            {0x06, "AND"},
            {0x07, "OR"},
            {0x08, "XOR"},
            {0x09, "NOT"},
            {0x10, "STR"},
            {0x11, "STL"},
            {0x12, "GT"},
            {0x13, "LT"},
            {0x14, "EQ"},
            {0x15, "GTE"},
            {0x16, "LTE"},
            {0x17, "JMP"},
            {0x18, "JMPT"},
            {0x19, "JMPi"},
            {0x20, "JMPTi"},
            {0x21, "SET"},
            {0x22, "STO"},
            {0x23, "PUSH"},
            {0x24, "POP"}
        };
        static Dictionary<string, string[]> Layouts = new Dictionary<string, string[]>()
        {
            { "DN", new string[]{"pad","pad","pad"} },
            { "ADD", new string[]{"R","R","R" } },
            { "SUB", new string[]{"R","R","R" } },
            { "MUL", new string[]{"R","R","R" } },
            { "DIV", new string[]{"R","R","R" } },
            { "MOD", new string[]{"R","R","R" } },
            { "AND",new string[]{"R","R","R" } },
            { "OR", new string[]{"R","R","R" } },
            { "XOR",new string[]{"R","R","R" } },
            { "NOT", new string[]{"R","R","R" } },
            { "STL", new string[]{"R","R","R" } },
            { "STR", new string[]{"R","R","R" } },
            { "GT", new string[]{"R","R","R" } },
            { "LT", new string[]{"R","R","R" } },
            { "EQ", new string[]{"R","R","R" } },
            { "GTE", new string[]{"R","R","R" } },
            { "LTE", new string[]{"R","R","R" } },
            { "JMP", new string[]{"AD","AD","pad" } },
            { "JMPT", new string[]{"R","AD","AD" } },
            { "JMPi", new string[]{"R","pad","pad" } },
            { "JMPTi", new string[]{"R","R","pad" } },
            { "SET", new string[]{"R","V","V" } },
            { "STO", new string[]{"R","AD","AD" } },
            { "PUSH",new string[]{"R","pad","pad" } },
            { "POP",new string[]{"R","pad","pad" } },
        };

        static string[] GetAssembly(List<byte[]> machineCode)
        {
            string[] assembly = new string[machineCode.Count];
            for(int i = 0; i < machineCode.Count;i++)
            {
                assembly[i] = "";

            }
            ;
            for (int i = 0; i < machineCode.Count; i++)
            {
                string OpCode = "";
                byte currentByte = 0;
                for (int x = 0; x < machineCode[i].Length; x++)
                {
                    currentByte = machineCode[i][x];
                    if (x == 0)
                    {
                        OpCode = OpCodes[currentByte];
                        assembly[i] = assembly[i] + OpCodes[currentByte] + " ";
                    }
                    else if (Layouts[OpCode][x / 2] == "R")
                    {
                        if (x / 2 == 3)
                        {
                            assembly[i] = assembly[i] + $"R{currentByte}";
                        }
                        else
                        {
                            assembly[i] = assembly[i] + $"R{currentByte}" + " ";
                        }
                    }
                    else
                    {
                        assembly[i] = assembly[i] + currentByte.ToString() + machineCode[i][x+1];
                        x = machineCode[i].Length;
                    }
                }
            }
            return assembly;
        }

        static void Main(string[] args)
        {
            byte[] bytes = File.ReadAllBytes("C:/Users/mason.lee/source/repos/VMAssembler/VMAssembler/MachineCode.txt");
            Span<byte> machineCode = bytes.AsSpan<byte>();

            List<byte[]> instructions = new List<byte[]>();
            for (int i = 0; i < machineCode.Length / 4; i++)
            {
                instructions.Add(machineCode.Slice(i * 4, 4).ToArray());
            }

            string[] thing = GetAssembly(instructions);
            File.WriteAllLines("C:/Users/mason.lee/source/repos/VMAssembler/Disassembler/Dissassembly.txt", thing);
        }
    }
}
