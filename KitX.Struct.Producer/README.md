# About

This is a dotnet tool for `KitX Project`.

This tool is used to generate `PluginStruct.json` and `LoaderStruct.json` files for `KitX Plugin`.

# Usage

## Requirements

You need install dotnet sdk first ! (6.0+ recommended)

## Install

```shell
# Use dotnet tool command to install.
dotnet tool install --global KitX.Struct.Producer

# After install, you can use `ksmaker` command to visit it.
ksmaker --version
```

## Help

```shell
# Use `--help` global argument to get help doc.
ksmaker --help
```

Output would like be:

```text
KitX.Struct.Producer 1.2.6543.440
Copyright (C) 2023 Crequency

  generate, gen    (Default Verb) Generate KitX (Loader/Plugin) Struct.

  help             Display more information on a specific command.

  version          Display version information.

```

If you want to get help doc for `generate` command, you can use:

```shell
ksmaker gen --help
```

## Generate

We can take a look output of `ksmaker gen --help` first.

It would like be:

```text
KitX.Struct.Producer 1.2.6543.440
Copyright (C) 2023 Crequency

  -a, --all        (Group: templates) (Default: true) Generate All.

  -l, --loader     (Group: templates) Generate Loader Struct.

  -p, --plugin     (Group: templates) Generate Plugin Struct.

  -o, --output     Output path.

  -v, --verbose    Set output to verbose messages.

  --help           Display this help screen.

  --version        Display version information.

```

Then we can use `ksmaker gen` command to generate struct files.

In default, it would generate `PluginStruct.json` and `LoaderStruct.json` files in current directory.

You can use `-o` argument to set output path.

If you pass `-l` argument, it would generate `LoaderStruct.json` file only.

If you pass `-p` argument, it would generate `PluginStruct.json` file only.

If you pass `-a` argument, it would generate `PluginStruct.json` and `LoaderStruct.json` files, this option is default set to true.

