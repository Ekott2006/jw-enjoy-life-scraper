# JW Scraper 🎬

## Introduction

A multi-threaded C# console application designed to scrape and bulk-download "Enjoy Life Forever!" video media content from JW.ORG with custom file naming, section filtering, and existing local directory deduplication.

## 🚀 Getting Started

### Prerequisites

    .NET 8.0 SDK or higher installed.

### Installation

    Clone the repository:
    Bash

    git clone [https://github.com/ekottifiok/jw-enjoy-life-scraper.git](https://github.com/ekottifiok/jw-enjoy-life-scraper.git)
    cd jw-enjoy-life-scraper

    Configure Section Links:
    Ensure a config.json file is present in your execution root directory mapping target languages to URL templates:
    JSON

    {
      "English": "[https://www.jw.org/en/library/books/enjoy-life-forever/section-](https://www.jw.org/en/library/books/enjoy-life-forever/section-){0}/"
    }

    Build & Run:
    Bash

    dotnet build
    dotnet run --project Console

### 📖 Usage Walkthrough

    Language Selection: Choose your preferred language configured in config.json.

    Section Selection: Select a specific section (e.g., Section 1) or choose All.

    Resolution & Preferences: Select target video resolution, file naming style, and folder structure.

    Folder Scan Check: Opt to scan an existing directory for existing .mp4 video files to prevent downloading duplicates.

    Download Execution: The application displays real-time download progress and completes the operation safely.

## 📄 License

Distributed under the MIT License. See LICENSE for more information.
