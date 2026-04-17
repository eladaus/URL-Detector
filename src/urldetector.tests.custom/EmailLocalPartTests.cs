using System.Collections.Generic;
using System.Linq;
using urldetector.detection;
using Xunit;

namespace urldetector.tests.custom;

/// <summary>
/// Coverage for RFC 5322 dot-atom local-part characters in email addresses
/// embedded in free text. Exercises Gmail/Fastmail-style sub-addressing
/// (user+tag, user-tag) and the wider set of special chars valid per RFC 5322.
/// </summary>
public class EmailLocalPartTests
{
    private static List<string> Detect(string text, UrlDetectorOptions options = UrlDetectorOptions.Default)
    {
        return new UrlDetector(text, options)
            .Detect()
            .Select(u => u.GetOriginalUrl())
            .ToList();
    }

    private static void AssertSingleMatch(string input, string expected)
    {
        var urls = Detect(input);
        Assert.Single(urls);
        Assert.Equal(expected, urls[0]);
    }

    [Theory]
    [InlineData("user+tag@gmail.com")]
    [InlineData("user.name+tag@gmail.com")]
    [InlineData("user+a+b@gmail.com")]
    [InlineData("+tag@gmail.com")]
    [InlineData("first.middle.last+tag@gmail.com")]
    public void PlusAddressing_IsExtractedAsSingleEmail(string email)
    {
        AssertSingleMatch(email, email);
    }

    [Theory]
    [InlineData("user-tag@fastmail.com")]
    [InlineData("user.name-tag@fastmail.com")]
    public void HyphenSubaddressing_IsExtractedAsSingleEmail(string email)
    {
        AssertSingleMatch(email, email);
    }

    [Theory]
    [InlineData("user_name@example.com")]
    [InlineData("user_name.foo@example.com")]
    [InlineData("user.name_tag@example.com")]
    public void Underscore_InLocalPart(string email)
    {
        AssertSingleMatch(email, email);
    }

    [Theory]
    [InlineData("user!tag@example.com")]
    [InlineData("user#tag@example.com")]
    [InlineData("user$tag@example.com")]
    [InlineData("user&tag@example.com")]
    [InlineData("user'tag@example.com")]
    [InlineData("user*tag@example.com")]
    [InlineData("user=tag@example.com")]
    [InlineData("user?tag@example.com")]
    [InlineData("user^tag@example.com")]
    [InlineData("user`tag@example.com")]
    [InlineData("user{tag@example.com")]
    [InlineData("user|tag@example.com")]
    [InlineData("user}tag@example.com")]
    [InlineData("user~tag@example.com")]
    public void RfcSpecialChars_LocalPart_NoDot(string email)
    {
        AssertSingleMatch(email, email);
    }

    [Theory]
    [InlineData("user.name!tag@example.com")]
    [InlineData("user.name#tag@example.com")]
    [InlineData("user.name$tag@example.com")]
    [InlineData("user.name&tag@example.com")]
    [InlineData("user.name'tag@example.com")]
    [InlineData("user.name*tag@example.com")]
    [InlineData("user.name=tag@example.com")]
    [InlineData("user.name?tag@example.com")]
    [InlineData("user.name^tag@example.com")]
    [InlineData("user.name`tag@example.com")]
    [InlineData("user.name~tag@example.com")]
    public void RfcSpecialChars_LocalPart_WithDot(string email)
    {
        AssertSingleMatch(email, email);
    }

    [Fact]
    public void EmbeddedInProse_ExtractsSingleEmail()
    {
        var urls = Detect("contact me at user+tag@gmail.com for info");
        Assert.Single(urls);
        Assert.Equal("user+tag@gmail.com", urls[0]);
    }

    [Fact]
    public void EmbeddedInProse_WithDottedLocalPart_ExtractsSingleEmail()
    {
        var urls = Detect("please email first.last+sales@eladaus.com when ready");
        Assert.Single(urls);
        Assert.Equal("first.last+sales@eladaus.com", urls[0]);
    }

    [Fact]
    public void EmbeddedInProse_MultipleEmails_ExtractsAll()
    {
        var urls = Detect(
            "Contact us at first.last+sales@eladaus.com or reach ops+urgent@gmail.com today"
        );
        Assert.Equal(2, urls.Count);
        Assert.Contains("first.last+sales@eladaus.com", urls);
        Assert.Contains("ops+urgent@gmail.com", urls);
    }

    [Fact]
    public void Ipv6LiteralHost_WithPlusLocalPart()
    {
        // Only IPv6 literal hosts are recognised by DomainNameReader (existing
        // behaviour predating this change). Mirror the style of
        // TestUriDetection.cs:735 which uses [AAbb:AAbb:AAbb::] as the host.
        var urls = Detect("user+tag@[AAbb:AAbb:AAbb::]");
        Assert.Single(urls);
        Assert.Equal("user+tag@[AAbb:AAbb:AAbb::]", urls[0]);
    }

    [Theory]
    [InlineData("name@gmail.com")]
    [InlineData("name.lastname@gmail.com")]
    [InlineData("gmail.com@gmail.com")]
    [InlineData("user@github.io/page")]
    public void Regression_ExistingEmailPatterns_StillMatch(string input)
    {
        AssertSingleMatch(input, input);
    }

    [Fact]
    public void Regression_DotInProse_WithoutAt_DoesNotTriggerEmailPath()
    {
        // Sanity guard: when there's no '@' ahead after a dot, the peek must
        // return false so the existing ReadDomainName path runs unchanged.
        // "example.com" with no '@' anywhere must still be detected.
        var urls = Detect("Visit example.com today");
        Assert.Single(urls);
        Assert.Equal("example.com", urls[0]);
    }
}
