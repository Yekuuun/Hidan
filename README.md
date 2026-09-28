```
                       ___  ___  ___  ________  ________  ________      
                      |\  \|\  \|\  \|\   ___ \|\   __  \|\   ___  \    
                      \ \  \\\  \ \  \ \  \_|\ \ \  \|\  \ \  \\ \  \   
                       \ \   __  \ \  \ \  \ \\ \ \   __  \ \  \\ \  \  
                        \ \  \ \  \ \  \ \  \_\\ \ \  \ \  \ \  \\ \  \ 
                         \ \__\ \__\ \__\ \_______\ \__\ \__\ \__\\ \__\
                          \|__|\|__|\|__|\|_______|\|__|\|__|\|__| \|__|
                                                                        
                 offensive eBPF techniques lab research. linux kernel version > 5.5  

```

---

## ⚠️ CRITICAL DISCLAIMERS

### Research Purpose Only
**Hidan** is a red team research laboratory designed exclusively for **authorized security testing**, **defensive security research**, and **educational contexts**. This project demonstrates offensive eBPF techniques for legitimate purposes only.

### Legal Warning
- **UNAUTHORIZED ACCESS PROHIBITED**: Deploying these techniques on systems you do not own or have explicit written permission to test is illegal and unethical.
- **COMPLIANCE REQUIRED**: Ensure you have proper authorization and comply with applicable laws in your jurisdiction.
- **YOUR RESPONSIBILITY**: Users are solely responsible for ensuring lawful and ethical use of this project.
- **NO WARRANTY**: This software is provided "as is" without any warranty of fitness or legality for any particular use.

### Authorized Use Only
This project is intended for use by:
- Security professionals conducting authorized penetration tests
- Red teams in controlled lab environments  
- Researchers studying kernel security mechanisms
- Defensive security teams evaluating system vulnerabilities
- Educational institutions with proper oversight

**Unauthorized deployment of these techniques constitutes a serious crime.**

---

## Overview

Hidan is a personal research laboratory that explores how legitimate eBPF (extended Berkeley Packet Filter) mechanisms can be weaponized in complex offensive scenarios. Rather than providing ready-to-use pentesting tools, Hidan serves as a comprehensive proof-of-concept for understanding modern kernel-level attack vectors.

### Core Philosophy
The project demonstrates that **defensive mechanisms designed for legitimate purposes** can be repurposed for offensive objectives. By understanding these techniques deeply, defenders can better protect systems against kernel-level threats.

### Technology Stack
- **Kernel Programs**: Written in eBPF/C with modern hooking techniques (kprobes, kretprobes, LSM hooks, tracepoints)
- **Userland Daemon**: Pure C# built on top of **Mango.Libbpf** (custom C# library for libbpf bindings)
- **Approach**: Avoiding Go/Rust constraints for maximum flexibility in userland application design

---

## Architecture & Design

### Two-Tier Architecture

```
┌─────────────────────────────────────────┐
│         Userland: C# Daemon             │
│  (eBPF Loading, Mapping, Events)        │
├─────────────────────────────────────────┤
│    Kernel: eBPF Programs (C)            │
│  (Hooks, Interception, Manipulation)    │
└─────────────────────────────────────────┘
```

#### Daemon Component (C#)
Responsible for:
- Loading compiled eBPF object files (.o binaries)
- Mapping eBPF programs to kernel hooks
- Managing eBPF maps for shared kernel-userland data
- Handling event ringbuffer consumption
- Providing CLI interface for runtime control
- Terminal GUI for debugging and monitoring

**Key Libraries**:
- `Mango.Libbpf` - Custom C# wrapper for libbpf
- `Terminal.Gui` - Rich terminal interface
- `System.CommandLine` - CLI argument parsing
- `Microsoft.Extensions.*` - Dependency injection & configuration

#### Kernel Component (eBPF)
Implements the actual hooking mechanisms:
- **LSM Hooks** - Linux Security Module framework for policy enforcement
- **Kprobes/Kretprobes** - Kernel function entry/return hooks
- **Tracepoints** - Static instrumentation points
- **Ring Buffers** - High-performance event exfiltration

---

## Project Structure

```
hidan/
├── README.md                          # This file
├── LICENSE                            # MIT License
├── docs/                              # Documentation assets
│   ├── block_rmdir.png               # Technique demonstration
│   ├── hidden_pid.png                # Process hiding example
│   └── hidden_user.png               # User hiding example
│
├── scripts/
│   └── vmlinux.sh                    # Generate vmlinux.h for current kernel
│
└── src/
    ├── build_all.sh                  # Master build script
    ├── Makefile                      # Build orchestration
    │
    ├── kernel/                       # eBPF kernel programs
    │   ├── main.bpf.c               # Main eBPF entry point
    │   ├── Makefile                 # Kernel build configuration
    │   │
    │   ├── includes/                # Header files
    │   │   ├── bpf_common.h         # Shared utilities
    │   │   ├── bpf_config.h         # Configuration constants
    │   │   ├── bpf_debug.h          # Debug helpers
    │   │   ├── bpf_events_handler.h # Event system
    │   │   ├── bpf_maps.h           # Map definitions
    │   │   ├── bpf_structs.h        # Data structures
    │   │   └── vmlinux.h            # Auto-generated kernel types
    │   │
    │   ├── modules/                 # Feature modules
    │   │   ├── mod_hide_from.h      # Hide from various mechanisms
    │   │   ├── mod_hook_open.h      # Hook open() syscall
    │   │   ├── mod_hook_read.h      # Hook read() syscall
    │   │   ├── mod_lsm.h            # LSM hook implementations
    │   │   └── mod_test_evt.h       # Test event generation
    │   │
    │   ├── lib/                     # Shared libraries
    │   │   └── ftlib.h              # Fast utility functions
    │   │
    │   ├── data/                    # Data structures
    │   │   ├── bpf_events.h         # Event definitions
    │   │   └── bpf_ringbuf.h        # Ring buffer structures
    │   │
    │   ├── bin/                     # Build output
    │   │   └── main.bpf.o          # Compiled eBPF object
    │   └── out/                     # Intermediate outputs
    │
    └── Deamon/                       # C# Userland Application
        ├── Deamon.csproj           # Project configuration
        ├── AppSettings.json         # Application settings
        ├── Startup.cs              # Initialization logic
        │
        ├── Ebpf/                   # eBPF Integration Layer
        │   ├── EbpfRuntime.cs      # Main runtime orchestrator
        │   ├── EbpfConfiguration.cs # Configuration management
        │   │
        │   ├── Abstraction/        # Interfaces & abstractions
        │   │   ├── IEbpfProgramActions.cs
        │   │   ├── IEbpfMapActions.cs
        │   │   ├── IEbpfEventReader.cs
        │   │   └── EbpfState.cs
        │   │
        │   ├── Mapping/            # eBPF Map handlers
        │   │   ├── BaseMapConfig.cs
        │   │   ├── MapHidePid.cs
        │   │   ├── MapCacheHideBin.cs
        │   │   ├── MapCacheHideDir.cs
        │   │   └── MapRingbuffer.cs
        │   │
        │   ├── Events/             # Event processing
        │   │   ├── EbpfEventReader.cs
        │   │   ├── EbpfEvent.cs
        │   │   ├── EbpfEventPayload.cs
        │   │   └── EbpfEventHeader.cs
        │   │
        │   ├── Runtime/            # Lifecycle & execution
        │   │   ├── EbpfLifecyleService.cs
        │   │   ├── EbpfRuntime.Actions.cs
        │   │   └── EbpfRuntime.Ringbuffer.cs
        │   │
        │   ├── Debug/              # Debugging utilities
        │   │   └── EbpfEventLoggerService.cs
        │   │
        │   ├── Config/             # Configuration
        │   │   └── EbpfConstants.cs
        │   │
        │   ├── DTO/                # Data transfer objects
        │   │   ├── MapDto.cs
        │   │   └── ProgramDto.cs
        │   │
        │   └── Utils/              # Utility functions
        │       └── EbfpUtils.cs
        │
        ├── Cmd/                    # CLI Command Framework
        │   ├── CommandRegistry.cs  # Command registration
        │   ├── OutputCommandWriter.cs
        │   │
        │   ├── Abstraction/
        │   │   ├── ICmdCommand.cs
        │   │   └── CliCommandBase.cs
        │   │
        │   └── Commands/           # Implemented commands
        │       ├── HelpCommand.cs
        │       ├── ProgCommand.cs   # Program control
        │       ├── MapCommand.cs    # Map inspection
        │       ├── DebugCommand.cs  # Debugging
        │       ├── ClearCommand.cs
        │       └── QuitCommand.cs
        │
        ├── Gui/                    # Terminal UI
        │   ├── TerminalGui.cs      # Main GUI orchestrator
        │   ├── TerminalGui.Setup.cs
        │   ├── TerminalGui.Events.cs
        │   ├── TerminalGuiService.cs
        │   ├── TerminalLogPane.cs
        │   │
        │   ├── Abstraction/
        │   │   └── IOutputCommand.cs
        │   │
        │   └── Config/
        │       └── EOutPane.cs
        │
        ├── Logger/                 # Logging system
        │   ├── DeamonLogger.cs
        │   └── ELogError.cs
        │
        ├── Config/                 # Configuration
        │   └── ConfigureServices.cs
        │
        └── Utils/                  # Utilities
            ├── StrUtils.cs
            └── ByteArrayComparer.cs
```

---

## Offensive Techniques Implemented

### 1. Process Hiding (PID Hiding)
**Mechanism**: Hook `getdents64()` syscall to filter process entries from `/proc`

![Hidden PID Example](docs/hidden_pid.png)

**Implementation**:
- Intercepts directory listing syscalls
- Removes entries matching hidden PIDs from kernel-level ringbuffer cache
- Transparent to userland - processes appear completely invisible
- **Impact**: Hidden processes don't appear in `ps`, `top`, or `/proc` enumeration

---

### 2. File/Directory Hiding
**Mechanism**: Hook file access syscalls to selectively hide files/directories

![Hidden User Example](docs/hidden_user.png)

**Implementation**:
- Intercepts `open()`, `openat()`, `getdents64()` syscalls
- Maintains kernel-space cache of hidden paths
- Blocks access attempts while hiding entries from directory listings
- **Impact**: Files effectively become invisible and inaccessible to user programs

---

### 3. Directory Operations Blocking
**Mechanism**: Use LSM hooks to prevent specific filesystem operations

![Block RMDIR Example](docs/block_rmdir.png)

**Implementation**:
- LSM `bprm_check_security` and `file_*` hooks
- Prevent directory removal, file deletion, or other operations
- **Impact**: Critical system paths can be immutable without traditional ACLs

---

### 4. LSM Hook-Based Access Control
**Technique**: Leverage Linux Security Module framework for fine-grained control

**Capabilities**:
- `bprm_check_security` - Control binary execution
- `file_open` - Intercept file access
- `file_permission` - Fine-grained permission checks
- `bpf_lsm_*` hooks - Modern eBPF-based LSM integration

---

## Dependencies

### System Requirements
- **Linux Kernel**: Version 5.5 or later (eBPF BPF LSM support)
- **Architecture**: x86_64 (primary), ARM64 (tested)
- **Privileges**: Root/sudo required for eBPF program loading

### Build Dependencies

#### Kernel-Space (eBPF)
```bash
# LLVM/Clang toolchain
llvm-dev
clang
llvm

# BPF development
libbpf-dev
linux-headers-$(uname -r)
bpftool

# Optional for development
pahole          # Required for vmlinux.h generation
```

#### Userland (C#/.NET)
```
.NET 10.0 SDK or later
  - Microsoft.Extensions.Hosting (10.0.12)
  - Microsoft.Extensions.Configuration (10.0.12)
  - Microsoft.Extensions.Options (10.0.12)
  - System.CommandLine (2.0.12)
  - Terminal.Gui (2.5.0)
  - Mango.Libbpf (0.0.4+)
```

### Installation

#### Debian/Ubuntu
```bash
sudo apt-get update
sudo apt-get install -y \
    clang llvm llvm-dev \
    libbpf-dev \
    linux-headers-$(uname -r) \
    bpftool \
    pahole
```

#### RHEL/CentOS/Fedora
```bash
sudo dnf install -y \
    clang llvm llvm-devel \
    libbpf-devel \
    kernel-devel-$(uname -r) \
    bpf-tools
```

#### .NET Runtime
Install from: https://dotnet.microsoft.com/download

---

## Building & Installation

### Prerequisites Check
```bash
# Verify kernel version
uname -r                    # Must be >= 5.5

# Check BPF support
cat /boot/config-$(uname -r) | grep CONFIG_BPF
# Should show: CONFIG_BPF=y

# Verify LSM BPF support
cat /boot/config-$(uname -r) | grep CONFIG_BPF_LSM
# Should show: CONFIG_BPF_LSM=y
```

### Build Steps

#### 1. Generate vmlinux.h (if not present)
```bash
cd scripts/
./vmlinux.sh
# Generates vmlinux.h for current kernel types
```

#### 2. Build Everything
```bash
cd src/
chmod +x build_all.sh
./build_all.sh
```

This script:
- Compiles eBPF kernel programs (`kernel/Makefile`)
- Builds C# daemon (`dotnet publish`)
- Places output in `out/` directory

#### 3. Run the Daemon
```bash
sudo ./out/deamon
# Loads eBPF programs and starts interactive CLI
```

### Build Troubleshooting

**Issue**: `clang: unknown target 'bpf'`
```bash
# Upgrade LLVM/Clang
clang --version  # Should be 10.0+
```

**Issue**: `libbpf not found`
```bash
# Ensure libbpf headers are available
pkg-config --cflags libbpf
```

**Issue**: `vmlinux.h generation fails`
```bash
# Manually generate from kernel build:
bpftool btf dump file /sys/kernel/btf/vmlinux format c > vmlinux.h
```

---

## Usage & Interactive Commands

Once the daemon is running (`sudo ./out/deamon`), interact via CLI:

### Program Management
```bash
prog list              # List loaded eBPF programs
prog load <id>         # Load specific program
prog unload <id>       # Unload program
prog state             # Show program states
```

### Map Inspection & Control
```bash
map list               # List all eBPF maps
map dump <name>        # Dump map contents
map clear <name>       # Clear map
map set <name> <k> <v> # Set map entry
```

### Debugging
```bash
debug on                # Enable debug output
debug off               # Disable debug output
debug events            # Show kernel events
```

### Utility Commands
```bash
help                   # Show command help
clear                  # Clear terminal
quit                   # Exit daemon
```

### Example Workflow
```bash
sudo ./out/deamon

# Load and activate PID hiding
prog load 0
map list
map set hide_pid 1234  # Hide PID 1234

# Verify process is hidden
# (In another terminal: ps aux | grep 1234 should show nothing)

debug on
debug events           # Monitor events in real-time
```

---

## Technical Deep-Dives

### Event System (Ring Buffer)
- **Location**: `src/kernel/data/bpf_ringbuf.h`
- **Purpose**: High-performance, lock-free event exfiltration from kernel to userland
- **Payload**: Event headers + context data for debugging and audit trails

### Map Abstractions
- **Hide PID Map** (`MapHidePid.cs`): Tracks processes to hide
- **File Cache Maps** (`MapCacheHideBin.cs`, etc.): Caches hidden file/directory paths
- **Ring Buffer Map** (`MapRingbuffer.cs`): Event queue

### Lifecycle Management
- `EbpfLifecyleService.cs` - Handles attachment/detachment of hooks
- `EbpfRuntime.cs` - Orchestrates loading and event consumption
- Graceful cleanup on daemon shutdown

### String Filtering
- **Location**: `src/kernel/lib/ftlib.h`
- **Purpose**: Efficient kernel-space string operations
- Custom `__bpf_strstr` implementation avoiding kernel helper calls

---

## Security Considerations

### Detection & Defense

These techniques can be detected by:
- **Hardware-based monitoring** (Intel PT, etc.)
- **Kernel logs** (`journalctl`, `dmesg`) - may contain hook activities
- **Signed code analysis** - verifying program integrity
- **Secureboot + LSM enforcement** - restricting eBPF loading
- **Audit subsystem** - detailed syscall tracking

### Mitigation Strategies

Defenders should:
1. **Disable eBPF in untrusted contexts** - Set `kernel.unprivileged_bpf_disabled=1`
2. **Require LSM enforcement** - Configure SELinux/AppArmor policies
3. **Monitor `/proc/sys/kernel/bpf_stats_enabled`** - Track eBPF activity
4. **Use integrity checkers** - Verify system binaries haven't been hidden
5. **Kernel module signing** - Restrict eBPF program loading
6. **Hardware security** - TPM-based attestation

---

## Limitations & Scope

### Current Limitations
- **Must run as root** - eBPF program loading requires root privileges
- **Limited scope** - Techniques target specific syscalls/operations
- **Research-only focus** - No advanced obfuscation or anti-forensics
- **Kernel version dependent** - Some hooks vary by kernel version
- **No binary self-protection** - Daemon can be analyzed and reverse-engineered

### What This Is NOT
- ❌ A production rootkit framework
- ❌ Designed for stealth in adversarial environments
- ❌ Anti-forensics or anti-detection oriented
- ❌ Multi-stage exploitation framework
- ❌ Exploit development toolkit

### What This IS
- ✅ Educational research on eBPF offensive capabilities
- ✅ Proof-of-concept for kernel attack vectors
- ✅ Demonstration of legitimate mechanism misuse
- ✅ Defensive security research material
- ✅ Reference implementation for understanding eBPF hooking

---

## References & Resources

### eBPF Documentation
- [eBPF Official Documentation](https://docs.ebpf.io/) - Comprehensive eBPF guide
- [BPF & XDP Reference Guide](https://docs.cilium.io/en/v1.12/bpf/) - Cilium's reference
- [Libbpf Documentation](https://github.com/libbpf/libbpf) - C library for eBPF

### Linux Kernel References
- [Linux x64 Syscalls](https://syscalls64.paolostivanin.com/) - Complete syscall reference
- [Linux Kernel Explorer](https://elixir.bootlin.com/) - Source code browser
- [LSM Framework](https://www.kernel.org/doc/html/latest/security/lsm.html) - Security modules docs

### eBPF Debugging
- [eBPF Debugging Guide](https://oneuptime.com/blog/ebpf-debugging-troubleshooting) - Troubleshooting
- [BPF Loader](https://github.com/libbpf/libbpf) - Official loader reference
- [Kernel Tracing](https://www.kernel.org/doc/html/latest/trace/ftrace.html) - ftrace guide

### Related Research
- [Singularity - Rootkit Research](https://github.com/MatheuZSecurity/Singularity) - eBPF rootkit research
- [TripleCross](https://github.com/Yekuuun/TripleCross) - Advanced eBPF techniques
- [bpfload - Program Loader](https://github.com/libbpf/libbpf-bootstrap) - Bootstrap reference

### Security Research Communities
- [Kernel Self Protection Project](https://kernsec.org/) - Kernel hardening
- [CanSecWest](https://cansecwest.com/) - Security conference
- [Black Hat](https://www.blackhat.com/) - Security research venue

---

## Contributing & Feedback

This is a personal research project. For questions, improvements, or discussions:
- **Issues**: Research ideas or technical improvements
- **Security**: Report responsibly to maintainer
- **Legal**: Ensure authorized use only

---

## License

MIT License - See [LICENSE](LICENSE) file
Copyright (c) 2026 0xYkn

---

## Author

**Yekuuun** - Red Team Security Research
- Focus: Kernel-level offensive techniques, eBPF research
- Contact: Research inquiries only

---

**Last Updated**: September 2026  
**Status**: Active Research  
**Kernel Target**: Linux 5.5+  

⚠️ **REMEMBER**: This is a research tool for authorized security professionals only. Unauthorized access is illegal.
