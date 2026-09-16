#!/usr/bin/env python3
"""Validate registry.json: the shape of every entry, and (unless --offline) that each URL
serves a file whose sha256 matches the entry. Exit 0 on pass, 1 with every problem listed.

    tools/validate.py registry.json [--offline]
"""
import hashlib, json, re, sys, urllib.request

ID_RE = re.compile(r"^[A-Za-z0-9_]+-[A-Za-z0-9_]+$")
SIDES = {"server", "client", "both"}
REQUIRED = ["id", "name", "version", "author", "side", "game_build", "dll", "dll_sha256", "sha256", "size", "url", "license"]

def main():
    if len(sys.argv) < 2:
        sys.exit(__doc__)
    path = sys.argv[1]
    offline = "--offline" in sys.argv
    problems = []
    try:
        reg = json.load(open(path, encoding="utf-8"))
    except Exception as e:
        sys.exit("registry is not valid JSON: %s" % e)
    mods = reg.get("mods")
    if not isinstance(mods, list):
        sys.exit("registry has no mods list")
    seen = set()
    for i, m in enumerate(mods):
        where = "mods[%d] %s" % (i, m.get("id", "?"))
        for k in REQUIRED:
            if k not in m:
                problems.append("%s: missing %s" % (where, k))
        if not ID_RE.match(str(m.get("id", ""))):
            problems.append("%s: id must be Author-Name" % where)
        if m.get("side") not in SIDES:
            problems.append("%s: side must be server, client or both" % where)
        if not re.match(r"^[0-9]+\.[0-9]+\.[0-9]+$", str(m.get("version", ""))):
            problems.append("%s: version must be x.y.z" % where)
        for h in ("dll_sha256", "sha256"):
            if not re.match(r"^[0-9a-f]{64}$", str(m.get(h, ""))):
                problems.append("%s: %s must be 64 lower-case hex characters" % (where, h))
        if not str(m.get("dll", "")).endswith(".dll") or "/" in str(m.get("dll", "")) or "\\" in str(m.get("dll", "")):
            problems.append("%s: dll must be a file name ending in .dll" % where)
        url = str(m.get("url", ""))
        if not url.startswith("https://github.com/") or "/releases/download/" not in url:
            problems.append("%s: url must be a GitHub release asset" % where)
        if not isinstance(m.get("game_build"), int):
            problems.append("%s: game_build must be a number" % where)
        key = (m.get("id"), m.get("version"))
        if key in seen:
            problems.append("%s: duplicate id+version" % where)
        seen.add(key)
        if m.get("author") and m.get("id") and not str(m["id"]).startswith(str(m["author"]) + "-"):
            problems.append("%s: id must start with the author's GitHub name" % where)
        if not offline and not problems:
            try:
                data = urllib.request.urlopen(url, timeout=60).read()
                got = hashlib.sha256(data).hexdigest()
                if got != m["sha256"]:
                    problems.append("%s: the asset at the url hashes to %s, not %s" % (where, got[:12], m["sha256"][:12]))
                if len(data) != m.get("size"):
                    problems.append("%s: size is %d, entry says %s" % (where, len(data), m.get("size")))
            except Exception as e:
                problems.append("%s: url does not serve a file: %s" % (where, e))
    if problems:
        print("\n".join(problems))
        sys.exit(1)
    print("registry ok: %d entries%s" % (len(mods), " (offline: urls not fetched)" if offline else ""))

if __name__ == "__main__":
    main()
