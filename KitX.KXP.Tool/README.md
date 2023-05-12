# About

This is a dotnet tool for `KitX Project`.

This tool is used to pack `KitX Plugin` files into `.kxp` format file for `KitX Dashboard`.

# Usage

## Requirements

You need install dotnet sdk first ! (6.0+ recommended)

## Install

```shell
# Use dotnet tool command to install.
dotnet tool install --global KitX.KXP.Tool

# After install, you can use `kxpmaker` command to visit it.
kxpmaker --version
```

## Help

```shell
# Use `--help` global argument to get help doc.
kxpmaker --help
```

Output would like be:

```text
KitX.KXP.Tool 1.2.6543.440
Copyright (C) 2023 Crequency

  pack       (Default Verb) Pack plugin files into `.kxp` file.

  help       Display more information on a specific command.

  version    Display version information.

```

If you want to get help doc for `pack` command, you can use:

```shell
kxpmaker pack --help
```

## Pack

We can take a look output of `kxpmaker pack --help` first.

It would like be:

```text
KitX.KXP.Tool 1.2.6543.440
Copyright (C) 2023 Crequency

ERROR(S):
  Required option 's, source' is missing.

  -s, --source              Required. Source files path.

  -o, --output              Output path of `.kxp` file.

  -n, --output-file-name    Output file name.

  -l, --loader              Loader struct file path.

  -p, --plugin              Plugin struct file path.

  -i, --ignore              File extensions to ignore.

  -v, --verbose             Set output to verbose messages.

  --help                    Display this help screen.

  --version                 Display version information.

```

Then we can use `kxpmaker` command as belowed to pack plugin files.

```shell
# Put all plugin files into `./plugin/` directory for example.
mkdir plugin

# Use `ksmaker` to generate `PluginStruct.json` and `LoaderStruct.json` files if you do not have them.
ksmaker -o ./plugin/

# Edit generated files to custom them, use `vim` for example.
vim ./plugin/PluginStruct.json
vim ./plugin/LoaderStruct.json

# Pack your files.
# In default, output file name would be directory name,
#   for example, belowed command would output to `plugin.kxp` file,
#   you can also use `-n` argument to custom output file name.
kxpmaker -s ./plugin/
```
