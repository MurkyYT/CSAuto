import os
import re

import typing as t

ROOT_DIR = os.path.dirname(os.path.dirname(__file__))
DATA_DIR = os.path.join(ROOT_DIR, "Data")
VERSION_FILEPATH = os.path.join(DATA_DIR, "version")
MAIN_APP_FILEPATH = os.path.join(ROOT_DIR, "src", "CSAuto", "MainApp.xaml.cs")
REGEX_PATTERN = re.compile(r"^\s+public const string VER = \"(?P<version>.+)\";$")


def get_version() -> t.Optional[str]:
    with open(MAIN_APP_FILEPATH, encoding="utf-8", mode="r") as f:
        match = REGEX_PATTERN.findall(f.read())
    if not match:
        return
    return match.group("version")


def main():
    version = get_version()
    if version is None:
        raise AttributeError("Version line not found")
    try:
        os.remove(VERSION_FILEPATH)
    except FileNotFoundError:
        pass
    with open(VERSION_FILEPATH, mode="w+", encoding="utf-8") as f:
        f.write(version)


if __name__ == "__main__":
    main()
