using System;
using System.Linq;
using System.Collections.Generic;
public static class StringConversion
{
    public static IEnumerable<string> Split(string str, int chunkSize)
    {
        return Enumerable.Range(0, str.Length / chunkSize)
            .Select(i => str.Substring(i * chunkSize, chunkSize));
    }
}