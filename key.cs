using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DES
{
    internal class key
    {
        public string[] GenerateKeyRound()
        {
            string inputString = "zershzaf";
            // Convert the input string to hexadecimal
            string hex = StringToHex(inputString);

            // Convert the hexadecimal string to binary
            string binaryString = HexToBinary(hex);

            // Apply PC1 permutation to the binary string
            string permutedKey = ApplyPC1(binaryString);

            // Split the permuted key into left and right halves
            string left = permutedKey.Substring(0, 28);
            string right = permutedKey.Substring(28, 28);

            // Generate the round keys using the left and right halves
            string[] roundKeys = GenerateRoundKeys(left, right);

            // Return the generated round keys
            return roundKeys;
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

        static string HexToBinary(string hex)
        {
            string binary = String.Join(String.Empty, hex.Select(
                c => Convert.ToString(Convert.ToInt32(c.ToString(), 16), 2).PadLeft(4, '0')
            ));
            return binary;
        }

        static string StringToHex(string input)
        {
            StringBuilder hex = new StringBuilder();

            foreach (char c in input)
            {
                hex.Append(((int)c).ToString("X2"));
            }

            return hex.ToString();
        }

        static string ApplyPC1(string binaryKey)
        {

            int[] pc1Table = {
    57, 49, 41, 33, 25, 17,  9,
     1, 58, 50, 42, 34, 26, 18,
    10,  2, 59, 51, 43, 35, 27,
    19, 11,  3, 60, 52, 44, 36,
    63, 55, 47, 39, 31, 23, 15,
     7, 62, 54, 46, 38, 30, 22,
    14,  6, 61, 53, 45, 37, 29,
    21, 13,  5, 28, 20, 12,  4
};


            char[] output = new char[56];
            for (int i = 0; i < pc1Table.Length; i++)
            {
                output[i] = binaryKey[pc1Table[i] - 1];
            }
            return new string(output);
        }

        private (uint left, uint right) CircularShift(uint left, uint right, int round)
        {
            int[] shifts = { 1, 1, 2, 2, 2, 2, 1, 2, 2, 2, 2, 1, 2, 2, 2, 1 };
            int shiftAmount = shifts[round];
            left = (left << shiftAmount) | (left >> (28 - shiftAmount));
            right = (right << shiftAmount) | (right >> (28 - shiftAmount));
            left &= 0x0FFFFFFF;
            right &= 0x0FFFFFFF;
            return (left, right);
        }

        static string[] GenerateRoundKeys(string leftHalf, string rightHalf)
        {
            int[] pc2Table = {
            14, 17, 11, 24, 1, 5, 3, 28, 15, 6, 21, 10, 23, 19, 12, 4,
            26, 8, 16, 7, 27, 20, 13, 2, 41, 52, 31, 37, 47, 55, 30, 40,
            51, 45, 33, 48, 44, 49, 39, 56, 34, 53, 46, 42, 50, 36, 29, 32
        };

            string[] roundKeys = new string[16];
            for (int round = 0; round < 16; round++)
            {
                int shiftCount = (round == 0 || round == 1 || round == 8 || round == 15) ? 1 : 2;
                leftHalf = LeftShift(leftHalf, shiftCount);
                rightHalf = LeftShift(rightHalf, shiftCount);
                string combined = leftHalf + rightHalf;
                roundKeys[round] = ApplyPC2(combined, pc2Table);
            }

            return roundKeys;
        }

        static string LeftShift(string half, int shiftCount)
        {
            return half.Substring(shiftCount) + half.Substring(0, shiftCount);
        }

        static string ApplyPC2(string combinedKey, int[] pc2Table)
        {
            char[] output = new char[48];
            for (int i = 0; i < pc2Table.Length; i++)
            {
                output[i] = combinedKey[pc2Table[i] - 1];
            }
            return new string(output);
        }

    }
}
