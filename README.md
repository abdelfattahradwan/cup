# Create Unity Package (cup)

Create Unity Package (cup) is a command-line app that allows you to create Unity packages from the command line without
installing Unity.

## Features

- Fast and lightweight packing process
- Create packages without installing Unity
- Ignore files and folders automatically
- Create packages with different compression levels

## Arguments

<table>
    <tr>
        <th>Argument</th>
        <th>Description</th>
    </tr>
    <tr>
        <td>-s, --src</td>
        <td>Root directory path.</td>
    </tr>
    <tr>
        <td>-o, --out</td>
        <td>Output package file path.</td>
    </tr>
    <tr>
        <td>-c, --cpr</td>
        <td>Level of compression. (0 = Optimal, 1 = Fastest, 2 = None, 3 = Smallest)</td>
    </tr>
    <tr>
        <td>-e, --exclude</td>
        <td>List of file/directory paths to ignore.</td>
    </tr>
    <tr>
        <td>--help</td>
        <td>Display this help screen.</td>
    </tr>
    <tr>
        <td>--version</td>
        <td>Display version information.</td>
    </tr>
</table>

## Ignoring Files/Folders

You can ignore certain files/folders by creating a `.cupignore` file at the *root directory path* that you specify.
The `.cupignore` must contain only a single `JSON` array with glob patterns to ignore.

```json
[
  "**/Editor/*",
  "**/Tests/*",
  "**/Examples/*",
  "**/*.tmp",
  "Assets/Scripts/Debug/**/*"
]
```

If the file contains anything else, an exception is thrown.

## Downloads

There are two different binaries available to download:

- A self-contained binary that ***does not*** require .NET to be installed
- A framework-dependent binary that ***requires .NET*** to be installed

NOTE: The framework-dependent binary requires at least ***.NET 8*** to be installed.

## Help the Project

If you find this project useful, consider supporting it by buying me a coffee:

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/Q5Q361YW5)

Your support is appreciated. Thank you!
