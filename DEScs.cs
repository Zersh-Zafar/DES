using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DES
{
    internal class DEScs
    {
        public List<string> EncryptData(string data, string[] roundKeys)
        {
            string data1 = RemoveNewlines2(data);
            // Convert the input data to binary representation
            string binaryRepresentation = StrToBin(data1);

            // Break the binary representation into 64-bit blocks
            List<string> binaryBlocks = Get64BitBlocks(binaryRepresentation);

            // List to store the final ciphertext blocks
            List<string> result = new List<string>();

            // Loop through each block for encryption
            foreach (string block in binaryBlocks)
            {
                // Apply the initial permutation
                string permutedResult = ApplyInitialPermutation(block);

                // Split the permuted result into left and right halves
                string leftHalf = permutedResult.Substring(0, 32);
                string rightHalf = permutedResult.Substring(32, 32);

                // Perform 16 rounds of encryption
                for (int round = 0; round < 16; round++)
                {
                    string rodk = roundKeys[round];
                    string rightTemp = rightHalf;

                    // Perform a round with the right half and round key
                    string encryption_value = PerformRound(rightHalf, rodk);

                    // XOR the left half with the encryption value to get the new right half
                    rightHalf = XOR(leftHalf, encryption_value);
                    leftHalf = rightTemp;  // Update the left half for the next round
                }

                // Combine the final left and right halves
                string combined = rightHalf + leftHalf;

                // Apply the final permutation
                string ciphertext = ApplyFinalPermutation(combined);

                // Ensure the final output is 64 bits long and convert it to hexadecimal
                string final64BitOutput = Ensure64BitLength(ciphertext);
                result.Add(ConvertBinaryToHex(final64BitOutput));
            }
            // Return the list of encrypted blocks as hexadecimal strings
            return result;
        }

        public List<string> Decrypt(string data, string[] roundKeys)
        {
           
            // Remove newlines from the input data (if any)
            string result_data = RemoveNewlines(data);

            // Convert the hex string to binary representation
            string binaryRepresentation = HexaToBinary(result_data);

            // Split the binary representation into 64-bit blocks
            List<string> binaryBlocks = Get64BitBlocks(binaryRepresentation);

            // List to store the decrypted blocks
            List<string> result = new List<string>();

            // Loop through each 64-bit block for decryption
            foreach (string block in binaryBlocks)
            {
                // Apply the initial permutation to the block
                string permutedResult = ApplyInitialPermutation(block);

                // Split the permuted result into left and right halves
                string leftHalf = permutedResult.Substring(0, 32);
                string rightHalf = permutedResult.Substring(32, 32);

                // Perform 16 rounds of decryption (in reverse order)
                for (int round = 15; round >= 0; round--)
                {
                    string rodk = roundKeys[round];
                    string rightTemp = rightHalf;

                    // Perform the round with the right half and round key
                    string encryption_value = PerformRound(rightHalf, rodk);

                    // XOR the left half with the encryption value to get the new right half
                    rightHalf = XOR(leftHalf, encryption_value);
                    leftHalf = rightTemp;  // Update the left half for the next round
                }

                // Combine the final left and right halves
                string combined = rightHalf + leftHalf;

                // Apply the final permutation to the combined result
                string ciphertext = ApplyFinalPermutation(combined);

                // Ensure the final output is 64 bits long and convert it back to text
                string final64BitOutput = Ensure64BitLength(ciphertext);

                // Debug output for each block
                result.Add(BinaryToText(final64BitOutput));
            }
            // Return the list of decrypted text blocks
            return result;
        }

        string ConvertBinaryToHex(string binaryData)
        {
            StringBuilder hex = new StringBuilder(binaryData.Length / 4);
            for (int i = 0; i < binaryData.Length; i += 4)
            {
                string nibble = binaryData.Substring(i, 4);
                hex.Append(Convert.ToInt32(nibble, 2).ToString("X"));
            }
            if (hex.Length % 2 != 0)
            {
                hex.Insert(0, "0");
            }

            return hex.ToString();
        }

        private string StrToBin(string input)
        {
            StringBuilder binaryRepresentation = new StringBuilder();
            foreach (char c in input)
            {
                binaryRepresentation.Append(Convert.ToString(c, 2).PadLeft(8, '0'));
            }
            return binaryRepresentation.ToString();
        }


        public static List<string> Get64BitBlocks(string binaryString)
        {
            List<string> blocks = new List<string>();
            int length = binaryString.Length;
            for (int i = 0; i < length; i += 64)
            {
                if (i + 64 <= length)
                {
                    blocks.Add(binaryString.Substring(i, 64));
                }
                else
                {
                    string lastBlock = binaryString.Substring(i);
                    blocks.Add(lastBlock.PadRight(64, '0'));
                }
            }

            return blocks;
        }


        private string ApplyInitialPermutation(string binary)
        {
            int[] ipTable = new int[]
            {
        58, 50, 42, 34, 26, 18, 10, 2,
        60, 52, 44, 36, 28, 20, 12, 4,
        62, 54, 46, 38, 30, 22, 14, 6,
        64, 56, 48, 40, 32, 24, 16, 8,
        57, 49, 41, 33, 25, 17, 9, 1,
        59, 51, 43, 35, 27, 19, 11, 3,
        61, 53, 45, 37, 29, 21, 13, 5,
        63, 55, 47, 39, 31, 23, 15, 7
            };

            char[] permuted = new char[64];
            for (int i = 0; i < ipTable.Length; i++)
            {
                permuted[i] = binary[ipTable[i] - 1];
            }
            return new string(permuted);
        }


        private static readonly int[] EBitSelectionTable = new int[]
        {
            32, 1, 2, 3, 4, 5,
            4, 5, 6, 7, 8, 9,
            8, 9, 10, 11, 12, 13,
            12, 13, 14, 15, 16, 17,
            16, 17, 18, 19, 20, 21,
            20, 21, 22, 23, 24, 25,
            24, 25, 26, 27, 28, 29,
            28, 29, 30, 31, 32, 1
        };

        private static string PerformRound(string rightHalf, string roundKey)
        {
            string expandedRight = ExpandRightHalf(rightHalf);
            string xored = XOR(expandedRight, roundKey);
            string sBoxOutput = ApplySBox(xored);
            string pBoxOutput = ApplyPBox(sBoxOutput);

            return pBoxOutput;
        }

        readonly static int[] straight_Permutation =
  { 16, 7, 20, 21,
29, 12, 28, 17,
1, 15, 23, 26,
5, 18, 31, 10,
2, 8, 24, 14,
32, 27, 3, 9,
19, 13, 30, 6,
22, 11, 4, 25};
        private static string ApplyPBox(string sBoxOutput)
        {
            char[] pBoxOutput = new char[32];
            for (int i = 0; i < straight_Permutation.Length; i++)
            {
                pBoxOutput[i] = sBoxOutput[straight_Permutation[i] - 1];
            }

            return new string(pBoxOutput);
        }


        private static string ExpandRightHalf(string rightHalf)
        {
            if (rightHalf.Length != 32)
                throw new ArgumentException("The right half must be 32 bits long.");

            StringBuilder expanded = new StringBuilder(48);
            foreach (int index in EBitSelectionTable)
            {
                expanded.Append(rightHalf[index - 1]);
            }

            return expanded.ToString();
        }


        private static string XOR(string a, string b)
        {
            if (a == null || b == null)
                throw new ArgumentNullException("Input strings cannot be null.");
            if (a.Length != b.Length)
                throw new ArgumentException("Input strings must be of the same length.");

            StringBuilder result = new StringBuilder(a.Length);
            for (int i = 0; i < a.Length; i++)
            {
                result.Append(a[i] == b[i] ? '0' : '1');
            }
            return result.ToString();
        }


        private static readonly int[,] SBox = new int[8, 64]
 {
  // S1
  {
      14, 4, 13, 1, 2, 15, 11, 8, 3, 10, 6, 12, 5, 9, 0, 7,
      0, 15, 7, 4, 14, 2, 13, 1, 10, 6, 12, 11, 9, 5, 3, 8,
      4, 1, 14, 8, 13, 6, 2, 11, 15, 12, 9, 7, 3, 10, 5, 0,
      15, 12, 8, 2, 4, 9, 1, 7, 5, 11, 3, 14, 10, 0, 6, 13
  },
  // S2
  {
      15, 1, 8, 14, 6, 11, 3, 4, 9, 7, 2, 13, 12, 0, 5, 10,
      3, 13, 4, 7, 15, 2, 8, 14, 12, 0, 1, 10, 6, 9, 11, 5,
      0, 14, 7, 11, 10, 4, 13, 1, 5, 8, 12, 6, 9, 3, 2, 15,
      13, 8, 10, 1, 3, 15, 4, 2, 11, 6, 7, 12, 0, 5, 14, 9
  },
  // S3
  {
      10, 0, 9, 14, 6, 3, 15, 5, 1, 13, 12, 7, 11, 4, 2, 8,
      13, 7, 0, 9, 3, 4, 6, 10, 2, 8, 5, 14, 12, 11, 15, 1,
      13, 6, 4, 9, 8, 15, 3, 0, 11, 1, 2, 12, 5, 10, 14, 7,
      1, 10, 13, 0, 6, 9, 8, 7, 4, 15, 14, 3, 11, 5, 2, 12
  },
  // S4
  {
      7, 13, 14, 3, 0, 6, 9, 10, 1, 2, 8, 5, 11, 12, 4, 15,
      13, 8, 11, 5, 6, 15, 0, 3, 4, 7, 2, 12, 1, 10, 14, 9,
      10, 6, 9, 0, 12, 11, 7, 13, 15, 1, 3, 14, 5, 2, 8, 4,
      3, 15, 0, 6, 10, 1, 13, 8, 9, 4, 5, 11, 12, 7, 2, 14
  },
  // S5
  {
      2, 12, 4, 1, 7, 10, 11, 6, 8, 5, 3, 15, 13, 0, 14, 9,
      14, 11, 2, 12, 4, 7, 13, 1, 5, 0, 15, 10, 3, 9, 8, 6,
      4, 2, 1, 11, 10, 13, 7, 8, 15, 9, 12, 5, 6, 3, 0, 14,
      11, 8, 12, 7, 1, 14, 2, 13, 6, 15, 0, 9, 10, 4, 5, 3
  },
  // S6
  {
      12, 1, 10, 15, 9, 2, 6, 8, 0, 13, 3, 4, 14, 7, 5, 11,
      10, 15, 4, 2, 7, 12, 9, 5, 6, 1, 13, 14, 0, 11, 3, 8,
      9, 14, 15, 5, 2, 8, 12, 3, 7, 0, 4, 10, 1, 13, 11, 6,
      4, 3, 2, 12, 9, 5, 15, 10, 11, 14, 1, 7, 6, 0, 8, 13
  },
  // S7
  {
      4, 11, 2, 14, 15, 0, 8, 13, 3, 12, 9, 7, 5, 10, 6, 1,
      13, 0, 11, 7, 4, 9, 1, 10, 14, 3, 5, 12, 2, 15, 8, 6,
      1, 4, 11, 13, 12, 3, 7, 14, 10, 15, 6, 8, 0, 5, 9, 2,
      6, 11, 13, 8, 1, 4, 10, 7, 9, 5, 0, 15, 14, 2, 3, 12
  },
  // S8
  {
      13, 2, 8, 4, 6, 15, 11, 1, 10, 9, 3, 14, 5, 0, 12, 7,
      1, 15, 13, 8, 10, 3, 7, 4, 12, 5, 6, 11, 0, 14, 9, 2,
      7, 11, 4, 1, 9, 12, 14, 2, 0, 6, 10, 13, 15, 3, 5, 8,
      2, 1, 14, 7, 4, 10, 8, 13, 15, 12, 9, 0, 3, 5, 6, 11
  }
 };

        private static string ApplySBox(string xored)
        {
            StringBuilder sBoxResult = new StringBuilder();
            for (int i = 0; i < 8; i++)
            {
                string segment = xored.Substring(i * 6, 6);
                int row = Convert.ToInt32($"{segment[0]}{segment[5]}", 2);
                int col = Convert.ToInt32(segment.Substring(1, 4), 2);
                int sBoxValue = SBox[i, row * 16 + col];
                sBoxResult.Append(Convert.ToString(sBoxValue, 2).PadLeft(4, '0'));
            }

            return sBoxResult.ToString();
        }
        static string ApplyFinalPermutation(string input)
        {
            int[] finalPermutationTable = {
        40, 8, 48, 16, 56, 24, 64, 32,
        39, 7, 47, 15, 55, 23, 63, 31,
        38, 6, 46, 14, 54, 22, 62, 30,
        37, 5, 45, 13, 53, 21, 61, 29,
        36, 4, 44, 12, 52, 20, 60, 28,
        35, 3, 43, 11, 51, 19, 59, 27,
        34, 2, 42, 10, 50, 18, 58, 26,
        33, 1, 41, 9, 49, 17, 57, 25
            };

            char[] output = new char[64];

            for (int i = 0; i < 64; i++)
            {
                output[i] = input[finalPermutationTable[i] - 1];
            }

            return new string(output);
        }

        public static string Ensure64BitLength(string binary)
        {
            int length = binary.Length;
            int padLength = (64 - length % 64) % 64;
            return binary.PadLeft(length + padLength, '0');
        }


        public static string ConvertBinaryToAscii(string binary)
        {
            StringBuilder asciiResult = new StringBuilder();

            for (int i = 0; i < binary.Length; i += 8)
            {
                string byteStr = binary.Substring(i, 8);
                char character = (char)Convert.ToByte(byteStr, 2);
                asciiResult.Append(character);
            }

            return asciiResult.ToString();
        }

        public string RemoveNewlines(string input)
        {
            return input.Replace("\r", "").Replace("\n", "").Replace("\u2019", ""); 
        }
        public string RemoveNewlines2(string input)
        {
            return input.Replace("’", "");
        }

        static string BinaryToText(string binary)
        {
            if (binary.Length % 8 != 0)
                throw new ArgumentException("Binary string length must be a multiple of 8.");

            StringBuilder text = new StringBuilder();

            for (int i = 0; i < binary.Length; i += 8)
            {
                string byteString = binary.Substring(i, 8);
                int asciiValue = Convert.ToInt32(byteString, 2);
                text.Append((char)asciiValue);
            }
            return text.ToString();
        }

        static string HexaToBinary(string hex)
        {

            StringBuilder binary = new StringBuilder();
            foreach (char hexChar in hex)
            {
                int value = Convert.ToInt32(hexChar.ToString(), 16);
                binary.Append(Convert.ToString(value, 2).PadLeft(4, '0'));
            }
            return binary.ToString();
        }

    }
}
