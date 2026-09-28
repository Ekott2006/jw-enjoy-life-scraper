# JW Enjoy Life Scraper

A .NET command-line tool that scrapes JW.org media pages for the Enjoy Life series and helps you download the files in bulk. The app walks you through selecting a language, section, download resolution, output folder, and file naming/path options.

## Overview

This project consists of:

- `Console/` – interactive CLI entry point
- `Lib/` – scraper logic, helpers, DTOs, and service layer
- `Directory.Packages.props` – centralized NuGet package versions

The app is designed around the flow in `Console/Program.cs`:

1. Load `config.json` to get language/section URLs.
2. Prompt the user to choose a language.
3. Scrape section pages for JSON metadata URLs.
4. Request the media JSON and build available file metadata.
5. Let the user choose a resolution and output settings.
6. Download the selected media files into the chosen folder.

## Features

- Interactive terminal prompts using Spectre.Console
- Multi-language support (`English`, `Pidgin`, `Efik`, etc.)
- Scrapes JW.org section URLs from a config file
- Selects section(s) to download
- Chooses output resolution based on available sizes
- Supports grouped or simple folder layout
- Supports simple or JW Library-style file names
- Optional copy directory after download
- Uses local caching to avoid repeated fetches

## Project Structure

```text
jw-enjoy-life-scraper/
├── Console/
│   ├── Program.cs
│   ├── config.json
│   └── Console.csproj
├── Lib/
│   ├── Dto/
│   ├── Interfaces/
│   ├── Services/
│   ├── Helper.cs
│   └── Lib.csproj
├── Directory.Packages.props
├── WebsiteMediaScraper.sln
├── README.md
└── .gitignore
```

## Requirements

- .NET SDK 10.0 or later (`TargetFramework` is set to `net10.0`)
- Internet access to reach JW.org
- Write access to the folder where the output files are saved

## Setup

From the project root:

```bash
dotnet restore
```

Then run the CLI:

```bash
dotnet run --project Console/Console.csproj
```

Or build first and execute the binary:

```bash
dotnet build Console/Console.csproj
./Console/bin/Debug/net10.0/Console
```

On Windows PowerShell, the app can also be run directly from the built output folder.

## Configuration

The app expects a file named `config.json` in the `Console` folder.

Example:

```json
{
  "English": {
    "Section 1": "https://www.jw.org/en/library/books/enjoy-life-forever/section-1/media/",
    "Section 2": "https://www.jw.org/en/library/books/enjoy-life-forever/section-2/media/",
    "Section 3": "https://www.jw.org/en/library/books/enjoy-life-forever/section-3/media/",
    "Section 4": "https://www.jw.org/en/library/books/enjoy-life-forever/section-4/media/"
  },
  "Pidgin": {
    "Section 1": "https://www.jw.org/wes-x-pgw/library/book/enjoy-life-now-and-forever/part-1/the-one-for-internet/"
  }
}
```

### How it works

- Top-level keys are the language names shown in the command line.
- Each language contains section names and the JW.org URL for that section.
- The app fetches HTML from these section pages and then finds the media JSON entries.
- You can add more languages or sections by extending this JSON file.

## Interactive Usage Flow

When the program starts, it asks for:

1. Language
2. Section(s) to download
3. Resolution
4. Download output directory
5. Optional copy directory
6. File naming format
7. Folder organization style

### Choice examples

- Resolution: based on available video formats like 240, 360, 480, 720, etc.
- Download directory: blank uses the current working directory or default `files` folder
- Copy directory: optional secondary folder where files are copied
- File name type:
  - `Simple` → easier to read names
  - `JwLibrary` → library-style naming
- File path type:
  - `Simple` → all files in one directory
  - `Grouped` → files separated by section

## Download Output

The app uses the default download folder name:

```text
files/
```

If the target folder is empty or invalid, it falls back to the current working directory plus `files`.

## Notes

- This project is built for scraping media pages from JW.org and downloading their public media files.
- The app stores a local cache in `.cache` during scraping.
- The files are copied or downloaded in a way that is suited for bulk media retrieval, but you should ensure your use follows the applicable website terms and local laws.

## Common Commands

```bash
dotnet restore
dotnet build Console/Console.csproj
dotnet run --project Console/Console.csproj
```

## Troubleshooting

- If the app cannot find config data, confirm that `Console/config.json` exists and is valid JSON.
- If no files download, verify the selected language and sections are valid and the website still publishes those media listings.
- If there is a build issue, ensure the .NET SDK version matches the project target framework.

## License

This project does not currently declare a license in the repository root. Check with the repository owner before redistributing or using it outside of a personal or internal project context.
