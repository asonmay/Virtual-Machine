using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace VMAssembler
{
    class Program
    {
        static Dictionary<string, byte> OpCodes = new Dictionary<string, byte>()
        {
            {"DN", 0x00 },
            {"ADD", 0x01 },
            {"SUB", 0x02 },
            {"MUL", 0x03 },
            {"DIV", 0x04 },
            {"MOD", 0x05 },
            {"AND", 0x06 },
            {"OR", 0x07 },
            {"XOR", 0x08 },
            {"NOT", 0x09 },
            {"STR", 0x10 },
            {"STL", 0x11 },
            {"GT", 0x12 },
            {"LT", 0x13 },
            {"EQ", 0x14 },
            {"GTE", 0x15 },
            {"LTE", 0x16 },
            {"JMP", 0x17 },
            {"JMPT", 0x18 },
            {"JMPi", 0x19 },
            {"JMPTi", 0x20 },
            {"SET", 0x21 },
            {"STO", 0x22 },
            {"PUSH", 0x23 },
            {"POP", 0x24 },
            {"STOi",0x26 }
        };
        static Dictionary<string, int> Colors = new Dictionary<string, int>()
        {
            {"Red", 1},
            {"Black", 0},
            {"Blue", 2},
            {"Green", 3},
            {"Yellow", 4},
            {"Orange", 5},
            {"Purple", 6},
            {"Pink", 7},
            {"Grey", 8},
            {"Blown", 9},
            {"White", 10}
        };
        static byte[] GetMachineCode(string[] assembly)
        {
            List<string> assemblyNoMacros = new List<string>();
            List<byte> machineCode = new List<byte>();
            for(int h = 0; h < assembly.Length;h++)
            {
                assemblyNoMacros.Add(assembly[h]);
                string currentByteMachine = "";
                int currentByte = 0;
                char current = new char();
                for (int x = 0; x <= assembly[h].Length; x++)
                {
                    if (x != assembly[h].Length)
                    {
                        current = assembly[h][x];
                    }

                    if (current != ' ' && x != assembly[h].Length)
                    {
                        currentByteMachine = currentByteMachine + current;
                    }

                    if (currentByteMachine == "PRNT")
                    {
                        string words = "";
                        for (int m = 4; m < assembly[h].Length; m++)
                        {
                            words += assembly[h][m];
                        }
                        for (int n = 0; n < words.Length; n++)
                        {
                            assemblyNoMacros.Remove(assembly[h]);
                            assemblyNoMacros.Add($"SET R11 {(byte)words[n]}");
                            assemblyNoMacros.Add($"SET R12 1");
                        }
                        x = assembly[h].Length + 1;
                        currentByte = 4;
                        continue;
                    }

                    if (currentByteMachine == "SPX")
                    {
                        string xPos = "";
                        string yPos = "";
                        string color = "";
                        int thing = 0;
                        for (int m = 4; m < assembly[h].Length; m++)
                        {
                            if (thing == 0)
                            {
                                xPos += assembly[h][m];
                            }
                            else if (thing == 1)
                            {
                                yPos += assembly[h][m];
                            }
                            else
                            {
                                color += assembly[h][m];
                            }
                            if (assembly[h][m] == ' ')
                            {
                                thing++;
                            }
                        }
                        assemblyNoMacros.Remove(assembly[h]);
                        assemblyNoMacros.Add($"SET R23 {Colors[color]}");
                        assemblyNoMacros.Add($"MUL R22 {xPos.Trim(' ')} R26");
                        assemblyNoMacros.Add($"ADD R22 R22 {yPos.Trim(' ')}");
                        assemblyNoMacros.Add($"STOi R23 R22");
                    }
                    if (currentByteMachine == "SC")
                    {
                        string color = "";
                        bool thing = false;
                        for (int m = 3; m < assembly[h].Length; m++)
                        {
                            color += assembly[h][m];
                        }
                        assemblyNoMacros.Remove(assembly[h]);
                        for (int w = 0; w < 16; w++)
                        {
                            for(int g = 0; g < 16; g++)
                            {
                                assemblyNoMacros.Add($"SET R7 {Colors[color]}");
                                assemblyNoMacros.Add($"STO R7 {w * 16 + g}");
                            }
                        }
                        
                    }
                    if (currentByteMachine == "RAND")
                    {
                        string maxValue = "";
                        for (int m = 4; m < assembly[h].Length; m++)
                        {
                            maxValue += assembly[h][m];
                        }
                        assemblyNoMacros.Remove(assembly[h]);
                        assemblyNoMacros.Add("SET R10 1");
                        assemblyNoMacros.Add($"SET R8 {maxValue.Trim(' ')}");
                        assemblyNoMacros.Add($"MOD R9 R9 R8");
                        x = assembly[h].Length + 1;
                        currentByte = 4;
                        continue;
                    }
                }

                
            }
            ;
            Dictionary<string, byte> Lables = new Dictionary<string, byte>();
            List<string> assemblyNoLables = new List<string>();
            List<string> ListOfLables = new List<string>();
            for (int s = 0; s < assemblyNoMacros.Count; s++)
            {
                if(assemblyNoMacros[s][assemblyNoMacros[s].Length-1] == ':')
                {
                    Lables.Add(assemblyNoMacros[s].Trim(':'), (byte)(s - Lables.Count));
                    ListOfLables.Add(assemblyNoMacros[s].Trim(':'));
                }
                else
                {
                    assemblyNoLables.Add(assemblyNoMacros[s]);
                }
            }
            ;
            for (int i = 0; i < assemblyNoLables.Count; i++)
            {
                string currentByteMachine = "";
                int currentByte = 0;
                char current = new char();
                for (int x = 0; x <= assemblyNoLables[i].Length; x++)
                {
                    if (x != assemblyNoLables[i].Length)
                    {
                        current = assemblyNoLables[i][x];
                    }

                    if (current != ' ' && x != assemblyNoLables[i].Length)
                    {
                        currentByteMachine = currentByteMachine + current;
                    }

                    if (current == ' ' || x == assemblyNoLables[i].Length)
                    {
                        if (currentByte == 0)
                        {
                            machineCode.Add(OpCodes[currentByteMachine]);
                        }
                        else if (currentByteMachine[0] == 'R' && !Lables.ContainsKey(currentByteMachine))
                        {
                            machineCode.Add(byte.Parse(currentByteMachine.Trim(currentByteMachine[0])));
                        }
                        else if(currentByteMachine[0] >= '0' && currentByteMachine[0] <= '9')
                        {
                            machineCode.Add(0);
                            machineCode.Add(byte.Parse(currentByteMachine));
                            currentByte++;
                        }
                        else if(Lables.ContainsKey(currentByteMachine))
                        {
                            machineCode.Add(0);
                            machineCode.Add(Lables[currentByteMachine]);
                            currentByte++;
                        }
                        else
                        {
                            machineCode.Add((byte)currentByteMachine[0]);
                        }
                        currentByteMachine = "";
                        currentByte++;
                    }
                }
                if(currentByte!=4)
                {
                    for(int b = 0; b<4-currentByte;b++)
                    {
                        machineCode.Add(0);
                    }
                }
            }
            ;
            return machineCode.ToArray();
        }
       
        static void Main(string[] args)
        {
            string[] assembly = File.ReadAllLines("C:/Users/mason.lee/source/repos/VMAssembler/VMAssembler/Assemly.txt");

            byte[] thing = GetMachineCode(assembly);
            File.WriteAllBytes("C:/Users/mason.lee/source/repos/VMAssembler/VMAssembler/MachineCode.txt",thing);
            ;
        }
    }
}
