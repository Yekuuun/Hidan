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
#define EXEC_TARGET_LAUNCH "/usr/bin/gnome-text-editor" //debian config.
#define MAX_ARGS           12

static char *null_ptr = NULL;

/**
 * Simple utility to check if calling proc is "bash" & avoiding stack exceed error with inlined condition handling in tp handling.
 */
static __always_inline int __is_bash_comm(void)
{
    char comm[TASK_COMM_LEN] = {0};
    bpf_get_current_comm(comm, sizeof(comm));

    return ft_strcmp(comm, EXEC_TARGET, 32) == 0;
}

/**
 * Hooking tp_sys_enter_execve syscall.
 */
SEC("tp/syscalls/sys_enter_execve")
int tp_sys_enter_execve(struct sys_enter_execve_ctx *ctx)
{
    if(!ctx || !ctx->filename || !ctx->argv)
        return 0;

    if(!__is_target_bin())
        return 0;

    //making sure only handling "bash" exec.
    if(!__is_bash_comm())
        return 0;
    //---------------------------------------

    //checking data.
    char *argv[MAX_ARGS]     = {0};
    char exec_path[MAX_PATH] = {0};

    if(bpf_probe_read_user_str(exec_path, sizeof(exec_path), ctx->filename) < 0) {
        PRINT_DEBUG("Error retrieving exec path data.");
        return 0;
    }

    //we could only focus a sample target binary like 'ls' or 'cat'
    if(__bpf_strstr(exec_path, EXEC_TARGET_BINARY) == NULL)
        return 0;

    if(bpf_probe_read_user(argv, sizeof(argv), ctx->argv) < 0) {
        PRINT_DEBUG("Error retrieving argv data from user space adress");
        return 0;
    }

    PRINT_DEBUG("Bash : %s is trying to launch : %s", exec_path);

    /**
     * NOTE : BE CAREFULL to stack exceed limits when un comment this section !
     */
    //showing args.
    // char arg[64];

    // #pragma unroll
    // for (short i = 0; i < MAX_ARGS; i++) {
    //     if (!argv[i])
    //         break;

    //     if (bpf_probe_read_user_str(arg, sizeof(arg), argv[i]) < 0)
    //         break;

    //     PRINT_DEBUG("Arg => %d & value : %s", i, arg);
    // }

    //1. Writing the new executable path.
    //to do.

    //2. Updating argv.
    //to do.

    //exec. & check logs.
    send_event(EVENT_TRIGGERED, "__x64_sys_execve" ,"Exec target launch ! gnome text editor.");
    return 0;
}

#endif