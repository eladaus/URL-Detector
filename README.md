# URL Detector

[![NuGet](https://img.shields.io/nuget/v/eladaus.urldetector.svg)](https://www.nuget.org/packages/eladaus.urldetector/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/eladaus.urldetector.svg)](https://www.nuget.org/packages/eladaus.urldetector/)

This codebase is an open source C# port of the Java code in https://github.com/URL-Detector/URL-Detector, which in turn
was a fork of the LinkedIn Engineering team's open sourced https://github.com/linkedin/URL-Detector, which seems to be
abandoned. This port was originally based the Java code as at July 20, 2019, and was created to allow continued
maintenance by the OS C# community.

For any and all future updates, the code releases will utilizing SemVer semantic versioning style.

Conveniently, eladaus.urldetector is [available on NuGet](https://www.nuget.org/packages/eladaus.urldetector/) as
`eladaus.urldetector`. Install it from NuGet Package Manager Console with:

~~~~
Install-Package eladaus.urldetector
~~~~

## Known Issues

None

## Description

The url detector is a library originally created by the Linkedin Security Team to detect and extract urls in a body of
text.

It is able to find and detect any urls such as:

* __HTML 5 Scheme__   - //www.linkedin.com
* __Usernames__       - user:pass@linkedin.com
* __Email__           - fred@linkedin.com
* __IPv4 Address__    - 192.168.1.1/hello.html
* __IPv4 Octets__     - 0x00.0x00.0x00.0x00
* __IPv4 Decimal__    - http://123123123123/
* __IPv6 Address__    - ftp://[::]/hello
* __IPv4-mapped IPv6 Address__  - http://[fe30:4:3:0:192.3.2.1]/

_Note: Keep in mind that for security purposes, its better to overdetect urls and check more against blacklists than to
not detect a url that was submitted. As such, some things that we detect might not be urls but somewhat look like urls.
Also, instead of complying with RFC 3986 (http://www.ietf.org/rfc/rfc3986.txt), we try to detect based on browser
behavior, optimizing detection for urls that are visitable through the address bar of Chrome, Firefox, Internet
Explorer, and Safari._

It is also able to identify the parts of the identified urls. For example, for the url:
`http://user@linkedin.com:39000/hello?boo=ff#frag`

* Scheme - "http"
* Username - "user"
* Password - null
* Host - "linkedin.com"
* Port - 39000
* Path - "/hello"
* Query - "?boo=ff"
* Fragment - "#frag"

---

## How to Use:

Using the URL detector library is simple. Simply import the UrlDetector object and give it some options. In response,
you will get a list of urls which were detected.

For example, the following code will find the url `linkedin.com`

```csharp

    UrlDetector parser = new UrlDetector("hello this is a url Linkedin.com", UrlDetectorOptions.Default);
    List<Url> found = parser.Detect();

    foreach (Url url in found)
    {
        Console.WriteLine("Scheme: " + url.GetScheme());
        Console.WriteLine("Host: " + url.GetHost());
        Console.WriteLine("Path: " + url.GetPath());
    }
```

### Quote Matching and HTML

Depending on your input string, you may want to handle certain characters in a special way. For example if you are
parsing HTML, you probably want to break out of things like quotes and brackets. For example, if your input looks like

> &lt;a href="http://linkedin.com/abc"&gt;linkedin.com&lt;/a&gt;

You probably want to make sure that the quotes and brackets are extracted. For that reason, using UrlDetectorOptions
will allow you to change the sensitivity level of detection based on your expected input type. This way you can detect
`linkedin.com` instead of `linkedin.com</a>`.

In code this looks like:

```csharp

    UrlDetector parser = new UrlDetector("<a href="linkedin.com/abc">linkedin.com</a>", UrlDetectorOptions.HTML);
    List<Url> found = parser.Detect();

```

---

## Developer Setup

After cloning the repository, restore the local dotnet tools and install the Husky git hooks:

```bash
dotnet tool restore
dotnet husky install
```

This installs [CSharpier](https://csharpier.com/) and [Husky.Net](https://alirezanet.github.io/Husky.Net/) as local
tools, and sets up a pre-commit hook that automatically formats staged `.cs` files with CSharpier before each commit.

To manually run CSharpier across the entire solution:

```bash
dotnet csharpier format src/
```

For more information, see the [CSharpier GitHub repository](https://github.com/belav/csharpier).

---

## Benchmarks

The project includes a [BenchmarkDotNet](https://benchmarkdotnet.org/) suite that measures CPU and memory usage across
different input sizes, URL densities, URL types, detector options, and structured content formats.

Benchmarks must be run in **Release** mode. From the repository root:

```bash
dotnet run -c Release --project src/urldetector.benchmarks
```

You will be prompted to select a benchmark class to run. To run a specific suite directly, pass a `--filter`:

```bash
# Run only the input-size scaling benchmarks
dotnet run -c Release --project src/urldetector.benchmarks -- --filter '*InputSizeBenchmarks*'

# Run only the URL density benchmarks
dotnet run -c Release --project src/urldetector.benchmarks -- --filter '*UrlDensityBenchmarks*'

# Run all benchmarks (no filter prompt)
dotnet run -c Release --project src/urldetector.benchmarks -- --filter '*'
```

Available benchmark suites:

| Suite                         | What it measures                                      |
|-------------------------------|-------------------------------------------------------|
| `InputSizeBenchmarks`         | Scaling from 200 B to 5 MB inputs                     |
| `UrlDensityBenchmarks`        | Impact of URL density (0%–90%) on 50 KB input         |
| `UrlTypeBenchmarks`           | Cost per URL type (web, email, IPv4, IPv6, mixed)     |
| `DetectorOptionsBenchmarks`   | Overhead of each `UrlDetectorOptions` mode            |
| `StructuredContentBenchmarks` | Matching options against HTML/JSON/XML/JS content     |
| `RealWorldHtmlBenchmarks`     | Real HTML sample files with default vs. all flags     |
| `TestCpuAndMemoryUsage`       | Legacy suite — real HTML files with all flags enabled |

Results are written to `src/urldetector.benchmarks/BenchmarkDotNet.Artifacts/`.

---

## About:

This C# port was originally created by Dale Holborow of eladaus oy. Future contributions are welcome.

The original Java library was written by the security team and Linkedin when other options did not exist. Some of the
primary authors are:

* Vlad Shlosberg (vshlosbe@linkedin.com)
* Tzu-Han Jan (tjan@linkedin.com)
* Yulia Astakhova (jastakho@linkedin.com)

---

## License

The C# port provided by eladaus is released under Apache License Version 2.0, as per the original LinkedIn java library.

Original Java code is Copyright 2015 LinkedIn Corp. All rights reserved.

Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the
License. You may obtain a copy of the license at http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "
AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
