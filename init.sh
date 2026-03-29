#!/usr/bin/env bash
set -euo pipefail

fail() {
    printf 'Error: %s\n' "$1" >&2
    exit 1
}

require_placeholder() {
    local content="$1"
    local placeholder="$2"

    if [[ "$content" != *"$placeholder"* ]]; then
        fail "Placeholder not found in template: $placeholder"
    fi
}

command -v git >/dev/null 2>&1 || fail "git is required to run init.sh"
command -v openssl >/dev/null 2>&1 || fail "openssl is required to run init.sh"

SCRIPT_ROOT="$(git rev-parse --show-toplevel 2>/dev/null)" || fail "Unable to determine repository root with git rev-parse --show-toplevel"
[[ -n "$SCRIPT_ROOT" ]] || fail "git rev-parse --show-toplevel returned an empty repository root"
[[ -d "$SCRIPT_ROOT" ]] || fail "Repository root does not exist: $SCRIPT_ROOT"

TEMPLATE_PATH="$SCRIPT_ROOT/src/AppHost/Realms/realm.example.json"
OUTPUT_PATH="$SCRIPT_ROOT/src/AppHost/Realms/realm.json"

[[ -f "$TEMPLATE_PATH" ]] || fail "Template file not found: $TEMPLATE_PATH"

WEATHER_API_CLIENT_SECRET="$(openssl rand -hex 24)"
WEATHER_CONSUMER_A_CLIENT_SECRET="$(openssl rand -hex 24)"
WEATHER_CONSUMER_B_CLIENT_SECRET="$(openssl rand -hex 24)"
DEV_ADMIN_PASSWORD="$(openssl rand -hex 4)"

CONTENT="$(<"$TEMPLATE_PATH")"

require_placeholder "$CONTENT" "__WEATHER_API_CLIENT_SECRET__"
require_placeholder "$CONTENT" "__WEATHER_CONSUMER_A_CLIENT_SECRET__"
require_placeholder "$CONTENT" "__WEATHER_CONSUMER_B_CLIENT_SECRET__"
require_placeholder "$CONTENT" "__DEV_ADMIN_PASSWORD__"

CONTENT="${CONTENT//__WEATHER_API_CLIENT_SECRET__/$WEATHER_API_CLIENT_SECRET}"
CONTENT="${CONTENT//__WEATHER_CONSUMER_A_CLIENT_SECRET__/$WEATHER_CONSUMER_A_CLIENT_SECRET}"
CONTENT="${CONTENT//__WEATHER_CONSUMER_B_CLIENT_SECRET__/$WEATHER_CONSUMER_B_CLIENT_SECRET}"
CONTENT="${CONTENT//__DEV_ADMIN_PASSWORD__/$DEV_ADMIN_PASSWORD}"

printf '%s\n' "$CONTENT" > "$OUTPUT_PATH"

printf 'Generated realm file: %s\n' "$OUTPUT_PATH"
printf 'weather_api secret: %s\n' "$WEATHER_API_CLIENT_SECRET"
printf 'weather_consumer_a secret: %s\n' "$WEATHER_CONSUMER_A_CLIENT_SECRET"
printf 'weather_consumer_b secret: %s\n' "$WEATHER_CONSUMER_B_CLIENT_SECRET"
printf 'dev-admin password: %s\n' "$DEV_ADMIN_PASSWORD"
