using System;
using System.IO;
using System.Linq;

namespace FrameGetter
{
    class Program
    {
        private static string MoviePath = @"/home/paulm/Pictures/test/";

        private static int Width = 320;
        private static int Height = 240;

        static void Main(string[] args)
        {
            // for (int i = 1; i < 488; i++)
            // {
            //     var bytes = File.ReadAllBytes(Path.Combine(MoviePath, "output" + i.ToString().PadLeft(4, '0') + ".bmp")).Skip(54).ToArray().Reverse().ToArray();
            //     File.WriteAllBytes("/home/paulm/Pictures/test/raw/" + "output" + i.ToString().PadLeft(4, '0') + ".raw"  , bytes);
            // }

            
            
            
            
            
            int totalSpace = 0;
            int noIntermediaryFrames = 10;
            
            
            for (int frame = 1; frame <= 468; frame += noIntermediaryFrames)
            {
                var referenceImage = File.ReadAllBytes("/home/paulm/Pictures/test/raw/" + "output" +
                                                       frame.ToString().PadLeft(4, '0') + ".raw");
                var referenceBlocks = new byte[4800][];
                var currentBlock = 0;
                for (int j = 0; j < Height / 4; j++)
                {
                    for (int k = 0; k < Width / 4; k++)
                    {
                        referenceBlocks[currentBlock] = new byte[16];
                        referenceBlocks[currentBlock][0] = referenceImage[k * 4 + j * Width + 0];
                        referenceBlocks[currentBlock][1] = referenceImage[k * 4 + j * Width + 1];
                        referenceBlocks[currentBlock][2] = referenceImage[k * 4 + j * Width + 2];
                        referenceBlocks[currentBlock][3] = referenceImage[k * 4 + j * Width + 3];
                        referenceBlocks[currentBlock][4] = referenceImage[k * 4 + j * Width * 4 + 0 + Width * 1];
                        referenceBlocks[currentBlock][5] = referenceImage[k * 4 + j * Width * 4 + 1 + Width * 1];
                        referenceBlocks[currentBlock][6] = referenceImage[k * 4 + j * Width * 4 + 2 + Width * 1];
                        referenceBlocks[currentBlock][7] = referenceImage[k * 4 + j * Width * 4 + 3 + Width * 1];
                        referenceBlocks[currentBlock][8] = referenceImage[k * 4 + j * Width * 4 + 0 + Width * 2];
                        referenceBlocks[currentBlock][9] = referenceImage[k * 4 + j * Width * 4 + 1 + Width * 2];
                        referenceBlocks[currentBlock][10] = referenceImage[k * 4 + j * Width * 4 + 2 + Width * 2];
                        referenceBlocks[currentBlock][11] = referenceImage[k * 4 + j * Width * 4 + 3 + Width * 2];
                        referenceBlocks[currentBlock][12] = referenceImage[k * 4 + j * Width * 4 + 0 + Width * 3];
                        referenceBlocks[currentBlock][13] = referenceImage[k * 4 + j * Width * 4 + 1 + Width * 3];
                        referenceBlocks[currentBlock][14] = referenceImage[k * 4 + j * Width * 4 + 2 + Width * 3];
                        referenceBlocks[currentBlock][15] = referenceImage[k * 4 + j * Width * 4 + 3 + Width * 3];
                        currentBlock++;
                    }
                }

                int sameBlocks = 0;
                for (int i = frame + 1; i < frame + noIntermediaryFrames; i++)
                {
                    int cBlock = 0;
                    var bytes = File.ReadAllBytes("/home/paulm/Pictures/test/raw/" + "output" +
                                                  i.ToString().PadLeft(4, '0') + ".raw");
                    // Console.WriteLine("TESTING IMAGE " + i);
                    for (int j = 0; j < Height / 4; j++)
                    {
                        for (int k = 0; k < Width / 4; k++)
                        {
                            var block = new byte[16];
                            block[0] = bytes[k * 4 + j * Width + 0];
                            block[1] = bytes[k * 4 + j * Width + 1];
                            block[2] = bytes[k * 4 + j * Width + 2];
                            block[3] = bytes[k * 4 + j * Width + 3];
                            block[4] = bytes[k * 4 + j * Width * 4 + 0 + Width * 1];
                            block[5] = bytes[k * 4 + j * Width * 4 + 1 + Width * 1];
                            block[6] = bytes[k * 4 + j * Width * 4 + 2 + Width * 1];
                            block[7] = bytes[k * 4 + j * Width * 4 + 3 + Width * 1];
                            block[8] = bytes[k * 4 + j * Width * 4 + 0 + Width * 2];
                            block[9] = bytes[k * 4 + j * Width * 4 + 1 + Width * 2];
                            block[10] = bytes[k * 4 + j * Width * 4 + 2 + Width * 2];
                            block[11] = bytes[k * 4 + j * Width * 4 + 3 + Width * 2];
                            block[12] = bytes[k * 4 + j * Width * 4 + 0 + Width * 3];
                            block[13] = bytes[k * 4 + j * Width * 4 + 1 + Width * 3];
                            block[14] = bytes[k * 4 + j * Width * 4 + 2 + Width * 3];
                            block[15] = bytes[k * 4 + j * Width * 4 + 3 + Width * 3];

                            int difference = 0;
                            for (int l = 0; l < 16; l++)
                            {
                                difference += Math.Abs(referenceBlocks[cBlock][l] - block[l]);
                            }

                            if (difference <= 64) sameBlocks++;

                            cBlock++;
                        }
                    }
                }

                totalSpace += (4800 * noIntermediaryFrames - sameBlocks) * 3;
                Console.WriteLine("frames" + frame + " - " + (frame + 4) + " TAkE UP " + ((4800 * noIntermediaryFrames - sameBlocks) * 3 / 1024.0));
            }
            
            Console.WriteLine($"INITIAL VIDEO BITRATE {320 * 240 * 30 / 1024.0} KBPS vs {totalSpace / 468 * 30 / 1024.0 } KBPS.");
            
            
        }
    }
}