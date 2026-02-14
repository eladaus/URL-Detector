using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using urldetector.benchmarks.Data;
using urldetector.detection;
using urldetector.tests.custom;

namespace urldetector.benchmarks.testsuites;

/// <summary>
/// Measures how detection scales with input size, from tiny 200-byte strings up to 5 MB bodies.
/// All inputs use the <see cref="UrlMixProfile.Mixed"/> profile with <see cref="UrlDetectorOptions.Default"/>.
/// </summary>
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net10_0)]
[MemoryDiagnoser]
public class InputSizeBenchmarks
{
    private string _tinyNoUrls;
    private string _tinyWithUrls;
    private string _small;
    private string _medium;
    private string _large;
    private string _veryLarge;

    [GlobalSetup]
    public void Setup()
    {
        _tinyNoUrls = BenchmarkDataGenerator.GenerateText(42, 200, 0.0, UrlMixProfile.NoUrls);
        _tinyWithUrls = BenchmarkDataGenerator.GenerateText(42, 200, 0.3, UrlMixProfile.Mixed);
        _small = BenchmarkDataGenerator.GenerateSmall(42, UrlMixProfile.Mixed);
        _medium = BenchmarkDataGenerator.GenerateMedium(42, UrlMixProfile.Mixed);
        _large = BenchmarkDataGenerator.GenerateLarge(42, UrlMixProfile.Mixed);
        _veryLarge = BenchmarkDataGenerator.GenerateVeryLarge(42, UrlMixProfile.Mixed);
    }

    [Benchmark]
    public List<Url> Tiny_NoUrls()
    {
        return new UrlDetector(_tinyNoUrls, UrlDetectorOptions.Default).Detect();
    }

    [Benchmark]
    public List<Url> Tiny_WithUrls()
    {
        return new UrlDetector(_tinyWithUrls, UrlDetectorOptions.Default).Detect();
    }

    [Benchmark]
    public List<Url> Small_500B()
    {
        return new UrlDetector(_small, UrlDetectorOptions.Default).Detect();
    }

    [Benchmark]
    public List<Url> Medium_50KB()
    {
        return new UrlDetector(_medium, UrlDetectorOptions.Default).Detect();
    }

    [Benchmark]
    public List<Url> Large_1MB()
    {
        return new UrlDetector(_large, UrlDetectorOptions.Default).Detect();
    }

    [Benchmark]
    public List<Url> VeryLarge_5MB()
    {
        return new UrlDetector(_veryLarge, UrlDetectorOptions.Default).Detect();
    }
}

/// <summary>
/// Measures the impact of URL density on a fixed ~50 KB input.
/// Densities range from 0% (pure filler) to 90% (almost entirely URLs).
/// </summary>
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net10_0)]
[MemoryDiagnoser]
public class UrlDensityBenchmarks
{
    private string _none;
    private string _low;
    private string _medium;
    private string _high;
    private string _veryHigh;

    [GlobalSetup]
    public void Setup()
    {
        _none = BenchmarkDataGenerator.GenerateText(100, 50 * 1024, 0.0, UrlMixProfile.NoUrls);
        _low = BenchmarkDataGenerator.GenerateText(100, 50 * 1024, 0.05, UrlMixProfile.Mixed);
        _medium = BenchmarkDataGenerator.GenerateText(100, 50 * 1024, 0.2, UrlMixProfile.Mixed);
        _high = BenchmarkDataGenerator.GenerateText(100, 50 * 1024, 0.5, UrlMixProfile.Mixed);
        _veryHigh = BenchmarkDataGenerator.GenerateText(100, 50 * 1024, 0.9, UrlMixProfile.Mixed);
    }

    [Benchmark(Baseline = true)]
    public List<Url> Density_None()
    {
        return new UrlDetector(_none, UrlDetectorOptions.Default).Detect();
    }

    [Benchmark]
    public List<Url> Density_Low()
    {
        return new UrlDetector(_low, UrlDetectorOptions.Default).Detect();
    }

    [Benchmark]
    public List<Url> Density_Medium()
    {
        return new UrlDetector(_medium, UrlDetectorOptions.Default).Detect();
    }

    [Benchmark]
    public List<Url> Density_High()
    {
        return new UrlDetector(_high, UrlDetectorOptions.Default).Detect();
    }

    [Benchmark]
    public List<Url> Density_VeryHigh()
    {
        return new UrlDetector(_veryHigh, UrlDetectorOptions.Default).Detect();
    }
}

/// <summary>
/// Measures detection cost per URL type on a fixed ~50 KB input at 20% density.
/// Isolates web, email, IPv4, IPv6, and mixed URL profiles against a plain-text baseline.
/// </summary>
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net10_0)]
[MemoryDiagnoser]
public class UrlTypeBenchmarks
{
    private string _web;
    private string _email;
    private string _ipv4;
    private string _ipv6;
    private string _mixed;
    private string _plainText;

    [GlobalSetup]
    public void Setup()
    {
        _web = BenchmarkDataGenerator.GenerateText(200, 50 * 1024, 0.2, UrlMixProfile.WebOnly);
        _email = BenchmarkDataGenerator.GenerateText(200, 50 * 1024, 0.2, UrlMixProfile.EmailOnly);
        _ipv4 = BenchmarkDataGenerator.GenerateText(200, 50 * 1024, 0.2, UrlMixProfile.IPv4Only);
        _ipv6 = BenchmarkDataGenerator.GenerateText(200, 50 * 1024, 0.2, UrlMixProfile.IPv6Only);
        _mixed = BenchmarkDataGenerator.GenerateText(200, 50 * 1024, 0.2, UrlMixProfile.Mixed);
        _plainText = BenchmarkDataGenerator.GenerateText(200, 50 * 1024, 0.0, UrlMixProfile.NoUrls);
    }

    [Benchmark]
    public List<Url> WebUrls()
    {
        return new UrlDetector(_web, UrlDetectorOptions.Default).Detect();
    }

    [Benchmark]
    public List<Url> EmailAddresses()
    {
        return new UrlDetector(_email, UrlDetectorOptions.Default).Detect();
    }

    [Benchmark]
    public List<Url> IPv4Urls()
    {
        return new UrlDetector(_ipv4, UrlDetectorOptions.Default).Detect();
    }

    [Benchmark]
    public List<Url> IPv6Urls()
    {
        return new UrlDetector(_ipv6, UrlDetectorOptions.Default).Detect();
    }

    [Benchmark]
    public List<Url> MixedUrls()
    {
        return new UrlDetector(_mixed, UrlDetectorOptions.Default).Detect();
    }

    [Benchmark(Baseline = true)]
    public List<Url> PlainText_NoUrls()
    {
        return new UrlDetector(_plainText, UrlDetectorOptions.Default).Detect();
    }
}

/// <summary>
/// Measures the cost of each <see cref="UrlDetectorOptions"/> mode on the same ~50 KB mixed input.
/// Provides a direct comparison of option overhead independent of content format.
/// </summary>
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net10_0)]
[MemoryDiagnoser]
public class DetectorOptionsBenchmarks
{
    private string _input;

    [GlobalSetup]
    public void Setup()
    {
        _input = BenchmarkDataGenerator.GenerateText(300, 50 * 1024, 0.2, UrlMixProfile.Mixed);
    }

    [Benchmark(Baseline = true)]
    public List<Url> Options_Default()
    {
        return new UrlDetector(_input, UrlDetectorOptions.Default).Detect();
    }

    [Benchmark]
    public List<Url> Options_Html()
    {
        return new UrlDetector(_input, UrlDetectorOptions.HTML).Detect();
    }

    [Benchmark]
    public List<Url> Options_Xml()
    {
        return new UrlDetector(_input, UrlDetectorOptions.XML).Detect();
    }

    [Benchmark]
    public List<Url> Options_Json()
    {
        return new UrlDetector(_input, UrlDetectorOptions.JSON).Detect();
    }

    [Benchmark]
    public List<Url> Options_Javascript()
    {
        return new UrlDetector(_input, UrlDetectorOptions.JAVASCRIPT).Detect();
    }

    [Benchmark]
    public List<Url> Options_QuoteMatch()
    {
        return new UrlDetector(_input, UrlDetectorOptions.QUOTE_MATCH).Detect();
    }

    [Benchmark]
    public List<Url> Options_BracketMatch()
    {
        return new UrlDetector(_input, UrlDetectorOptions.BRACKET_MATCH).Detect();
    }

    [Benchmark]
    public List<Url> Options_SingleLevelDomain()
    {
        return new UrlDetector(_input, UrlDetectorOptions.ALLOW_SINGLE_LEVEL_DOMAIN).Detect();
    }

    [Benchmark]
    public List<Url> Options_HtmlWithSingleLevel()
    {
        return new UrlDetector(
            _input,
            UrlDetectorOptions.HTML | UrlDetectorOptions.ALLOW_SINGLE_LEVEL_DOMAIN
        ).Detect();
    }
}

/// <summary>
/// Tests each <see cref="UrlDetectorOptions"/> mode against content structured in the matching format.
/// HTML pages are tested with <see cref="UrlDetectorOptions.HTML"/>, JSON documents with
/// <see cref="UrlDetectorOptions.JSON"/>, XML feeds with <see cref="UrlDetectorOptions.XML"/>, and so on.
/// Each content type is tested with both <see cref="UrlDetectorOptions.Default"/> and its matching
/// option to measure the overhead or benefit of using the correct mode.
/// </summary>
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net10_0)]
[MemoryDiagnoser]
public class StructuredContentBenchmarks
{
    private const int Size = 50 * 1024;
    private const double Density = 0.2;
    private const int Seed = 400;

    private string _html;
    private string _json;
    private string _xml;
    private string _javaScript;
    private string _quoted;
    private string _singleQuoted;
    private string _bracketed;
    private string _singleLevelDomain;

    [GlobalSetup]
    public void Setup()
    {
        _html = BenchmarkDataGenerator.GenerateHtmlContent(Seed, Size, Density);
        _json = BenchmarkDataGenerator.GenerateJsonContent(Seed + 1, Size, Density);
        _xml = BenchmarkDataGenerator.GenerateXmlContent(Seed + 2, Size, Density);
        _javaScript = BenchmarkDataGenerator.GenerateJavaScriptContent(Seed + 3, Size, Density);
        _quoted = BenchmarkDataGenerator.GenerateQuotedContent(Seed + 4, Size, Density);
        _singleQuoted = BenchmarkDataGenerator.GenerateSingleQuotedContent(Seed + 5, Size, Density);
        _bracketed = BenchmarkDataGenerator.GenerateBracketedContent(Seed + 6, Size, Density);
        _singleLevelDomain = BenchmarkDataGenerator.GenerateSingleLevelDomainContent(
            Seed + 7,
            Size,
            Density
        );
    }

    [Benchmark(Baseline = true)]
    public List<Url> Html_DefaultOption()
    {
        return new UrlDetector(_html, UrlDetectorOptions.Default).Detect();
    }

    [Benchmark]
    public List<Url> Html_HtmlOption()
    {
        return new UrlDetector(_html, UrlDetectorOptions.HTML).Detect();
    }

    [Benchmark]
    public List<Url> Html_HtmlSingleLevelOption()
    {
        return new UrlDetector(
            _html,
            UrlDetectorOptions.HTML | UrlDetectorOptions.ALLOW_SINGLE_LEVEL_DOMAIN
        ).Detect();
    }

    [Benchmark]
    public List<Url> Json_DefaultOption()
    {
        return new UrlDetector(_json, UrlDetectorOptions.Default).Detect();
    }

    [Benchmark]
    public List<Url> Json_JsonOption()
    {
        return new UrlDetector(_json, UrlDetectorOptions.JSON).Detect();
    }

    [Benchmark]
    public List<Url> Xml_DefaultOption()
    {
        return new UrlDetector(_xml, UrlDetectorOptions.Default).Detect();
    }

    [Benchmark]
    public List<Url> Xml_XmlOption()
    {
        return new UrlDetector(_xml, UrlDetectorOptions.XML).Detect();
    }

    [Benchmark]
    public List<Url> JavaScript_DefaultOption()
    {
        return new UrlDetector(_javaScript, UrlDetectorOptions.Default).Detect();
    }

    [Benchmark]
    public List<Url> JavaScript_JavaScriptOption()
    {
        return new UrlDetector(_javaScript, UrlDetectorOptions.JAVASCRIPT).Detect();
    }

    [Benchmark]
    public List<Url> Quoted_DefaultOption()
    {
        return new UrlDetector(_quoted, UrlDetectorOptions.Default).Detect();
    }

    [Benchmark]
    public List<Url> Quoted_QuoteMatchOption()
    {
        return new UrlDetector(_quoted, UrlDetectorOptions.QUOTE_MATCH).Detect();
    }

    [Benchmark]
    public List<Url> SingleQuoted_DefaultOption()
    {
        return new UrlDetector(_singleQuoted, UrlDetectorOptions.Default).Detect();
    }

    [Benchmark]
    public List<Url> SingleQuoted_SingleQuoteMatchOption()
    {
        return new UrlDetector(_singleQuoted, UrlDetectorOptions.SINGLE_QUOTE_MATCH).Detect();
    }

    [Benchmark]
    public List<Url> Bracketed_DefaultOption()
    {
        return new UrlDetector(_bracketed, UrlDetectorOptions.Default).Detect();
    }

    [Benchmark]
    public List<Url> Bracketed_BracketMatchOption()
    {
        return new UrlDetector(_bracketed, UrlDetectorOptions.BRACKET_MATCH).Detect();
    }

    [Benchmark]
    public List<Url> SingleLevel_DefaultOption()
    {
        return new UrlDetector(_singleLevelDomain, UrlDetectorOptions.Default).Detect();
    }

    [Benchmark]
    public List<Url> SingleLevel_SingleLevelOption()
    {
        return new UrlDetector(
            _singleLevelDomain,
            UrlDetectorOptions.ALLOW_SINGLE_LEVEL_DOMAIN
        ).Detect();
    }
}

/// <summary>
/// Tests real HTML sample files (tiny through large) with <see cref="UrlDetectorOptions.Default"/>
/// versus all flags enabled, measuring the overhead of enabling every option on actual web pages.
/// </summary>
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net10_0)]
[MemoryDiagnoser]
public class RealWorldHtmlBenchmarks
{
    private static readonly UrlDetectorOptions AllFlags =
        UrlDetectorOptions.ALLOW_SINGLE_LEVEL_DOMAIN
        | UrlDetectorOptions.BRACKET_MATCH
        | UrlDetectorOptions.Default
        | UrlDetectorOptions.HTML
        | UrlDetectorOptions.JAVASCRIPT
        | UrlDetectorOptions.JSON
        | UrlDetectorOptions.QUOTE_MATCH
        | UrlDetectorOptions.SINGLE_QUOTE_MATCH
        | UrlDetectorOptions.XML;

    private string _tiny;
    private string _small;
    private string _medium;
    private string _large;

    [GlobalSetup]
    public void Setup()
    {
        _tiny = TestDataHelper.LoadTinyHtmlFile();
        _small = TestDataHelper.LoadSmallHtmlFile();
        _medium = TestDataHelper.LoadMediumHtmlFile();
        _large = TestDataHelper.LoadLargeHtmlFile();
    }

    [Benchmark]
    public List<Url> RealHtml_Tiny_Default()
    {
        return new UrlDetector(_tiny, UrlDetectorOptions.Default).Detect();
    }

    [Benchmark]
    public List<Url> RealHtml_Tiny_AllFlags()
    {
        return new UrlDetector(_tiny, AllFlags).Detect();
    }

    [Benchmark]
    public List<Url> RealHtml_Small_Default()
    {
        return new UrlDetector(_small, UrlDetectorOptions.Default).Detect();
    }

    [Benchmark]
    public List<Url> RealHtml_Small_AllFlags()
    {
        return new UrlDetector(_small, AllFlags).Detect();
    }

    [Benchmark]
    public List<Url> RealHtml_Medium_Default()
    {
        return new UrlDetector(_medium, UrlDetectorOptions.Default).Detect();
    }

    [Benchmark]
    public List<Url> RealHtml_Medium_AllFlags()
    {
        return new UrlDetector(_medium, AllFlags).Detect();
    }

    [Benchmark]
    public List<Url> RealHtml_Large_Default()
    {
        return new UrlDetector(_large, UrlDetectorOptions.Default).Detect();
    }

    [Benchmark]
    public List<Url> RealHtml_Large_AllFlags()
    {
        return new UrlDetector(_large, AllFlags).Detect();
    }
}
