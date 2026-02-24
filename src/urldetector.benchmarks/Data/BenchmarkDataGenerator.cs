using System;
using System.Text;

namespace urldetector.benchmarks.Data;

public enum UrlMixProfile
{
    WebOnly,
    EmailOnly,
    IPv4Only,
    IPv6Only,
    Mixed,
    NoUrls
}

public static class BenchmarkDataGenerator
{
    private static readonly string[] WebUrls =
    {
        "https://www.example.com/path?q=1",
        "http://subdomain.site.co.uk/page#section",
        "https://api.service.io/v2/users/123",
        "http://www.google.com/search?q=hello+world&lang=en",
        "https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css",
        "https://github.com/dotnet/runtime/issues/12345",
        "http://localhost:8080/api/health",
        "https://en.wikipedia.org/wiki/URL_detection",
        "https://news.ycombinator.com/item?id=98765",
        "http://blog.example.org/2024/01/my-post.html",
        "https://myapp.azurewebsites.net/dashboard",
        "https://registry.npmjs.org/@scope/package",
        "http://www.amazon.com/dp/B08N5WRWNW?tag=example",
        "https://docs.microsoft.com/en-us/dotnet/api/system.string",
        "https://stackoverflow.com/questions/12345678/how-to-parse-urls",
        "http://192.168.1.1:3000/admin/settings",
        "https://example.com/path/to/resource.json",
        "https://shop.example.com/products?category=electronics&sort=price",
        "http://test.example.com:9090/v1/endpoint?token=abc123&format=json",
        "https://www.reddit.com/r/programming/comments/abc123/url_parsing/"
    };

    private static readonly string[] EmailAddresses =
    {
        "user@example.com",
        "first.last@company.co.uk",
        "admin+tag@mail-server.org",
        "support@helpdesk.example.com",
        "john.doe42@gmail.com",
        "info@my-domain.io",
        "noreply@notifications.service.com",
        "dev.team@startup.co",
        "contact@museum.gallery.org",
        "alice_b@university.edu"
    };

    private static readonly string[] IPv4Urls =
    {
        "http://192.168.1.1/path",
        "http://10.0.0.1:8080/api",
        "https://172.16.0.100/dashboard",
        "http://127.0.0.1/localhost-test",
        "http://10.10.10.10:3000/metrics",
        "https://192.168.0.50:443/secure",
        "http://172.20.10.1/gateway",
        "http://10.0.1.254:9200/elasticsearch"
    };

    private static readonly string[] IPv6Urls =
    {
        "http://[::1]/",
        "http://[fe80::1%25eth0]/path",
        "http://[2001:db8::1]:8080/",
        "http://[::ffff:192.168.1.1]/mapped",
        "http://[2001:db8:85a3::8a2e:370:7334]/resource",
        "http://[fe80::200:5aee:feaa:20a2]/link-local",
        "http://[2001:db8::ff00:42:8329]:9090/api",
        "http://[::1]:5000/dev"
    };

    private static readonly string[] FtpUrls =
    {
        "ftp://files.example.com/pub/doc.pdf",
        "ftp://mirror.example.org/linux/iso/debian.iso",
        "ftp://ftp.mozilla.org/pub/firefox/releases/",
        "ftp://archive.example.com/datasets/2024/data.csv",
        "ftp://ftp.gnu.org/gnu/gcc/"
    };

    private static readonly string[] MixedEdgeCaseUrls =
    {
        "http://user:pass@host.com/path",
        "https://example.com/path%20to/file",
        "https://example.com/search?q=hello%20world&lang=en-US#results",
        "http://xn--nxasmq6b.example.com/internationalized",
        "https://very-long-subdomain.another-level.deep.example.com/with/a/very/long/path/that/goes/on",
        "https://example.com:8443/api/v2/users?page=1&limit=100&sort=created_at",
        "http://example.com/path?redirect=https%3A%2F%2Fother.com%2Fpage",
        "https://example.com/file.html?v=1.2.3&cache=false#section-2"
    };

    private static readonly string[] FillerText =
    {
        "The quick brown fox jumps over the lazy dog.",
        "Lorem ipsum dolor sit amet, consectetur adipiscing elit.",
        "Please review the attached document for more information.",
        "This is a sample paragraph that contains no links at all.",
        "According to recent studies, performance benchmarks are essential for software quality.",
        "The system processes incoming requests and returns structured responses.",
        "Data validation ensures that all inputs conform to expected formats.",
        "Error handling should be comprehensive and provide meaningful feedback.",
        "The configuration file specifies runtime parameters for the application.",
        "Testing is a critical part of the software development lifecycle.",
        "Memory allocation patterns can significantly affect application performance.",
        "String parsing operations are common in web scraping and data extraction tools.",
        "The detector scans through text content to identify patterns of interest.",
        "Network protocols define the rules for communication between systems.",
        "Regular expressions provide powerful pattern matching capabilities.",
        "The output buffer accumulates results before flushing to the consumer.",
        "Benchmark results should be interpreted in context of the target workload.",
        "Concurrent access to shared resources requires careful synchronization.",
        "The cache layer reduces latency by storing frequently accessed data.",
        "Profiling tools help identify bottlenecks in application performance."
    };

    private static readonly string[] SingleLevelDomainUrls =
    {
        "http://localhost/path",
        "http://localhost:8080/api/health",
        "http://intranet/dashboard",
        "http://server/api/v1/status",
        "http://gateway/route",
        "http://mailserver/admin",
        "http://buildserver:9090/jobs",
        "http://wiki/pages/home"
    };

    // HTML element templates — {0} is replaced with a URL
    private static readonly string[] HtmlUrlTemplates =
    {
        "<a href=\"{0}\">Click here for details</a>",
        "<a class=\"btn btn-primary\" href=\"{0}\" target=\"_blank\">Visit</a>",
        "<img src=\"{0}\" alt=\"photo\" width=\"640\" height=\"480\">",
        "<script src=\"{0}\"></script>",
        "<link rel=\"stylesheet\" href=\"{0}\">",
        "<iframe src=\"{0}\" width=\"100%\" height=\"400\"></iframe>",
        "<video src=\"{0}\" controls></video>",
        "<source src=\"{0}\" type=\"video/mp4\">",
        "<a href=\"{0}\" rel=\"noopener noreferrer\">External link</a>",
        "<form action=\"{0}\" method=\"post\"><input type=\"submit\"></form>",
        "<object data=\"{0}\" type=\"application/pdf\" width=\"100%\"></object>",
        "<meta property=\"og:url\" content=\"{0}\">"
    };

    // HTML filler templates — {0} is replaced with filler text
    private static readonly string[] HtmlFillerTemplates =
    {
        "<p>{0}</p>",
        "<div class=\"section\"><p>{0}</p></div>",
        "<span class=\"text-muted\">{0}</span>",
        "<h2>{0}</h2>",
        "<h3>{0}</h3>",
        "<li>{0}</li>",
        "<blockquote><p>{0}</p></blockquote>",
        "<td>{0}</td>",
        "<div class=\"card\"><div class=\"card-body\">{0}</div></div>",
        "<!-- {0} -->"
    };

    // JSON templates — {0} is a URL, {1} is filler text
    private static readonly string[] JsonUrlEntryTemplates =
    {
        "    {{ \"url\": \"{0}\", \"title\": \"{1}\" }}",
        "    {{ \"homepage\": \"{0}\", \"description\": \"{1}\" }}",
        "    {{ \"link\": \"{0}\", \"label\": \"{1}\", \"active\": true }}",
        "    {{ \"endpoint\": \"{0}\", \"method\": \"GET\", \"note\": \"{1}\" }}",
        "    {{ \"image\": \"{0}\", \"alt\": \"{1}\", \"width\": 640 }}",
        "    {{ \"redirect\": \"{0}\", \"status\": 301, \"comment\": \"{1}\" }}"
    };

    private static readonly string[] JsonFillerEntryTemplates =
    {
        "    {{ \"name\": \"{0}\", \"id\": {1} }}",
        "    {{ \"message\": \"{0}\", \"code\": {1}, \"success\": true }}",
        "    {{ \"text\": \"{0}\", \"index\": {1} }}",
        "    {{ \"description\": \"{0}\", \"priority\": {1}, \"tags\": [\"a\", \"b\"] }}"
    };

    // XML templates — {0} is a URL, {1} is filler text
    private static readonly string[] XmlUrlEntryTemplates =
    {
        "  <entry>\n    <link href=\"{0}\"/>\n    <summary>{1}</summary>\n  </entry>",
        "  <item>\n    <url>{0}</url>\n    <description>{1}</description>\n  </item>",
        "  <resource uri=\"{0}\">\n    <label>{1}</label>\n  </resource>",
        "  <link rel=\"alternate\" href=\"{0}\" title=\"{1}\"/>",
        "  <image>\n    <url>{0}</url>\n    <title>{1}</title>\n  </image>"
    };

    private static readonly string[] XmlFillerEntryTemplates =
    {
        "  <entry>\n    <title>{0}</title>\n    <id>{1}</id>\n  </entry>",
        "  <item>\n    <description>{0}</description>\n    <pubDate>2024-01-15</pubDate>\n  </item>",
        "  <node id=\"{1}\">\n    <content>{0}</content>\n  </node>"
    };

    // JavaScript templates — {0} is a URL, {1} is filler text
    private static readonly string[] JsUrlStatementTemplates =
    {
        "  var endpoint = \"{0}\"; // {1}",
        "  const apiUrl = \"{0}\";",
        "  let redirectUrl = '{0}'; // {1}",
        "  fetch(\"{0}\").then(r => r.json());",
        "  window.location.href = \"{0}\";",
        "  config.baseUrl = '{0}';",
        "  urls.push(\"{0}\"); // {1}",
        "  const img = new Image(); img.src = \"{0}\";",
        "  $.ajax({{ url: \"{0}\", method: 'GET' }});",
        "  import(\"{0}\").then(module => init(module));"
    };

    private static readonly string[] JsFillerStatementTemplates =
    {
        "  // {0}",
        "  var data = {{ message: \"{0}\", count: {1} }};",
        "  console.log(\"{0}\");",
        "  if (status === {1}) {{ throw new Error(\"{0}\"); }}",
        "  const label = \"{0}\";"
    };

    public static string GenerateText(
        int seed,
        int approximateSizeBytes,
        double urlDensity,
        UrlMixProfile profile
    )
    {
        var random = new Random(seed);
        var sb = new StringBuilder(approximateSizeBytes + 256);

        while (sb.Length < approximateSizeBytes)
        {
            var insertUrl = profile != UrlMixProfile.NoUrls && random.NextDouble() < urlDensity;

            if (insertUrl)
            {
                sb.Append(PickUrl(random, profile));
            }
            else
            {
                sb.Append(FillerText[random.Next(FillerText.Length)]);
            }

            // Random separator: space, newline, or double space
            var sep = random.Next(3);
            if (sep == 0)
            {
                sb.Append('\n');
            }
            else if (sep == 1)
            {
                sb.Append("  ");
            }
            else
            {
                sb.Append(' ');
            }
        }

        return sb.ToString();
    }

    private static string PickUrl(Random random, UrlMixProfile profile)
    {
        switch (profile)
        {
            case UrlMixProfile.WebOnly:
                return WebUrls[random.Next(WebUrls.Length)];

            case UrlMixProfile.EmailOnly:
                return EmailAddresses[random.Next(EmailAddresses.Length)];

            case UrlMixProfile.IPv4Only:
                return IPv4Urls[random.Next(IPv4Urls.Length)];

            case UrlMixProfile.IPv6Only:
                return IPv6Urls[random.Next(IPv6Urls.Length)];

            case UrlMixProfile.Mixed:
                // Pick from all pools with equal weighting across pools
                var pool = random.Next(5);
                switch (pool)
                {
                    case 0:
                        return WebUrls[random.Next(WebUrls.Length)];
                    case 1:
                        return EmailAddresses[random.Next(EmailAddresses.Length)];
                    case 2:
                        return IPv4Urls[random.Next(IPv4Urls.Length)];
                    case 3:
                        return IPv6Urls[random.Next(IPv6Urls.Length)];
                    case 4:
                        // Split between FTP and edge-case URLs
                        if (random.Next(2) == 0)
                        {
                            return FtpUrls[random.Next(FtpUrls.Length)];
                        }

                        return MixedEdgeCaseUrls[random.Next(MixedEdgeCaseUrls.Length)];
                    default:
                        return WebUrls[random.Next(WebUrls.Length)];
                }

            default:
                return WebUrls[random.Next(WebUrls.Length)];
        }
    }

    public static string GenerateSmall(int seed, UrlMixProfile profile)
    {
        return GenerateText(seed, 500, 0.3, profile);
    }

    public static string GenerateMedium(int seed, UrlMixProfile profile)
    {
        return GenerateText(seed, 50 * 1024, 0.2, profile);
    }

    public static string GenerateLarge(int seed, UrlMixProfile profile)
    {
        return GenerateText(seed, 1024 * 1024, 0.15, profile);
    }

    public static string GenerateVeryLarge(int seed, UrlMixProfile profile)
    {
        return GenerateText(seed, 5 * 1024 * 1024, 0.1, profile);
    }

    /// <summary>
    /// Generates a realistic HTML page with URLs in href, src, and other attributes.
    /// </summary>
    public static string GenerateHtmlContent(int seed, int approximateSizeBytes, double urlDensity)
    {
        var random = new Random(seed);
        var sb = new StringBuilder(approximateSizeBytes + 512);

        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"en\">");
        sb.AppendLine("<head>");
        sb.AppendLine("  <meta charset=\"UTF-8\">");
        sb.AppendLine("  <title>Benchmark Page</title>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine("<div class=\"container\">");

        var inList = false;
        var inTable = false;

        while (sb.Length < approximateSizeBytes)
        {
            // Occasionally open/close structural elements for realism
            var structural = random.Next(20);
            if (structural == 0 && !inList)
            {
                sb.AppendLine("<ul>");
                inList = true;
            }
            else if (structural == 1 && inList)
            {
                sb.AppendLine("</ul>");
                inList = false;
            }
            else if (structural == 2 && !inTable)
            {
                sb.AppendLine("<table><tbody><tr>");
                inTable = true;
            }
            else if (structural == 3 && inTable)
            {
                sb.AppendLine("</tr></tbody></table>");
                inTable = false;
            }

            var insertUrl = random.NextDouble() < urlDensity;

            if (insertUrl)
            {
                var url = PickUrl(random, UrlMixProfile.WebOnly);
                var template = HtmlUrlTemplates[random.Next(HtmlUrlTemplates.Length)];
                sb.AppendLine(string.Format(template, url));
            }
            else
            {
                var filler = FillerText[random.Next(FillerText.Length)];
                var template = HtmlFillerTemplates[random.Next(HtmlFillerTemplates.Length)];
                sb.AppendLine(string.Format(template, filler));
            }
        }

        if (inList)
        {
            sb.AppendLine("</ul>");
        }

        if (inTable)
        {
            sb.AppendLine("</tr></tbody></table>");
        }

        sb.AppendLine("</div>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }

    /// <summary>
    /// Generates a realistic JSON document with URLs as string values.
    /// </summary>
    public static string GenerateJsonContent(int seed, int approximateSizeBytes, double urlDensity)
    {
        var random = new Random(seed);
        var sb = new StringBuilder(approximateSizeBytes + 256);

        sb.AppendLine("{");
        sb.AppendLine("  \"items\": [");

        var first = true;

        while (sb.Length < approximateSizeBytes)
        {
            if (!first)
            {
                sb.AppendLine(",");
            }

            first = false;

            var insertUrl = random.NextDouble() < urlDensity;
            var filler = FillerText[random.Next(FillerText.Length)];
            // Escape any quotes in filler for JSON safety
            filler = filler.Replace("\"", "\\\"");
            var id = random.Next(1, 100000);

            if (insertUrl)
            {
                var url = PickUrl(random, UrlMixProfile.WebOnly);
                var template = JsonUrlEntryTemplates[random.Next(JsonUrlEntryTemplates.Length)];
                sb.Append(string.Format(template, url, filler));
            }
            else
            {
                var template = JsonFillerEntryTemplates[
                    random.Next(JsonFillerEntryTemplates.Length)
                ];
                sb.Append(string.Format(template, filler, id));
            }
        }

        sb.AppendLine();
        sb.AppendLine("  ]");
        sb.AppendLine("}");

        return sb.ToString();
    }

    /// <summary>
    /// Generates a realistic XML feed/document with URLs in attributes and text nodes.
    /// </summary>
    public static string GenerateXmlContent(int seed, int approximateSizeBytes, double urlDensity)
    {
        var random = new Random(seed);
        var sb = new StringBuilder(approximateSizeBytes + 256);

        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine("<feed xmlns=\"http://www.w3.org/2005/Atom\">");
        sb.AppendLine("  <title>Benchmark Feed</title>");

        var counter = 0;

        while (sb.Length < approximateSizeBytes)
        {
            counter++;
            var insertUrl = random.NextDouble() < urlDensity;
            var filler = FillerText[random.Next(FillerText.Length)];
            // Escape XML special characters in filler
            filler = filler.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");

            if (insertUrl)
            {
                var url = PickUrl(random, UrlMixProfile.WebOnly);
                var template = XmlUrlEntryTemplates[random.Next(XmlUrlEntryTemplates.Length)];
                sb.AppendLine(string.Format(template, url, filler));
            }
            else
            {
                var template = XmlFillerEntryTemplates[random.Next(XmlFillerEntryTemplates.Length)];
                sb.AppendLine(string.Format(template, filler, counter));
            }
        }

        sb.AppendLine("</feed>");

        return sb.ToString();
    }

    /// <summary>
    /// Generates realistic JavaScript code with URLs in string literals (double and single quoted).
    /// </summary>
    public static string GenerateJavaScriptContent(
        int seed,
        int approximateSizeBytes,
        double urlDensity
    )
    {
        var random = new Random(seed);
        var sb = new StringBuilder(approximateSizeBytes + 256);

        sb.AppendLine("(function() {");
        sb.AppendLine("  'use strict';");
        sb.AppendLine("  var urls = [];");
        sb.AppendLine("  var config = {};");
        sb.AppendLine();

        var funcCounter = 0;

        while (sb.Length < approximateSizeBytes)
        {
            // Occasionally wrap in function blocks for realism
            if (random.Next(15) == 0)
            {
                funcCounter++;
                sb.AppendLine();
                sb.AppendLine(string.Format("  function handler{0}() {{", funcCounter));
            }

            var insertUrl = random.NextDouble() < urlDensity;
            var filler = FillerText[random.Next(FillerText.Length)];
            filler = filler.Replace("'", "\\'").Replace("\"", "\\\"");
            var num = random.Next(1, 10000);

            if (insertUrl)
            {
                var url = PickUrl(random, UrlMixProfile.WebOnly);
                var template = JsUrlStatementTemplates[random.Next(JsUrlStatementTemplates.Length)];
                sb.AppendLine(string.Format(template, url, filler));
            }
            else
            {
                var template = JsFillerStatementTemplates[
                    random.Next(JsFillerStatementTemplates.Length)
                ];
                sb.AppendLine(string.Format(template, filler, num));
            }

            if (random.Next(15) == 0 && funcCounter > 0)
            {
                sb.AppendLine("  }");
            }
        }

        sb.AppendLine("})();");

        return sb.ToString();
    }

    /// <summary>
    /// Generates text with URLs wrapped in double quotes, exercising QUOTE_MATCH.
    /// </summary>
    public static string GenerateQuotedContent(
        int seed,
        int approximateSizeBytes,
        double urlDensity
    )
    {
        var random = new Random(seed);
        var sb = new StringBuilder(approximateSizeBytes + 256);

        while (sb.Length < approximateSizeBytes)
        {
            var insertUrl = random.NextDouble() < urlDensity;

            if (insertUrl)
            {
                var url = PickUrl(random, UrlMixProfile.WebOnly);
                var style = random.Next(4);
                switch (style)
                {
                    case 0:
                        sb.Append(string.Format("See \"{0}\" for more details.", url));
                        break;
                    case 1:
                        sb.Append(string.Format("The resource at \"{0}\" is available.", url));
                        break;
                    case 2:
                        sb.Append(string.Format("Visit \"{0}\" to learn more.", url));
                        break;
                    default:
                        sb.Append(string.Format("Link: \"{0}\"", url));
                        break;
                }
            }
            else
            {
                sb.Append(FillerText[random.Next(FillerText.Length)]);
            }

            sb.Append(random.Next(2) == 0 ? '\n' : ' ');
        }

        return sb.ToString();
    }

    /// <summary>
    /// Generates text with URLs wrapped in single quotes, exercising SINGLE_QUOTE_MATCH.
    /// </summary>
    public static string GenerateSingleQuotedContent(
        int seed,
        int approximateSizeBytes,
        double urlDensity
    )
    {
        var random = new Random(seed);
        var sb = new StringBuilder(approximateSizeBytes + 256);

        while (sb.Length < approximateSizeBytes)
        {
            var insertUrl = random.NextDouble() < urlDensity;

            if (insertUrl)
            {
                var url = PickUrl(random, UrlMixProfile.WebOnly);
                var style = random.Next(4);
                switch (style)
                {
                    case 0:
                        sb.Append(string.Format("See '{0}' for more details.", url));
                        break;
                    case 1:
                        sb.Append(string.Format("The resource at '{0}' is available.", url));
                        break;
                    case 2:
                        sb.Append(string.Format("Visit '{0}' to learn more.", url));
                        break;
                    default:
                        sb.Append(string.Format("Link: '{0}'", url));
                        break;
                }
            }
            else
            {
                sb.Append(FillerText[random.Next(FillerText.Length)]);
            }

            sb.Append(random.Next(2) == 0 ? '\n' : ' ');
        }

        return sb.ToString();
    }

    /// <summary>
    /// Generates text with URLs inside brackets and parentheses, exercising BRACKET_MATCH.
    /// </summary>
    public static string GenerateBracketedContent(
        int seed,
        int approximateSizeBytes,
        double urlDensity
    )
    {
        var random = new Random(seed);
        var sb = new StringBuilder(approximateSizeBytes + 256);

        while (sb.Length < approximateSizeBytes)
        {
            var insertUrl = random.NextDouble() < urlDensity;

            if (insertUrl)
            {
                var url = PickUrl(random, UrlMixProfile.WebOnly);
                var style = random.Next(5);
                switch (style)
                {
                    case 0:
                        sb.Append(string.Format("More info at ({0}) in the docs.", url));
                        break;
                    case 1:
                        sb.Append(string.Format("See [{0}] for the reference.", url));
                        break;
                    case 2:
                        sb.Append(string.Format("Resource: ({0})", url));
                        break;
                    case 3:
                        sb.Append(string.Format("Check [{0}] or the manual.", url));
                        break;
                    default:
                        sb.Append(string.Format("Link ({0}) is recommended.", url));
                        break;
                }
            }
            else
            {
                sb.Append(FillerText[random.Next(FillerText.Length)]);
            }

            sb.Append(random.Next(2) == 0 ? '\n' : ' ');
        }

        return sb.ToString();
    }

    /// <summary>
    /// Generates text containing single-level domain URLs (e.g. http://localhost, http://intranet).
    /// </summary>
    public static string GenerateSingleLevelDomainContent(
        int seed,
        int approximateSizeBytes,
        double urlDensity
    )
    {
        var random = new Random(seed);
        var sb = new StringBuilder(approximateSizeBytes + 256);

        while (sb.Length < approximateSizeBytes)
        {
            var insertUrl = random.NextDouble() < urlDensity;

            if (insertUrl)
            {
                sb.Append(SingleLevelDomainUrls[random.Next(SingleLevelDomainUrls.Length)]);
            }
            else
            {
                sb.Append(FillerText[random.Next(FillerText.Length)]);
            }

            var sep = random.Next(3);
            if (sep == 0)
            {
                sb.Append('\n');
            }
            else if (sep == 1)
            {
                sb.Append("  ");
            }
            else
            {
                sb.Append(' ');
            }
        }

        return sb.ToString();
    }
}