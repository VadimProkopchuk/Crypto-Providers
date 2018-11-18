// ======================================
// Copyright © 2018 Vadim Prokopchuk. All rights reserved.
// Contacts: mailvadimprokopchuk@gmail.com
// License:  http://opensource.org/licenses/MIT
// ======================================

namespace CryptoProviders.Des.Contracts
{
    public interface IDesCryptoTransform
    {
        int GetByte(byte[] data, int pos);
        void SetByte(byte[] data, int pos, int value);
        byte[] Map(byte[] data, byte[] map);
        byte[] Map(byte[] input, int pos, int length);
        byte[] Xor(byte[] first, byte[] second);
        byte[] Concat(byte[] left, int leftLength, byte[] right, int rightLength);
        byte[] Rotate(byte[] data, int length, int step);
        byte[] Split(byte[] data, int length);
        byte[] Replace(byte[] data);
        byte[][] GenerateSubKeys(byte[] key);
    }
}