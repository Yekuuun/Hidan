#!/bin/bash

OUT_DIR="./out"
MAIN_BPF_O="main.bpf.o"
DEAMON="Deamon"
NET_FRAMEWORK="net10.0"

# -----------------------------------------
# Functions
# -----------------------------------------

create_out_dir() {
    mkdir -p "$OUT_DIR"
}

cp_out_files() {
    local path="$1"

    if [ ! -e "$path" ]; then
        echo "[!] File or directory not found: $path"
        return 1
    fi

    cp -a "$path" "$OUT_DIR/"
}

# -----------------------------------------

echo -e "[*] STARTING COMPILATION :\n"
rm -rf $OUT_DIR

sleep 0.5

# Compile all
make clean && make all
make_res=$?

if [ "$make_res" -ne 0 ]; then
    echo -e "[!] Error compiling project. Make returned: $make_res."
    exit "$make_res"
fi

echo -e "[*] Successfully compiled project."

# -----------------------------------------
# Recreate output directory
# -----------------------------------------

rm -rf "$OUT_DIR"
create_out_dir

# -----------------------------------------
# Copying files
# -----------------------------------------

echo "[*] Copying kernel files..."
cp -a "./kernel/bin/." "$OUT_DIR/"

echo "[*] Copying daemon..."
cp -a "./Deamon/bin/Debug/$NET_FRAMEWORK/." "$OUT_DIR/"

echo -ne "[*] Output files: \n"
find "$OUT_DIR" -maxdepth 2 -type f

echo -ne "\n\n[*] Done.\n"
