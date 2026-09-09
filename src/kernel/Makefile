CC     		:= clang
ARCH		:= x86

SRC_DIR 	:= .
OUT_DIR 	:= ./bin

INCLUDE 	:= ./includes
MODULES		:= ./modules 
LIB			:= ./lib

BPF_SRC := $(SRC_DIR)/main.bpf.c 
BPF_OBJ := $(OUT_DIR)/main.bpf.o

BPF_CFLAGS := \
	-g \
	-O2 \
	-target bpf \
	-D__bpf__ \
	-D__TARGET_ARCH_$(ARCH) \
	-I$(INCLUDE) \
	-I$(MODULES) \
	-I$(LIB) \
	-I/usr/include/bpf \
	-I/usr/include/$(shell uname -m)-linux-gnu

.PHONY: all clean

all: $(BPF_OBJ)

$(OUT_DIR):
	mkdir -p $(OUT_DIR)

$(BPF_OBJ): $(BPF_SRC) | $(OUT_DIR)
	$(CC) $(BPF_CFLAGS) -c $< -o $@

clean:
	rm -rf $(OUT_DIR)