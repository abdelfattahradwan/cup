# Create Unity Package (cup)

Create Unity Package (cup) is a command-line app that allows you to create Unity packages from the command line without
installing Unity.

## Features

- Fast and lightweight packing process
- Create packages without installing Unity
- Ignore files and folders automatically
- Create packages with different compression levels

## Arguments

`-s, --src    (Required) Root directory path.`

`-o, --out    (Required) Output package file path.`

`-c, --cpr    (Optional) Level of compression. (0 = Optimal, 1 = Fastest, 2 = None, 3 = Smallest)`

`--ignore     (Optional) List of file/directory paths to ignore.`

`--help       Display this help screen.`

`--version    Display version information.`

## Ignoring Files/Folders

You can ignore certains files/folders by creating a `.cupignore` file at the *root directory path* that you specify.
The `.cupignore` must contain only a single `JSON` array like to following example:

```json
[
  "Materials",
  "Prefabs/Private",
  "Scripts/Private/MyScript.cs"
]
```

If the file contains anything else, an exception is thrown.

## Downloads

There are two different binaries available to download:

- A self-contained binary that ***does not*** require .NET to be installed
- A framework-dependent binary that ***requires .NET*** to be installed

NOTE: The framework-dependent binary requires at least ***.NET 6*** to be installed.

## Donating

[Donations](https://ko-fi.com/winterboltgames) are not required but are greatly appreciated ❤️
