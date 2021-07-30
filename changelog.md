

## [v1.0.5](https://github.com/dotnetconfig/dotnet-config/tree/v1.0.5) (2021-07-30)

[Full Changelog](https://github.com/dotnetconfig/dotnet-config/compare/v1.0.4...v1.0.5)

:sparkles: Implemented enhancements:

- Include readme in package [\#77](https://github.com/dotnetconfig/dotnet-config/issues/77)

:bug: Fixed bugs:

- Configuration extension AddDotNetConfig inverts settings hierarchy/priority [\#78](https://github.com/dotnetconfig/dotnet-config/issues/78)
- Impossible to have an extension-less config file name [\#76](https://github.com/dotnetconfig/dotnet-config/issues/76)
- Dependency on Configuration.Abstractions doesn't work with Functions v3 [\#73](https://github.com/dotnetconfig/dotnet-config/issues/73)

## [v1.0.4](https://github.com/dotnetconfig/dotnet-config/tree/v1.0.4) (2021-06-12)

[Full Changelog](https://github.com/dotnetconfig/dotnet-config/compare/v1.0.3...v1.0.4)

:sparkles: Implemented enhancements:

- Expose Section and Subsection for a ConfigSection [\#70](https://github.com/dotnetconfig/dotnet-config/issues/70)

:twisted_rightwards_arrows: Merged:

- Bump files with dotnet-file sync [\#72](https://github.com/dotnetconfig/dotnet-config/pull/72) (@kzu)

## [v1.0.3](https://github.com/dotnetconfig/dotnet-config/tree/v1.0.3) (2021-04-29)

[Full Changelog](https://github.com/dotnetconfig/dotnet-config/compare/v1.0.0...v1.0.3)

:sparkles: Implemented enhancements:

- ≥ Add System.CommandLine support [\#65](https://github.com/dotnetconfig/dotnet-config/pull/65) (@kzu)

:bug: Fixed bugs:

- Position class should not be public, it's an internal implementation detail [\#66](https://github.com/dotnetconfig/dotnet-config/issues/66)
- dotnet-config tool is missing repository/project properties [\#64](https://github.com/dotnetconfig/dotnet-config/issues/64)
- ConfigSection facade over Config is missing immutability feature [\#61](https://github.com/dotnetconfig/dotnet-config/issues/61)

## [v1.0.0](https://github.com/dotnetconfig/dotnet-config/tree/v1.0.0) (2021-04-27)

[Full Changelog](https://github.com/dotnetconfig/dotnet-config/compare/v1.0.0-rc.3...v1.0.0)

:sparkles: Implemented enhancements:

- Disable async IO since configuration loading is synchronous [\#57](https://github.com/dotnetconfig/dotnet-config/issues/57)
- Add Microsoft.Extensions.Configuration support [\#3](https://github.com/dotnetconfig/dotnet-config/issues/3)

:twisted_rightwards_arrows: Merged:

- ⚙ Account for test flakyness in CI [\#59](https://github.com/dotnetconfig/dotnet-config/pull/59) (@kzu)
- ⚙ Add basic Microsoft.Extensions.Configuration support [\#58](https://github.com/dotnetconfig/dotnet-config/pull/58) (@kzu)

## [v1.0.0-rc.3](https://github.com/dotnetconfig/dotnet-config/tree/v1.0.0-rc.3) (2021-04-26)

[Full Changelog](https://github.com/dotnetconfig/dotnet-config/compare/v1.0.0-rc.2...v1.0.0-rc.3)

:sparkles: Implemented enhancements:

- Switch to an immutable internal structure and public API [\#54](https://github.com/dotnetconfig/dotnet-config/issues/54)
- When writing initial file, add header line [\#53](https://github.com/dotnetconfig/dotnet-config/issues/53)

:bug: Fixed bugs:

- When concurrently reading from config file, IO exception may be thrown [\#55](https://github.com/dotnetconfig/dotnet-config/issues/55)
- Fails to save variables in global dir [\#51](https://github.com/dotnetconfig/dotnet-config/issues/51)
- Fix \(code\) editor launch on Windows [\#35](https://github.com/dotnetconfig/dotnet-config/issues/35)

:twisted_rightwards_arrows: Merged:

- Make model immutable to avoid concurrency issues [\#56](https://github.com/dotnetconfig/dotnet-config/pull/56) (@kzu)
- When saving to an aggregate config, use fallback locations [\#52](https://github.com/dotnetconfig/dotnet-config/pull/52) (@kzu)
- Bump files with dotnet-file sync [\#48](https://github.com/dotnetconfig/dotnet-config/pull/48) (@kzu)
- Bump files with dotnet-file sync [\#46](https://github.com/dotnetconfig/dotnet-config/pull/46) (@kzu)
- 🔄 dotnet-file sync [\#42](https://github.com/dotnetconfig/dotnet-config/pull/42) (@kzu)
- 🔄 dotnet-file sync [\#41](https://github.com/dotnetconfig/dotnet-config/pull/41) (@kzu)
- 🖆 Apply devlooped/oss template via dotnet-file [\#38](https://github.com/dotnetconfig/dotnet-config/pull/38) (@kzu)

## [v1.0.0-rc.2](https://github.com/dotnetconfig/dotnet-config/tree/v1.0.0-rc.2) (2020-12-21)

[Full Changelog](https://github.com/dotnetconfig/dotnet-config/compare/v1.0.0-rc.1...v1.0.0-rc.2)

:bug: Fixed bugs:

- When reading hierarchical configurations, ensure files are read only once [\#31](https://github.com/dotnetconfig/dotnet-config/issues/31)

:twisted_rightwards_arrows: Merged:

- Fix \(code\) editor launch on Windows [\#33](https://github.com/dotnetconfig/dotnet-config/pull/33) (@atifaziz)
- ☝ Ensure files are read only once when building Config [\#32](https://github.com/dotnetconfig/dotnet-config/pull/32) (@kzu)

## [v1.0.0-rc.1](https://github.com/dotnetconfig/dotnet-config/tree/v1.0.0-rc.1) (2020-12-15)

[Full Changelog](https://github.com/dotnetconfig/dotnet-config/compare/v1.0.0-rc...v1.0.0-rc.1)

:sparkles: Implemented enhancements:

- 🖐 Add native .NET5 support to dotnet-config tool [\#30](https://github.com/dotnetconfig/dotnet-config/pull/30) (@kzu)

:hammer: Other:

- 🖐 Add native .NET5 support to dotnet-config tool [\#29](https://github.com/dotnetconfig/dotnet-config/issues/29)

:twisted_rightwards_arrows: Merged:

- Add ReportGenerator tool [\#16](https://github.com/dotnetconfig/dotnet-config/pull/16) (@kzu)

## [v1.0.0-rc](https://github.com/dotnetconfig/dotnet-config/tree/v1.0.0-rc) (2020-09-06)

[Full Changelog](https://github.com/dotnetconfig/dotnet-config/compare/392c1087a84a2cb49a280b30d638213fa6b36c7d...v1.0.0-rc)

:hammer: Other:

- Allow resolving file path variable values relative to their declaring file [\#5](https://github.com/dotnetconfig/dotnet-config/issues/5)

:twisted_rightwards_arrows: Merged:

- Run CI validation via GH actions for speed [\#13](https://github.com/dotnetconfig/dotnet-config/pull/13) (@kzu)
- Build and test in all supported platforms [\#12](https://github.com/dotnetconfig/dotnet-config/pull/12) (@kzu)
- Get normalized and resolved relative paths from config [\#11](https://github.com/dotnetconfig/dotnet-config/pull/11) (@kzu)
- Rename dotnet-config-lib to DotNetConfig [\#10](https://github.com/dotnetconfig/dotnet-config/pull/10) (@kzu)
- Fix inclusion of build metadata in package id in non-main builds [\#9](https://github.com/dotnetconfig/dotnet-config/pull/9) (@kzu)
- Add support for .netconfig.user files [\#8](https://github.com/dotnetconfig/dotnet-config/pull/8) (@kzu)
- Add source link to library to support debugging [\#7](https://github.com/dotnetconfig/dotnet-config/pull/7) (@kzu)
- Replace the parsing from Superpower to manual [\#6](https://github.com/dotnetconfig/dotnet-config/pull/6) (@kzu)
- Rename Microsoft.DotNet \> DotNetConfig [\#4](https://github.com/dotnetconfig/dotnet-config/pull/4) (@kzu)
- Initial docfx-based site and corresponding GH actions [\#2](https://github.com/dotnetconfig/dotnet-config/pull/2) (@kzu)
- API improvements based on feedback from dotnet-vs [\#1](https://github.com/dotnetconfig/dotnet-config/pull/1) (@kzu)



\* *This Changelog was automatically generated by [github_changelog_generator](https://github.com/github-changelog-generator/github-changelog-generator)*
