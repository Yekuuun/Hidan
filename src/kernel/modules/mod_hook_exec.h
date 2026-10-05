#ifndef MOD_HOOK_EXEC
#define MOD_HOOK_EXEC

#include "../includes/bpf_config.h"
#include "../includes/bpf_common.h"
#include "../includes/bpf_structs.h"
#include "../includes/bpf_debug.h"
#include "../includes/bpf_events_handler.h"
#include "../data/bpf_events.h"
#include "../data/bpf_ringbuf.h"
#include "../lib/ftlib.h"

/**
 * For the sake of simplicity, using bash as the main target for the execve exploit.
 * 
 * @ref https://man7.org/linux/man-pages/man2/execve.2.html 
 * @ref https://syscalls64.paolostivanin.com/
 */
#define EXEC_TARGET        "bash"
#define EXEC_TARGET_BINARY "ls"

SEC("tp/syscalls/sys_enter_execve")
int tp_sys_enter_execve(struct sys_enter_execve_ctx *ctx)
{
    if(!ctx || !ctx->filename || !ctx->argv)
        return 0;

    if(!__is_target_bin())
        return 0;

    //making sure only handling "bash" exec.
    char comm[TASK_COMM_LEN] = {0};
    bpf_get_current_comm(comm, sizeof(comm));

    if(ft_strcmp(comm, EXEC_TARGET, 32) != 0)
        return 0;
    //---------------------------------------

    //checking data.
    char exec_path[MAX_PATH] = {0};
    if(bpf_probe_read_user_str(exec_path, sizeof(exec_path), ctx->filename) < 0) {
        PRINT_DEBUG("Error retrieving exec path data.");
        return 0;
    }

    //we could only focus a sample target binary like 'ls' or 'cat'
    if(__bpf_strstr(exec_path, EXEC_TARGET_BINARY) == NULL)
        return 0;

    PRINT_DEBUG("Comm : %s is trying to launch : %s", comm, exec_path);
    return 0;
}

#endif