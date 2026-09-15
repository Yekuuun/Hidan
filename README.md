```
 ___  ___  ___  ________  ________  ________      
|\  \|\  \|\  \|\   ___ \|\   __  \|\   ___  \    
\ \  \\\  \ \  \ \  \_|\ \ \  \|\  \ \  \\ \  \   
 \ \   __  \ \  \ \  \ \\ \ \   __  \ \  \\ \  \  
  \ \  \ \  \ \  \ \  \_\\ \ \  \ \  \ \  \\ \  \ 
   \ \__\ \__\ \__\ \_______\ \__\ \__\ \__\\ \__\
    \|__|\|__|\|__|\|_______|\|__|\|__|\|__| \|__|
                                                  
offensive eBPF techniques lab researchs. linux kernel version > 5.5                                 

```

Hidan is a personnal lab build mainly to focus on how to "hack" legitimate EBPF purpose into complexe offensive scenarios. Consider it as a "big" POC built non for ready to
go pentesting tool but for research purposes. Hidan's Deamon is built using pure C# based on the Mango.Libbpf lib I wrote. Avoiding to be stuck with Go & Rust for userland apps.

## How it works ?

//to do.

> [!Important]
> I did not focus on advanced obfuscation techniques etc. I'm only here for working demos.

## Project structure
.
├── LICENSE
├── README.md
├── scripts
│   └── vmlinux.sh
└── src
    ├── build_all.sh
    ├── Deamon
    ├── kernel
    ├── Makefile
    └── out

## Pipeline detail

// to do.

## Build 

// to do.

## Limitations

// to do.

## References

- [Debug EBPF](https://oneuptime.com/blog/post/2026-01-07-ebpf-debugging-troubleshooting/view) — Debug Ebpf programs.
- [Linux x64 syscalls](https://syscalls64.paolostivanin.com/) - Linux syscal references x64
- [Linux kernel explorer](https://elixir.bootlin.com/linux/v6.17.3/A/ident/) - Linux kernel explorer
- [EBPF OFFICIAL DOC](https://docs.ebpf.io/) - Ebpf official documentation
- [MattheuZ Singularity](https://github.com/MatheuZSecurity/Singularity) - Singularity Rootkit
- [TripleCross](https://github.com/Yekuuun/TripleCross/blob/master/src/ebpf/include/bpf/fs.h) - TripleCross Ebpf based rootkit.