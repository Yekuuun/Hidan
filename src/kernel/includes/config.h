/**
 * Contains all utilities for bpf module configuration & resolves
 */

#ifndef CONFIG_H
#define CONFIG_H

#define MAX_PID_ENTRIES 32

#ifndef TASK_COMM_LEN
#define TASK_COMM_LEN 16
#endif

#ifndef MAX_ENTRIES
#define MAX_ENTRIES 128
#endif

#endif