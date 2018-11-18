// ======================================
// Copyright © 2018 Vadim Prokopchuk. All rights reserved.
// Contacts: mailvadimprokopchuk@gmail.com
// License:  http://opensource.org/licenses/MIT
// ======================================

using System;
using System.Collections.Generic;
using CryptoProviders.Des.Contracts;

namespace CryptoProviders.Des
{
    public class DesCryptoProvider : IDesCryptoProvider
    {
        private readonly IDesCryptoTransform _desCryptoTransform;
        private readonly IDesCryptoSettings _desCryptoSettings;
        private readonly int _blockSize;

        public DesCryptoProvider(IDesCryptoTransform desCryptoTransform, IDesCryptoSettings desCryptoSettings)
        {
            this._desCryptoTransform = desCryptoTransform;
            this._desCryptoSettings = desCryptoSettings;
            this._blockSize = desCryptoSettings.BlockSize;
        }

        public byte[] Decrypt(byte[] data, byte[] key) => ProcessInner(data, key, GeneratorMode.Decrypt);

        public byte[] Encrypt(byte[] data, byte[] key) => ProcessInner(data, key, GeneratorMode.Encrypt);

        private byte[] ProcessInner(byte[] data, byte[] key, GeneratorMode mode)
        {
            var subKeys = _desCryptoTransform.GenerateSubKeys(key);
            var result = new byte[data.Length];
            var block = new byte[_blockSize];

            for (var i = 0; i < data.Length / _blockSize; i++)
            {
                Array.Copy(data, _blockSize * i, block, 0, _blockSize);
                Array.Copy(mode == GeneratorMode.Decrypt ? DecryptBlock(block, subKeys) : EncryptBlock(block, subKeys),
                    0, result, _blockSize * i, _blockSize);
            }

            return result;
        }

        private byte[] DecryptBlock(byte[] data, IReadOnlyList<byte[]> subkeys) => GenerateBlock(data, subkeys, GeneratorMode.Decrypt);

        private byte[] EncryptBlock(byte[] data, IReadOnlyList<byte[]> subKeys) => GenerateBlock(data, subKeys, GeneratorMode.Encrypt);

        private byte[] GenerateBlock(byte[] data, IReadOnlyList<byte[]> subKeys, GeneratorMode mode)
        {
            var message = _desCryptoTransform.Map(data, _desCryptoSettings.InputPermutation);
            var blockSize = _desCryptoSettings.InputPermutation.Length;
            var left = _desCryptoTransform.Map(message, 0, blockSize / 2);
            var right = _desCryptoTransform.Map(message, blockSize / 2, blockSize / 2);

            for (var i = 0; i < subKeys.Count; i++)
            {
                var buffer = right;

                right = _desCryptoTransform.Map(right, _desCryptoSettings.ExpansionPermutation);
                right = _desCryptoTransform.Xor(right, GetSubKey(i));
                right = _desCryptoTransform.Replace(right);
                right = _desCryptoTransform.Map(right, _desCryptoSettings.Permutation);
                right = _desCryptoTransform.Xor(left, right);
                left = buffer;
            }

            var concatinationResult = _desCryptoTransform.Concat(right, blockSize / 2, left, blockSize / 2);

            return _desCryptoTransform.Map(concatinationResult, _desCryptoSettings.FinalPermutation);

            byte[] GetSubKey(int index) =>
                mode == GeneratorMode.Decrypt ? subKeys[subKeys.Count - index - 1] : subKeys[index];
        }

        private enum GeneratorMode
        {
            Encrypt = 1,
            Decrypt = 2
        }
    }
}