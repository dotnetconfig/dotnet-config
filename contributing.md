# How to contribute

One of the easiest ways to contribute is to participate in discussions on GitHub issues. You can also contribute by submitting pull requests with code changes. We also have a [discord channel](https://discord.gg/x4qhjYd) for more immediate discussions with the community.

## Contributing code

Simply fork the repo, open the main solution and build it with your IDE of choice. You can build and run all tests with the *dotnet* CLI:

```
> dotnet build
> dotnet test
```

This works across Windows, macOS and Linux, our [PR validation CI](https://github.com/dotnetconfig/dotnet-config/blob/dev/.github/workflows/pr.yml) ensures that any contributed PR runs in all supported platforms, so you're free to just build and test on your favorite one :).

The repository provides a `.editorconfig` that should take care of formatting code appropriately on most code editors so you don't have to do that manually. The only requirement for code styling is that you try to follow the style of the surrounding code so your contribution blends in as much as possible. We believe in common sense, tooling and direct PR feedback rather than lengthy convention docs.

## Contributing documentation

The project uses [docfx](https://dotnet.github.io/docfx/) to generate the https://dotnetconfig.org site. The *docs* folder containings the main article contents, and the root *README.md* is included in its entirety to make up the [main page](docs/index.md).

Documentation is published automatically from the *main* branch, which is also used for building releases.

The most common type of doc contribution would be adding a third-party tool that leverages this project for configuration, by adding an entry to the [toc.yml](docs/who/toc.yml) and a corresponding showcase doc for it. Use the existing docs a guideline.