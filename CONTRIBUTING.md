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