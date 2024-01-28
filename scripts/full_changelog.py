import time
import os
import json
import urllib.parse
import urllib.request

import typing as t


def is_github_workflow() -> bool:
    """Is script running on GitHub actions"""
    return "GITHUB_ACTIONS" in os.environ


if is_github_workflow():
    class GitHubVars:
        @property
        def repository(self) -> str:
            return os.environ["GITHUB_REPOSITORY"]

        @property
        def token(self) -> t.Optional[str]:
            return os.environ.get("GITHUB_TOKEN")  # If not token found => return None
else:
    # If not in workflow => Use default settings
    class GitHubVars:
        repository = "MurkyYT/CSAuto"
        token = None


ROOT_DIR = os.path.dirname(os.path.dirname(__file__))
DOCS_DIR = os.path.join(ROOT_DIR, "Docs")
CHANGELOG_PATH = os.path.join(DOCS_DIR, "FullChangelog.MD")


class GitHubAPI:
    VERSION = "2022-11-28"

    def __init__(self, token: t.Optional[str]):
        self.headers = {
            "X-GitHub-Api-Version": self.VERSION
        }
        if token:
            self.headers["Authorization"] = f"Bearer {token}"

    def make_request(self, path: str, params: dict):
        if not path.startswith("/"):
            path = "/" + path
        query = "&".join([f"{key}={value}" for key, value in params.items()])
        url = urllib.parse.urlunsplit(("https", "api.github.com", path, query, ""))
        req = urllib.request.Request(url=url, headers=self.headers)
        resp = urllib.request.urlopen(req)
        return json.loads(resp.read().decode(resp.headers.get_content_charset("utf-8")))

    def releases(self, repository: str, per_page: int = 30, page: int = 1):
        return self.make_request(f"repos/{repository}/releases", params=dict(per_page=per_page, page=page))

    def get_all_releases(self, repository: str):
        res = list()
        page_num = 1
        while page := self.releases(repository, per_page=100, page=page_num):
            res.extend(page)
            page_num += 1
            time.sleep(0.5)
        return res


def prepare_changelog(changelog_ver: str):
    changelog_ver = changelog_ver.split("**Full Changelog**")[0].split("🛡 [VirusTotal")[0]
    changelog_ver = changelog_ver.strip("\n").strip("\r\n")  # Remove unneeded \n's
    # changelog_ver = re.sub(r"#(?P<ref_id>\d+)", r"[#\g<ref_id>](https://github.com/MurkyYT/CSAuto/issues/\g<ref_id>)", changelog_ver)  # (Not needed for GitHub MD, just testing)
    return changelog_ver


def main():
    github_vars = GitHubVars()
    github_api = GitHubAPI(token=github_vars.token)
    releases = github_api.get_all_releases(github_vars.repository)
    changelog_vers = [prepare_changelog(x["body"]) for x in releases if x["body"]]
    full_changelog_md = "\n\n<!--Version split-->\n\n".join(changelog_vers)
    full_changelog_md = full_changelog_md + "\n"  # Add single newline
    try:
        os.remove(CHANGELOG_PATH)
    except FileNotFoundError:
        pass
    with open(CHANGELOG_PATH, mode="w+", encoding="utf-8", newline="\n") as f:
        f.write(full_changelog_md)


if __name__ == '__main__':
    main()
