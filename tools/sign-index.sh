#!/usr/bin/env bash
# Sign registry.json into index.json, the file the Waygate app reads.
#
#   tools/sign-index.sh [path/to/registry.json]
#
# index.json is an envelope: {"schema":1,"signature_alg":"ed25519","signature":"<hex>",
# "payload_b64":"<base64 of registry.json, byte for byte>"}. Signing the raw bytes means
# the signer (this script, openssl) and the verifier (the app, .NET) never have to agree
# on how to serialise JSON. The key is the same offline ed25519 key that signs launcher
# updates, so the app trusts exactly one key for everything it downloads.
#
# Refuses to sign if the key's public half is not the one the launcher embeds, and
# refuses if registry.json fails validation (tools/validate.py).
set -euo pipefail
HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REG="${1:-$HERE/../registry.json}"
OUT="$(dirname "$REG")/index.json"
KEYFILE="${WAYGATE_UPDATE_KEY:-$HOME/.waygate-update-keys/waygate-update-ed25519.pem}"
[[ -f "$KEYFILE" ]] || { echo "missing signing key at $KEYFILE" >&2; exit 1; }
[[ -f "$REG" ]] || { echo "missing $REG" >&2; exit 1; }

if [[ "${2:-}" != "--no-validate" ]]; then python3 "$HERE/validate.py" "$REG" --offline; fi

KEY_HEX="$(openssl pkey -in "$KEYFILE" -pubout -outform DER | tail -c 32 | xxd -p | tr -d '\n')"
EXPECT="${WAYGATE_UPDATE_PUBKEY_HEX:-}"
if [[ -n "$EXPECT" && "$KEY_HEX" != "$EXPECT" ]]; then
  echo "signing key pubkey ($KEY_HEX) is not the launcher's ($EXPECT); the app would reject this index" >&2; exit 1
fi

WORK="$(mktemp -d)"; trap 'rm -rf "$WORK"' EXIT
SIG_HEX="$(openssl pkeyutl -sign -inkey "$KEYFILE" -rawin -in "$REG" | xxd -p | tr -d '\n')"
openssl pkey -in "$KEYFILE" -pubout -out "$WORK/pub.pem"
printf '%s' "$SIG_HEX" | xxd -r -p > "$WORK/sig.bin"
openssl pkeyutl -verify -pubin -inkey "$WORK/pub.pem" -rawin -in "$REG" -sigfile "$WORK/sig.bin" >/dev/null \
  || { echo "self-verify FAILED" >&2; exit 1; }
B64="$(base64 -w0 "$REG")"
printf '{"schema":1,"signature_alg":"ed25519","signature":"%s","payload_b64":"%s"}\n' "$SIG_HEX" "$B64" > "$OUT"
echo "signed $(basename "$REG") -> $OUT (pubkey $KEY_HEX, $(python3 -c "import json;print(len(json.load(open('$REG'))['mods']))") mods)"
