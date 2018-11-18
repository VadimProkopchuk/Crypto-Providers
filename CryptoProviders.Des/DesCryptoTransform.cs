// ======================================
// Copyright © 2018 Vadim Prokopchuk. All rights reserved.
// Contacts: mailvadimprokopchuk@gmail.com
// License:  http://opensource.org/licenses/MIT
// ======================================

using CryptoProviders.Des.Contracts;

namespace CryptoProviders.Des
{
    public class DesCryptoTransform : IDesCryptoTransform
    {
        private readonly IDesCryptoSettings _desCryptoSettings;
        private readonly int _blockSize;

        public DesCryptoTransform(IDesCryptoSettings desCryptoSettings)
        {
            this._desCryptoSettings = desCryptoSettings;
            this._blockSize = desCryptoSettings.BlockSize;
        }

        public byte[] Concat(byte[] left, int leftLength, byte[] right, int rightLength)
        {
            var numberOfBytes = (leftLength + rightLength - 1) / _blockSize + 1;
            var output = new byte[numberOfBytes];
            var j = 0;

            for (var i = 0; i < leftLength; i++)
                SetByte(output, j++, GetByte(left, i));

            for (var i = 0; i < rightLength; i++)
                SetByte(output, j++, GetByte(right, i));

            return output;
        }

        public byte[][] GenerateSubKeys(byte[] key)
        {
            var numberOfSubkeys = _desCryptoSettings.Rotations.Length;
            var activeKey = Map(key, _desCryptoSettings.Pc1Permutation);
            var halfKeySize = _desCryptoSettings.Pc1Permutation.Length / 2;

            var left = Map(activeKey, 0, halfKeySize);
            var right = Map(activeKey, halfKeySize, halfKeySize);
            var subkeys = new byte[numberOfSubkeys][];

            for (var i = 0; i < numberOfSubkeys; i++)
            {
                left = Rotate(left, halfKeySize, _desCryptoSettings.Rotations[i]);
                right = Rotate(right, halfKeySize, _desCryptoSettings.Rotations[i]);

                subkeys[i] = Map(Concat(left, halfKeySize, right, halfKeySize), _desCryptoSettings.Pc2Permutation);
            }

            return subkeys;
        }

        public int GetByte(byte[] data, int pos) => data[pos / _blockSize] >> (_blockSize - ((pos % _blockSize) + 1)) & 0x0001;

        public byte[] Map(byte[] data, byte[] map)
        {
            var output = new byte[(map.Length - 1) / _blockSize + 1];

            for (var i = 0; i < map.Length; i++)
                SetByte(output, i, GetByte(data, map[i] - 1));

            return output;
        }

        public byte[] Map(byte[] input, int pos, int length)
        {
            var output = new byte[(length - 1) / _blockSize + 1];

            for (var i = 0; i < length; i++)
                SetByte(output, i, GetByte(input, pos + i));

            return output;
        }

        public byte[] Replace(byte[] data)
        {
            data = Split(data, 6);

            var output = new byte[data.Length / 2];
            var leftHalfByte = 0;

            for (var i = 0; i < data.Length; i++)
            {
                var halfByte = _desCryptoSettings.SubstitutionBoxes[i][GetJ(data[i])];

                if (i % 2 == 0)
                    leftHalfByte = halfByte;
                else
                    output[i / 2] = (byte)((_blockSize * 2) * leftHalfByte + halfByte);
            }

            return output;

            int GetJ(byte val) => _blockSize * 4 * (val >> 7 & 0x0001) + (val >> 2 & 0x0001) + val >> 3 & 0x000F;
        }

        public byte[] Rotate(byte[] data, int length, int step)
        {
            var output = new byte[(length - 1) / _blockSize + 1];

            for (var i = 0; i < length; i++)
                SetByte(output, i, GetByte(data, (i + step) % length));

            return output;
        }

        public void SetByte(byte[] data, int pos, int value) => data[pos / _blockSize] = (byte)((value << (_blockSize - ((pos % _blockSize) + 1))) | data[pos / _blockSize]);

        public byte[] Split(byte[] data, int length)
        {
            var numberOfBytes = (_blockSize * data.Length - 1) / length + 1;
            var output = new byte[numberOfBytes];

            for (var i = 0; i < numberOfBytes; i++)
                for (var j = 0; j < length; j++)
                    SetByte(output, _blockSize * i + j, GetByte(data, length * i + j));

            return output;
        }

        public byte[] Xor(byte[] first, byte[] second)
        {
            var output = new byte[first.Length];

            for (var i = 0; i < first.Length; i++)
                output[i] = (byte)(first[i] ^ second[i]);

            return output;
        }
    }
}