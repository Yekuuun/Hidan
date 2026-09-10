# Makefile global

KERNEL_DIR    := kernel
DAEMON_DIR    := Deamon
DAEMON_PROJ   := $(DAEMON_DIR)/Deamon.csproj

CONFIGURATION ?= Release
RID           ?= linux-x64
OUT_DIR       ?= build
DOTNET        ?= dotnet

.DEFAULT_GOAL := all
.PHONY: all kernel daemon publish run clean

all: daemon

kernel:
	$(MAKE) -C $(KERNEL_DIR)

daemon: kernel
	$(DOTNET) build $(DAEMON_PROJ) -c $(CONFIGURATION)

publish: kernel
	$(DOTNET) publish $(DAEMON_PROJ) -c $(CONFIGURATION) -r $(RID) \
		--self-contained true -o $(OUT_DIR)

run: publish
	sudo ./$(OUT_DIR)/Deamon

clean:
	$(MAKE) -C $(KERNEL_DIR) clean
	$(DOTNET) clean $(DAEMON_PROJ) -c $(CONFIGURATION)
	rm -rf $(OUT_DIR)