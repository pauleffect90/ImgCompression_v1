using System;
using System.IO;
using System.Linq;
using BitMiracle.LibJpeg;
// 6 x 6 = 36 * 8 = 288
// 16 + 72 = 88



namespace ImgCompression
{
    
  
    
    class Program
    {
        

        public class Block
        {
            private byte[] Payload = new byte[16];
            private byte[] Compressed = new byte[16];
            private byte[] Decompressed = new byte[16];
            private int _index = 0;
            public void Append(byte value)
            {
                Payload[_index++] = value;
                if (_index == 16)
                {
                   Decompressed = CDecompress(Payload);
                }
            }

            public byte[] GetLine(int line)
            {
                switch (line)
                {
                    case 0:
                        return Decompressed.Take(4).ToArray();
                    case 1:
                        return Decompressed.Skip(4).Take(4).ToArray();
                    case 2:
                        return Decompressed.Skip(8).Take(4).ToArray();
                    default:
                        return Decompressed.Skip(12).Take(4).ToArray();
                }
            }
            
            public byte[] GetOriginal(int line)
            {
                switch (line)
                {
                    case 0:
                        return Payload.Take(4).ToArray();
                    case 1:
                        return Payload.Skip(4).Take(4).ToArray();
                    case 2:
                        return Payload.Skip(8).Take(4).ToArray();
                    default:
                        return Payload.Skip(12).Take(4).ToArray();
                }
            }

            public static int HighDetailBlock = 0;
            public static int LowDetailBlock = 0;
            
            byte[] CDecompress(byte[] payload)
            {
                var retVal = new byte[16];
                var mean = 0;
                for (int i = 0; i < 16; i++)
                {
                    mean += payload[i];
                }
                mean /= 16;
                
                var hMean = 0;
                var lMean = 0;
                var hQ = 0;
                var lQ = 0;
                var stepValue = 0;
                var Q = new int[4];

                var compressed = new byte[3];
                var byteMap = new byte[16];
                
                for (int i = 0; i < 16; i++)
                {
                    if (payload[i] > mean)
                    {
                        hQ++;
                        hMean += payload[i];
                        byteMap[i] = 1;
                    }
                    else
                    {
                        lQ++;
                        lMean += payload[i];
                        byteMap[i] = 0;
                    }
                }

                compressed[0] = (byte)(hMean/2);
                compressed[1] = (byte) lMean;
                compressed[2] = (byte)(byteMap[0] | byteMap[3] << 1 | byteMap[5] << 2 | byteMap[6] << 3 | byteMap[9] << 4 | byteMap[10] << 5 | byteMap[12] << 6 | byteMap[15] << 7);

                // COMPUTE DIFFERENCES SUM
                var sum = 0;
                for (int i = 0; i < 16; i++)
                {
                    sum += Math.Abs(mean - payload[i]);
                }

                if (sum <= 50)
                {
                    LowDetailBlock++;
                }
                else
                {
                    HighDetailBlock++;
                }

                if (hQ == 0) hQ = 1;
                if (lQ == 0) hQ = 1;
                hMean /= hQ;
                lMean /= lQ;
                stepValue = (hMean - lMean) / 3;
                Q[0] = lMean;
                Q[3] = hMean;
                Q[0] /= 4;                
                Q[3] /= 4;                
                Q[0] *= 4;                
                Q[3] *= 4;                
                
                Q[1] = lMean + stepValue;
                Q[2] = Q[1] + stepValue;
                
                

                var decomputedValues = new int[16];
                for (int i = 0; i < 16; i++)
                {
                    if (payload[i] < Q[0])
                    {
                        decomputedValues[i] = Q[0];
                        continue;
                    }

                    if (payload[i] < Q[1])
                    {
                        if (payload[i] - Q[0] < Q[1] - payload[i])
                        {
                            decomputedValues[i] = Q[0];
                        }
                        else
                        {
                            decomputedValues[i] = Q[1];
                        }

                        continue;
                    }

                    if (payload[i] < Q[2])
                    {
                        if (payload[i] - Q[1] < Q[2] - payload[i])
                        {
                            decomputedValues[i] = Q[1];
                        }
                        else
                        {
                            decomputedValues[i] = Q[2];
                        }

                        continue;
                    }

                    if (payload[i] < Q[3])
                    {
                        if (payload[i] - Q[2] < Q[3] - payload[i])
                        {
                            decomputedValues[i] = Q[2];
                        }
                        else
                        {
                            decomputedValues[i] = Q[3];
                        }

                        continue;
                    }

                    decomputedValues[i] = Q[3];
                }

                decomputedValues[0] = (decomputedValues[1] + decomputedValues[4]) / 2;
                decomputedValues[10] = (decomputedValues[11] + decomputedValues[14]) / 2;
                decomputedValues[3] = (decomputedValues[2] + decomputedValues[7]) / 2;
                decomputedValues[10] = (decomputedValues[9] + decomputedValues[14]) / 2;
                decomputedValues[5] = (decomputedValues[4] + decomputedValues[1]) / 2;
                decomputedValues[15] = (decomputedValues[14] + decomputedValues[11]) / 2;
                decomputedValues[6] = (decomputedValues[7] + decomputedValues[2]) / 2;
                decomputedValues[13] = (decomputedValues[14] + decomputedValues[9]) / 2;
                
                
                for (int i = 0; i < 16; i++)
                {
                    retVal[i] = Convert.ToByte(decomputedValues[i]);
                }
                
                
                
                
                return retVal;
            }
        }

        private static string BASE_PATH = @"/home/paulm/RiderProjects/ImgCompression/ImgCompression/bin/Debug/netcoreapp3.1/COMPRESSION/";
        
        static void Main(string[] args)
        {
            var bytes = File.ReadAllBytes("lena_gray.raw");
            var blocks = new Block[640 * 480 / 16];

            var width = 640;
            var height = 480;
            var blockPerLine = width / 4;
            var blockPerRow = height / 4;
            
            for (int i = 0; i < blocks.Length; i++)
            {
                blocks[i] = new Block();
            }
            
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < blockPerLine; j++)
                {
                    // Console.WriteLine($"PROCESSING BLOCK {i/4*128 + j} BYTES {i*512+j*4+0}  {i*512+j*4+1}  {i*512+j*4+2}  {i*512+j*4+3}");
                    // Console.WriteLine($"PROCESSING BLOCK {i/4*128 + j} BYTES {i*512+j*4+1}");
                    // Console.WriteLine($"PROCESSING BLOCK {i/4*128 + j} BYTES {i*512+j*4+2}");
                    // Console.WriteLine($"PROCESSING BLOCK {i/4*128 + j} BYTES {i*512+j*4+3}");
                    blocks[i/4*blockPerLine + j].Append(bytes[i*width+j*4+0]);
                    blocks[i/4*blockPerLine + j].Append(bytes[i*width+j*4+1]);
                    blocks[i/4*blockPerLine + j].Append(bytes[i*width+j*4+2]);
                    blocks[i/4*blockPerLine + j].Append(bytes[i*width+j*4+3]);
                }
            }

            using (var fs = new FileStream("output.raw", FileMode.Create))
            {
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < blockPerLine; j++)
                    {
                        fs.Write(blocks[i / 4 * blockPerLine + j].GetLine(i%4));
                    }
                    fs.Flush();
                }
                fs.Close();
            }

            Console.WriteLine($"GOT {Block.HighDetailBlock} High Blocks - {Block.LowDetailBlock} Low Blocks");
            Console.WriteLine($"SIZE: {Block.HighDetailBlock * 3 + Block.LowDetailBlock} BYTES vs {512*512} BYTES. COMPRESSION RATE IS {8.0 / (262144.0 / (Block.HighDetailBlock * 3.0 + Block.LowDetailBlock))} BPP.");
            Console.WriteLine($"SIZE: {Block.HighDetailBlock * 3 + Block.LowDetailBlock * 3} BYTES vs {512*512} BYTES. ");
           
            
            // Console.WriteLine($"{computedValues[0]:X} {computedValues[1]:X} {computedValues[2]:X} {computedValues[3]:X}\n" +
            //                   $"{computedValues[4]:X} {computedValues[5]:X} {computedValues[6]:X} {computedValues[7]:X}\n" +
            //                   $"{computedValues[8]:X} {computedValues[9]:X} {computedValues[10]:X} {computedValues[11]:X}\n" +
            //                   $"{computedValues[12]:X} {computedValues[13]:X} {computedValues[14]:X} {computedValues[15]:X}\n");
            

        }
    }
}