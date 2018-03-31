using System;
using System.IO;

namespace test
{
    public static class ArrayUtil
    {
        public static byte[] ReadFile(string fileName)
        {
            using (FileStream fsSource = new FileStream(fileName,
                   FileMode.Open, FileAccess.Read))
            {
                // Read the source file into a byte array.
                byte[] bytes = new byte[fsSource.Length];
                int numBytesToRead = (int)fsSource.Length;
                int numBytesRead = 0;
                while (numBytesToRead > 0)
                {
                    // Read may return anything from 0 to numBytesToRead.
                    int n = fsSource.Read(bytes, numBytesRead, numBytesToRead);

                    // Break when the end of the file is reached.
                    if (n == 0)
                        break;

                    numBytesRead += n;
                    numBytesToRead -= n;
                }
                // Write the byte array to memory.
                using (MemoryStream result = new MemoryStream())
                {
                    result.Write(bytes, 0, bytes.Length);
                    return result.ToArray();
               }
            }
        }
    }
}