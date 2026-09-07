#!/bin/bash

# Generates vmlinux.h for CO-RE eBPF compilation.
# Requires bpftool:
# Ubuntu/Debian : sudo apt install -y linux-tools-$(uname -r)
# Fedora/RHEL   : sudo dnf install -y bpftool

set -euo pipefail

FILENAME="vmlinux.h"
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)/src"
OUTPUT="$ROOT/kernel/includes/$FILENAME"

if [[ -s "$OUTPUT" ]]; then
    echo "vmlinux.h already exists."
    exit 0
fi

echo "Generating vmlinux.h..."

mkdir -p "$(dirname "$OUTPUT")"

sudo bpftool btf dump file /sys/kernel/btf/vmlinux format c > "$OUTPUT"

echo "vmlinux.h generated: $OUTPUT"