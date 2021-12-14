/*
        _____  _____   _____   _____
       |      |_____| |_____| |_____
       |_____ |     | |       |_____  version 3.0
    Cape Copyright (c) 2012-2018, Giovanni Blu Mitolo All rights reserved.
    Licensed under the Apache License, Version 2.0 (the "License");
    you may not use this file except in compliance with the License.
    You may obtain a copy of the License at
        http://www.apache.org/licenses/LICENSE-2.0
    Unless required by applicable law or agreed to in writing, software
    distributed under the License is distributed on an "AS IS" BASIS,
    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    See the License for the specific language governing permissions and
    limitations under the License.
*/

//
// Cape is developed by @gioblu (Giovanni Blu Mitolo)
// The 3.0 version of Cape can be found at https://github.com/gioblu/Cape/releases/tag/3.0
// The current version of cape can be found at https://github.com/gioblu/Cape
// Other releases of Cape can be found at https://github.com/gioblu/Cape/releases
//

/*
   CapeDotNet Copyright (C) 2018 Pharap (@Pharap)
   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at
        http://www.apache.org/licenses/LICENSE-2.0
   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

//
// A C# port of Cape 3.0, ported by @Pharap (Pharap)
// The latest release can be found at https://github.com/Pharap/CapeDotNet/releases/latest
// The current version can be found at https://github.com/Pharap/CapeDotNet
//

using System;
using System.Collections.Generic;

namespace DataCommunicator.Utils
{
    public class Cape
    {
        private byte[] key;
        private byte reducedKey;
        private byte salt;

        public Cape(byte[] key, byte salt)
        {
            this.key = new byte[key.Length];
            Array.Copy(key, this.key, this.key.Length);
            this.reducedKey = ComputeReducedKey(this.key);
            this.salt = salt;
        }

        public byte Salt
        {
            get { return this.salt; }
            set { this.salt = value; }
        }

        //
        // Main functions
        //

        public byte[] Encrypt(byte[] source, byte initialisationVector)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            byte[] result = new byte[source.Length + 1];
            byte saltedKey = (byte)(this.salt ^ this.reducedKey);
            int lastIndex = result.Length - 1;

            // Encrypt initialisation vector
            result[lastIndex] = (byte)(initialisationVector ^ lastIndex ^ this.key[(lastIndex ^ saltedKey) % this.key.Length]);

            // Encrypt data
            for (int i = 0; i < source.Length; ++i)
                result[i] = (byte)(source[i] ^ initialisationVector ^ i ^ this.key[(i ^ saltedKey) % this.key.Length]);

            return result;
        }

        public void Encrypt(byte[] source, byte[] destination, byte initialisationVector)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (destination.Length <= source.Length)
                throw new ArgumentException("destination must be at least 1 byte larger than source");

            byte saltedKey = (byte)(this.salt ^ this.reducedKey);
            int lastIndex = destination.Length - 1;

            // Encrypt initialisation vector
            destination[lastIndex] = (byte)(initialisationVector ^ lastIndex ^ this.key[(lastIndex ^ saltedKey) % this.key.Length]);

            // Encrypt data
            for (int i = 0; i < source.Length; ++i)
                destination[i] = (byte)(source[i] ^ initialisationVector ^ i ^ this.key[(i ^ saltedKey) % this.key.Length]);
        }

        public byte[] Decrypt(byte[] source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            byte[] result = new byte[source.Length];
            byte saltedKey = (byte)(this.salt ^ this.reducedKey);
            int lastIndex = source.Length - 1;

            // Decrypt initialisation vector
            byte initialisationVector = (byte)(source[lastIndex] ^ lastIndex ^ this.key[(lastIndex ^ saltedKey) % this.key.Length]);

            // Decrypt data
            for (int i = 0; i < source.Length; ++i)
                result[i] = (byte)(source[i] ^ initialisationVector ^ i ^ this.key[(lastIndex ^ saltedKey) % this.key.Length]);

            return result;
        }

        public void Decrypt(byte[] source, byte[] destination)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (destination.Length <= source.Length - 1)
                throw new ArgumentException("destination length must be at least source length - 1");

            byte saltedKey = (byte)(this.salt ^ this.reducedKey);
            int lastIndex = source.Length - 1;

            // Decrypt initialisation vector
            byte initialisationVector = (byte)(source[lastIndex] ^ lastIndex ^ this.key[(lastIndex ^ saltedKey) % this.key.Length]);

            // Decrypt data
            for (int i = 0; i < source.Length; ++i)
                destination[i] = (byte)(source[i] ^ initialisationVector ^ i ^ this.key[(lastIndex ^ saltedKey) % this.key.Length]);
        }

        public byte[] Hash(byte[] source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            byte[] result = new byte[source.Length];
            byte saltedKey = (byte)(this.salt ^ this.reducedKey);

            for (int i = 0; i < source.Length; i++)
            {
                byte a = (byte)(saltedKey ^ i);
                result[i] = (byte)(source[i] ^ a ^ this.key[a % this.key.Length]);
            }

            return result;
        }

        public void Hash(byte[] source, byte[] destination)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (destination == null)
                throw new ArgumentNullException("destination");
            if (destination.Length < source.Length)
                throw new ArgumentNullException("destination length is less than source length");

            byte saltedKey = (byte)(this.salt ^ this.reducedKey);

            for (int i = 0; i < source.Length; i++)
            {
                byte a = (byte)(saltedKey ^ i);
                destination[i] = (byte)(source[i] ^ a ^ this.key[a % this.key.Length]);
            }
        }

        //
        // Enumerable versions
        //

        // source must be an array because the iv is stored at the end rather than the start
        // and enumerating an enumerable to reach its last element might
        // render it unusable for the main encryption
        public IEnumerable<byte> EnumerableEncrypt(byte[] source, byte initialisationVector)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            byte saltedKey = (byte)(this.salt ^ this.reducedKey);

            // Encrypt data
            for (int i = 0; i < source.Length; ++i)
                yield return (byte)(source[i] ^ initialisationVector ^ i ^ this.key[(i ^ saltedKey) % this.key.Length]);

            // Encrypt initialisation vector
            int lastIndex = source.Length;
            yield return (byte)(initialisationVector ^ lastIndex ^ this.key[(lastIndex ^ saltedKey) % this.key.Length]);
        }

        public IEnumerable<byte> EnumerableEncrypt(IEnumerable<byte> source, byte initialisationVector)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            byte saltedKey = (byte)(this.salt ^ this.reducedKey);

            // Encrypt data
            int i = 0;
            foreach (var x in source)
            {
                yield return (byte)(x ^ initialisationVector ^ i ^ this.key[(i ^ saltedKey) % this.key.Length]);
                ++i;
            }

            // Encrypt initialisation vector
            int lastIndex = i;
            yield return (byte)(initialisationVector ^ lastIndex ^ this.key[(lastIndex ^ saltedKey) % this.key.Length]);
        }

        public IEnumerable<byte> EnumerableDecrypt(byte[] source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            byte saltedKey = (byte)(this.salt ^ this.reducedKey);
            int lastIndex = source.Length - 1;

            // Decrypt initialisation vector
            byte initialisationVector = (byte)(source[lastIndex] ^ lastIndex ^ this.key[(lastIndex ^ saltedKey) % this.key.Length]);

            // Decrypt data
            for (int i = 0; i < source.Length; ++i)
                yield return (byte)(source[i] ^ initialisationVector ^ i ^ this.key[(lastIndex ^ saltedKey) % this.key.Length]);
        }

        public IEnumerable<byte> EnumerableHash(IEnumerable<byte> source)
        {
            byte saltedKey = (byte)(this.salt ^ this.reducedKey);

            int i = 0;
            foreach (var value in source)
            {
                byte a = (byte)(saltedKey ^ i);
                yield return (byte)(value ^ a ^ this.key[a % this.key.Length]);
                ++i;
            }
        }

        private static byte ComputeReducedKey(byte[] key)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            byte result = 0;
            for (int i = 0; i < key.Length; ++i)
                result ^= (byte)(key[i] << (i % 8));
            return result;
        }

        //
        // Intentionally unused
        //
        private IEnumerable<byte> GenerateKeyStream()
        {
            byte saltedKey = (byte)(this.salt ^ this.reducedKey);
            for (int i = 0; i < int.MaxValue; ++i)
                yield return this.key[(i ^ saltedKey) % this.key.Length];
        }
    }
}