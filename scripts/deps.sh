#!/bin/bash

sudo apt update

sudo apt install -y \
    clang \
    llvm \
    libbpf-dev \
    libelf-dev \
    gcc \
    make \
    pkg-config \
    linux-headers-$(uname -r) \
    bpftool