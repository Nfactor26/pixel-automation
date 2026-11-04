# NuGet dependency update review

Source: `dotnet package list --project Pixel.Automation.sln --outdated --format json` against the configured NuGet sources on 2026-11-04. Most versions are pinned in `Directory.Build.props`; a few are declared directly in project files.

Low risk updates were applied together and validated with solution restore and build. High risk means an API, behavior, runtime, or broad integration review is needed before updating; it is not a claim that the current version is safe.

## Low risk — updated

| Package | Previous | Updated |
| --- | --- | --- |
| DotNetProjects.Extended.Wpf.Toolkit | 5.0.124 | 5.0.129 |
| MahApps.Metro | 2.4.10 | 2.4.11 |
| Newtonsoft.Json | 13.0.3 | 13.0.4 |
| NUnit.Analyzers | 4.10.0 | 4.15.0 |
| Serilog | 4.3.0 | 4.4.0 |
| Serilog.Sinks.Console | 6.0.0 | 6.1.1 |
| Serilog.Sinks.Http | 9.2.0 | 9.2.1 |

## High risk — pending review

| Package | Current | Latest |
| --- | --- | --- |
| Appium.WebDriver | 8.0.1 | 9.0.0 |
| AutoMapper | 15.0.1 | 16.2.0 |
| Caliburn.Micro | 4.0.230 | 5.0.258 |
| coverlet.collector | 6.0.4 | 10.1.0 |
| CsvHelper | 33.1.0 | 34.0.0 |
| Dirkster.AvalonDock | 4.72.1 | 5.0.1 |
| FluentAssertions | 8.6.0 | 8.11.0 |
| IdentityModel.OidcClient | 5.1.0 | Not found at the sources |
| MahApps.Metro.IconPacks | 4.11.0 | 6.2.1 |
| Microsoft.CodeAnalysis.Common | 4.14.0 | 5.9.0 |
| Microsoft.CodeAnalysis.CSharp.Features | 4.14.0 | 5.9.0 |
| Microsoft.CodeAnalysis.CSharp.Scripting | 4.14.0 | 5.9.0 |
| Microsoft.CodeAnalysis.CSharp.Workspaces | 4.14.0 | 5.9.0 |
| Microsoft.Extensions.Configuration | 9.0.8 | 10.0.14 |
| Microsoft.Extensions.Configuration.Binder | 9.0.8 | 10.0.14 |
| Microsoft.Extensions.Configuration.Json | 9.0.8 | 10.0.14 |
| Microsoft.NET.Test.Sdk | 17.14.1 | 18.11.0 |
| Microsoft.Playwright | 1.55.0 | 1.64.1 |
| Microsoft.Web.WebView2 | 1.0.1370.28 | 1.0.4314.35 |
| NSubstitute | 5.3.0 | 6.2.0 |
| nunit | 4.4.0 | 5.0.0 |
| NUnit3TestAdapter | 5.1.0 | 6.3.1 |
| OpenCvSharp4 | 4.11.0.20250507 | 4.13.0.20260627 |
| OpenCvSharp4.runtime.win | 4.11.0.20250507 | 4.13.0.20260627 |
| OpenTelemetry.Exporter.Console | 1.12.0 | 1.20.1 |
| OpenTelemetry.Exporter.OpenTelemetryProtocol | 1.12.0 | 1.20.1 |
| OpenTelemetry.Extensions.Hosting | 1.12.0 | 1.20.1 |
| OpenTelemetry.Instrumentation.Http | 1.12.0 | 1.20.1 |
| Polly | 8.6.2 | 8.9.0 |
| RestSharp | 112.1.0 | 114.1.0 |
| Selenium.Support | 4.38.0 | 4.52.0 |
| Selenium.WebDriver | 4.38.0 | 4.52.0 |
| Serilog.Settings.Configuration | 9.0.0 | 10.0.1 |
| Serilog.Sinks.SpectreConsole | 0.3.3 | 0.4.0 |
| SixLabors.ImageSharp | 3.1.11 | 4.1.3 |
| Spectre.Console | 0.53.0 | 0.57.2 |
| Spectre.Console.Cli | 0.53.0 | 0.57.2 |
| System.Diagnostics.DiagnosticSource | 9.0.8 | 10.0.14 |
| System.Drawing.Common | 9.0.8 | 10.0.14 |
| System.Management | 9.0.8 | 10.0.14 |
| System.Reactive | 6.0.1 | 7.0.0 |
| Vanara.Core | 4.2.1 | 5.0.8 |
| Vanara.PInvoke.CldApi | 4.2.1 | 5.0.8 |
| Vanara.PInvoke.Gdi32 | 4.2.1 | 5.0.8 |
| Vanara.PInvoke.Kernel32 | 4.2.1 | 5.0.8 |
| Vanara.PInvoke.Ole | 4.2.1 | 5.0.8 |
| Vanara.PInvoke.Shared | 4.2.1 | 5.0.0 |
| Vanara.PInvoke.Shell32 | 4.2.1 | 5.0.8 |
| Vanara.PInvoke.User32 | 4.2.1 | 5.0.8 |
| Vanara.Windows.Shell | 4.2.1 | 5.0.8 |

Review priorities: AutoMapper, OpenTelemetry, and SixLabors.ImageSharp currently produce NuGet vulnerability warnings. Keep related packages in a family aligned when upgrading. `IdentityModel.OidcClient` returned “Not found at the sources”; verify its package source before changing it.

## Verification

Solution restore and build succeeded with 0 errors. All 16 unit test projects other than `Pixel.Automation.UIAComWrapper.Tests` passed when run individually: 704 passed, 2 skipped, 0 failed. `Pixel.Automation.Test.Helpers` is a helper project and has no test assembly.
