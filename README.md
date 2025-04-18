# Create Unity Package (cup)

Create Unity Package (cup) is a command-line app that allows you to create Unity packages from the command line without
installing Unity.

## Features

- Fast and efficient packing process
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

You can ignore certain files/folders by creating a `.cupignore` file at the _root directory path_ that you specify. The
`.cupignore` file should contain glob patterns to ignore, with one pattern per line:

```plaintext
**/Editor/*
**/Tests/*
**/Examples/*
**/*.tmp
Assets/Scripts/Debug/**/*
```

These patterns will be added to any patterns specified with the `-e, --exclude` command-line argument.

If the file cannot be read, you will be prompted to decide whether to continue without the ignore patterns.

## Downloads

There are two different flavours of binaries available to download, slim and standalone:

- A slim binary ***requires .NET*** to be installed.
- A standalone binary ***does not*** require .NET to be installed.

NOTE: slim binaries require at least ***.NET 9*** to be installed.

## Help the Project

If you find this project useful, consider supporting it by buying me a coffee:

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/Q5Q361YW5)

Your support is appreciated. Thank you!
