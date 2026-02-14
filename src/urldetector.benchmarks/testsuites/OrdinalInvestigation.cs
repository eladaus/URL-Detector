using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace urldetector.benchmarks.testsuites
{
    /// <summary>
    /// Investigation: Is OrdinalIgnoreCase actually slower than InvariantCultureIgnoreCase on .NET 8?
    /// Tests the exact patterns used in the URL detector library.
    /// </summary>
    [SimpleJob(RuntimeMoniker.Net80)]
    [SimpleJob(RuntimeMoniker.Net10_0)]
    [MemoryDiagnoser]
    public class OrdinalVsInvariant_SpanEquals
    {
        // Simulates the PeekSpan pattern: ReadOnlySpan<char>.Equals(string, comparison)
        private string _input;

        [GlobalSetup]
        public void Setup()
        {
            // Simulates a typical input buffer with hex-encoded colons scattered through it
            _input = "http://www.example.com:8080/path?q=1&x=2#frag some text 0x3a more text %3a end";
        }

        [Benchmark(Baseline = true)]
        public int Span_InvariantCulture()
        {
            int count = 0;
            var span = _input.AsSpan();
            for (int i = 0; i < span.Length - 1; i++)
            {
                var peek = span.Slice(i, 2);
                if (peek.Equals("3a", StringComparison.InvariantCultureIgnoreCase))
                    count++;
                if (peek.Equals("0x", StringComparison.InvariantCultureIgnoreCase))
                    count++;
                if (peek.Equals("2e", StringComparison.InvariantCultureIgnoreCase))
                    count++;
            }
            return count;
        }

        [Benchmark]
        public int Span_Ordinal()
        {
            int count = 0;
            var span = _input.AsSpan();
            for (int i = 0; i < span.Length - 1; i++)
            {
                var peek = span.Slice(i, 2);
                if (peek.Equals("3a", StringComparison.OrdinalIgnoreCase))
                    count++;
                if (peek.Equals("0x", StringComparison.OrdinalIgnoreCase))
                    count++;
                if (peek.Equals("2e", StringComparison.OrdinalIgnoreCase))
                    count++;
            }
            return count;
        }
    }

    [SimpleJob(RuntimeMoniker.Net80)]
    [SimpleJob(RuntimeMoniker.Net10_0)]
    [MemoryDiagnoser]
    public class OrdinalVsInvariant_StringEquals
    {
        // Simulates the topLevelStart.Equals("xn--", ...) pattern
        private string[] _testStrings;

        [GlobalSetup]
        public void Setup()
        {
            _testStrings = new[]
            {
                "com", "org", "net", "xn--", "co.uk", "XN--", "Xn--", "io", "dev",
                "com", "org", "net", "xn--", "co.uk", "XN--", "Xn--", "io", "dev",
            };
        }

        [Benchmark(Baseline = true)]
        public int String_InvariantCulture()
        {
            int count = 0;
            for (int i = 0; i < _testStrings.Length; i++)
            {
                if (_testStrings[i].Equals("xn--", StringComparison.InvariantCultureIgnoreCase))
                    count++;
            }
            return count;
        }

        [Benchmark]
        public int String_Ordinal()
        {
            int count = 0;
            for (int i = 0; i < _testStrings.Length; i++)
            {
                if (_testStrings[i].Equals("xn--", StringComparison.OrdinalIgnoreCase))
                    count++;
            }
            return count;
        }
    }

    [SimpleJob(RuntimeMoniker.Net80)]
    [SimpleJob(RuntimeMoniker.Net10_0)]
    [MemoryDiagnoser]
    public class OrdinalVsCurrent_LastIndexOf
    {
        // Simulates the DesuffixUriScheme and HostNormalizer patterns
        private string[] _schemes;

        [GlobalSetup]
        public void Setup()
        {
            _schemes = new[]
            {
                "http://", "https://", "ftp://", "mailto:", "ssh://",
                "ws://", "wss://", "sftp://", "git://", "svn://",
            };
        }

        [Benchmark(Baseline = true)]
        public int LastIndexOf_CurrentCulture()
        {
            int sum = 0;
            for (int i = 0; i < _schemes.Length; i++)
            {
                sum += _schemes[i].LastIndexOf(":", StringComparison.CurrentCultureIgnoreCase);
            }
            return sum;
        }

        [Benchmark]
        public int LastIndexOf_Ordinal()
        {
            int sum = 0;
            for (int i = 0; i < _schemes.Length; i++)
            {
                sum += _schemes[i].LastIndexOf(":", StringComparison.OrdinalIgnoreCase);
            }
            return sum;
        }

        [Benchmark]
        public int LastIndexOf_OrdinalChar()
        {
            int sum = 0;
            for (int i = 0; i < _schemes.Length; i++)
            {
                sum += _schemes[i].LastIndexOf(':');
            }
            return sum;
        }
    }
}
