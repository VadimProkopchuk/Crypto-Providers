// ======================================
// Copyright © 2018 Vadim Prokopchuk. All rights reserved.
// Contacts: mailvadimprokopchuk@gmail.com
// License:  http://opensource.org/licenses/MIT
// ======================================

namespace CryptoProviders.Des.Contracts
{
    public interface IDesCryptoSettings
    {
        int BlockSize { get; }
        byte[] InputPermutation { get; }
        byte[] FinalPermutation { get; }
        byte[] ExpansionPermutation { get; }
        byte[][] SubstitutionBoxes { get; }
        byte[] Permutation { get; }
        byte[] Pc1Permutation { get; }
        byte[] Pc2Permutation { get; }
        byte[] Rotations { get; }
    }
}
