# dotnet-alphasecret [![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://raw.githubusercontent.com/nikeee/dotnet-alphasecret/master/LICENSE) ![Build](https://github.com/nikeee/dotnet-alphasecret/workflows/Build/badge.svg)
> .NET Core implementation of [Hanno Böck's Alphasecret](https://github.com/hannob/alphasecret).

Excerpt from hannob's readme:
> GIMP has an unexpected behavior that when "deleting" content from an image with an alpha channel it will not actually delete the content, it will just be marked transparent.
>
> This can have the unintended sideeffect of leaking information if you use GIMP to remove private parts of an image, e.g. a screenshot.

## Download
Check out the [releases section](https://github.com/nikeee/dotnet-alphasecret/releases) for the latest builds.

## Usage
```
# Inspect folder:
./AlphaSecret examples

# Inspect single file:
./AlphaSecret examples/color-suspicious.png

# Reading targets from stdin:
ls -1 **/*.png | ./AlphaSecret --stdin
```

## Building from Source
```
git clone https://github.com/nikeee/dotnet-alphasecret

# Linux:
dotnet publish -c Release --self-contained --runtime linux-x64
# Resulting executable will be in:
./bin/Release/net9.0/linux-x64/publish/AlphaSecret

# Windows:
dotnet publish -c Release --self-contained --runtime win-x64
# Resulting executable will be in:
./bin/Release/net9.0/win-x64/publish/AlphaSecret.exe

# Linux:
dotnet publish -c Release --self-contained --runtime osx-x64
# Resulting executable will be in:
./bin/Release/net9.0/osx-x64/publish/AlphaSecret
```

## Why?
- Calling ImageMagick's `convert` and piping `hexdump` into `grep` works for a few images, but will be slow on millions of images.
- A bash script does not run very well on windows (WSL might help, but it has poor IO performance)

## Background
- News article: https://www.golem.de/news/alphakanal-gimp-verraet-geheimnisse-in-bildern-2002-146504.html
- Discusson on Gitlab: https://gitlab.gnome.org/GNOME/gimp/issues/4487
